using System;
using System.Collections;
using UnityEngine;

public abstract class NormalSkill: ActiveSkill, BattlePauseListener
{

    protected override IEnumerator SkillAction()
    {
        switch (battleData.grade)
        {
            case 1:
                StartCoroutine(GradeIAction()); 
                break;
            case 2:
                StopAllCoroutines();
                StartCoroutine(GradeIIAction()); 
                break;
            case 3:
                StartCoroutine(GradeIIIAction()); 
                break;
            case 4:
                StartCoroutine(GradeIVAction()); 
                break;
            default:
                Debug.LogError("Invalid projectile number");
                break;
        }
        yield return null;
    }

    public override void LevelUp()
    {
        if (battleData.grade < 4)
        {
            battleData.grade++;
        }
        else throw new ArgumentOutOfRangeException();
        
        UpdateDMG();
        ResetSkillAction();
        isPaused = false;
        OnUpgrade();
    }

    protected void OnUpgrade()
    {
        StopAllCoroutines();
        StartCoroutine(SkillAction());
    }

    protected abstract IEnumerator GradeIAction();
    protected abstract IEnumerator GradeIIAction();
    protected abstract IEnumerator GradeIIIAction();
    protected abstract IEnumerator GradeIVAction();
}