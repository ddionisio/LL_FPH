using LoL;
using LoL.Data;
using LoL.SimpleJSON;

namespace LoL.Examples.Cooking
{
    // Generated from State Attribute. Do not manual edit.
    public partial class FoodData : IStateSaveable
    {
        void IDeserializable.Deserialize(JSONNode jsonNode)
        {
             name = jsonNode[nameof(name)];
             level = jsonNode[nameof(level)];
             cooking_temp = jsonNode[nameof(cooking_temp)];
             image_key = jsonNode[nameof(image_key)];
             available = jsonNode[nameof(available)];
        }

        void IResetable.Reset()
        {
             name = "null";
             level = 0;
             cooking_temp = 0;
             image_key = "null";
             available = true;
        }

        JSONNode ISerializable.Serialize()
        {
            return CreateJSONObject();
        }

        JSONNode CreateJSONObject()
        {
            return new SimpleJSON.JSONObject()
                .Write(nameof(name), name)
                .Write(nameof(level), level)
                .Write(nameof(cooking_temp), cooking_temp)
                .Write(nameof(image_key), image_key)
                .Write(nameof(available), available)
            ;
        }

        public override string ToString()
        {
            return "FoodData: " + CreateJSONObject().ToString();
        }

        public static void Load(int version, System.Action<FoodData> onLoad)
        {
            State.Load("lol-1584400325831-fooddata", version, onLoad);
        }

        public static void Save(int version, FoodData data)
        {
            State.Save("lol-1584400325831-fooddata", version, data);
        }

#if UNITY_EDITOR
        public static void Reset(int version)
        {
            State.Reset("lol-1584400325831-fooddata", version);
        }
#endif
    }
}
