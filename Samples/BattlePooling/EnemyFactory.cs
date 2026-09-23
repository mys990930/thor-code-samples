using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Pool;
using Random = UnityEngine.Random;

public class EnemyFactory: MonoBehaviour
{
    private Transform charaTr;
    private Dictionary<string, IPool> enemyPoolDict;

    private void OnEnable()
    {
        enemyPoolDict = ObjectPoolManager.instance.enemyObjPools;
        charaTr = BattleManager.instance.charaObj.GetComponent<Transform>();
    }
    //duration은 이벤트의 총 길이, 실제로 아래의 모든 함수들은 매 0.5초마다 호출된다.
    public void Summon(string enemyCode, int enemyNum)
    {
        for (int i = 0; i < enemyNum; i++)
        {
            if (!enemyPoolDict[enemyCode].TryGet(out GameObject obj)) return;
            Enemy enemy = obj.GetComponent<Enemy>();
            float angle = Random.Range(0, 6.28f);
            float mag = Random.Range(5.5f, 6);
            Vector2 summonPos = new Vector2(Mathf.Cos(angle) * mag, Mathf.Sin(angle) * mag)
                + (Vector2)charaTr.position;
            enemy.Init(enemyCode, summonPos);
        }
    }

    public void GroupSummon(string enemyCode, int enemyNum)
    {
        float angle = Random.Range(0, 6.28f);
        for (int i = 0; i < enemyNum; i++)
        {
            float mag = Random.Range(5.5f, 6);
            angle += Random.Range(-0.5f, 0.5f);
            if (!enemyPoolDict[enemyCode].TryGet(out GameObject obj)) return;
            Enemy enemy = obj.GetComponent<Enemy>();
            Vector2 summonPos = new Vector2(Mathf.Cos(angle) * mag, Mathf.Sin(angle) * mag)
                + (Vector2)charaTr.position;
            enemy.Init(enemyCode, summonPos);
        }
    }

    public void MixSummon(Queue<string> enemyCodes, int enemyNum)
    {
        float angle = Random.Range(0, 6.28f);
        for (int i = 0; i < enemyNum; i++)
        {
            float mag = Random.Range(5.5f, 6);
            foreach (var enemyCode in enemyCodes)
            {
                if (!enemyPoolDict[enemyCode].TryGet(out GameObject obj)) return;
                Enemy enemy = obj.GetComponent<Enemy>();
                Vector2 summonPos = new Vector2(Mathf.Cos(angle) * mag, Mathf.Sin(angle) * mag)
                    + (Vector2)charaTr.position;
                enemy.Init(enemyCode, summonPos);
            }
        }
    }

    public void SummonElite(string enemyCode, int enemyNum)
    {
        for (int i = 0; i < enemyNum; i++)
        {
            if (!enemyPoolDict[enemyCode].TryGet(out GameObject obj)) return;
            Enemy enemy = obj.GetComponent<Enemy>();
            float angle = Random.Range(0, 6.28f);
            float mag = Random.Range(5.5f, 6);
            Vector2 summonPos = new Vector2(Mathf.Cos(angle) * mag, Mathf.Sin(angle) * mag)
                + (Vector2)charaTr.position;
            enemy.Init(enemyCode, summonPos);
        }
    }

    public void SummonMiddleBoss(string enemyCode)
    {
        GameObject bossObj = ObjectPoolManager.instance.bossObjects[enemyCode];

        float angle = Random.Range(0, 6.28f);
        Vector2 summonPos = new Vector2(Mathf.Cos(angle) * 2, Mathf.Sin(angle) * 2)
            + (Vector2)charaTr.position;

        GameObject obj = Instantiate(bossObj, summonPos, Quaternion.identity);
        obj.transform.SetParent(BattleManager.instance.currentBattle.transform);
        Boss boss = obj.GetComponent<Boss>();
        boss.Init(enemyCode, summonPos, false);
        SummonObstacles(summonPos);
    }

    public void SummonFinalBoss(string enemyCode)
    {
        GameObject bossObj = ObjectPoolManager.instance.bossObjects[enemyCode];

        float angle = Random.Range(0, 6.28f);
        Vector2 summonPos = new Vector2(Mathf.Cos(angle) * 2, Mathf.Sin(angle) * 2)
            + (Vector2)charaTr.position;

        GameObject obj = Instantiate(bossObj, summonPos, Quaternion.identity);
        obj.transform.SetParent(BattleManager.instance.currentBattle.transform);
        Boss boss = obj.GetComponent<Boss>();
        boss.Init(enemyCode, summonPos, true);
        SummonObstacles(summonPos);
    }

    public GameObject SummonRaidBoss(string enemyCode)
    {
        GameObject bossObj = ObjectPoolManager.instance.bossObjects[enemyCode];

        float angle = Random.Range(0, 6.28f);
        Vector2 summonPos = new Vector2(Mathf.Cos(angle) * 2, Mathf.Sin(angle) * 2)
            + (Vector2)charaTr.position;

        GameObject obj = Instantiate(bossObj, summonPos, Quaternion.identity);
        RaidBoss boss = obj.GetComponent<RaidBoss>();
        boss.Init(enemyCode, summonPos, true);
        SummonObstacles(summonPos);
        return boss.gameObject;
    }

    public void SummonObstacles(Vector2 targetPos)
    {
        int obstacleCnt = 80;
        float angle = 0;
        float angleDiff = 6.28f / obstacleCnt;
        for (int i = 0; i < obstacleCnt; i++)
        {
            if (!ObjectPoolManager.instance.obstacleObjPool.TryGet(out GameObject obj)) return;
            var obstacle = obj.GetComponent<Obstacle>();

            angle += angleDiff;
            Vector2 summonPos = new Vector2(Mathf.Cos(angle) * 5, Mathf.Sin(angle) * 5)+ targetPos;
            obstacle.Init(summonPos);
        }
    }
}
