using LoL.Data;
using System;
using System.Collections.Generic;
using UnityEngine;

namespace LoL.Data
{
    public interface IDeserializable
    {
        void Deserialize(SimpleJSON.JSONNode jsonNode);
    }

    public interface IOverwritable
    {
        void Overwrite(SimpleJSON.JSONNode jsonNode);
    }

    public interface ISerializable
    {
        SimpleJSON.JSONNode Serialize();
    }

    // Used to reset an pool'd object before deserialization or a new usage.
    public interface IResetable
    {
        void Reset();
    }

    /* Example of use.
    public class TestModel : IDeserializable, ISerializable
    {
        int _id;
        string _name;
        static bool _Active;
        public float amt = 5.0f;

        public virtual void Deserialize(SimpleJSON.JSONNode jsonNode)
        {
            _id = jsonNode["id"].AsInt;
            _name = jsonNode["name"].Value;
            _Active = jsonNode["active"].AsBool;
            amt = jsonNode[nameof(amt)].AsFloat;
        }

        public virtual SimpleJSON.JSONNode Serialize()
        {
            var json = new SimpleJSON.JSONObject();
            json["id"] = _id;
            json["name"] = _name;
            json["active"] = _Active;
            json[nameof(amt)] = amt;
            return json;
        }
    }
    */

}

namespace LoL.SimpleJSON
{
    public partial class JSONNode
    {
        public static implicit operator JSONNode(Color col)
        {
            return new JSONObject()
                .Write("r", col.r)
                .Write("g", col.g)
                .Write("b", col.b)
                .Write("a", col.a)
                ;
        }

        public static implicit operator Color(JSONNode aNode)
        {
            return aNode.ReadColor();
        }

        public Color ReadColor(Color aDefault)
        {
            if (IsObject)
            {
                return new Color(
                    this["r"],
                    this["g"],
                    this["b"],
                    this["a"]
                    );
            }
            if (IsArray)
            {
                return Count == 3
                    ? new Color(
                        this[0],
                        this[1],
                        this[2]
                    )
                    : new Color(
                        this[0],
                        this[1],
                        this[2],
                        this[3]
                    );
            }
            return aDefault;
        }

        public Color ReadColor()
        {
            return ReadColor(Color.white);
        }
    }

    public abstract class DeserializableObject : IDeserializable
    {
        public abstract void Deserialize(JSONNode jsonNode);
    }

    public static class SimpleJsonExtensions
    {
        public static string EmptyJsonObject { get { return $"{{ \"v\": \"{Application.version}\" }}"; } }

        public static bool IsNotNullable(this Type type)
        {
            return type.IsValueType && Nullable.GetUnderlyingType(type) == null;
        }

        public static bool IsNull<T>(this T subject)
        {
            return !typeof(T).IsNotNullable() && ReferenceEquals(subject, null);
        }

        public static void Overwrite<T>(this JSONNode jsonNode, T obj) where T : IOverwritable
        {
            if (jsonNode == null || jsonNode.IsNull)
            {
                return;
            }
            obj.Overwrite(jsonNode);
        }

        /// <summary>
        /// Gets the value using the supplied keys.
        /// This gets nested children nodes.
        /// </summary>
        /// <returns>The value.</returns>
        /// <param name="jsonNode">Json node.</param>
        /// <param name="keys">Keys.</param>
        public static JSONNode GetValue(this JSONNode jsonNode, params object[] keys)
        {
            if (keys == null || keys.Length == 0)
                return jsonNode;

            var jToken = jsonNode;
            for (int i = 0; i < keys.Length; ++i)
            {
                var key = keys[i];
                jToken = key is int num ? jToken[num] : jToken[key.ToString()];
            }

            return jToken;
        }

        public static void Deserialize<T>(this JSONNode jsonNode, out T? value) where T : struct, IDeserializable
        {
            if (jsonNode == null) // If json value is null and T can't be null
            {
                value = default;
            }
            else
            {
                value = jsonNode.Deserialize<T>();
            }
        }

        public static T Deserialize<T>(this JSONNode jsonNode, T defaultValue = default) where T : IDeserializable
        {
            return (T)Deserialize(jsonNode, typeof(T), defaultValue);
        }

