using System.Collections;
using System.Collections.Generic;
using UnityEngine;


namespace HutongGames.PlayMaker.Actions.LoL {
    [ActionCategory("Legends of Learning")]
    [Tooltip("This will show the Drag To Guide.")]
    public class DragToGuideShow : ComponentAction<DragToGuideWidget> {
        [RequiredField]
        [CheckForComponent(typeof(DragToGuideWidget))]
        [Tooltip("The GameObject that contains DragToGuideWidget.")]
        public FsmOwnerDefault gameObject;

        public bool isPause;

        public FsmGameObject startGameObject;
        public bool startIsUI;

        public FsmGameObject endGameObject;
        public bool endIsUI;

        public override void Reset() {
            gameObject = null;

            isPause = false;

            startGameObject = null;
            startIsUI = false;

            endGameObject = null;
            endIsUI = false;
        }

        public override void OnEnter() {
            var go = Fsm.GetOwnerDefaultTarget(gameObject);
            if(UpdateCache(go)) {
                var startPos = GetPosition(startGameObject.Value, startIsUI);
                var endPos = GetPosition(endGameObject.Value, endIsUI);

                cachedComponent.Show(isPause, startPos, endPos);
            }

            Finish();
        }

        Vector2 GetPosition(GameObject go, bool isUI) {
            if(!go)
                return cachedComponent.dragStart;

            var pos = go.transform.position;

            if(!isUI) {
                var cam = Camera.main;
                pos = cam.WorldToScreenPoint(pos);
            }

            return pos;
        }
    }
}