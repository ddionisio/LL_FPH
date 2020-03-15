using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.EventSystems;

namespace Renegadeware {
    public class DragWidget : MonoBehaviour, IPointerEnterHandler, IPointerExitHandler, IBeginDragHandler, IDragHandler, IEndDragHandler {
        [Header("Drag Info")]
        public GameObject highlightGO;
        [M8.TagSelector]
        public string dragTagDropArea; //areas we can drop to
        public Transform dragArea; //place to put dragRoot while dragging
        public Transform dragRoot; //use to drag around
        public DG.Tweening.Ease dragDropEase = DG.Tweening.Ease.OutSine;
        public float dragDropSpeed = 1000f; //speed moving from current position to new position after dropping

        [Header("Sound")]
        [M8.SoundPlaylist]
        public string sfxDragBegin;
        [M8.SoundPlaylist]
        public string sfxDragDrop;

        public bool inputEnabled {
            get { return mInputEnabled && mDropMoveRout == null; }
            set {
                mInputEnabled = value;
                UpdateHighlight();
            }
        }

        public bool isDragging { get; private set; }

        public event System.Action<DragWidget> dragEndCallback;

        private bool mInputEnabled;
        private bool mIsPointerEnter;

        private DropAreaWidget mCurDropAreaWidget;

        private Coroutine mDropMoveRout;

        private Vector2 mDragRootDefaultLPos;

        public void DropTo(Transform root, int siblingIndex) {
            dragRoot.SetParent(dragArea, true);
            dragRoot.SetSiblingIndex(siblingIndex);

            transform.SetParent(root, false);

            DropMoveStart();
        }

        void OnApplicationFocus(bool focus) {
            if(!focus) {
                EndDrag();
            }
        }

        void OnEnable() {
            isDragging = false;
            mIsPointerEnter = false;
            UpdateHighlight();
        }
                
        void OnDisable() {
            DropMoveStop();
            ResetDrop();

            if(mCurDropAreaWidget) {
                mCurDropAreaWidget.isHighlight = false;
                mCurDropAreaWidget = null;
            }
        }

        void Awake() {
            mDragRootDefaultLPos = dragRoot.localPosition;
        }

        void IPointerEnterHandler.OnPointerEnter(PointerEventData eventData) {
            if(!mIsPointerEnter) {
                mIsPointerEnter = true;
                UpdateHighlight();
            }
        }

        void IPointerExitHandler.OnPointerExit(PointerEventData eventData) {
            if(mIsPointerEnter) {
                mIsPointerEnter = false;
                UpdateHighlight();
            }
        }

        void IBeginDragHandler.OnBeginDrag(PointerEventData eventData) {
            if(!inputEnabled)
                return;

            if(!string.IsNullOrEmpty(sfxDragBegin))
                M8.SoundPlaylist.instance.Play(sfxDragBegin, false);

            isDragging = true;

            dragRoot.SetParent(dragArea, true);

            UpdateHighlight();
        }

        void IDragHandler.OnDrag(PointerEventData eventData) {
            if(!isDragging)
                return;

            dragRoot.position = eventData.position;

            //check if we are in a drop area
            var pointerGO = eventData.pointerCurrentRaycast.isValid ? eventData.pointerCurrentRaycast.gameObject : null;
            if(pointerGO) {
                if(!mCurDropAreaWidget || mCurDropAreaWidget.gameObject != pointerGO) {
                    var dropArea = GetDropAreaWidget(pointerGO);
                    if(dropArea) {
                        if(mCurDropAreaWidget)
                            mCurDropAreaWidget.isHighlight = false;

                        if(!dropArea.isFull) {
                            mCurDropAreaWidget = dropArea;
                            mCurDropAreaWidget.isHighlight = true;
                        }
                        else
                            mCurDropAreaWidget = null;
                    }
                    else if(mCurDropAreaWidget) {
                        mCurDropAreaWidget.isHighlight = false;
                        mCurDropAreaWidget = null;
                    }
                }
            }
            else if(mCurDropAreaWidget) {
                mCurDropAreaWidget.isHighlight = false;
                mCurDropAreaWidget = null;
            }
        }

        void IEndDragHandler.OnEndDrag(PointerEventData eventData) {
            if(!isDragging)
                return;

            var pointerGO = eventData.pointerCurrentRaycast.isValid ? eventData.pointerCurrentRaycast.gameObject : null;
            if(pointerGO) {
                //swap with another food item?
                var otherFoodItemWidget = pointerGO.GetComponent<TCSFoodItemWidget>();
                if(otherFoodItemWidget) {
                    if(otherFoodItemWidget != this) {
                        if(!string.IsNullOrEmpty(sfxDragDrop))
                            M8.SoundPlaylist.instance.Play(sfxDragDrop, false);

                        var siblingIndex = transform.GetSiblingIndex();
                        var otherSiblingIndex = otherFoodItemWidget.transform.GetSiblingIndex();

                        var parent = transform.parent;
                        var otherParent = otherFoodItemWidget.transform.parent;

                        otherFoodItemWidget.DropTo(parent, siblingIndex);

                        transform.SetParent(otherParent, false);
                        transform.SetSiblingIndex(otherSiblingIndex);
                    }
                }
                else if(transform.parent != pointerGO.transform) { //drop area?
                    var dropArea = GetDropAreaWidget(pointerGO);
                    if(dropArea && !dropArea.isFull) {
                        if(!string.IsNullOrEmpty(sfxDragDrop))
                            M8.SoundPlaylist.instance.Play(sfxDragDrop, false);

                        transform.SetParent(pointerGO.transform, false);
                    }
                }
            }

            EndDrag();

            dragEndCallback?.Invoke(this);
        }

        IEnumerator DoDropMove() {
            UpdateHighlight();

            yield return null;

            var easeFunc = DG.Tweening.Core.Easing.EaseManager.ToEaseFunction(dragDropEase);

            Vector2 startPos = dragRoot.position;
            Vector2 endPos = (Vector2)transform.position + mDragRootDefaultLPos;

            var len = (endPos - startPos).magnitude;

            var delay = len / dragDropSpeed;

            var curTime = 0f;
            while(curTime < delay) {
                yield return null;

                curTime += Time.deltaTime;

                var t = easeFunc(curTime, delay, 0f, 0f);

                dragRoot.position = Vector2.Lerp(startPos, endPos, t);
            }

            ResetDrop();

            mDropMoveRout = null;

            UpdateHighlight();
        }

        private DropAreaWidget GetDropAreaWidget(GameObject go) {
            if(go.CompareTag(dragTagDropArea)) {
                return go.GetComponent<DropAreaWidget>();
            }

            return null;
        }

        private void EndDrag() {
            if(isDragging) {
                isDragging = false;

                if(mCurDropAreaWidget) {
                    mCurDropAreaWidget.isHighlight = false;
                    mCurDropAreaWidget = null;
                }

                DropMoveStart();
            }
        }

        private void UpdateHighlight() {
            if(highlightGO)
                highlightGO.SetActive(inputEnabled && !isDragging && mIsPointerEnter);
        }

        private void DropMoveStart() {
            DropMoveStop();

            mDropMoveRout = StartCoroutine(DoDropMove());
        }

        private void DropMoveStop() {
            if(mDropMoveRout != null) {
                StopCoroutine(mDropMoveRout);
                mDropMoveRout = null;
            }
        }

        private void ResetDrop() {
            dragRoot.SetParent(transform, false);
            dragRoot.localPosition = mDragRootDefaultLPos;
        }
    }
}