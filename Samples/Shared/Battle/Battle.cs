using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public abstract class Battle: MonoBehaviour, BattlePauseListener, BossKillListener
{
    public Transform canvas;
    public IdleUIView idleUIView;
    public SurviveUIView surviveUIView;
    public RaidUIView raidUIView;

    protected BattleManager manager;
    protected EnemyFactory enemyFactory;

    protected Image fadeScreen;
    protected Sprite bgSprite;

    protected ClientStageData currentStageData;

    protected void Awake()
    {
        RegisterListener();
    }

    public void Init(int stageLv)
    {
        canvas = GameObject.FindGameObjectWithTag("Canvas").transform;
        idleUIView = canvas.GetChild(1).GetComponent<IdleUIView>();
        surviveUIView = canvas.GetChild(2).GetComponent<SurviveUIView>();
        raidUIView = canvas.GetChild(3).GetComponent<RaidUIView>();

        manager = BattleManager.instance;
        fadeScreen = canvas.GetChild(4).GetComponent<Image>();
        StartCoroutine(SetupBattle(stageLv));
    }

    public void RegisterListener()
    {
        BattleEventManager.instance.onBattlePause += OnBattlePause;
        BattleEventManager.instance.onBattleResume += OnBattleResume;
        BattleEventManager.instance.onBossKill += OnBossKill;
    }

    public void DeregisterListener()
    {
        BattleEventManager.instance.onBattlePause -= OnBattlePause;
        BattleEventManager.instance.onBattleResume -= OnBattleResume;
        BattleEventManager.instance.onBossKill -= OnBossKill;
    }

    protected IEnumerator SetupBattle(int stageLv)
    {
        bool isComplete = false;
        yield return StartCoroutine(FadeOut(() => { isComplete = true; }));
        yield return new WaitUntil(() => isComplete);

        GameManager.instance.StartLoading();

        var currentStageData = SetupStageData(manager.currentStageType, stageLv);
        manager.battleDataContainer = new BattleDataContainer(currentStageData);

        manager.isAutoBattle = false;
        CreateChara();
        PaintBG(bgSprite); //SetupStageData()에서 set
        SetupUI();
        enemyFactory = gameObject.AddComponent<EnemyFactory>();

        ChangeBattleMode();

        GameManager.instance.EndLoading();
        isComplete = false;
        EventManager.instance.onBattleStart();
        yield return StartCoroutine(FadeIn(() => { isComplete = true; }));
        yield return new WaitUntil(() => isComplete);
        StartCombat();
        BattleManager.instance.StartTimer();
    }

    public IEnumerator StopBattle(StageResultState stageResult, Action<StageResult> callback = null)
    {
        BattleManager.instance.StopTimer();
        GameManager.instance.StartLoading();

        StopCombat();

        bool isComplete = false;
        yield return StartCoroutine(FadeOut(() => { isComplete = true; }));
        yield return new WaitUntil(() => isComplete);

        EventManager.instance.onBattleEnd();

        manager.currentBattleState = BattleState.eNone;
        manager.currentSurvivalState = SurvivalBattleState.eNone;

        Destroy(manager.charaObj);
        manager.chara = null;
        manager.charaObj = null;

        manager.currentBattle = null;
        GameManager.instance.EndLoading();

        if (callback != null)
        {
            var result = GetStageResult(stageResult);
            callback(result);
        }

        Destroy(this.gameObject);
    }

    protected IEnumerator CancelBattle(Action endCallback)
    {
        StopCombat();

        bool isComplete = false;
        yield return StartCoroutine(FadeOut(() => { isComplete = true; }));
        yield return new WaitUntil(() => isComplete);

        EventManager.instance.onBattleEnd();

        manager.currentBattleState = BattleState.eNone;
        manager.currentSurvivalState = SurvivalBattleState.eNone;

        Destroy(manager.charaObj);
        manager.chara = null;
        manager.charaObj = null;

        endCallback();
    }

    protected Coroutine StartTrackedCoroutine(ref Coroutine coroutine, IEnumerator routine)
    {
        StopTrackedCoroutine(ref coroutine);
        coroutine = StartCoroutine(routine);
        return coroutine;
    }

    protected void StopTrackedCoroutine(ref Coroutine coroutine)
    {
        if (coroutine == null) return;

        StopCoroutine(coroutine);
        coroutine = null;
    }

    protected void StopTrackedCoroutines(List<Coroutine> coroutines)
    {
        foreach (Coroutine coroutine in coroutines)
        {
            if (coroutine != null) StopCoroutine(coroutine);
        }
        coroutines.Clear();
    }

    public abstract void OnBattlePause();
    public abstract void OnBattleResume();
    public abstract void OnBossKill(bool isFinalBoss);
    protected abstract void ChangeBattleMode();
    protected abstract ClientStageData SetupStageData(StageType type, int lv);
    protected abstract void SetupUI();
    protected abstract void CreateChara();
    protected abstract void StartCombat();
    protected abstract void StopCombat();
    protected abstract StageResult GetStageResult(StageResultState stageResult);

    private IEnumerator FadeOut(Action endCallback)
    {
        fadeScreen.raycastTarget = true;
        WaitForSeconds wait = new WaitForSeconds(0.02f);
        Color color = fadeScreen.color;
        float alpha = color.a;

        while (fadeScreen.color.a <= 1)
        {
            alpha += 0.1f;
            Color tempColor = new Color(0, 0, 0, alpha);
            fadeScreen.color = tempColor;
            yield return wait;
        }
        endCallback();
    }

    private IEnumerator FadeIn(Action endCallback)
    {
        WaitForSeconds wait = new WaitForSeconds(0.02f);
        Color color = fadeScreen.color;
        float alpha = color.a;
        while (fadeScreen.color.a >= 0)
        {
            alpha -= 0.1f;
            Color tempColor = new Color(0, 0, 0, alpha);
            fadeScreen.color = tempColor;
            yield return wait;
        }
        fadeScreen.raycastTarget = false;

        endCallback();
    }

    private void PaintBG(Sprite bg)
    {
        MapBGControlManager.instance.bgSprite = bg;
        MapBGControlManager.instance.InitBattle();
    }
}
