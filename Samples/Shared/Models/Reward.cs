using System;

[Serializable]
public struct Reward
{
    public MoneyType moneyType;
    public big value;

    public Reward(MoneyType moneyType, big value)
    {
        this.moneyType = moneyType;
        this.value = value;
    }
}