using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

namespace Renegadeware {
    public class ThermometerCalibrateCurDegreeWidget : MonoBehaviour {
        public ThermometerCalibrateWidget source;

        public Text degreeLabel;

        void OnEnable() {
            degreeLabel.text = ThermometerCalibrateData.GetDegreeString(source.currentDegree);
        }
    }
}