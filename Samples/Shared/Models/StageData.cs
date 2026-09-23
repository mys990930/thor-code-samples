using System;
using System.Collections.Generic;
using UnityEngine;

[Serializable]
public enum StageType
{
    MAIN,
    GOLD_DUNGEON,
    EXP_DUNGEON,
    STONE_DUNGEON,
    GEM_DUNGEON,
    DAILY_RAID,
    AWAKENING_RAID,
    BATTLEFIELD,
    PVP,
    BOOK_DUNGEON,

    Max
}

public class StageAsset
{
    public BattleTimelineSO timeline;
    public Sprite bgSprite;

    public StageAsset(BattleTimelineSO timeline, Sprite bgSpriteCode)
    {
        this.timeline = timeline;
        this.bgSprite = bgSpriteCode;
    }
}

[Serializable]
public struct ClientStageData
{
    public StageType stageType;
    public int stageLv;
    public string timelineCode;
    public string bgSpriteCode;
    public int timeLimit;
    public big reqPower;
    public big atkCoeff;
    public SerializableDictionary<MoneyType, Reward> rewards;

    public ClientStageData(StageType type, int lv, string timelineCode, string bgSpriteCode, int timeLimit, big reqPower, 
        big atkCoeff, SerializableDictionary<MoneyType, Reward> rewards)
    {
        this.stageType = type;
        this.stageLv = lv;
        this.timelineCode = timelineCode;
        this.bgSpriteCode = bgSpriteCode;
        this.timeLimit = timeLimit;
        this.reqPower = reqPower;
        this.atkCoeff = atkCoeff;
        this.rewards = rewards;
    }
}