using System.Collections;
using System.Collections.Generic;
using UnityEngine;

using M8;

namespace HutongGames.PlayMaker.Actions.LoL {
    [ActionCategory("Legends of Learning")]
    [Tooltip("Description and text only choices. Choice text refs are based on index numbers (start at 0) appended on descTextRef.")]
    public class ModalDialogChoiceSimple : FsmStateAction {
        public FsmString modal;

        [Tooltip("Text ref. for description. Choices are grabbed by appending index numbers starting at 0.")]
        public M8.FsmLocalize descTextRef;

        [Tooltip("Starting selected choice.")]
        public FsmInt startIndex;

        public FsmBool isShuffle;

        [RequiredField]
        [UIHint(UIHint.Variable)]
        public FsmInt outputChoiceIndex;

        private LoLExt.ModalChoiceItemInfo[] mInfos;

        private GenericParams mParms = new GenericParams();
        private int mChoiceSelectedIndex;
        private bool mIsNext;

        public override void Reset() {
            modal = null;
            descTextRef = null;
            startIndex = -1;
            isShuffle = false;
        }

        public override void OnEnter() {
            var descRef = descTextRef.GetStringRef();
                        
            var infoList = new List<LoLExt.ModalChoiceItemInfo>();

            var localize = Localize.instance;

            int i = 0;
            while(true) {
                var choiceTextRef = descRef + i;
                if(localize.Exists(choiceTextRef)) {
                    infoList.Add(new LoLExt.ModalChoiceItemInfo() { textRef = choiceTextRef });
                    i++;
                }
                else
                    break;
            }

            mInfos = infoList.ToArray();

            if(isShuffle.Value)
                ArrayUtil.Shuffle(mInfos);

            mParms.Add(LoLExt.ModalChoice.parmDescTextRef, descRef);
            mParms.Add(LoLExt.ModalChoice.parmChoices, mInfos);
            mParms.Add(LoLExt.ModalChoice.parmStartSelect, startIndex.IsNone ? -1 : startIndex.Value);
            mParms.Add(LoLExt.ModalChoice.parmNextCallback, (System.Action<int>)OnNext);

            mChoiceSelectedIndex = -1;
            mIsNext = false;

            ModalManager.main.Open(modal.Value, mParms);
        }

        public override void OnUpdate() {
            //wait for next
            if(mIsNext) {
                //wait for dialog to close
                if(!(ModalManager.main.isBusy || ModalManager.main.IsInStack(modal.Value)))
                    Finish();
            }
        }

        void OnNext(int index) {
            mChoiceSelectedIndex = index;
            mIsNext = true;

            ModalManager.main.CloseUpTo(modal.Value, true);
        }
    }
}