        public static IDeserializable Deserialize(this JSONNode jsonNode, Type type, IDeserializable defaultValue = default)
        {
            if (jsonNode == null ||
                (jsonNode.IsNull && type.IsNotNullable())) // If json value is null and T can't be null
            {
                return defaultValue;
            }

            // (type, isPooled)
            // First check if the raw type name is the key.
            Type registeredType = LoL.Types.GetType(type.ToString());

            string eventString = jsonNode["action_event"];
            // Check for Deserializing to this action_event type.
            if (!string.IsNullOrEmpty(eventString))
            {
                // If this type has been registered, deserialized this action.
                registeredType = LoL.Types.GetType(eventString);
            }

            // If not, use the type fallback.
            // Check for DeserializableTypes.
            string typeString = jsonNode["$type"].AsString(jsonNode["data_type"]);
            if (string.IsNullOrEmpty(typeString))
                typeString = jsonNode["action_type"];

            // If the event type was not found, check action type.
            if (registeredType == null && !string.IsNullOrEmpty(typeString))
            {
                registeredType = LoL.Types.GetType(typeString);

                // Error checking.
                // Fallback with warning.
                if (registeredType == null)
                {
                    registeredType = LoL.Types.GetType("default_fallback");
                }
            }

            // Check the pool for an available instance, if not create.
            // If not pooled, just create.
            if (registeredType == null)
                defaultValue = (IDeserializable)Activator.CreateInstance(type, true);
            else
                defaultValue = (IDeserializable)LoL.Types.TakeOrCreate(registeredType);

            defaultValue.Deserialize(jsonNode);
            return defaultValue;
        }

#if LOL_GAMEFRAME
        public static bool RunLoLEvent(this JSONNode questActionJSON, bool runImmediate = false)
        {
            if (questActionJSON != null)
            {
                // Create and run the action.
                // Must create so it can be reused.
                if (runImmediate)
                {
                    questActionJSON.Deserialize<LoL.IQuestAction>().Execute();
                }
                else
                {
                    LoL.GameController.GameSwitch(questActionJSON.Deserialize<LoL.IQuestAction>());
                }
                return true;
            }

            return false;
        }
#endif

        public static TEnum ToEnum<TEnum>(this JSONNode jsonNode, TEnum defaultValue = default) where TEnum : struct, Enum
        {
            if (jsonNode == null || jsonNode.IsNull)
            {
                return defaultValue;
            }
            if (!Enum.TryParse(jsonNode, true, out TEnum value))
            {
                value = (TEnum)(object)jsonNode.AsInt;
            }
            return value;
        }

        public static Color ToColorFromHex(this JSONNode jsonNode, Color defaultValue = default)
        {
            if (jsonNode == null || jsonNode.IsNull)
            {
                return defaultValue;
            }

            string hexColor = jsonNode;

            if (hexColor[0] != '#')
                hexColor = "#" + hexColor;

            if (ColorUtility.TryParseHtmlString(jsonNode, out var color))
                return color;
#if LOL_GAMEFRAME
            LoL.Utils.LoLDebug.LogError("Failed to parse html color: " + jsonNode.Value);
#endif
            return defaultValue;
        }

        public static int ToInt32(this JSONNode jsonNode, int defaultValue = default)
        {
            if (jsonNode == null || jsonNode.IsNull)
            {
                return defaultValue;
            }
            return jsonNode.AsInt;
        }

        public static float ToSingle(this JSONNode jsonNode, float defaultValue = default)
        {
            if (jsonNode == null || jsonNode.IsNull)
            {
                return defaultValue;
            }
            return jsonNode.AsFloat;
        }

        public static bool ToBool(this JSONNode jsonNode, bool defaultValue = default)
        {
            if (jsonNode == null || jsonNode.IsNull)
            {
                return defaultValue;
            }
            return jsonNode.AsBool;
        }

        // Jeff. Switched to AsString because I think the compiler was using this version instead of the JSONNode.ToString()
        // When no param is supplied.
        public static string AsString(this JSONNode jsonNode, string defaultValue = default)
        {
            if (jsonNode == null || jsonNode.IsNull)
            {
                return defaultValue;
            }
            return jsonNode;
        }



        /// <summary>
        /// If <paramref name="defaultIsNull"/> is false and jsonNode is null, returns an empty list.
        /// </summary>
        public static T[][] ToJaggedArray2D<T>(this JSONNode jsonNode, bool defaultIsNull = true) where T : IDeserializable
        {
            if (jsonNode == null || jsonNode.IsNull)
            {
                return defaultIsNull ? null : new T[0][];
            }

            var outerJArray = jsonNode.AsArray;
            T[][] array = new T[outerJArray.Count][];

            for (int i = 0; i < outerJArray.Count; ++i)
            {
                var innerJArray = outerJArray[i]?.AsArray;
                if (innerJArray == null)
                {
                    continue;
                }

                array[i] = new T[innerJArray.Count];

                for (int j = 0; j < innerJArray.Count; ++j)
                {
                    var node = innerJArray[j];
                    if (node == null)
                    {
                        continue;
                    }
                    array[i][j] = node.Deserialize<T>();
                }
            }
            return array;
        }

