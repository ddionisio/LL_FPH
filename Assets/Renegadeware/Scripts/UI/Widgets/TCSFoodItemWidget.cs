﻿using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.EventSystems;

namespace Renegadeware {
    public class TCSFoodItemWidget : MonoBehaviour, IPointerEnterHandler, IPointerExitHandler, IBeginDragHandler, IDragHandler, IEndDragHandler {
        [Header("Display")]
        public Image icon;
        public Text nameLabel;

        public GameObject highlightGO;
        public GameObject correctGO;
        public GameObject wrongGO;

        [Header("Drag Info")]
        [M8.TagSelector]
        public string dragTagDropArea; //areas we can drop to
        public Transform dragArea;
        public Transform dragRoot; //use to drag around
        public DG.Tweening.Ease dragDropEase = DG.Tweening.Ease.OutSine;
        public float dragDropSpeed; //speed moving from current position to new position after dropping

        public bool inputEnabled {
            get { return mInputEnabled && mDropMoveRout == null; }
            set {
                mInputEnabled = value;
                UpdateHighlight();
            }
        }

        public FoodProductData data { get; private set; }

        public bool isDragging { get; private set; }

        public event System.Action<TCSFoodItemWidget> dragEndCallback;

        private bool mInputEnabled;
        private bool mIsPointerEnter;

        private DropAreaWidget mCurDropAreaWidget;

        private Coroutine mDropMoveRout;

        private Vector2 mDragRootDefaultLPos;

        public void Setup(FoodProductData foodData) {
            data = foodData;

            isDragging = false;

            mInputEnabled = false;
            mIsPointerEnter = false;

            icon.sprite = foodData.iconSprite;
            nameLabel.text = M8.Localize.Get(foodData.nameRef);

            if(correctGO) correctGO.SetActive(false);
            if(wrongGO) wrongGO.SetActive(false);

            UpdateHighlight();
        }

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
                    if(dropArea && !dropArea.isFull)
                        transform.SetParent(pointerGO.transform, false);
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