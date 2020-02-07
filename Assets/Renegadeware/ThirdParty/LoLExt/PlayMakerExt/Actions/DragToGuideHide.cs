using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace HutongGames.PlayMaker.Actions.LoL {
    [ActionCategory("Legends of Learning")]
    [Tooltip("This will hide the Drag To Guide.")]
    public class DragToGuideHide : ComponentAction<DragToGuideWidget> {
        [RequiredField]
        [CheckForComponent(typeof(DragToGuideWidget))]
        [Tooltip("The GameObject that contains DragToGuideWidget.")]
        public FsmOwnerDefault gameObject;

        public override void Reset() {
            gameObject = null;
        }

        public override void OnEnter() {
            var go = Fsm.GetOwnerDefaultTarget(gameObject);
            if(UpdateCache(go)) {
                cachedComponent.Hide();
            }

            Finish();
        }
    }
}