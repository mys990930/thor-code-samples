using System.Collections.Generic;

public class SkillStat
{
    private SkillLvData[] activeLv;
    private SkillLvData[] passive;
    private SkillLvData[] combination;

    public Dictionary<SkillType, SkillLvData[]> stat;
    public Dictionary<string, SkillLvData> skillLevels;

    public SkillStat(SkillLvData[] activeLv, SkillLvData[] passive, SkillLvData[] combination)
    {
        this.activeLv = activeLv;
        this.passive = passive;
        this.combination = combination;

        stat = new Dictionary<SkillType, SkillLvData[]>() {
            {SkillType.ACTIVE, this.activeLv },
            {SkillType.PASSIVE, this.passive },
            {SkillType.COMBINATION, this.combination }
        };
        skillLevels = new Dictionary<string, SkillLvData>();
        foreach(SkillLvData skill in activeLv)
        {
            skillLevels.Add(skill.code, skill);
        }
        foreach (SkillLvData skill in passive)
        {
            skillLevels.Add(skill.code, skill);
        }
        foreach (SkillLvData skill in combination)
        {
            skillLevels.Add(skill.code, skill);
        }
    }

}