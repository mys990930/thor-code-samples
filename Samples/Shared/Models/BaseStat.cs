
using System.Collections.Generic;

public class BaseStat
{
    private int hpLv;
    private int healLV;
    private int defLv;
    private int dmgLv;

    public Dictionary<BaseStatType, int> stat;

    public BaseStat(int dmgLv, int hpLv, int healLV, int defLv)
    {
        this.dmgLv = dmgLv;
        this.hpLv = hpLv;
        this.healLV = healLV;
        this.defLv = defLv;

        stat = new Dictionary<BaseStatType, int>() {
            { BaseStatType.DMG, this.dmgLv},
            { BaseStatType.MAXHP, this.hpLv},
            { BaseStatType.HEAL, this.healLV},
            { BaseStatType.DEF, this.defLv},
        };
    }
}