        /// <summary>
        /// If <paramref name="defaultIsNull"/> is false and jsonNode is null, returns an empty list.
        /// </summary>
        public static T[] ToArray<T>(this JSONNode jsonNode, bool defaultIsNull = true) where T : IDeserializable
        {
            if (jsonNode == null || jsonNode.IsNull)
            {
                return defaultIsNull ? null : new T[0];
            }

            var jArray = jsonNode.AsArray;
            T[] array = new T[jArray.Count];
            for (int i = 0; i < jArray.Count; ++i)
            {
                array[i] = jArray[i].Deserialize<T>();
            }
            return array;
        }

        /// <summary>
        /// If <paramref name="defaultIsNull"/> is false and jsonNode is null, returns an empty list.
        /// </summary>
        public static TEnum[] ToArrayEnum<TEnum>(this JSONNode jsonNode, bool defaultIsNull = true) where TEnum : struct, Enum
        {
            if (jsonNode == null || jsonNode.IsNull)
            {
                return defaultIsNull ? null : new TEnum[0];
            }

            var jArray = jsonNode.AsArray;
            TEnum[] array = new TEnum[jArray.Count];
            for (int i = 0; i < jArray.Count; ++i)
            {
                array[i] = jArray[i].ToEnum<TEnum>();
            }
            return array;
        }

        /// <summary>
        /// If <paramref name="defaultIsNull"/> is false and jsonNode is null, returns an empty list.
        /// </summary>
        public static string[] ToArrayString(this JSONNode jsonNode, bool defaultIsNull = true)
        {
            if (jsonNode == null || jsonNode.IsNull)
            {
                return defaultIsNull ? null : new string[0];
            }

            var jArray = jsonNode.AsArray;
            string[] array = new string[jArray.Count];
            for (int i = 0; i < jArray.Count; ++i)
            {
                array[i] = jArray[i];
            }
            return array;
        }

        /// <summary>
        /// If <paramref name="defaultIsNull"/> is false and jsonNode is null, returns an empty list.
        /// </summary>
        public static int[] ToArrayInt32(this JSONNode jsonNode, bool defaultIsNull = true)
        {
            if (jsonNode == null || jsonNode.IsNull)
            {
                return defaultIsNull ? null : new int[0];
            }

            var jArray = jsonNode.AsArray;
            int[] array = new int[jArray.Count];
            for (int i = 0; i < jArray.Count; ++i)
            {
                array[i] = jArray[i];
            }
            return array;
        }

        public static List<T> ToList<T>(this JSONNode jsonNode, bool defaultIsNull = true) where T : IDeserializable
        {
            if (jsonNode == null || jsonNode.IsNull)
            {
                return defaultIsNull ? null : new List<T>();
            }

            var jArray = jsonNode.AsArray;
            var list = new List<T>();
            for (int i = 0; i < jArray.Count; ++i)
            {
                var obj = jArray[i].Deserialize<T>();
                list.Add(obj);
            }
            return list;
        }

        /// <summary>
        /// If <paramref name="defaultIsNull"/> is false and jsonNode is null, returns an empty dictionary.
        /// </summary>
        public static Dictionary<string, string> ToDictionaryStringString(this JSONNode jsonNode, bool defaultIsNull = true)
        {
            if (jsonNode == null || jsonNode.IsNull)
            {
                return defaultIsNull ? null : new Dictionary<string, string>();
            }

            var dict = new Dictionary<string, string>();
            foreach (var item in jsonNode)
            {
                dict.Add(item.Key, item.Value);
            }
            return dict;
        }

        /// <summary>
        /// If <paramref name="defaultIsNull"/> is false and jsonNode is null, returns an empty dictionary.
        /// </summary>
        public static Dictionary<string, int> ToDictionaryStringInt(this JSONNode jsonNode, bool defaultIsNull = true)
        {
            if (jsonNode == null || jsonNode.IsNull)
            {
                return defaultIsNull ? null : new Dictionary<string, int>();
            }

            var dict = new Dictionary<string, int>();
            foreach (var item in jsonNode)
            {
                dict.Add(item.Key, item.Value.ToInt32());
            }
            return dict;
        }

