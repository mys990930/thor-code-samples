using JetBrains.Annotations;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;

#region ?ㅽ궗 愿??紐⑤뜽

[Serializable]
public struct IntegratedSkillModel
{
    public SkillModel activeSkillModels;
    public SkillModel passiveSkillModels;
    public SkillModel combiSkillModels;

    public IntegratedSkillModel(SkillModel actives, SkillModel passives, SkillModel combis)
    {
        activeSkillModels = actives;
        passiveSkillModels = passives;
        combiSkillModels = combis;
    }
}

[Serializable]
public struct SkillUnitModel
{
    public string code;
    public int idx;
    public int lv;

    public SkillUnitModel(string code, int idx, int lv)
    {
        this.code = code;
        this.idx = idx;
        this.lv = lv;
    }
}

[Serializable]
public struct SkillModel
{
    public SkillUnitModel[] array;

    public SkillModel(SkillUnitModel[] array)
    {
        this.array = array;
    }
}

[Serializable]
public struct DuoSkillUnitModel
{
    public SkillType type;
    public int idx;
    public int selectSkillLv;
    public int combineSkillLv;

    public DuoSkillUnitModel(SkillType type, int idx, int selectSkillLv, int combineSkillLv)
    {
        this.type = type;
        this.idx = idx;
        this.selectSkillLv = selectSkillLv;
        this.combineSkillLv = combineSkillLv;
    }
}
#endregion
