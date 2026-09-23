using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class SkillsDataContainer : MonoBehaviour, IGameInit
{
    public static SkillsDataContainer instance;

    //public Dictionary<string, SkillLvData> charaSkillLevels;
    public SkillStat skillData;

    private void Awake()
    {
        if (instance == null) instance = this;
        else Destroy(gameObject);
    }

    public IEnumerator Init()
    {
        SkillLvData[] tempActiveArray = null;
        SkillLvData[] tempPassiveArray = null;
        SkillLvData[] tempCombineArray = null;

        RequestResult loadResult = RequestResult.NETWORK_ERROR;
        yield return StartCoroutine(NetworkManager.instance.skillDAO.GetAllSkillData((result, data) =>
        {
            loadResult = result;
            StaticDataInitGuard.ApplyRequiredData(nameof(SkillsDataContainer), result, () =>
            {
                tempActiveArray = new SkillLvData[data.activeSkillModels.array.Length];
                tempPassiveArray = new SkillLvData[data.passiveSkillModels.array.Length];
                tempCombineArray = new SkillLvData[data.combiSkillModels.array.Length];
                for (int i = 0; i < tempActiveArray.Length; i++)
                {
                    tempActiveArray[i] = new SkillLvData(data.activeSkillModels.array[i].code, data.activeSkillModels.array[i].lv);
                }
                for (int i = 0; i < tempPassiveArray.Length; i++)
                {
                    tempPassiveArray[i] = new SkillLvData(data.passiveSkillModels.array[i].code, data.passiveSkillModels.array[i].lv);
                }
                for (int i = 0; i < tempCombineArray.Length; i++)
                {
                    tempCombineArray[i] = new SkillLvData(data.combiSkillModels.array[i].code, data.combiSkillModels.array[i].lv);
                }
            });
        }));
        if (!StaticDataInitGuard.RequireSuccess(nameof(SkillsDataContainer), loadResult)) yield break;

        skillData = new SkillStat(tempActiveArray, tempPassiveArray, tempCombineArray);
    }
}