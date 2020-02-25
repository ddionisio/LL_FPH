using System.Collections;
using System.Collections.Generic;
using UnityEngine;

using Renegadeware;

namespace HutongGames.PlayMaker.Actions.Game {
    [ActionCategory("Game")]
    public class ExpressionGOActiveSet : ComponentAction<ExpressionGOActive> {
        [RequiredField]
        [CheckForComponent(typeof(ExpressionGOActive))]
        [Tooltip("The ExpressionGOActive source.")]
        public FsmOwnerDefault gameObject;

        public ExpressionType expression;

        public override void Reset() {
            gameObject = null;
            expression = ExpressionType.None;
        }

        public override void OnEnter() {
            var go = Fsm.GetOwnerDefaultTarget(gameObject);
            if(UpdateCache(go)) {
                cachedComponent.Apply(expression);
            }

            Finish();
        }
    }
}