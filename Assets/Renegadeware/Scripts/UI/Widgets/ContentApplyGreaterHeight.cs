using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Renegadeware {
    public class ContentApplyGreaterHeight : MonoBehaviour {
        public float delay;

        void OnEnable() {
            StartCoroutine(DoApply());
        }

        IEnumerator DoApply() {
            var rTrans = transform as RectTransform;
            if(!rTrans)
                yield break;

            yield return new WaitForSeconds(delay);

            var s = rTrans.sizeDelta;

            float height = s.y;

            for(int i = 0; i < rTrans.childCount; i++) {
                var rChild = rTrans.GetChild(i) as RectTransform;
                if(!rChild)
                    continue;

                if(rChild.sizeDelta.y > height)
                    height = rChild.sizeDelta.y;
            }

            s.y = height;

            rTrans.sizeDelta = s;
        }
    }
}