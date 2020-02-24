using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

namespace LoLExt {
    public class ModalChoice : M8.ModalController, M8.IModalActive, M8.IModalPush, M8.IModalPop {
        public const int choiceCapacity = 4;

        public const string parmTitleTextRef = "t";
        public const string parmDescTextRef = "d";
        public const string parmChoices = "c"; //array of ModalChoiceItemInfo
        public const string parmStartSelect = "i";
        public const string parmNextCallback = "n";

        [Header("Display")]        
        public Text titleLabel;
        public Text descLabel;

        [Header("Choice")]
        public ModalChoiceItem choiceTemplate; //put this in a hierarchy to spawn choice items.

        [Header("Confirm")]
        public Button confirmButton; //if not null, user must click on a choice and click on confirm. Otherwise, click on choice directly
        public GameObject confirmReadyGO; //if a choice has been made, activate this

        [Header("Text Speech")]
        public bool textSpeechAuto;

        private M8.CacheList<ModalChoiceItem> mItemActives = new M8.CacheList<ModalChoiceItem>(choiceCapacity);
        private M8.CacheList<ModalChoiceItem> mItemCache = new M8.CacheList<ModalChoiceItem>(choiceCapacity);

        private string mTitleTextRef;
        private string mDescTextRef;

        private int mCurChoiceIndex;
        private System.Action<int> mNextCallback;

        public void PlayDialogSpeech() {
            var grpName = name;
            int ind = 0;

            LoLManager.instance.StopSpeakQueue();

            if(!string.IsNullOrEmpty(mTitleTextRef)) {
                LoLManager.instance.SpeakTextQueue(mTitleTextRef, grpName, ind);
                ind++;
            }

            if(!string.IsNullOrEmpty(mDescTextRef)) {
                LoLManager.instance.SpeakTextQueue(mDescTextRef, grpName, ind);
                ind++;
            }

            for(int i = 0; i < mItemActives.Count; i++) {
                LoLManager.instance.SpeakTextQueue(mItemActives[i].textRef, grpName, ind + i);
            }
        }

        void M8.IModalActive.SetActive(bool aActive) {
            if(aActive) {
                if(textSpeechAuto)
                    PlayDialogSpeech();

                if(confirmReadyGO)
                    confirmReadyGO.SetActive(mCurChoiceIndex != -1);

                if(confirmButton)
                    confirmButton.interactable = mCurChoiceIndex != -1;
            }
            else {
                if(confirmReadyGO)
                    confirmReadyGO.SetActive(false);

                if(confirmButton)
                    confirmButton.interactable = false;
            }

            //apply selected
            //enable/disable choice interactions
            for(int i = 0; i < mItemActives.Count; i++) {
                if(aActive) {
                    mItemActives[i].selected = mCurChoiceIndex == i;
                    mItemActives[i].interactable = true;
                }
                else {
                    mItemActives[i].selected = false;
                    mItemActives[i].interactable = true;
                }
            }
        }
                
        void M8.IModalPush.Push(M8.GenericParams parms) {
            if(parms == null)
                return;

            ModalChoiceItemInfo[] infos;

            //grab configuration
            parms.TryGetValue(parmTitleTextRef, out mTitleTextRef);

            parms.TryGetValue(parmDescTextRef, out mDescTextRef);
                        
            parms.TryGetValue(parmChoices, out infos);

            parms.TryGetValue(parmNextCallback, out mNextCallback);

            mCurChoiceIndex = parms.ContainsKey(parmStartSelect) ? parms.GetValue<int>(parmStartSelect) : -1;

            //setup display
            titleLabel.text = !string.IsNullOrEmpty(mTitleTextRef) ? M8.Localize.Get(mTitleTextRef) : "";

            descLabel.text = !string.IsNullOrEmpty(mDescTextRef) ? M8.Localize.Get(mDescTextRef) : "";

            //setup choices
            ClearChoices();
            if(infos != null)
                GenerateChoices(infos);
                        
            //setup confirm
            if(confirmButton)
                confirmButton.interactable = false;

            if(confirmReadyGO)
                confirmReadyGO.SetActive(false);
        }

        void M8.IModalPop.Pop() {
            mNextCallback = null;
        }

        void Awake() {
            if(choiceTemplate)
                choiceTemplate.gameObject.SetActive(false);

            if(confirmButton)
                confirmButton.onClick.AddListener(OnConfirmClick);
        }

        void OnChoiceClick(int index) {
            //update selection
            if(mCurChoiceIndex != index) {
                var prevChoiceIndex = mCurChoiceIndex;
                mCurChoiceIndex = index;

                if(prevChoiceIndex != -1)
                    mItemActives[prevChoiceIndex].selected = false;

                mItemActives[mCurChoiceIndex].selected = true;

                if(confirmButton && prevChoiceIndex == -1)
                    confirmButton.interactable = true;
            }

            //update confirm
            if(confirmReadyGO)
                confirmReadyGO.SetActive(true);

            if(!confirmButton) //call next if no confirm
                OnConfirmClick();
        }

        void OnConfirmClick() {
            if(confirmReadyGO)
                confirmReadyGO.SetActive(false);

            if(confirmButton)
                confirmButton.interactable = false;

            if(mCurChoiceIndex != -1)
                mNextCallback?.Invoke(mCurChoiceIndex);
        }

        private void GenerateChoices(ModalChoiceItemInfo[] infos) {
            if(choiceTemplate) {
                var choiceRoot = choiceTemplate.transform.parent;

                for(int i = 0; i < infos.Length; i++) {
                    ModalChoiceItem itm;

                    if(mItemCache.Count > 0) {
                        itm = mItemCache.RemoveLast();
                    }
                    else { //add new
                        itm = Instantiate(choiceTemplate);
                        itm.transform.SetParent(choiceRoot, false);

                        itm.clickCallback += OnChoiceClick;
                    }

                    itm.Setup(i, infos[i]);
                    itm.selected = false;
                    itm.interactable = false;
                    itm.gameObject.SetActive(true);
                }
            }
        }

        private void ClearChoices() {
            for(int i = 0; i < mItemActives.Count; i++) {
                var itm = mItemActives[i];
                if(itm) {
                    itm.gameObject.SetActive(false);
                    mItemCache.Add(itm);
                }
            }

            mItemActives.Clear();
        }
    }
}