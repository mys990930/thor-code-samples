using System;
using System.Collections.Generic;
using System.Diagnostics;

public class CharaStat: StatChangeListener, IDisposable
{
    public big DMG;
    public big MAXHP;
    public big HEAL;
    public big DEF;
    public big CRATE;
    public big CDMG;
    public big SPD;

    //----------------------이하 퍼센트 단위 (0.2 = 0.2% ...)
    public float GOLD_GAIN;
    public float XP_GAIN;
    public float MAGNET;
    public float BOX_RATE;

    private EventManager subscribedEventManager;
    private bool isDisposed;

    public void RegisterListener()
    {
        if (isDisposed) throw new ObjectDisposedException(nameof(CharaStat));

        EventManager publisher = EventManager.instance;
        if (ReferenceEquals(subscribedEventManager, publisher)) return;

        DeregisterListener();
        if (publisher == null) return;

        subscribedEventManager = publisher;
        subscribedEventManager.onCharaStatChange += OnStatChange;
    }

    public void DeregisterListener()
    {
        if (ReferenceEquals(subscribedEventManager, null)) return;

        subscribedEventManager.onCharaStatChange -= OnStatChange;
        subscribedEventManager = null;
    }

    public void Dispose()
    {
        if (isDisposed) return;

        DeregisterListener();
        isDisposed = true;
    }

    public CharaStat()
    {
        SPD = 1.1f;
        OnStatChange();

        //Dictionary<BaseStatType, int> statLvDict = StatsDataContainer.instance.baseStat.stat;
        //DMG = ClientStatsDataContainer.instance.baseStatDatas[BaseStatType.DMG].table[statLvDict[BaseStatType.DMG]].value;
        //MAXHP = ClientStatsDataContainer.instance.baseStatDatas[BaseStatType.MAXHP].table[statLvDict[BaseStatType.MAXHP]].value;
        //HEAL = ClientStatsDataContainer.instance.baseStatDatas[BaseStatType.HEAL].table[statLvDict[BaseStatType.HEAL]].value;
        //DEF = ClientStatsDataContainer.instance.baseStatDatas[BaseStatType.DEF].table[statLvDict[BaseStatType.DEF]].value;

        //SPD = 1;

        //Dictionary<BaseStatType, big> dict = ClientItemsDataContainer.instance.GetStatsFromItems();
        //foreach(var temp in dict)
        //{
        //    //GameManager.instance.Log((temp.Key, temp.Value));
        //}
        //DMG += dict[BaseStatType.DMG];
        //MAXHP += dict[BaseStatType.MAXHP];
        //HEAL+= dict[BaseStatType.HEAL];
        //DEF += dict[BaseStatType.DEF];
        //CRATE = dict[BaseStatType.CRATE] * 0.01f;
        //CDMG = "0.5E0" + dict[BaseStatType.CDMG];

        //Dictionary<BaseStatType, big> dict2 = ClientSkillsDataContainer.instance.GetStatsFromSkills();
        //DMG *= (1 + dict2[BaseStatType.DMG]);
        //CDMG += dict2[BaseStatType.CDMG];

        RegisterListener();
    }

    public void OnStatChange()
    {
        Dictionary<BaseStatType, int> baseLvDict = StatsDataContainer.instance.baseStat.stat;
        Dictionary<SpecialStatType, int> specialLvDict = StatsDataContainer.instance.specialStat.stat;
        Dictionary<BaseStatType, big> dict = ClientItemsDataContainer.instance.GetStatsFromItems();

        DMG = ClientStatsDataContainer.instance.baseStatDatas[BaseStatType.DMG].table[baseLvDict[BaseStatType.DMG]].value + dict[BaseStatType.DMG];
        DEF = ClientStatsDataContainer.instance.baseStatDatas[BaseStatType.DEF].table[baseLvDict[BaseStatType.DEF]].value + dict[BaseStatType.DEF];
        MAXHP = ClientStatsDataContainer.instance.baseStatDatas[BaseStatType.MAXHP].table[baseLvDict[BaseStatType.MAXHP]].value + dict[BaseStatType.MAXHP];
        HEAL = ClientStatsDataContainer.instance.baseStatDatas[BaseStatType.HEAL].table[baseLvDict[BaseStatType.HEAL]].value + dict[BaseStatType.HEAL];
        CRATE = dict[BaseStatType.CRATE] * 0.01f;
        CDMG = 0.5f + dict[BaseStatType.CDMG];

        GOLD_GAIN = (float)ClientStatsDataContainer.instance.specialStatDatas[SpecialStatType.GOLD_GAIN].table[specialLvDict[SpecialStatType.GOLD_GAIN]].value;
        XP_GAIN = (float)ClientStatsDataContainer.instance.specialStatDatas[SpecialStatType.XP_GAIN].table[specialLvDict[SpecialStatType.XP_GAIN]].value;
        MAGNET = (float)ClientStatsDataContainer.instance.specialStatDatas[SpecialStatType.MAGNET].table[specialLvDict[SpecialStatType.MAGNET]].value;
        BOX_RATE = (float)ClientStatsDataContainer.instance.specialStatDatas[SpecialStatType.BOX_RATE].table[specialLvDict[SpecialStatType.BOX_RATE]].value;

        float FDMG, FDEF, FMAXHP, FHEAL; //%값들

        SerializableDictionary<ArtifactStatType, int> fdict = StatsDataContainer.instance.artifactStat.statLvs;

        FDMG = (float)ClientStatsDataContainer.instance.artifactStatDatas[ArtifactStatType.moru].table[fdict[ArtifactStatType.moru]].value;
        FDEF = (float)ClientStatsDataContainer.instance.artifactStatDatas[ArtifactStatType.goul].table[fdict[ArtifactStatType.goul]].value;
        FMAXHP = (float)ClientStatsDataContainer.instance.artifactStatDatas[ArtifactStatType.baqui].table[fdict[ArtifactStatType.baqui]].value;
        FHEAL = (float)ClientStatsDataContainer.instance.artifactStatDatas[ArtifactStatType.sigie].table[fdict[ArtifactStatType.sigie]].value;

        Dictionary<BaseStatType, big> dict2 = ClientSkillsDataContainer.instance.GetStatsFromSkills();
        FDMG += (float)dict2[BaseStatType.DMG];
        CDMG += dict2[BaseStatType.CDMG] * 0.01f;

        DMG *= 1 + FDMG *0.01f;
        DEF *= 1 + FDEF * 0.01f;
        MAXHP *= 1 + FMAXHP * 0.01f;
        HEAL *= 1 + FHEAL * 0.01f;

        DMG *= (1 + 0.5f * StatsDataContainer.instance.awakeningStat.lv); //각성 스탯 계수 반영
    }
}