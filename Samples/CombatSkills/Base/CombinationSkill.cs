using System;
using System.Collections;
using UnityEngine;

public abstract class CombinationSkill : ActiveSkill, BattlePauseListener
{
    protected abstract override IEnumerator SkillAction();

    public override void LevelUp()
    {
        //unnecessary;
    }
}