using System.Collections;
using System.Collections.Generic;
using UnityEngine;

using Renegadeware;

namespace HutongGames.PlayMaker.Actions.Game {
    [ActionCategory("Game")]
    public class GetSanitationScore : FsmStateAction {
        public SanitationData data;

        [UIHint(UIHint.Variable)]
        [RequiredField]
        public FsmInt output;

        public override void Reset() {
            output = null;
        }

        public override void OnEnter() {
            output.Value = data.score;

            Finish();
        }
    }
}