using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Renegadeware {
    public class ExpressionGOActive : MonoBehaviour {

        public Transform root;

        public void Apply(ExpressionType expressionType) {
            var str = expressionType.ToString();

            var t = root ? root : transform;
            for(int i = 0; i < t.childCount; i++) {
                var child = t.GetChild(i);
                child.gameObject.SetActive(child.name == str);
            }
        }
    }
}