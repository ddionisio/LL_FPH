using LoL;
using LoL.Data;
using LoL.SimpleJSON;

namespace LoL.Examples.Cooking
{
    // Generated from State Attribute. Do not manual edit.
    public partial class CookingData : IStateSaveable
    {
        void IDeserializable.Deserialize(JSONNode jsonNode)
        {
             coins = jsonNode[nameof(coins)];
             cost_of_pan = jsonNode[nameof(cost_of_pan)];
             num_of_pans = jsonNode[nameof(num_of_pans)];
             food_in_pan = jsonNode[nameof(food_in_pan)].ToDictionaryIntString(false);
             food = jsonNode[nameof(food)].ToList<LoL.Examples.Cooking.FoodData>(true);
        }

        void IResetable.Reset()
        {
             coins = 200;
             cost_of_pan = 70;
             num_of_pans = 0;
             food_in_pan = new System.Collections.Generic.Dictionary<int, string>();
             food = null;
        }

        JSONNode ISerializable.Serialize()
        {
            return CreateJSONObject();
        }

        JSONNode CreateJSONObject()
        {
            return new SimpleJSON.JSONObject()
                .Write(nameof(coins), coins)
                .Write(nameof(cost_of_pan), cost_of_pan)
                .Write(nameof(num_of_pans), num_of_pans)
                .Write(nameof(food_in_pan), food_in_pan)
                .Write(nameof(food), food)
            ;
        }

        public override string ToString()
        {
            return "CookingData: " + CreateJSONObject().ToString();
        }

        public static void Load(int version, System.Action<CookingData> onLoad)
        {
            State.Load("lol-1584400325831-cookingdata", version, onLoad);
        }

        public static void Save(int version, CookingData data)
        {
            State.Save("lol-1584400325831-cookingdata", version, data);
        }

#if UNITY_EDITOR
        public static void Reset(int version)
        {
            State.Reset("lol-1584400325831-cookingdata", version);
        }
#endif
    }
}
