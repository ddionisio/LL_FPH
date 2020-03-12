using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

namespace Renegadeware {
    public class TextResizePreferredHeight : MonoBehaviour {
        public float delay;

        private Text mText;

        void OnEnable() {
            if(!mText)
                mText = GetComponent<Text>();

            if(mText)
                StartCoroutine(DoResize());
        }

        IEnumerator DoResize() {
            yield return new WaitForSeconds(delay);

            var s = mText.rectTransform.sizeDelta;
            s.y = mText.preferredHeight;

            mText.rectTransform.sizeDelta = s;
        }
    }
}