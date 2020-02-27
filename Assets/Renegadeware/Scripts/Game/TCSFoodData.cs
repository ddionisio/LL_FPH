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
    }
}