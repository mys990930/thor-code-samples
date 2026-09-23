using System;
using System.Collections.Generic; 

[Serializable]
public class SkillAssetData
{
    public SkillType skillType;
    public string code;
    public string name;
    public string combinationTargetCode;
    public string combinationSkillCode;

    public SkillValueTableSO damageTableSO;
    public List<string> battleViewDescriptionTexts;
    public List<string> uiViewDescriptionTexts;
    public string iconCode;
}