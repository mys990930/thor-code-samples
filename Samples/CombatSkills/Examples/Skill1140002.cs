using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Skill1140002 : NormalSkill
{
    private Transform tr;
    private List<SkillProj1140002> projs = new List<SkillProj1140002>();

    protected override void SetSkillCode()
    {
        skillCode = "1140002";
    }

    protected override void OnSkillInit()
    {
        tr = GetComponent<Transform>();
        chara = BattleManager.instance.chara;
        tr.position = chara.transform.position;
        for (int i = 0; i < tr.childCount; i++)
        {
            projs.Add(tr.GetChild(i).GetComponent<SkillProj1140002>());
        }
        StopAllCoroutines();
    }

    protected override void ResetSkillAction()
    {
        foreach(SkillProj1140002 proj in projs)
        {
            proj.gameObject.SetActive(false);
        }
    }

    protected override IEnumerator GradeIAction()
    {
        projs[0].gameObject.SetActive(true);
        yield return null;
    }

    protected override IEnumerator GradeIIAction()
    {
        projs[0].gameObject.SetActive(true);
        projs[1].gameObject.SetActive(true);
        yield return null;
    }

    protected override IEnumerator GradeIIIAction()
    {
        projs[0].SetSpeed(1.5f);
        projs[1].SetSpeed(1.5f);
        projs[0].gameObject.SetActive(true);
        projs[1].gameObject.SetActive(true);
        yield return null;
    }

    protected override IEnumerator GradeIVAction()
    {
        projs[2].SetSpeed(1.5f);
        projs[0].gameObject.SetActive(true);
        projs[1].gameObject.SetActive(true);
        projs[2].gameObject.SetActive(true);
        yield return null;
    }
}