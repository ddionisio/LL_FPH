using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Renegadeware {
    [CreateAssetMenu(fileName = "TCSFoodData", menuName = "Game/TCS Food Data")]
    public class TCSFoodData : ScriptableObject {
        public FoodProductData[] TCSFoods;
        public FoodProductData[] nonTCSFoods;

        public int foodCount; //count of food to place in controller
        public int foodScorePerItem;

        public int foodScoreTotal { get { return foodCount * foodScorePerItem; } }

        public FoodProductData[] GenerateItems() {
            var ret = new FoodProductData[foodCount];

            int hCount = foodCount / 2;

            //grab random items from TCS Foods
            M8.ArrayUtil.Shuffle(TCSFoods);

            for(int i = 0; i < hCount; i++)
                ret[i] = TCSFoods[i];

            M8.ArrayUtil.Shuffle(nonTCSFoods);

            for(int i = 0; i < hCount; i++)
                ret[i + hCount] = nonTCSFoods[i];

            M8.ArrayUtil.Shuffle(ret, 1, ret.Length - 1);

            return ret;
        }
    }
}