        /// <summary>
        /// If <paramref name="defaultIsNull"/> is false and jsonNode is null, returns an empty dictionary.
        /// </summary>
        public static Dictionary<int, string> ToDictionaryIntString(this JSONNode jsonNode, bool defaultIsNull = true)
        {
            if (jsonNode == null || jsonNode.IsNull)
            {
                return defaultIsNull ? null : new Dictionary<int, string>();
            }

            var dict = new Dictionary<int, string>();
            foreach (var item in jsonNode)
            {
                dict.Add(int.Parse(item.Key), item.Value);
            }
            return dict;
        }

        /// <summary>
        /// If <paramref name="defaultIsNull"/> is false and jsonNode is null, returns an empty dictionary.
        /// </summary>
        public static Dictionary<string, T> ToDictionaryString<T>(this JSONNode jsonNode, bool defaultIsNull = true) where T : IDeserializable
        {
            if (jsonNode == null || jsonNode.IsNull)
            {
                return defaultIsNull ? null : new Dictionary<string, T>();
            }

            var dict = new Dictionary<string, T>();
            foreach (var item in jsonNode)
            {
                dict.Add(item.Key, item.Value.Deserialize<T>());
            }
            return dict;
        }

        /// <summary>
        /// If <paramref name="defaultIsNull"/> is false and jsonNode is null, returns an empty dictionary.
        /// </summary>
        public static Dictionary<int, T> ToDictionaryInt<T>(this JSONNode jsonNode, bool defaultIsNull = true) where T : IDeserializable
        {
            if (jsonNode == null || jsonNode.IsNull)
            {
                return defaultIsNull ? null : new Dictionary<int, T>();
            }

            var dict = new Dictionary<int, T>();
            foreach (var item in jsonNode)
            {
                dict.Add(int.Parse(item.Key), item.Value.Deserialize<T>());
            }
            return dict;
        }



        public static JSONNode Serialize(IEnumerable<string> value)
        {
            if (value == null)
            {
                return JSONNull.CreateOrGet();
            }
            var jArray = new JSONArray();
            foreach (var item in value)
            {
                jArray.Add(item);
            }
            return jArray;
        }

        public static JSONNode Serialize(IEnumerable<int> value)
        {
            if (value == null)
            {
                return JSONNull.CreateOrGet();
            }
            var jArray = new JSONArray();
            foreach (var item in value)
            {
                jArray.Add(item);
            }
            return jArray;
        }

        public static JSONNode Serialize<T>(IEnumerable<T> value) where T : ISerializable
        {
            if (value == null)
            {
                return JSONNull.CreateOrGet();
            }
            var jArray = new JSONArray();
            foreach (var item in value)
            {
                jArray.Add(item.Serialize());
            }
            return jArray;
        }

        /*public static JSONNode Serialize<T>(T[] value) where T : ISerializable
        {
            if (value == null)
            {
                return JSONNull.CreateOrGet();
            }
            var jArray = new JSONArray();
            foreach (var item in value)
            {
                jArray.Add(item.Serialize());
            }
            return jArray;
        }*/

        public static JSONNode Serialize<T>(IDictionary<string, T> value) where T : ISerializable
        {
            if (value == null)
            {
                return JSONNull.CreateOrGet();
            }
            var jObject = new JSONObject();
            foreach (var item in value)
            {
                jObject.Add(item.Key, item.Value.Serialize());
            }
            return jObject;
        }

        public static JSONNode Serialize<T>(IDictionary<int, T> value) where T : ISerializable
        {
            if (value == null)
            {
                return JSONNull.CreateOrGet();
            }
            var jObject = new JSONObject();
            foreach (var item in value)
            {
                jObject.Add(item.Key.ToString(), item.Value.Serialize());
            }
            return jObject;
        }

        public static JSONNode Serialize(IDictionary<string, int> value)
        {
            if (value == null)
            {
                return JSONNull.CreateOrGet();
            }
            var jObject = new JSONObject();
            foreach (var item in value)
            {
                jObject.Add(item.Key, item.Value);
            }
            return jObject;
        }

        public static JSONNode Serialize(IDictionary<int, string> value)
        {
            if (value == null)
            {
                return JSONNull.CreateOrGet();
            }
            var jObject = new JSONObject();
            foreach (var item in value)
            {
                jObject.Add(item.Key.ToString(), item.Value);
            }
            return jObject;
        }

