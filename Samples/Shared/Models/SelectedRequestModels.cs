using System;
using System.Collections.Generic;

#region ?ㅽ궗 ?낅줈??援ъ“
public struct UpgradeSkillData
{
    public SkillType type;
    public int idx;
    public int selectSkillLv;
    public int currentCombineLv;
    public big usedMoney;

    public UpgradeSkillData(SkillType type, int idx, int selectSkillLv, int currentCombineLv, big usedMoney)
    {
        this.type = type;
        this.idx = idx;
        this.selectSkillLv = selectSkillLv;
        this.currentCombineLv = currentCombineLv;
        this.usedMoney = usedMoney;
    }
}

public struct NewlyAcquiredSkillData
{
    public SkillType type;
    public int idx;
    public int selectSkillLv;
    public int currentCombineLv;

    public NewlyAcquiredSkillData(SkillType type, int idx, int selectSkillLv, int currentCombineLv)
    {
        this.type = type;
        this.idx = idx;
        this.selectSkillLv = selectSkillLv;
        this.currentCombineLv = currentCombineLv;
    }
}
#endregion
public struct StageResult
{
    public StageResultState state;
    public int clearedLv;
    public big highscore;
    public List<Reward> rewardDatas;
    public List<int> artifactRewardDatas;

    public StageResult(StageResultState state, int clearedLv, big highscore, List<Reward> rewards, List<int> artifactRewards)
    {
        this.state = state;
        this.clearedLv = clearedLv;
        this.highscore = highscore;
        this.rewardDatas = rewards;
        this.artifactRewardDatas = artifactRewards;
    }
}
