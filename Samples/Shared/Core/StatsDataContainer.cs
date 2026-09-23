using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class StatsDataContainer : MonoBehaviour, IGameInit
{
    public static StatsDataContainer instance;

    public CharaStat charaStat;

    public BaseStat baseStat;
    public SpecialStat specialStat;
    public AwakeningStat awakeningStat;
    [SerializeField]
    public ArtifactStat artifactStat;

    public float MAXHP, HEAL, DEF, AS, SPD, DMG, RANGE;

    private void Awake()
    {
        if (instance == null) instance = this;
        else Destroy(gameObject);
    }
    public IEnumerator Init()
    {
        RequestResult loadResult = RequestResult.NETWORK_ERROR;
        yield return StartCoroutine(NetworkManager.instance.statDAO.GetAllStatData((result, data) =>
        {
            loadResult = result;
            StaticDataInitGuard.ApplyRequiredData(nameof(StatsDataContainer), result, () =>
            {
                baseStat = new BaseStat(
                    data.baseStatModel.dmgLv,
                    data.baseStatModel.maxHpLv,
                    data.baseStatModel.healLv,
                    data.baseStatModel.defLv
                    );
                specialStat = new SpecialStat(
                    data.specialStatModel.xpGainLv,
                    data.specialStatModel.magneticLv,
                    data.specialStatModel.boxSpawnRateLv,
                    data.specialStatModel.counterAttackLv
                    );
                awakeningStat = new AwakeningStat(
                    data.awakeningStatModel.awakeningLv
                    );
                artifactStat = new ArtifactStat(
                    data.artifactStatModel.ART_1,
                    data.artifactStatModel.ART_2,
                    data.artifactStatModel.ART_3,
                    data.artifactStatModel.ART_4
                    );
            });
        }));
        if (!StaticDataInitGuard.RequireSuccess(nameof(StatsDataContainer), loadResult)) yield break;

        CharaStat nextCharaStat = new CharaStat();
        charaStat?.Dispose();
        charaStat = nextCharaStat;
    }

    private void OnDestroy()
    {
        charaStat?.Dispose();
        charaStat = null;
        if (instance == this) instance = null;
    }
}