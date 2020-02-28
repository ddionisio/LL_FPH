using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Renegadeware {
    /// <summary>
    /// Simple script to allow draggable items to highlight a droppable area, and cap on items.
    /// </summary>
    public class DropAreaWidget : MonoBehaviour {
        [SerializeField]
        GameObject _highlightGO = null;
        [SerializeField]
        int _capacity = 0;

        public bool isHighlight {
            get { return _highlightGO ? _highlightGO.activeSelf : false; }
            set {
                if(_highlightGO)
                    _highlightGO.SetActive(value);
            }
        }

        public bool isFull {
            get { return transform.childCount >= _capacity; }
        }

        void OnEnable() {
            isHighlight = false;
        }
    }
}