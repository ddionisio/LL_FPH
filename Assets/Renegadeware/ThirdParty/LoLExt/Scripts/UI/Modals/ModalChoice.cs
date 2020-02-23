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
        public string textSpeechGroup;
        public bool textSpeechAuto;

        private M8.CacheList<ModalChoiceItem> mItemActives = new M8.CacheList<ModalChoiceItem>(choiceCapacity);
        private M8.CacheList<ModalChoiceItem> mItemCache = new M8.CacheList<ModalChoiceItem>(choiceCapacity);

        private string mTitleTextRef;
        private string mDescTextRef;

        private int mCurChoiceIndex;
        private System.Action<int> mNextCallback;

        public void PlayDialogSpeech() {
            int ind = 0;

            if(!string.IsNullOrEmpty(mTitleTextRef)) {
                LoLManager.instance.SpeakTextQueue(mTitleTextRef, textSpeechGroup, ind);
                ind++;
            }

            if(!string.IsNullOrEmpty(mDescTextRef)) {
                LoLManager.instance.SpeakTextQueue(mDescTextRef, textSpeechGroup, ind);
                ind++;
            }

            for(int i = 0; i < mItemActives.Count; i++) {
                LoLManager.instance.SpeakTextQueue(mItemActives[i].textRef, textSpeechGroup, ind + i);
            }
        }

        void M8.IModalActive.SetActive(bool aActive) {
            if(aActive) {
                if(textSpeechAuto)
                    PlayDialogSpeech();
            }
            else {
                if(confirmButton)
                    confirmButton.interactable = false;
            }

            //enable/disable choice interactions
            for(int i = 0; i < mItemActives.Count; i++) {
                mItemActives[i].interactable = aActive;
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

            //setup display
            titleLabel.text = !string.IsNullOrEmpty(mTitleTextRef) ? M8.Localize.Get(mTitleTextRef) : "";

            descLabel.text = !string.IsNullOrEmpty(mDescTextRef) ? M8.Localize.Get(mDescTextRef) : "";

            //setup choices
            ClearChoices();
            if(infos != null)
                GenerateChoices(infos);

            mCurChoiceIndex = -1;

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
            var prevChoiceIndex = mCurChoiceIndex;
            mCurChoiceIndex = index;

            if(prevChoiceIndex != -1)
                mItemActives[prevChoiceIndex].selected = false;

            mItemActives[mCurChoiceIndex].selected = true;

            if(confirmReadyGO)
                confirmReadyGO.SetActive(true);

            if(!confirmButton) { //call next if no confirm
                mNextCallback?.Invoke(mCurChoiceIndex);
            }
        }

        void OnConfirmClick() {
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