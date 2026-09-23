using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Random = UnityEngine.Random;

public class IdleBattle: Battle
{
    public Dictionary<MoneyType, int> earnedMoneyBuffer = new Dictionary<MoneyType, int>();
    public List<string> enemyObjCodes = new List<string>();

    private WaitForSeconds wait = new WaitForSeconds(1);
    private Coroutine spawnCoroutine;

    public override void OnBattlePause()
    {
        //isBattlePaused = true;
    }

    public override void OnBattleResume()
    {
        //isBattlePaused = false;
    }

    protected override void ChangeBattleMode()
    {
        manager.battleMode = BattleMode.eIdleMode;
        manager.currentBattleState = BattleState.eBattleState;
        manager.currentSurvivalState = SurvivalBattleState.eNone;
    }

    protected override ClientStageData SetupStageData(StageType type, int lv)
    {
        ClientStageData currentStageData =  ClientContentsDataContainer.instance.stageDatas[type][lv - 1];
        bgSprite = ClientAssetContainer.instance.spriteDatas[currentStageData.bgSpriteCode];

        BattleTimelineSO timeline = ClientContentsDataContainer.instance.battleTimelineSOs[currentStageData.timelineCode];

        foreach(SpawnEventData spawnEvent in timeline.spawnEvents)
        {
            if (spawnEvent.eventType != BattleEventType.middleBoss && spawnEvent.eventType != BattleEventType.finalBoss)
            {
                if (!spawnEvent.enemyCode.StartsWith("112000"))
                {
                    enemyObjCodes.Add(spawnEvent.enemyCode);
                }
            }
        }
        var svc = new StageEnterSvc();
        svc.SetupEnemies(enemyObjCodes);

        return currentStageData;
    }

    protected override void SetupUI()
    {
        idleUIView.gameObject.SetActive(false);
        surviveUIView.gameObject.SetActive(false);
        raidUIView.gameObject.SetActive(false);
        idleUIView.gameObject.SetActive(true);
    }

    protected override void CreateChara()
    {
        CharaBuildSvc charaBuildSvc = gameObject.GetComponent<CharaBuildSvc>();
        if (charaBuildSvc == null)
        {
            charaBuildSvc = gameObject.AddComponent<CharaBuildSvc>();
        }
        charaBuildSvc.enabled = true;
        charaBuildSvc.BuildChara(BattleMode.eIdleMode);
        manager.charaObj = manager.chara.gameObject;
        charaBuildSvc.enabled = false;
    }

    protected override void StartCombat()
    {
        manager.chara.ChangeState(Chara.CharaState.eOnBattle);
        manager.SetAutoBattle(true);
        StartTrackedCoroutine(ref spawnCoroutine, SpawnEnemies());
    }

    private IEnumerator SpawnEnemies()
    {
        while (true)
        {
            if (BattleManager.instance.liveEnemyNum < 50)
            {
                int randomIdx = Random.Range(0, enemyObjCodes.Count);
                enemyFactory.GroupSummon(enemyObjCodes[randomIdx], Random.Range(1, 4));
            }
            yield return wait;
        }
    }

    protected override void StopCombat()
    {
        StopTrackedCoroutine(ref spawnCoroutine);
    }

    public override void OnBossKill(bool isFinalBoss)
    {
        //none
    }

    protected override StageResult GetStageResult(StageResultState stageResult)
    {
        StageResult result = new StageResult();
        result.state = StageResultState.SUCCESS;
        result.clearedLv = 1;
        return result;
    }

    private void OnDestroy()
    {
        if (BattleEventManager.instance != null) DeregisterListener();
    }

}
