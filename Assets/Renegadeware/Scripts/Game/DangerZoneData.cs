using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Renegadeware {
    [CreateAssetMenu(fileName = "dangerZoneData", menuName = "Game/Danger Zone")]
    public class DangerZoneData : ScriptableObject {
        [System.Serializable]
        public class ItemData {
            public Sprite icon;

            [M8.Localize]
            public string titleTextRef;

            [M8.Localize]
            public string choiceTextBaseRef;

            public int correctIndex;

            public int tempDegree;
        }

        public ItemData[] items;

        public int scorePerItem;
    }
}