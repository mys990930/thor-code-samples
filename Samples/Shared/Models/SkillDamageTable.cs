using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UIElements;

/// <summary>
/// 스킬 데미지가 %값(int)으로 들어있는 테이블
/// </summary>
[Serializable]
public struct SkillDamageTable
{
    [SerializeField]
    public List<SkillRow> dmgs; //1lv~끝lv까지의 값이 들어감)

    public int GetDmg(int skillLv, int grade)
    {
        SkillRow row = dmgs[skillLv - 1];
        //Debug.Log("skill grade: " + (grade-1));
        return row.grades[grade-1];
    }

    public SkillRow GetDmgsPerGrade(int skillLv)
    {
        return dmgs[skillLv - 1];
    }

    public SkillDamageTable(List<SkillRow> dmgs)
    {
        this.dmgs = dmgs;
    }
}

[Serializable]
public struct SkillRow
{
    public int[] grades;

    public SkillRow(int[] args)
    {
        grades = args;
    }
}
