using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Renegadeware {
    public class ModalWashProduceSteps : M8.ModalController, M8.IModalActive, M8.IModalPush, M8.IModalPop {
        public enum Mode {
            None,
            Select,
            Result
        }

        [Header("Answers Data")]
        [M8.Localize]
        public string[] answerTextRefs;

        [Header("Slots Data")]
        public AnswerWidget itemTemplate;

        public Transform itemsRoot;
        public Transform stepsRoot;

        void M8.IModalActive.SetActive(bool aActive) {

        }

        void M8.IModalPush.Push(M8.GenericParams parms) {

        }

        void M8.IModalPop.Pop() {

        }
    }
}