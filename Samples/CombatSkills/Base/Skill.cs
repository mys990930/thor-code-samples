using System.Collections.Generic;
using UnityEngine;

public abstract class Skill: MonoBehaviour
{
    public SkillAssetData data;
    public SkillBattleData battleData;
    public int skillLv;
    public SkillDamageTable damageTable;
    public List<float> percentValuePerGrade;

    protected Chara chara;
    protected string skillCode;

    public void Init()
    {
        SetSkillCode();
        chara = BattleManager.instance.chara;
        LoadSkillData();
        OnInit();
    }

    protected void LoadSkillData()
    {
        data = ClientSkillsDataContainer.instance.skillAssetDatas[skillCode];
        battleData = new SkillBattleData
        {
            code = skillCode,
            name = data.name,
            description = data.battleViewDescriptionTexts[0],
            grade = 1
        };
        skillLv = SkillsDataContainer.instance.skillData.skillLevels[skillCode].lv;
        damageTable = data.damageTableSO.skillDamageTable;
    }

    protected abstract void SetSkillCode();
    protected abstract void OnInit();
    public abstract void LevelUp();
}