using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.EventSystems;

namespace LoLExt {
    [System.Serializable]
    public struct ModalChoiceItemInfo {
        public Sprite iconRef;
        public string textRef;
    }

    public class ModalChoiceItem : MonoBehaviour, IPointerClickHandler {
        [Header("Display")]
        public Image iconImage;
        public Text label;
        public Selectable interaction;
        public GameObject selectedGO;

        public event System.Action<int> clickCallback;

        public int index { get; private set; }        
        public bool interactable { 
            get { return interaction ? interaction.interactable : false; } 
            set {
                if(interaction)
                    interaction.interactable = value;
            }
        }

        public bool selected {
            get { return selectedGO ? selectedGO.activeSelf : false; }
            set {
                if(selectedGO)
                    selectedGO.SetActive(value);
            }
        }

        public string textRef { get; private set; }

        public void Setup(int index, ModalChoiceItemInfo info) {
            this.index = index;

            if(iconImage)
                iconImage.sprite = info.iconRef;

            textRef = info.textRef;

            if(label)
                label.text = M8.Localize.Get(textRef);

            selected = false;
        }
        
        void IPointerClickHandler.OnPointerClick(PointerEventData eventData) {
            if(!interactable)
                return;

            clickCallback?.Invoke(index);
        }
    }
}