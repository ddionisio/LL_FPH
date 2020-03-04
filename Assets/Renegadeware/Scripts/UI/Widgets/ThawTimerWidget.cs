using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class ThawTimerWidget : MonoBehaviour {
    [System.Serializable]
    public class MaterialData {
        public Material material;
        public string colorVarName;
        public Color colorStart;
        public Color colorEnd;

        public void Apply(float t) {
            material.SetColor(colorVarName, Color.Lerp(colorStart, colorEnd, t));
        }
    }

    [Header("Data")]
    public float delay;

    [Header("Display")]
    public Text timeLabel;
    public Image fillImage;

    [Header("Materials")]
    public MaterialData[] materials;
        
    [Header("Signals")]
    public M8.Signal signalListenStart;
    public M8.SignalFloat signalListenChangeScale;

    public M8.Signal signalInvokeFinish;

    private float mTimeScale;
    private float mCurTime;

    void OnEnable() {
        Init();
    }

    void Awake() {
        signalListenStart.callback += OnSignalStart;
        signalListenChangeScale.callback += OnSignalChangeScale;
    }

    void OnDestroy() {
        signalListenStart.callback -= OnSignalStart;
        signalListenChangeScale.callback -= OnSignalChangeScale;
    }

    void OnSignalStart() {
        StopAllCoroutines();

        Init();

        StartCoroutine(DoUpdate());
    }

    void OnSignalChangeScale(float s) {
        mTimeScale = s;
    }

    IEnumerator DoUpdate() {
        while(mCurTime < delay) {
            yield return null;

            mCurTime += Time.deltaTime * mTimeScale;
            if(mCurTime > delay)
                mCurTime = delay;

            UpdateDisplay();
        }

        signalInvokeFinish.Invoke();
    }

    private void Init() {
        mTimeScale = 1f;
        mCurTime = 0f;

        UpdateDisplay();
    }

    private void UpdateDisplay() {
        int seconds = Mathf.FloorToInt(mCurTime);
        int minutes = seconds / 60;
        int hours = minutes / 60;

        seconds %= 60;
        minutes %= 60;

        timeLabel.text = string.Format("{0}:{1:00}:{2:00}", hours, minutes, seconds);

        var t = Mathf.Clamp01(mCurTime / delay);

        fillImage.fillAmount = Mathf.Clamp01(1f - t);

        for(int i = 0; i < materials.Length; i++)
            materials[i].Apply(t);
    }
}
