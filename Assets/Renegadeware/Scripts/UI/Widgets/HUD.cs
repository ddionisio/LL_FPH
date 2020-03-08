using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class HUD : MonoBehaviour {
    [Header("Data")]
    public float stayDelay = 4f;

    [Header("Display")]
    public GameObject displayGO;
    public M8.UI.Texts.TextCounter scoreCounter;

    [Header("Animation")]
    public M8.Animator.Animate animator;
    [M8.Animator.TakeSelector(animatorField = "animator")]
    public string takeEnter;
    [M8.Animator.TakeSelector(animatorField = "animator")]
    public string takeExit;

    private bool mIsInit;
    private Coroutine mShowRout;
    private float mCurTime;

    void OnEnable() {
        displayGO.SetActive(false);
    }

    void OnDestroy() {
        if(LoLExt.LoLManager.isInstantiated)
            LoLExt.LoLManager.instance.scoreUpdateCallback -= OnScoreUpdate;
    }

    void OnDisable() {
        mShowRout = null;
    }

    IEnumerator Start() {
        while(!LoLExt.LoLManager.isInstantiated)
            yield return null;

        scoreCounter.SetCountImmediate(LoLExt.LoLManager.instance.curScore);

        LoLExt.LoLManager.instance.scoreUpdateCallback += OnScoreUpdate;
    }

    void OnScoreUpdate(LoLExt.LoLManager lol) {
        mCurTime = 0f;

        if(mShowRout == null)
            mShowRout = StartCoroutine(DoShow());
        else
            scoreCounter.count = lol.curScore;
    }

    IEnumerator DoShow() {
        displayGO.SetActive(true);

        yield return animator.PlayWait(takeEnter);

        scoreCounter.count = LoLExt.LoLManager.instance.curScore;

        while(scoreCounter.isPlaying)
            yield return null;

        while(mCurTime < stayDelay) {
            yield return null;
            mCurTime += Time.deltaTime;
        }

        yield return animator.PlayWait(takeExit);

        displayGO.SetActive(false);

        mShowRout = null;
    }
}
