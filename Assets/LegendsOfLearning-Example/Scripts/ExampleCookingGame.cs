using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace LoL.Examples.Cooking
{
    [System.Serializable, State(2)]
    public partial class CookingData
    {
        public int coins = 200;
        // use _ and not camelCasing for easy porting to server if needed.
        public int cost_of_pan = 70;
        public int num_of_pans;
        // You must create the new dict.
        public Dictionary<int, string> food_in_pan = new Dictionary<int, string>();
        // Unity serialization will create the list.
        public List<FoodData> food;
    }

    [System.Serializable, State]
    public partial class FoodData
    {
        public string name;
        public int level;
        public float cooking_temp;
        public string image_key;
        public bool available = true;
    }

    public class ExampleCookingGame : MonoBehaviour
    {
        [SerializeField] int version;

        #region Mock Game Fields
        [SerializeField] Button panPrefab, foodPrefab, purchasePanButton, pantryButton;
        [SerializeField] TextMeshProUGUI purchasePanText, coinText, feedbackText;
        [SerializeField] Transform panHolder;
        [SerializeField] Sprite steak, onion, broccoli;
        [SerializeField] CookingData cookingData;

        Dictionary<string, Button> _food = new Dictionary<string, Button>();
        Button _selectedFood;
        bool _init;
        WaitForSeconds _feedbackTimer = new WaitForSeconds(2);
        Coroutine _feedbackMethod;

        #endregion Mock Game Fields

        void Start()
        {
            purchasePanButton.onClick.AddListener(AddPan);
            pantryButton.onClick.AddListener(AddFoodToPantry);

            //CookingData.Load(version, OnLoad);
        }

        void Save()
        {
            //CookingData.Save(version, cookingData);

            if (_feedbackMethod != null)
                StopCoroutine(_feedbackMethod);
            _feedbackMethod = StartCoroutine(_Feedback("...Saving complete"));
        }

        // Editor ref'd
        public void ReturnTo()
        {
            State.LoadReturnScene();
        }

#if UNITY_EDITOR
        // Editor ref'd
        public void ResetTestingData()
        {
            //CookingData.Reset(version);
        }
#endif

        void OnLoad(CookingData loadedCookingData)
        {
            if (loadedCookingData != null)
                cookingData = loadedCookingData;

            for (int i = 0; i < cookingData.num_of_pans; ++i)
            {
                CreatePan();
            }

            foreach (var food in cookingData.food)
            {
                CreateFood(food);
            }

            foreach (var kvp in cookingData.food_in_pan)
            {
                _selectedFood = _food[kvp.Value];
                AssignFood(panHolder.GetChild(kvp.Key));
            }

            coinText.text = cookingData.coins.ToString();
            purchasePanText.text = $"Purchase Pan  <color=#F9DD3B>{cookingData.cost_of_pan}</color>";

            _init = true;
        }

        #region Mock Game Methods
        void AddPan()
        {
            if (_init)
            {
                cookingData.num_of_pans++;
                cookingData.coins -= cookingData.cost_of_pan;
                Save();
            }

            CreatePan();
            coinText.text = cookingData.coins.ToString();
        }

        void AddFoodToPantry()
        {
            if (_selectedFood == null)
                return;

            if (_init)
            {
                // Reset the food pan link.
                cookingData.food_in_pan.Remove(_selectedFood.transform.parent.GetSiblingIndex());
                Save();
            }

            _selectedFood.transform.SetParent(pantryButton.transform, false);
        }

        void CreatePan()
        {
            var pan = Instantiate(panPrefab, panHolder);
            pan.onClick.AddListener(() => AssignFood(pan.transform));
            pan.gameObject.SetActive(true);

            purchasePanButton.interactable = cookingData.coins >= cookingData.cost_of_pan && cookingData.num_of_pans < 4;
        }

        void AssignFood(Transform pan)
        {
            if (_selectedFood == null || pan.childCount > 0)
                return;

            _selectedFood.transform.SetParent(pan, false);
            // Account for offset.
            ((RectTransform)_selectedFood.transform).anchoredPosition = new Vector2(100, -160);

            if (_init)
            {
                cookingData.food_in_pan[pan.GetSiblingIndex()] = _selectedFood.name;
                Save();
            }
        }

        void CreateFood(FoodData foodData)
        {
            var food = Instantiate(foodPrefab, foodPrefab.transform.parent);
            food.name = foodData.name;
            food.interactable = foodData.available;
            food.onClick.AddListener(() => _selectedFood = food);
            food.GetComponent<Image>().sprite = GetFoodSprite(foodData.image_key);
            _food[foodData.name] = food;
            food.gameObject.SetActive(true);
        }

        // This would actually use addressables, just doing this as a quick, baked in example.
        Sprite GetFoodSprite(string image_key)
        {
            switch (image_key)
            {
                case "steak":
                    return steak;
                case "onion":
                    return onion;
                default:
                    return broccoli;
            }
        }

        IEnumerator _Feedback(string text)
        {
            feedbackText.text = text;
            yield return _feedbackTimer;
            feedbackText.text = string.Empty;
            _feedbackMethod = null;
        }
        #endregion Mock Game Methods
    }
}
