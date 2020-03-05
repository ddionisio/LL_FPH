using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

namespace Renegadeware {
    public class ThermometerCalibrateScoreWidget : MonoBehaviour {
        [Header("Data")]
        public ThermometerCalibrateData data;

        [Header("Display")]
        public Text scoreText;

        void OnEnable() {
            scoreText.text = data.scorePerAdjust.ToString();
        }
    }
}