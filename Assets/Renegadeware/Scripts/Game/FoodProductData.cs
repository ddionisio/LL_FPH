using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Renegadeware {
    [CreateAssetMenu(fileName = "foodProduct", menuName = "Game/Food Product")]
    public class FoodProductData : ScriptableObject {
        public Sprite iconSprite;

        [M8.Localize]
        public string nameRef;

        public bool isHazardous;
    }
}