using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using System.Runtime.InteropServices.WindowsRuntime;

public class SkillMapper
{
    public string SerializeUpgradeSkillData(UpgradeSkillData data)
    {
        var json = new JObject
        {
            { "type", (int)data.type },
            { "idx", data.idx },
            { "lv", data.selectSkillLv },
            { "combine_lv", data.currentCombineLv },
            { "used_book", data.usedMoney.ToString() },
        };
        return json.ToString();
    }

    public string SerializeNewSkillData(NewlyAcquiredSkillData data)
    {
        var json = new JObject
        {
            { "idx", data.idx },
            { "lv", data.selectSkillLv },
            { "combi_lv", data.currentCombineLv }
        };

        return json.ToString();
    }
    public IntegratedSkillModel MapIntegratedSkill(string json)
    {
        JObject obj = JObject.Parse(json);
        IntegratedSkillModel model = new IntegratedSkillModel();

        SkillModel activeSkillModels;
        SkillModel passiveSkillModels;
        SkillModel combiSkillModels;

        JArray activeSkills = JArray.Parse(obj["actives"].ToString());
        JArray passiveSkills = JArray.Parse(obj["passives"].ToString());
        JArray combiSkills = JArray.Parse(obj["combis"].ToString());

        SkillUnitModel[] activeArr = new SkillUnitModel[activeSkills.Count];
        SkillUnitModel[] passiveArr = new SkillUnitModel[passiveSkills.Count];
        SkillUnitModel[] combiArr = new SkillUnitModel[combiSkills.Count];
        for (int i = 0; i < activeArr.Length; i++)
        {
            activeArr[i] = JsonConvert.DeserializeObject<SkillUnitModel>(activeSkills[i].ToString());
        }
        for (int i = 0; i < passiveArr.Length; i++)
        {
            passiveArr[i] = JsonConvert.DeserializeObject<SkillUnitModel>(passiveSkills[i].ToString());
        }
        for (int i = 0; i < combiArr.Length; i++)
        {
            combiArr[i] = JsonConvert.DeserializeObject<SkillUnitModel>(combiSkills[i].ToString());
        }

        activeSkillModels = new SkillModel(activeArr);
        passiveSkillModels = new SkillModel(passiveArr);
        combiSkillModels = new SkillModel(combiArr);
        model.activeSkillModels = activeSkillModels;
        model.passiveSkillModels = passiveSkillModels;
        model.combiSkillModels = combiSkillModels;

        return model;
    }

    public DuoSkillUnitModel MapDuoSkillModel(SkillType type, string json)
    {
        JObject obj = JObject.Parse(json);
        DuoSkillUnitModel model = new DuoSkillUnitModel(
            type,
            (int)obj["idx"],
            (int)obj["skill_lv"],
            (int)obj["combi_lv"]) ;

        return model;
    }

    public big GetCurrentMoney(string json)
    {
        JObject obj = JObject.Parse(json);
        return obj["money"].ToString();
    }
}