        public static JSONNode Serialize(IDictionary<string, string> value)
        {
            if (value == null)
            {
                return JSONNull.CreateOrGet();
            }
            var jObject = new JSONObject();
            foreach (var item in value)
            {
                jObject.Add(item.Key, item.Value);
            }
            return jObject;
        }

        public static JSONNode Write(this JSONNode jsonNode, string name, Vector2 vector2)
        {
            jsonNode[name] = new JSONObject().WriteVector2(vector2);
            return jsonNode;
        }

        public static JSONNode Write(this JSONNode jsonNode, string name, Vector3 vector3)
        {
            jsonNode[name] = new JSONObject().WriteVector3(vector3);
            return jsonNode;
        }

        public static JSONNode Write(this JSONNode jsonNode, string name, Vector4 vector4)
        {
            jsonNode[name] = new JSONObject().WriteVector4(vector4);
            return jsonNode;
        }

        public static JSONNode Write(this JSONNode jsonNode, string name, Quaternion quaternion)
        {
            jsonNode[name] = new JSONObject().WriteQuaternion(quaternion);
            return jsonNode;
        }

        public static JSONNode Write<T>(this JSONNode jsonNode, string name, IDictionary<string, T> dictionary) where T : ISerializable
        {
            jsonNode[name] = Serialize(dictionary);
            return jsonNode;
        }

        public static JSONNode Write<T>(this JSONNode jsonNode, string name, IDictionary<int, T> dictionary) where T : ISerializable
        {
            jsonNode[name] = Serialize(dictionary);
            return jsonNode;
        }

        public static JSONNode Write(this JSONNode jsonNode, string name, IDictionary<string, string> dictionary)
        {
            jsonNode[name] = Serialize(dictionary);
            return jsonNode;
        }

        public static JSONNode Write(this JSONNode jsonNode, string name, IDictionary<string, int> dictionary)
        {
            jsonNode[name] = Serialize(dictionary);
            return jsonNode;
        }

        public static JSONNode Write(this JSONNode jsonNode, string name, IDictionary<int, string> dictionary)
        {
            jsonNode[name] = Serialize(dictionary);
            return jsonNode;
        }

        public static JSONNode Write(this JSONNode jsonNode, string name, JSONNode value)
        {
            jsonNode[name] = value ?? JSONNull.CreateOrGet();
            return jsonNode;
        }

        public static JSONNode WriteIfNotNull(this JSONNode jsonNode, string name, JSONNode value)
        {
            if (value != null)
            {
                Write(jsonNode, name, value);
            }
            return jsonNode;
        }

        public static JSONNode Write(this JSONNode jsonNode, string name, IEnumerable<string> value)
        {
            jsonNode[name] = Serialize(value);
            return jsonNode;
        }

        public static JSONNode Write<T>(this JSONNode jsonNode, string name, IEnumerable<T> value) where T : ISerializable
        {
            jsonNode[name] = Serialize(value);
            return jsonNode;
        }

        public static JSONNode WriteIfNotNull<T>(this JSONNode jsonNode, string name, IEnumerable<T> value) where T : ISerializable
        {
            if (value != null)
            {
                Write(jsonNode, name, value);
            }
            return jsonNode;
        }

        public static JSONNode WriteIfNotNullOrEmpty<T>(this JSONNode jsonNode, string name, IEnumerable<T> value) where T : ISerializable
        {
            if (value != null)
            {
                bool notEmpty = false;
                var enumerator = value.GetEnumerator();

                notEmpty = enumerator.MoveNext();

                // Make sure the enumerable is not empty before creating a new JSONArray.
                if (notEmpty)
                {
                    var jArray = new JSONArray();
                    do
                    {
                        var item = enumerator.Current;
                        jArray.Add(item.Serialize());
                    } while (enumerator.MoveNext());
                    jsonNode[name] = jArray;
                }
            }
            return jsonNode;
        }

        /// <summary>
        /// if <paramref name="serializeAsString"/> is true, will serialize as the enum's ToString value. Otherwise will serialize as an integer.
        /// </summary>
        public static JSONNode WriteIfNotNullOrEmptyEnum<TEnum>(this JSONNode jsonNode, string name, IEnumerable<TEnum> value, bool serializeAsString = false) where TEnum : struct, Enum
        {
            if (value != null)
            {
                bool notEmpty = false;
                var enumerator = value.GetEnumerator();

                notEmpty = enumerator.MoveNext();

                // Make sure the enumerable is not empty before creating a new JSONArray.
                if (notEmpty)
                {
                    var jArray = new JSONArray();
                    do
                    {
                        var item = enumerator.Current;
                        if (serializeAsString)
                        {
                            jArray.Add(item.ToString());
                        }
                        else
                        {
                            jArray.Add((int)(object)item);
                        }
                    } while (enumerator.MoveNext());
                    jsonNode[name] = jArray;
                }
            }
            return jsonNode;
        }

