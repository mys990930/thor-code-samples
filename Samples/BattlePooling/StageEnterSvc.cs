using System;
using System.Collections.Generic;
using UnityEngine;

public class StageEnterSvc
{
    public StageAsset GetStageAssets(ClientStageData currentStageData)
    {
        //ShowLoadingScreenAsync();
        List<string> enemyObjCodes = GetEnemyObjCodes(currentStageData.timelineCode);
        SetupEnemies(enemyObjCodes);

        BattleTimelineSO timeline = ClientContentsDataContainer.instance.battleTimelineSOs[currentStageData.timelineCode];
        Sprite bgSprite = ClientAssetContainer.instance.spriteDatas[currentStageData.bgSpriteCode];

        return new StageAsset(timeline, bgSprite);
    }

    public void SetupEnemies(List<string> enemyObjCodes)
    {
        ObjectPoolManager.instance.InitEnemyPools(enemyObjCodes);
    }

    private List<string> GetEnemyObjCodes(string timelineCode)
    {
        BattleTimelineSO timeline = ClientContentsDataContainer.instance.battleTimelineSOs[timelineCode];
        List<string> targetEnemyCodes = new List<string>();
        foreach (SpawnEventData spawnEvent in timeline.spawnEvents)
        {
            if (!targetEnemyCodes.Contains(spawnEvent.enemyCode))
            {
                targetEnemyCodes.Add(spawnEvent.enemyCode);
            }
        }
        return targetEnemyCodes;
    }
}