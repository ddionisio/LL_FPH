using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

namespace Renegadeware {
    public class ThermometerCalibrateWidget : MonoBehaviour {
        [System.Serializable]
        public class ThermometerPointer {
            public Transform target;
            public Vector3 rotateStart;
            public Vector3 rotateEnd;

            public void SetRotate(float t) {
                target.localEulerAngles = Vector3.Lerp(rotateStart, rotateEnd, t);
            }
        }

        [Header("Data")]
        public ThermometerCalibrateData data;

        [Header("Thermometer")]
        public M8.RangeFloat thermometerDegreeRange;        
        public float thermometerAdjustIncrement;
        public float thermometerMoveDelay;

        public Text thermometerDegreeLabel;

        public ThermometerPointer[] thermometerPointers;

        [Header("Interact")]
        public M8.Animator.Animate interactAnimator;
        [M8.Animator.TakeSelector(animatorField = "interactAnimator")]
        public string interactTakeEnter;
        [M8.Animator.TakeSelector(animatorField = "interactAnimator")]
        public string interactTakeExit;

        [M8.SoundPlaylist]
        public string interactAdjustClickSound;
        [M8.SoundPlaylist]
        public string interactAdjustErrorSound;

        [Header("Signals")]
        public M8.SignalBoolean signalInvokeConfirm; //returns true if adjustment correct

        public M8.SignalFloat signalListenSetThermometerDegree;
        public M8.SignalFloat signalListenMoveThermometerDegree;

        public M8.SignalBoolean signalListenInteractSetActive;

        public M8.SignalBoolean signalListenSetBroken;

        public float currentDegree { get { return mMoveToDegree; } }

        private bool mIsBroken = false;
        private float mCurDegree;
        private float mMoveToDegree;
        private float mDegreeVel = 0f;

        public void AdjustLeft() {
            if(mIsBroken)
                M8.SoundPlaylist.instance.Play(interactAdjustErrorSound, false);
            else {
                M8.SoundPlaylist.instance.Play(interactAdjustClickSound, false);

                ApplyDegree(mMoveToDegree - thermometerAdjustIncrement, false);
            }
        }

        public void AdjustRight() {
            if(mIsBroken)
                M8.SoundPlaylist.instance.Play(interactAdjustErrorSound, false);
            else {
                M8.SoundPlaylist.instance.Play(interactAdjustClickSound, false);

                ApplyDegree(mMoveToDegree + thermometerAdjustIncrement, false);
            }
        }

        public void Confirm() {
            bool isMatch = mMoveToDegree == data.targetDegree;

            signalInvokeConfirm.Invoke(isMatch);
        }

        void OnEnable() {
            mDegreeVel = 0f;
        }

        void OnDestroy() {
            signalListenSetThermometerDegree.callback -= OnSetThermometerDegree;
            signalListenMoveThermometerDegree.callback -= OnMoveThermometerDegree;
            signalListenInteractSetActive.callback -= OnSetInteractActive;
            signalListenSetBroken.callback -= OnSetBroken;
        }

        void Awake() {
            signalListenSetThermometerDegree.callback += OnSetThermometerDegree;
            signalListenMoveThermometerDegree.callback += OnMoveThermometerDegree;
            signalListenInteractSetActive.callback += OnSetInteractActive;
            signalListenSetBroken.callback += OnSetBroken;
        }

        void Update() {
            if(mCurDegree != mMoveToDegree) {
                mCurDegree = Mathf.SmoothDamp(mCurDegree, mMoveToDegree, ref mDegreeVel, thermometerMoveDelay);

                UpdateThermometerDisplay();
            }
        }

        void OnSetThermometerDegree(float degree) {
            ApplyDegree(degree, true);
        }

        void OnMoveThermometerDegree(float degree) {
            ApplyDegree(degree, false);
        }

        void OnSetInteractActive(bool active) {
            if(active)
                interactAnimator.Play(interactTakeEnter);
            else
                interactAnimator.Play(interactTakeExit);
        }

        void OnSetBroken(bool broken) {
            mIsBroken = broken;
        }

        private void ApplyDegree(float degree, bool instant) {
            degree = thermometerDegreeRange.Clamp(degree);

            mMoveToDegree = degree;
            if(instant) {
                mCurDegree = mMoveToDegree;
                UpdateThermometerDisplay();
            }

            thermometerDegreeLabel.text = ThermometerCalibrateData.GetDegreeString(currentDegree);
        }

        private void UpdateThermometerDisplay() {
            var t = thermometerDegreeRange.GetT(mCurDegree);

            for(int i = 0; i < thermometerPointers.Length; i++) {
                thermometerPointers[i].SetRotate(t);
            }
        }
    }
}