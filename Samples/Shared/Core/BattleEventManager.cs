using System.Collections;
using UnityEngine;

public class BattleEventManager: MonoBehaviour//, IGameInit
{
    public static BattleEventManager instance;

    public delegate void OnLevelUp();
    public OnLevelUp onLevelUp = new OnLevelUp(voidDelegate);
    public delegate void OnEXPGet(int exp);
    public OnEXPGet onEXPGet = new OnEXPGet(intDelegate);
    public delegate void OnBattlePause();
    public OnBattlePause onBattlePause = new OnBattlePause(voidDelegate);
    public delegate void OnBattleResume();
    public OnBattleResume onBattleResume = new OnBattleResume(voidDelegate);
    public delegate void OnEnemyKill();
    public OnEnemyKill onEnemyKill = new OnEnemyKill(voidDelegate);
    public delegate void OnBossKill(bool isFinalBoss);
    public OnBossKill onBossKill = new OnBossKill(boolDelegate);
    public delegate void OnDmgAdded();
    public OnDmgAdded onDmgAdded = new OnDmgAdded(voidDelegate);
    public delegate void OnGoldGet();
    public OnGoldGet onGoldGet = new OnGoldGet(voidDelegate);


    private void Awake()
    {
        if (instance == null) instance = this;
        else Destroy(gameObject);
    }

    public void PauseBattle()
    {
        onBattlePause();
    }

    public void ResumeBattle() 
    {
        onBattleResume(); 
    }

    private static void voidDelegate()
    {
        return;
    }

    private static void intDelegate(int i)
    {
        return;
    }

    private static void boolDelegate(bool b)
    {
        return;
    }
}