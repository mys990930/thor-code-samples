using System;
using UnityEngine;

public abstract class PassiveSkill: Skill
{

    protected override void OnInit()
    {
        ApplySkillEffect(battleData.grade);
    }
    public override void LevelUp()
    {
        if (battleData.grade < 4)
        {
            battleData.grade++;
        }
        else throw new ArgumentOutOfRangeException();

        ApplySkillEffect(battleData.grade);
    }

    protected abstract void ApplySkillEffect(int grade);
}