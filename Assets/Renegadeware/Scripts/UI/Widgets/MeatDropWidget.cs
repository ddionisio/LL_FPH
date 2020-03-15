using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.EventSystems;

namespace Renegadeware {
    public class MeatDropWidget : MonoBehaviour, IBeginDragHandler, IDragHandler, IEndDragHandler {
        public Transform dragRoot;
        public Transform dragToArea;

        [M8.TagSelector]
        public string tagDragGuide;

        public M8.Signal signalInvokeDragEnd;

        [Header("Sound")]
        [M8.SoundPlaylist]
        public string sfxDragBegin;
        [M8.SoundPlaylist]
        public string sfxDragDrop;

        private bool mIsDragging;
        private LoLExt.DragToGuideWidget mDragGuide;

        void OnApplicationFocus(bool focus) {
            if(!focus) {
                EndDrag();
            }
        }

        void OnEnable() {
            dragRoot.gameObject.SetActive(false);

            mIsDragging = false;

            if(!mDragGuide && !string.IsNullOrEmpty(tagDragGuide)) {
                var go = GameObject.FindGameObjectWithTag(tagDragGuide);
                if(go)
                    mDragGuide = go.GetComponent<LoLExt.DragToGuideWidget>();
            }

            if(mDragGuide)
                mDragGuide.Show(false, transform.position, dragToArea.position);
        }

        void OnDisable() {
            if(mDragGuide)
                mDragGuide.Hide();
        }

        void IBeginDragHandler.OnBeginDrag(PointerEventData eventData) {
            if(!string.IsNullOrEmpty(sfxDragBegin))
                M8.SoundPlaylist.instance.Play(sfxDragBegin, false);

            dragRoot.gameObject.SetActive(true);

            mIsDragging = true;
        }

        void IDragHandler.OnDrag(PointerEventData eventData) {
            if(!mIsDragging)
                return;

            dragRoot.position = eventData.position;
        }

        void IEndDragHandler.OnEndDrag(PointerEventData eventData) {
            if(!mIsDragging)
                return;

            if(eventData.pointerCurrentRaycast.isValid && eventData.pointerCurrentRaycast.gameObject == dragToArea.gameObject) {
                if(!string.IsNullOrEmpty(sfxDragDrop))
                    M8.SoundPlaylist.instance.Play(sfxDragDrop, false);

                if(signalInvokeDragEnd)
                    signalInvokeDragEnd.Invoke();

                if(mDragGuide)
                    mDragGuide.Hide();
            }

            EndDrag();
        }

        void EndDrag() {
            dragRoot.gameObject.SetActive(false);

            mIsDragging = false;
        }
    }
}