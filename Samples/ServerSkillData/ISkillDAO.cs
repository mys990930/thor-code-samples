using System;
using System.Collections;
using System.Collections.Generic;

public interface ISkillDAO
{
    public IEnumerator GetAllSkillData(Action<RequestResult, IntegratedSkillModel> callback);

    public IEnumerator UpgradeSkill(UpgradeSkillData data, Action<RequestResult, DuoSkillUnitModel> callback);
    public IEnumerator GetNewSkill(int idx, Action callback);
}
