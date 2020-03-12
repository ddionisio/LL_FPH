using System.Collections;
using System.Collections.Generic;
using UnityEngine;

using Renegadeware;

namespace HutongGames.PlayMaker.Actions.Game {
    [ActionCategory("Game")]
    public class ThawTimerCheckFinished : ComponentAction<ThawTimerWidget> {
        [RequiredField]
        [CheckForComponent(typeof(ThawTimerWidget))]
        public FsmOwnerDefault gameObject;

        public FsmEvent isTrue;
        public FsmEvent isFalse;

        [UIHint(UIHint.Variable)]
        public FsmBool storeResult;

        public FsmBool everyFrame;

        public override void Reset() {
            gameObject = null;
            isTrue = null;
            isFalse = null;
            storeResult = null;
            everyFrame = false;
        }

        public override void OnEnter() {
            DoCheck();

            if(!everyFrame.Value)
                Finish();
        }

        public override void OnUpdate() {
            DoCheck();
        }

        void DoCheck() {
            var go = Fsm.GetOwnerDefaultTarget(gameObject);
            if(!UpdateCache(go))
                return;

            var isFinished = cachedComponent.isFinished;

            if(!storeResult.IsNone)
                storeResult.Value = isFinished;

            Fsm.Event(isFinished ? isTrue : isFalse);
        }
    }
}