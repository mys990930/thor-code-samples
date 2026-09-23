using System;

[Serializable]
public struct SpawnEventData
{
    /// <summary>
    /// enemyDensity: 초 당 주위에 몇마리나 나올지
    /// </summary>
    public int startTime;
    public string enemyCode;
    public int enemyDensity;
    public BattleEventType eventType;

    public SpawnEventData(int startTime, string enemyCode, int enemyDensity, int eventType)
    {
        this.startTime = startTime;
        this.enemyCode = enemyCode;
        this.enemyDensity = enemyDensity;
        this.eventType = (BattleEventType)eventType;
    }

    public static bool operator ==(SpawnEventData a, SpawnEventData b)
    {
        if (a.startTime == b.startTime && a.enemyCode == b.enemyCode && a.enemyDensity == b.enemyDensity && a.eventType == b.eventType)
            return true;
        else return false;
    }
    public static bool operator !=(SpawnEventData a, SpawnEventData b)
    {
        if (a.startTime != b.startTime || a.enemyCode != b.enemyCode || a.enemyDensity != b.enemyDensity || a.eventType != b.eventType)
            return true;
        else return false;
    }

    public override bool Equals(object obj)
    {
        return obj is SpawnEventData data &&
               startTime == data.startTime &&
               enemyCode == data.enemyCode &&
               enemyDensity == data.enemyDensity &&
               eventType == data.eventType;
    }

    public override int GetHashCode()
    {
        return HashCode.Combine(startTime, enemyCode, enemyDensity, eventType);
    }
}

/// <summary>
/// [0] summon: 산개 소환 (개체끼리 따로 소환)
/// [1] groupSummon: 군집 소환 (동 개체끼리 묶어서 따로 소환)
/// [2] mixSummon: 섞인 군집 소환 (군집끼리 섞어서 소환)
/// [3] eliteSummon: 체력 많은 엘리트몹 소환
/// [4] middleBoss: 중보스
/// [5] finalBoss: 최종보스
/// </summary>
public enum BattleEventType { summon, groupSummon, mixSummon, eliteSummon, middleBoss, finalBoss }
