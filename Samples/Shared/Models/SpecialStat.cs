using System.Collections.Generic;

public class SpecialStat
{
    private int xpGainAmountLv;
    private int magneticLv;
    private int boxSpawnRateLv;
    private int counterAttackLv;

    public Dictionary<SpecialStatType, int> stat;

    public SpecialStat(int xpGainAmountLv, int magneticLv, int boxSpawnRateLv, int counterAttackLv)
    {
        this.xpGainAmountLv = xpGainAmountLv;
        this.magneticLv = magneticLv;
        this.boxSpawnRateLv = boxSpawnRateLv;
        this.counterAttackLv = counterAttackLv;

        stat = new Dictionary<SpecialStatType, int>() {
            { SpecialStatType.XP_GAIN, this.xpGainAmountLv },
            { SpecialStatType.MAGNET, this.magneticLv },
            { SpecialStatType.BOX_RATE, this.boxSpawnRateLv},
            { SpecialStatType.GOLD_GAIN, this.counterAttackLv }
        };
    }
}