using System.Collections;
using System.Collections.Generic;
using UnityEngine;

using Renegadeware;

namespace HutongGames.PlayMaker.Actions.Game {
    [ActionCategory("Game")]
    public class LookAtGoto : ComponentAction<LookAtGotoPoints> {
        [RequiredField]
        [CheckForComponent(typeof(LookAtGotoPoints))]
        [Tooltip("The LookAtGotoPoints source.")]
        public FsmOwnerDefault gameObject;

        public FsmGameObject target;

        public FsmString pointName;

        [Tooltip("Set to Unset to use default.")]
        public DG.Tweening.Ease ease;

        [Tooltip("Set to none to use default.")]
        public FsmFloat delay;

        public FsmBool isWait;

        public override void Reset() {
            pointName = null;
            ease = DG.Tweening.Ease.Unset;
            delay = new FsmFloat(); delay.UseVariable = true;
            isWait = true;
        }

        public override void OnEnter() {
            if(string.IsNullOrEmpty(pointName.Value)) {
                Finish();
                return;
            }

            var targetTrans = target.Value ? target.Value.transform : null;
            if(!targetTrans) {
                Finish();
                return;
            }

            var go = Fsm.GetOwnerDefaultTarget(gameObject);
            if(!UpdateCache(go)) {
                Finish();
                return;
            }

            if(ease != DG.Tweening.Ease.Unset && !delay.IsNone)
                cachedComponent.Goto(targetTrans, pointName.Value, ease, delay.Value);
            else if(ease != DG.Tweening.Ease.Unset)
                cachedComponent.Goto(targetTrans, pointName.Value, ease);
            else if(!delay.IsNone)
                cachedComponent.Goto(targetTrans, pointName.Value, delay.Value);
            else
                cachedComponent.Goto(targetTrans, pointName.Value);

            if(!isWait.Value)
                Finish();
        }

        public override void OnUpdate() {
            if(!cachedComponent.isBusy)
                Finish();
        }
    }
}