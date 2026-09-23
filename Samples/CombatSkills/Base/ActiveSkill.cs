using System.Collections;
using UnityEngine;

public abstract class ActiveSkill: Skill, BattlePauseListener, StatChangeListener
{
    public big DMG;
    protected bool isPaused = false;

    protected void Awake()
    {
        RegisterListener();
    }

    protected override void OnInit()
    {
        OnSkillInit();
        UpdateDMG();
        StartCoroutine(SkillAction());
    }

    public void RegisterListener()
    {
        BattleEventManager.instance.onBattlePause += OnBattlePause;
        BattleEventManager.instance.onBattleResume += OnBattleResume;
        EventManager.instance.onCharaStatChange += OnStatChange;
    }

    public void DeregisterListener()
    {
        BattleEventManager.instance.onBattlePause -= OnBattlePause;
        BattleEventManager.instance.onBattleResume -= OnBattleResume;
        EventManager.instance.onCharaStatChange -= OnStatChange;
    }

    public void OnStatChange()
    {
        UpdateDMG();
    }

    protected void UpdateDMG()
    {
        skillLv = SkillsDataContainer.instance.skillData.skillLevels[skillCode].lv;
        big percentDmg = damageTable.GetDmg(skillLv, battleData.grade);
        DMG = chara.stat.DMG * (percentDmg * 0.01f);
    }

    public void OnBattlePause()
    {
        isPaused = true;
    }

    public void OnBattleResume()
    {
        isPaused = false;
    }

    protected abstract IEnumerator SkillAction();
    protected abstract void ResetSkillAction();
    protected abstract void OnSkillInit();

    protected void OnDestroy()
    {
        DeregisterListener();
    }
}