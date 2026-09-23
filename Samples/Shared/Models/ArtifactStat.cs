using System;
using System.Collections.Generic;

[Serializable]
public class ArtifactStat
{
    private int watchLv;
    private int computerLv;
    private int vacuumLv;
    private int busLv;

    public SerializableDictionary<ArtifactStatType, int> statLvs;

    public ArtifactStat(int watchLv, int computerLv, int vacuumLv, int busLv)
    {
        this.watchLv = watchLv;
        this.computerLv = computerLv;
        this.vacuumLv = vacuumLv;
        this.busLv = busLv;

        statLvs = new SerializableDictionary<ArtifactStatType, int>() {
            {ArtifactStatType.moru, this.watchLv },
            {ArtifactStatType.goul, this.computerLv },
            {ArtifactStatType.baqui, this.vacuumLv },
            {ArtifactStatType.sigie, this.busLv }
            };
    }
}