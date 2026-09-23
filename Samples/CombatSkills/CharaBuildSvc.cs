using System.Collections.Generic;
using UnityEngine;

public class CharaBuildSvc: MonoBehaviour
{
    public GameObject charaPrefab;

    private Chara chara;

    public void BuildChara(BattleMode battleMode)
    {
        //charaPrefab = Resources.Load<GameObject>("./Prefabs/Chara");
        charaPrefab = ClientDataContainer.instance.charaPrefab;
        GameObject charaObj = Instantiate(charaPrefab, new Vector2(0, 0), Quaternion.identity);
        chara = charaObj.GetComponent<Chara>();
        chara.stat = StatsDataContainer.instance.charaStat;

        if (battleMode == BattleMode.eIdleMode)
        {
            chara.col.enabled = false;
        }
        else if (battleMode == BattleMode.eSurvivalMode)
        {
        }

        BattleManager.instance.chara = chara;
        AddSkills();
    }

    private void AddSkills()
    {
        if (BattleManager.instance.currentStageType == StageType.DAILY_RAID)
        {
            var codes = ContentsDataContainer.instance.dailyRaidSkillCodes;
            foreach (var code in codes)
            {
                if (code != "")
                {
                    SkillBattleData skill = BattleManager.instance.battleDataContainer.availableSkillPool[code];
                    chara.UpdateSkill(skill);
                    BattleManager.instance.battleDataContainer.availableSkillPool[code].grade += 1;
                }
            }
        }
        else if (BattleManager.instance.battleMode == BattleMode.eIdleMode)
        {
            List<string> codes = new(){"1140001", "1140002", "1140003", "1140004", "1140005"};

            foreach (var code in codes)
            {
                if (code != "")
                {
                    SkillBattleData skill = BattleManager.instance.battleDataContainer.availableSkillPool[code];
                    chara.UpdateSkill(skill);
                    BattleManager.instance.battleDataContainer.availableSkillPool[code].grade += 1;
                }
            }
        }
        else
        {
            SkillBattleData skill = BattleManager.instance.battleDataContainer.availableSkillPool["1140001"];
            chara.UpdateSkill(skill);
            BattleManager.instance.battleDataContainer.availableSkillPool["1140001"].grade += 1;
        }
    }
}