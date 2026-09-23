using Newtonsoft.Json.Linq;
using System;
using System.Collections;
using UnityEngine;

public class SkillDAO: MonoBehaviour, ISkillDAO
{
    private ServerRequester requester;
    private SkillMapper mapper;

    private void OnEnable()
    {
        mapper = new SkillMapper();
        requester = NetworkManager.instance.serverRequester;
    }

    public IEnumerator GetAllSkillData(Action<RequestResult, IntegratedSkillModel> callback)
    {
        string uri = "/skills/all";
        yield return StartCoroutine(requester.SendGetRequest(uri, mapper.MapIntegratedSkill, callback));
    }

    public IEnumerator UpgradeSkill(UpgradeSkillData data, Action<RequestResult, DuoSkillUnitModel> callback)
    {
        string uri = "/skills/skill";
        string body = mapper.SerializeUpgradeSkillData(data);
        yield return StartCoroutine(requester.SendPutRequest(uri, body, (RequestResult result, string responseBody) =>
        {
            if (result == RequestResult.SUCCESS)
            {
                var currentBook = mapper.GetCurrentMoney(responseBody);
                DataContainer.instance.money.wallet[MoneyType.skillBook].value = currentBook;

                var skillModel = mapper.MapDuoSkillModel(data.type, responseBody);
                callback(result, skillModel);
                return;
            }

            callback(result, default);
        }));
    }

    public IEnumerator GetNewSkill(int idx, Action callback)
    {
        string uri = "/skills/new";
        var json = new JObject
        {
            { "idx", idx },
        };
        string body = json.ToString();
        yield return StartCoroutine(requester.SendPostRequest(uri, body, (string responseBody) =>
        {
            callback();
        }));
    }
}
