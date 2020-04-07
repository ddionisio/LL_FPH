using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

namespace Renegadeware {
    public class DangerZoneController : MonoBehaviour {
        [Header("Config")]
        public DangerZoneData data;

        [Header("Choice Data")]
        public string choiceModalRef;

        [Header("Thermometer")]
        public ThermometerCalibrateWidget thermometer;
        public LoLExt.AnimatorEnterExit thermometerTransition;
        public LoLExt.AnimatorEnterExit thermometerZonesTransition;

        [Header("Illustration")]
        public Image illustrationImage;
        public Text illustrationLabel;
        public LoLExt.AnimatorEnterExit illustrationTransition;

        [Header("Result Description")]
        public LoLExt.AnimatorEnterExit resultDescTransition;
        public Text resultDescText;

        [Header("Score")]
        public M8.UI.Texts.TextCounter scoreCounter;
        public LoLExt.AnimatorEnterExit scoreTransition;

        [Header("Signals")]
        public M8.Signal signalListenStart;

        public M8.Signal signalInvokeComplete;

        [Header("Sound")]
        [M8.SoundPlaylist]
        public string sfxCorrect;
        [M8.SoundPlaylist]
        public string sfxWrong;

        private int mChoiceIndex;
        private bool mChoiceIsNext;

        private M8.GenericParams mParms = new M8.GenericParams();

        void OnDestroy() {
            signalListenStart.callback -= OnSignalStart;
        }

        void Awake() {
            //mix up the items
            M8.ArrayUtil.Shuffle(data.items);

            signalListenStart.callback += OnSignalStart;
        }

        void OnSignalStart() {
            StartCoroutine(DoPlay());
        }

        void OnModalChoiceNext(int ind) {
            mChoiceIndex = ind;
            mChoiceIsNext = true;
        }

        IEnumerator DoPlay() {
            var waitPostItem = new WaitForSeconds(0.5f);

            var modalMgr = M8.ModalManager.main;
            var choiceModal = modalMgr.GetBehaviour<LoLExt.ModalChoice>(choiceModalRef);

            thermometer.ApplyDegree(32f, true);

            yield return thermometerTransition.PlayEnterWait();

            var items = data.items;
            for(int i = 0; i < items.Length; i++) {
                var itm = items[i];

                //show item
                illustrationImage.sprite = itm.icon;
                illustrationLabel.text = M8.Localize.Get(itm.titleTextRef);

                yield return illustrationTransition.PlayEnterWait();

                //thermometer
                thermometer.ApplyDegree(itm.tempDegree, false);

                //setup and open choice
                mChoiceIndex = -1;
                mChoiceIsNext = false;

                mParms[LoLExt.ModalChoice.parmDescTextRef] = itm.choiceTextBaseRef;
                mParms[LoLExt.ModalChoice.parmChoices] = GenerateChoices(itm.choiceTextBaseRef);
                mParms[LoLExt.ModalChoice.parmStartSelect] = -1;
                mParms[LoLExt.ModalChoice.parmNextCallback] = (System.Action<int>)OnModalChoiceNext;
                mParms[LoLExt.ModalChoice.parmShuffle] = true;
                mParms[LoLExt.ModalChoice.parmDisplayPostSelected] = true;

                modalMgr.Open(choiceModalRef, mParms);

                //wait for choice
                while(!mChoiceIsNext)
                    yield return null;

                bool isCorrect = mChoiceIndex == itm.correctIndex;

                //show correct
                mChoiceIsNext = false;

                choiceModal.ShowCorrectChoice(itm.correctIndex, true);

                //show thermometer zone
                thermometerZonesTransition.PlayEnter();

                //show score, update score
                if(isCorrect) {
                    if(!string.IsNullOrEmpty(sfxCorrect))
                        M8.SoundPlaylist.instance.Play(sfxCorrect, false);

                    scoreCounter.SetCountImmediate(0);

                    yield return scoreTransition.PlayEnterWait();

                    scoreCounter.count = data.scorePerItem;

                    LoLExt.LoLManager.instance.curScore += data.scorePerItem;
                }
                else {
                    if(!string.IsNullOrEmpty(sfxWrong))
                        M8.SoundPlaylist.instance.Play(sfxWrong, false);
                }

                //show result description
                resultDescText.text = M8.Localize.Get(itm.resultDescRef);
                resultDescTransition.PlayEnter();

                //wait for next
                while(!mChoiceIsNext)
                    yield return null;

                //hide stuff
                thermometerZonesTransition.PlayExit();

                if(isCorrect)
                    scoreTransition.PlayExit();

                illustrationTransition.PlayExit();

                resultDescTransition.PlayExit();

                modalMgr.CloseUpTo(choiceModalRef, true);

                while(modalMgr.isBusy || modalMgr.IsInStack(choiceModalRef))
                    yield return null;

                if(i < items.Length - 1)
                    yield return waitPostItem;
            }

            yield return thermometerTransition.PlayExitWait();

            signalInvokeComplete.Invoke();
        }

        private LoLExt.ModalChoiceItemInfo[] GenerateChoices(string baseTextRef) {
            var localize = M8.Localize.instance;

            var choiceList = new List<LoLExt.ModalChoiceItemInfo>();

            int ind = 0;
            while(true) {
                var textRef = baseTextRef + ind.ToString();

                if(!localize.Exists(textRef))
                    break;

                choiceList.Add(new LoLExt.ModalChoiceItemInfo() { textRef = textRef });

                ind++;
            }

            return choiceList.ToArray();
        }
    }
}
 