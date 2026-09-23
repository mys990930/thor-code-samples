using System;
using System.Collections.Generic;
using UnityEngine;

public class EventManager : MonoBehaviour//, IGameInit
{
    public static EventManager instance;

    public delegate void OnBattlePrepare();
    public OnBattlePrepare onBattlePrepare = new OnBattlePrepare(VoidDelegate);
    public delegate void OnBattleStart();
    public OnBattleStart onBattleStart = new OnBattleStart(VoidDelegate);
    public delegate void OnBattleEnd();
    public OnBattleEnd onBattleEnd = new OnBattleEnd(VoidDelegate);

    public delegate void OnCharaLookChange();
    public OnCharaLookChange onCharaLookChange = new OnCharaLookChange(VoidDelegate);
    public delegate void OnCharaStatChange();
    /// <summary>
    /// 스탯 DataContainer 업데이트 이후 호출해야 함.
    /// </summary>
    public OnCharaStatChange onCharaStatChange = new OnCharaStatChange(CharaStatChange);
    public delegate void OnPowerChange();
    public OnPowerChange onPowerChange = new OnPowerChange(VoidDelegate);
    public delegate void OnRankUp(int value);
    public OnRankUp onRankUp = new OnRankUp(VoidIntDelegate);

    public Dictionary<QuestConditionType, Action<int>> questProgress = new Dictionary<QuestConditionType, Action<int>>();
    
    public Dictionary<NotifyType, OnValueChanged<bool>> notifyButtons = new Dictionary<NotifyType, OnValueChanged<bool>>();

    private void Awake()
    {
        if (instance == null) instance = this;
        else Destroy(gameObject);

        InitEventDictionary();
    }

    private static void VoidDelegate()
    {
        return;
    }

    private static void VoidIntDelegate(int value)
    {
        return;
    }

    /// <summary>
    /// CharaStatChange() 호출 시 자동으로 파워 계산해서 OnPowerChange까지 호출되도록
    /// </summary>
    private static void CharaStatChange()
    {
        ClientDataContainer.instance.CalculatePower();
    }

    public void InitEventDictionary()
    {
        for (int i = 0; i < (int)QuestConditionType.Max; i++)
        {
            questProgress.Add((QuestConditionType)i, null);
        }
        for(int i = 0; i < (int)NotifyType.Max; i++)
        {
            notifyButtons.Add((NotifyType)i, new OnValueChanged<bool>());
        }
    }
}