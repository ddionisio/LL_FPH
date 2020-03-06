using System.Collections;
using System.Collections.Generic;
using UnityEngine;

using Renegadeware;

namespace HutongGames.PlayMaker.Actions.Game {
    [ActionCategory("Game")]
    public class GetThermometerCalibrateScore : FsmStateAction {
        public ThermometerCalibrateData data;

        [UIHint(UIHint.Variable)]
        [RequiredField]
        public FsmInt output;

        public bool isBroken;

        public override void Reset() {
            output = null;
        }

        public override void OnEnter() {
            output.Value = isBroken ? data.scoreBroken : data.scorePerAdjust;

            Finish();
        }
    }
}