using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Renegadeware {
    /// <summary>
    /// Use this to tween a transform to given point.
    /// </summary>
    public class LookAtGotoPoints : MonoBehaviour {
        public Transform root;

        public DG.Tweening.Ease defaultEase = DG.Tweening.Ease.OutSine;
        public float defaultDelay = 0.3f;

        public float gizmoSize = 0.3f;
        public Color gizmoColor = Color.green;

        private Coroutine mRout;

        public bool isBusy { get { return mRout != null; } }

        public void Goto(Transform target, string pointName) {
            Goto(target, pointName, defaultEase, defaultDelay);
        }

        public void Goto(Transform target, string pointName, DG.Tweening.Ease ease) {
            Goto(target, pointName, ease, defaultDelay);
        }

        public void Goto(Transform target, string pointName, float delay) {
            Goto(target, pointName, defaultEase, delay);
        }

        public void Goto(Transform target, string pointName, DG.Tweening.Ease ease, float delay) {
            Stop();

            var child = GetChild(pointName);
            if(child) {
                if(delay > 0f)
                    mRout = StartCoroutine(DoGoto(target, child, ease, delay));
                else {
                    target.position = child.position;
                    target.forward = child.forward;
                }
            }
        }

        public void Stop() {
            if(mRout != null) {
                StopCoroutine(mRout);
                mRout = null;
            }
        }

        void OnDisable() {
            Stop();
        }

        IEnumerator DoGoto(Transform target, Transform dest, DG.Tweening.Ease ease, float delay) {
            var tweenFunc = DG.Tweening.Core.Easing.EaseManager.ToEaseFunction(ease);

            var startPos = target.position;
            var startLook = target.forward;

            var endPos = dest.position;
            var endLook = dest.forward;

            var curTime = 0f;
            while(curTime < delay) {
                yield return null;

                curTime += Time.deltaTime;

                var t = tweenFunc(curTime, delay, 0f, 0f);

                target.position = Vector3.Lerp(startPos, endPos, t);
                target.forward = Vector3.Lerp(startLook, endLook, t).normalized;
            }

            mRout = null;
        }

        private Transform GetChild(string childName) {
            var t = root ? root : transform;

            for(int i = 0; i < t.childCount; i++) {
                var child = t.GetChild(i);
                if(child.name == childName)
                    return child;
            }

            return null;
        }

        private void OnDrawGizmos() {
            var t = root ? root : transform;

            Gizmos.color = gizmoColor;

            for(int i = 0; i < t.childCount; i++) {
                var child = t.GetChild(i);
                Gizmos.DrawSphere(child.position, gizmoSize);
                M8.Gizmo.Arrow(child.position, child.forward);
            }
        }
    }
}