using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

namespace Renegadeware {
    public class ThermometerCalibrateTargetWidget : MonoBehaviour {
        [Header("Data")]
        public ThermometerCalibrateData data;

        [Header("Display")]
        public Text degreeLabel;

        void OnEnable() {
            degreeLabel.text = ThermometerCalibrateData.GetDegreeString(data.targetDegree);
        }
    }
}