using System;
using System.Collections.Generic;
using UnityEngine;

[Serializable]
public class SkillValueTableSO : ScriptableObject
{
    public string skillCode;
    [SerializeField]
    public SkillDamageTable skillDamageTable;
}