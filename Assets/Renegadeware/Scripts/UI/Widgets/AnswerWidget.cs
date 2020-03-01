using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

namespace Renegadeware {
    public class AnswerWidget : DragWidget {
        [Header("Display")]
        public Text label;
        public GameObject correctGO;
        public GameObject wrongGO;

        public void Setup(string textRef) {            
            label.text = M8.Localize.Get(textRef);

            if(correctGO) correctGO.SetActive(false);
            if(wrongGO) wrongGO.SetActive(false);
        }
    }
}