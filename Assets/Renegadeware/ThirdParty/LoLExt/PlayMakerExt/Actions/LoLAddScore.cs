using System.Collections;
using System.Collections.Generic;
using UnityEngine;

using LoLExt;

namespace HutongGames.PlayMaker.Actions.LoL {
    [ActionCategory("Legends of Learning")]
    [Tooltip("Add to current score. Make sure to call either: (ApplyCurrentProgress, IncrementProgress, SetProgress) to push it to LoL service.")]
    public class LoLAddScore : FsmStateAction {
        public FsmInt score;

        public override void Reset() {
            score = null;
        }

        public override void OnEnter() {
            if(LoLManager.isInstantiated && !score.IsNone)
                LoLManager.instance.curScore += score.Value;

            Finish();
        }
    }
}