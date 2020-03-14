using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

namespace Renegadeware {
    public class TotalScoreWidget : MonoBehaviour {
        [Header("Data")]
        public DangerZoneData dangerZoneData;
        public SanitationData sanitationData;
        public TCSFoodData TCSFoodData;
        public ThermometerCalibrateData thermometerCalibrateData;

        [M8.Localize]
        public string scoreFormatRef;

        [Header("Display")]
        public Text scoreText;

        void OnEnable() {
            var dangerZoneMaxScore = dangerZoneData.scoreMax;
            var sanitationMaxScore = sanitationData.score;
            var tcsFoodMaxScore = TCSFoodData.foodScoreTotal;
            var thermometerCalibrateMaxScore = thermometerCalibrateData.scoreMax;

            int maxScore = dangerZoneMaxScore + sanitationMaxScore + tcsFoodMaxScore + thermometerCalibrateMaxScore;

            scoreText.text = string.Format(M8.Localize.Get(scoreFormatRef), LoLExt.LoLManager.instance.curScore, maxScore);
        }
    }
}