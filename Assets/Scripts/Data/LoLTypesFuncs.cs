using System;
using System.Collections.Concurrent;
using System.Collections.Generic;

#pragma warning disable IDE0041 // Use 'is null' check

namespace LoL
{
    public static partial class Types
    {
        // Static optional pool for all DeserializableType.
        static Dictionary<Type, ConcurrentBag<object>> _TypePool = new Dictionary<Type, ConcurrentBag<object>>();

        // Take or Create a new instance.
        public static object TakeOrCreate(Type type)
        {
            if (_TypePool.TryGetValue(type, out var bag) && bag.TryTake(out var item))
            {
                return TryReset(item);
            }
            return Activator.CreateInstance(type, true);
        }

        // Take or Create a new instance.
        public static T TakeOrCreate<T>()
        {
            return (T) TakeOrCreate(typeof(T));
        }

        // Reset if implemented.
        static object TryReset(object obj)
        {
            if (obj is Data.IResetable resetable)
            {
                resetable.Reset();
            }
            return obj;
        }

        // Add the instance back to the pool.
        public static void AddBackToPool<T>(T obj)
        {
            Type type = typeof(T);
            // Ignore value types and nulls
            if (
#if LOL_GAMEFRAME
            !Data.DebugConfig.DataPooling || 
#endif
                type.IsValueType || ReferenceEquals(obj, null))
            {
                return;
            }
            type = obj.GetType();
            if (!_TypePool.TryGetValue(type, out var bag))
            {
                bag = new ConcurrentBag<object>();
                _TypePool.Add(type, bag);
            }
            bag.Add(obj);
        }

        // Remove a type pool. Useful if the type is only needed in certain scenes.
        public static void RemovePool(Type type)
        {
            _TypePool.Remove(type);
        }

        // Clears all the pools. Good for complete memory clean up.
        public static void ClearAllPools()
        {
#if LOL_GAMEFRAME
            Utils.LoLDebug.Log("Clearing all type pools");
#endif
            _TypePool.Clear();
        }

        public static Type GetType(string key)
        {
#if LOL_GENERATED
            _Types.TryGetValue(key.ToLower(), out var type);
            return type;
#else
            return null;
#endif
        }
    }
}