using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Renegadeware {
    [CreateAssetMenu(fileName = "ThermometerCalibrateData", menuName = "Game/Thermometer Calibrate Data")]
    public class ThermometerCalibrateData : ScriptableObject {
        public int scorePerAdjust;
        public int scoreBroken;
        public int adjustCount;

        public float targetDegree;

        public int scoreMax { get { return (scorePerAdjust * adjustCount) + scoreBroken; } }

        public static string GetDegreeString(float degree) {
            return Mathf.FloorToInt(degree).ToString() + "° F";
        }
    }
}