using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ObjectPoolManager: MonoBehaviour, IGameInit, BattleEndListener
{
    public static ObjectPoolManager instance;

    public GameObject obstaclePrefab, expPrefab, dmgPrefab, treasurePrefab;

    public GameObject enemyPool, obstaclePool, expPool, dmgPool, treasurePool, moneyPool;
    private Vector2 initPos = new Vector2(500, 500);

    public IPool targetPool { get; private set; }

    public Dictionary<string, IPool> enemyObjPools = new Dictionary<string, IPool>();
    public Dictionary<string, GameObject> bossObjects = new Dictionary<string, GameObject>();
    public IPool obstacleObjPool { get; private set; }

    public IPool expObjPool { get; private set; }
    public IPool dmgObjPool { get; private set; }
    public IPool treasureObjPool { get; private set; }
    public Dictionary<MoneyType, IPool> moneyItemPools = new Dictionary<MoneyType, IPool>();

    private List<MoneyType> lastMoneyTypes = new List<MoneyType>();

    private void Awake()
    {
        if (instance == null) instance = this;
        else Destroy(gameObject);
    }

    public IEnumerator Init()
    {
        RegisterListener();
        InitBossPools();
        InitPools();
        yield return null;
    }

    public void RegisterListener()
    {
        EventManager.instance.onBattleEnd += OnBattleEnd;
    }

    public void DeregisterListener()
    {
        EventManager.instance.onBattleEnd -= OnBattleEnd;
    }

    private void InitPools()
    {
        Transform poolParent = new GameObject("ObjectPools").transform;

        obstaclePool = new GameObject("ObstaclePool");
        enemyPool = new GameObject("EnemyPool");
        expPool = new GameObject("ExpPool");
        dmgPool = new GameObject("DmgPool");
        treasurePool = new GameObject("TreasurePool");
        moneyPool = new GameObject("MoneyPool");

        obstacleObjPool = obstaclePool.AddComponent<ObjectPool>();
        obstacleObjPool.Init(obstaclePrefab, 85, 105);
        expObjPool = expPool.AddComponent<ObjectPool>();
        expObjPool.Init(expPrefab, 1000, 1000);
        dmgObjPool = dmgPool.AddComponent<ObjectPool>();
        dmgObjPool.Init(dmgPrefab, 200, 300);
        treasureObjPool = treasurePool.AddComponent<ObjectPool>();
        treasureObjPool.Init(treasurePrefab, 100, 150);

        obstaclePool.transform.SetParent(poolParent, false);
        enemyPool.transform.SetParent(poolParent, false);
        expPool.transform.SetParent(poolParent, false);
        dmgPool.transform.SetParent(poolParent, false);
        treasurePool.transform.SetParent(poolParent, false);
        moneyPool.transform.SetParent(poolParent, false);

    }

    public void InitBossPools()
    {
        List<string> bossCodeList = new List<string> { "1121001_go", "1121002_go", "1121101_go"};
        foreach (string code in bossCodeList)
        {
            GameObject targetObj = BattleAssetContainer.instance.monsterPrefabs[code];
            bossObjects.Add(code, targetObj);
        }
    }

    public void InitEnemyPools(List<string> poolTargetCodes)
    {
        foreach (string code in poolTargetCodes)
        {
            if (!enemyObjPools.ContainsKey(code) && !code.StartsWith("1121"))
            {
                GameObject targetObj = BattleAssetContainer.instance.monsterPrefabs[code];

                var poolRoot = new GameObject(code);
                poolRoot.transform.SetParent(enemyPool.transform, false);
                ObjectPool pool = poolRoot.AddComponent<ObjectPool>();
                pool.Init(targetObj, 150, 300);
                enemyObjPools.Add(code, pool);
            }
        }
    }

    public void InitMoneyPools(List<MoneyType> types)
    {
        foreach(MoneyType type in types)
        {
            if (moneyItemPools.ContainsKey(type)) continue;
            string code = ClientDataContainer.instance.moneyDataTable[(int)type].iconCode;
            GameObject moneyObj = ClientMainAssetContainer.instance.gameObjectDatas[Enum.GetName(typeof(MoneyType), type)];

            var poolRoot = new GameObject(type.ToString());
            poolRoot.transform.SetParent(moneyPool.transform, false);
            ObjectPool pool = poolRoot.AddComponent<ObjectPool>();
            pool.Init(moneyObj, 30, 50);
            moneyItemPools.Add(type, pool);
            lastMoneyTypes.Add(type);
        }

    }

    public void OnBattleEnd()
    {
        foreach(var enemyPool in enemyObjPools.Values)
        {
            enemyPool.Clear();
            Debug.LogWarning("오버헤드가 과할 경우 수정 필요");
            var pool = enemyPool as ObjectPool;
            Destroy(pool.gameObject);
        }
        enemyObjPools.Clear();
        BattleManager.instance.summonedObjNum = 0;
        BattleManager.instance.liveEnemyNum = 0;
        BattleManager.instance.liveEnemyDict = new Dictionary<int, Enemy>();
        foreach (var type in lastMoneyTypes)
        {
            var pool = moneyItemPools[type] as ObjectPool;
            pool.Clear();
            Destroy(pool.gameObject);
            moneyItemPools.Remove(type);
        }
        lastMoneyTypes.Clear();

        obstacleObjPool.Disable();
        expObjPool.Disable();
        dmgObjPool.Disable();
        treasureObjPool.Disable();

    }

    private void OnDestroy()
    {
        DeregisterListener();
    }
}
