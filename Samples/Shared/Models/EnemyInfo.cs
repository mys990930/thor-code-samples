using System;

[Serializable]
public struct EnemyInfo
{
    public string enemyCode;
    public string monsterName;
    public int HP;
    public int EXP;
    public int SPD;
    public int ATK;

    public EnemyInfo(string enemyCode, string monsterName, int HP, int EXP, int SPD, int ATK)
    {
        this.enemyCode = enemyCode;
        this.monsterName = monsterName;
        this.HP = HP;
        this.EXP = EXP;
        this.SPD = SPD;
        this.ATK = ATK;
    }
}