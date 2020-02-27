using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Renegadeware {
    /// <summary>
    /// Simple script to allow draggable items to highlight a droppable area.
    /// </summary>
    public class DropAreaWidget : MonoBehaviour {
        [SerializeField]
        GameObject _highlightGO = null;

        public bool isHighlight {
            get { return _highlightGO ? _highlightGO.activeSelf : false; }
            set {
                if(_highlightGO)
                    _highlightGO.SetActive(value);
            }
        }

        void OnDisable() {
            isHighlight = false;
        }
    }
}