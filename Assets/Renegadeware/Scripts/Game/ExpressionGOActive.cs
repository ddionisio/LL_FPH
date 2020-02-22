using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ExpressionGOActive : MonoBehaviour {
    
    public void Apply(ExpressionType expressionType) {
        var str = expressionType.ToString();

        var t = transform;
        for(int i = 0; i < t.childCount; i++) {
            var child = t.GetChild(i);
            child.gameObject.SetActive(child.name == str);
        }
    }
}
