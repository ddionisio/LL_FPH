using System.Collections;
using System.Collections.Generic;
using UnityEngine;
//using UnityEngine.UI;

namespace Renegadeware {
    public class ModalTCSFood : M8.ModalController, M8.IModalActive, M8.IModalPush, M8.IModalPop {
        public enum Mode {
            None,
            Inventory,
            Result
        }

        [Header("Data")]
        public TCSFoodData data;

        [Header("TCS Food Panel")]
        public TCSFoodItemWidget TCSFoodWidgetTemplate;

        public Transform TCSFoodRoot;
        public Transform nonTCSFoodRoot;

        [Header("Inventory")]
        public LoLExt.AnimatorEnterExit inventoryTransition;
        public Transform inventoryFoodRoot;

        [Header("Result")]
        public LoLExt.AnimatorEnterExit resultTransition;
        public M8.UI.Texts.TextCounter resultScoreCounter;
        public float resultItemNextDelay = 0.3f;

        [Header("Drag Guide")]
        public LoLExt.DragToGuideWidget dragGuide;

        [Header("Sound")]
        [M8.SoundPlaylist]
        public string sfxResultItem;
        [M8.SoundPlaylist]
        public string sfxResult;

        public LoLExt.AnimatorEnterExit nextTransition;

        private M8.CacheList<TCSFoodItemWidget> mFoodItemActives;
        private M8.CacheList<TCSFoodItemWidget> mFoodItemCache;

        private Mode mCurMode;
        private Coroutine mRout;

        private bool mIsDragEnd;

        public void Next() {
            switch(mCurMode) {
                case Mode.Inventory:
                    mRout = StartCoroutine(DoResult());
                    break;

                case Mode.Result:
                    Close();
                    break;
            }

            nextTransition.PlayExit();
        }

        void M8.IModalActive.SetActive(bool aActive) {
            if(aActive) {
                mRout = StartCoroutine(DoInventory());
            }
            else {
                if(mRout != null) {
                    StopCoroutine(mRout);
                    mRout = null;
                }

                //disable input for items
                for(int i = 0; i < mFoodItemActives.Count; i++)
                    mFoodItemActives[i].inputEnabled = false;

                mCurMode = Mode.None;
            }
        }

        void M8.IModalPush.Push(M8.GenericParams parms) {
            //populate inventory
            ClearFoodItems();
            
            var foodData = data.GenerateItems();
            for(int i = 0; i < foodData.Length; i++) {
                var dat = foodData[i];

                TCSFoodItemWidget widget;
                if(mFoodItemCache.Count > 0)
                    widget = mFoodItemCache.RemoveLast();
                else {
                    widget = Instantiate(TCSFoodWidgetTemplate);

                    widget.dragEndCallback += OnItemDragEnd;
                }

                widget.Setup(dat);

                widget.transform.SetParent(inventoryFoodRoot, false);
                widget.gameObject.SetActive(true);

                mFoodItemActives.Add(widget);
            }

            mCurMode = Mode.None;

            resultScoreCounter.SetCountImmediate(0);

            //setup default display
            resultTransition.gameObject.SetActive(false);
            nextTransition.gameObject.SetActive(false);

            //start showing inventory
            inventoryTransition.gameObject.SetActive(true);
            inventoryTransition.PlayEnter();
        }

        void M8.IModalPop.Pop() {

        }

        void Awake() {
            mFoodItemActives = new M8.CacheList<TCSFoodItemWidget>(data.foodCount);
            mFoodItemCache = new M8.CacheList<TCSFoodItemWidget>(data.foodCount);

            TCSFoodWidgetTemplate.gameObject.SetActive(false);
        }

        IEnumerator DoInventory() {
            mCurMode = Mode.Inventory;

            while(inventoryTransition.isEntering)
                yield return null;

            //enable input for items
            for(int i = 0; i < mFoodItemActives.Count; i++)
                mFoodItemActives[i].inputEnabled = true;

            //show drag guide of first item
            dragGuide.Show(false, mFoodItemActives[0].transform.position, TCSFoodRoot.position);

            //wait for all items to be placed
            while(true) {
                mIsDragEnd = false; //wait for an item drag end
                while(!mIsDragEnd)
                    yield return null;

                //check items placed
                int itemsPlacedCount = 0;
                for(int i = 0; i < mFoodItemActives.Count; i++) {
                    var itm = mFoodItemActives[i];
                    var itmT = itm.transform;

                    if(itmT.parent == TCSFoodRoot || itmT.parent == nonTCSFoodRoot)
                        itemsPlacedCount++;
                }

                if(itemsPlacedCount == mFoodItemActives.Count)
                    break;

                yield return null;
            }

            //show next
            yield return inventoryTransition.PlayExitWait();

            inventoryTransition.gameObject.SetActive(false);

            nextTransition.gameObject.SetActive(true);
            nextTransition.PlayEnter();

            mRout = null;
        }

        IEnumerator DoResult() {
            mCurMode = Mode.Result;

            int correctCount = 0;

            var waitDelay = new WaitForSeconds(resultItemNextDelay);

            //evaluate correct and incorrect placements
            for(int i = 0; i < TCSFoodRoot.childCount; i++) {
                var child = TCSFoodRoot.GetChild(i);
                var itm = child.GetComponent<TCSFoodItemWidget>();
                if(itm.data.isHazardous) {
                    itm.correctGO.SetActive(true);
                    correctCount++;
                }
                else
                    itm.wrongGO.SetActive(true);

                if(!string.IsNullOrEmpty(sfxResultItem))
                    M8.SoundPlaylist.instance.Play(sfxResultItem, false);

                yield return waitDelay;
            }

            for(int i = 0; i < nonTCSFoodRoot.childCount; i++) {
                var child = nonTCSFoodRoot.GetChild(i);
                var itm = child.GetComponent<TCSFoodItemWidget>();
                if(!itm.data.isHazardous) {
                    itm.correctGO.SetActive(true);
                    correctCount++;
                }
                else
                    itm.wrongGO.SetActive(true);

                yield return waitDelay;
            }

            //show result
            if(!string.IsNullOrEmpty(sfxResult))
                M8.SoundPlaylist.instance.Play(sfxResult, false);

            resultTransition.gameObject.SetActive(true);
            yield return resultTransition.PlayEnterWait();

            //apply score
            var score = correctCount * data.foodScorePerItem;

            LoLExt.LoLManager.instance.curScore += score;

            resultScoreCounter.count = score;

            //show next
            nextTransition.gameObject.SetActive(true);
            nextTransition.PlayEnter();

            mRout = null;
        }

        void OnItemDragEnd(DragWidget itm) {
            mIsDragEnd = true;

            //hide drag guide
            if(dragGuide.isActive) {
                var itmT = itm.transform;
                if(itmT.parent == TCSFoodRoot || itmT.parent == nonTCSFoodRoot) {
                    dragGuide.Hide();
                }
            }
        }

        private void ClearFoodItems() {
            for(int i = 0; i < mFoodItemActives.Count; i++) {
                var itm = mFoodItemActives[i];
                itm.gameObject.SetActive(false);

                mFoodItemCache.Add(itm);
            }

            mFoodItemActives.Clear();
        }
    }
}