using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.EventSystems;

namespace Renegadeware {
    public class TCSFoodItemWidget : DragWidget {
        [Header("Display")]
        public Image icon;
        public Text nameLabel;

        public GameObject correctGO;
        public GameObject wrongGO;

        public FoodProductData data { get; private set; }

        public void Setup(FoodProductData foodData) {
            data = foodData;

            icon.sprite = foodData.iconSprite;
            nameLabel.text = M8.Localize.Get(foodData.nameRef);

            if(correctGO) correctGO.SetActive(false);
            if(wrongGO) wrongGO.SetActive(false);
        }
    }
}