        public static JSONNode WriteIfNotNullOrEmpty(this JSONNode jsonNode, string name, IEnumerable<string> value)
        {
            if (value != null)
            {
                bool notEmpty = false;
                var enumerator = value.GetEnumerator();

                notEmpty = enumerator.MoveNext();

                // Make sure the enumerable is not empty before creating a new JSONArray.
                if (notEmpty)
                {
                    var jArray = new JSONArray();
                    do
                    {
                        var item = enumerator.Current;
                        jArray.Add(item);
                    } while (enumerator.MoveNext());
                    jsonNode[name] = jArray;
                }
            }
            return jsonNode;
        }

        public static JSONNode Write<T>(this JSONNode jsonNode, string name, T value) where T : ISerializable
        {
            jsonNode[name] = value.IsNull() ? JSONNull.CreateOrGet() : value.Serialize();
            return jsonNode;
        }

        public static JSONNode WriteIfNotDefault<T>(this JSONNode jsonNode, string name, T value, T defaultValue = default) where T : ISerializable
        {
            if (!EqualityComparer<T>.Default.Equals(value, defaultValue))
            {
                Write(jsonNode, name, value);
            }
            return jsonNode;
        }

        /// <summary>
        /// if <paramref name="serializeAsString"/> is true, will serialize as the enum's ToString value. Otherwise will serialize as an integer.
        /// </summary>
        public static JSONNode Write<TEnum>(this JSONNode jsonNode, string name, TEnum value, bool serializeAsString = false) where TEnum : struct, Enum
        {
            if (serializeAsString)
            {
                jsonNode[name] = value.ToString();
            }
            else
            {
                jsonNode[name] = (int)(object)value;
            }
            return jsonNode;
        }

        /// <summary>
        /// if <paramref name="serializeAsString"/> is true, will serialize as the enum's ToString value. Otherwise will serialize as an integer.
        /// </summary>
        public static JSONNode WriteIfNotDefault<TEnum>(this JSONNode jsonNode, string name, TEnum value, TEnum defaultValue = default, bool serializeAsString = false) where TEnum : struct, Enum
        {
            if (!EqualityComparer<TEnum>.Default.Equals(value, defaultValue))
            {
                Write(jsonNode, name, value, serializeAsString);
            }
            return jsonNode;
        }


        public static JSONNode Write(this JSONNode jsonNode, string name, float value)
        {
            jsonNode[name] = value;
            return jsonNode;
        }

        public static JSONNode WriteIfNotDefault(this JSONNode jsonNode, string name, float value, float defaultValue = default)
        {
#pragma warning disable RECS0018 // Comparison of floating point numbers with equality operator
            if (value != defaultValue)
#pragma warning restore RECS0018 // Comparison of floating point numbers with equality operator
            {
                Write(jsonNode, name, value);
            }
            return jsonNode;
        }

        public static JSONNode Write(this JSONNode jsonNode, string name, int value)
        {
            jsonNode[name] = value;
            return jsonNode;
        }

        public static JSONNode WriteIfNotDefault(this JSONNode jsonNode, string name, int value, int defaultValue = default)
        {
            if (value != defaultValue)
            {
                Write(jsonNode, name, value);
            }
            return jsonNode;
        }

        public static JSONNode Write(this JSONNode jsonNode, string name, bool value)
        {
            jsonNode[name] = value;
            return jsonNode;
        }

        public static JSONNode WriteIfNotDefault(this JSONNode jsonNode, string name, bool value, bool defaultValue = default)
        {
            if (value != defaultValue)
            {
                Write(jsonNode, name, value);
            }
            return jsonNode;
        }

        public static JSONNode Write(this JSONNode jsonNode, string name, string value)
        {
            jsonNode[name] = value != null ? (JSONNode)value : JSONNull.CreateOrGet();
            return jsonNode;
        }

        public static JSONNode WriteIfNotDefault(this JSONNode jsonNode, string name, string value, string defaultValue = default)
        {
            if (value != defaultValue)
            {
                Write(jsonNode, name, value);
            }
            return jsonNode;
        }
    }
}
