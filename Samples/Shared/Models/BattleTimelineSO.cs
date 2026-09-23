using System.Collections.Generic;
using UnityEngine;

public class BattleTimelineSO: ScriptableObject
{
    public string timelineCode;
    public List<SpawnEventData> spawnEvents;
}