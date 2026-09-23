using System.Collections;
using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.Pool;

public class EXP : MonoBehaviour, IObjectPoolable
{
    public int idx;

    private Transform tr;

    public IPool pool;

    public int exp = 10;

    private float range = 1;

    private Color c10 = new Color(1f, 1f, 1f, 1f);
    private Color c50 = new Color(1f, 1f, 0, 1f);
    private Color c100 = new Color(0f, 1f, 1f, 1f);

    private void OnEnable()
    {
    }

    private void OnDisable()
    {
        BattleManager.instance.liveEXPs.Remove(idx);
    }

    public void Init(Vector2 initPos, int expValue)
    {
        tr = GetComponent<Transform>();
        tr.position = initPos;
        this.exp = expValue;
        idx = BattleManager.instance.summonedObjNum++;
        BattleManager.instance.liveEXPs.Add(idx, this);
        range = 1 + StatsDataContainer.instance.charaStat.MAGNET * 0.01f;

        var sr = GetComponent<SpriteRenderer>();
        if (expValue < 10) sr.color = c10;
        else if (expValue < 30) sr.color = c50;
        else sr.color = c100;

        StartCoroutine(TrackCharaMovement());
    }


    public void InitializePool(IPool pool)
    {
        this.pool = pool;
    }


    public void MoveToChara(int speed)
    {
        if (gameObject.activeSelf) StartCoroutine(MoveToCharaCor(speed));
    }

    private IEnumerator TrackCharaMovement()
    {
        WaitForFixedUpdate wait = new WaitForFixedUpdate();
        while (true)
        {
            if (Vector2.Distance(transform.position, BattleManager.instance.charaObj.transform.position) < 0.5f * range)
            {
                break;
            }
            yield return wait;
            yield return wait;
            yield return wait;
        }
        MoveToChara(4);
    }

    public IEnumerator MoveToCharaCor(float attractSpeed, float bounceDist = 0.6f, float bounceTime = 0.3f, float snapDist = 0.1f)
    {
        Transform tr = this.transform;
        Vector2 charPos = BattleManager.instance.charaObj.transform.position;

        // 1) 반대 방향으로 '정해진 거리'만큼 '정해진 시간'에 정확히 이동
        Vector2 awayDir = (Vector2)(tr.position - (Vector3)charPos);
        if (awayDir.sqrMagnitude < 1e-6f) awayDir = Vector2.up; // 동일 위치 방지
        awayDir.Normalize();
        Vector2 bounceTarget = (Vector2)tr.position + awayDir * bounceDist;

        float t = 0f;
        while (t < bounceTime)
        {
            t += Time.deltaTime;
            float u = Mathf.Clamp01(t / bounceTime); // 0→1
            tr.position = Vector2.LerpUnclamped((Vector2)tr.position, bounceTarget, u); // 시간 기반
            yield return BattleManager.instance.waitPause; // 또는 'yield return null;'
        }
        tr.position = bounceTarget;

        // 2) 캐릭터로 '등속 이동'
        while (true)
        {
            charPos = BattleManager.instance.charaObj.transform.position; // 이동 캐릭터 추적
            Vector2 curr = tr.position;
            Vector2 next = Vector2.MoveTowards(curr, charPos, attractSpeed * Time.deltaTime);
            tr.position = next;

            if (Vector2.SqrMagnitude((Vector2)next - charPos) <= snapDist * snapDist)
                break;

            yield return BattleManager.instance.waitPause; // 또는 null
        }

        // 3) 스냅 + 처리
        tr.position = BattleManager.instance.charaObj.transform.position;
        BattleManager.instance.chara.AddExperience(
            (int)(exp * BattleManager.instance.chara.battleStat.EXPEarnFactor)
        );
        pool.Release(this.gameObject);
    }


    public void Disable()
    {
        StopAllCoroutines();
    }
}