using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

public abstract class SkillProjectile: MonoBehaviour, BattlePauseListener
{
    public ActiveSkill skill;

    protected Transform tr;

    protected float projectileDuration = 0.5f;
    protected bool shouldDestroy = false;
    protected bool isPaused = false;

    protected Camera cam;

    protected void Awake()
    {
        RegisterListener();
    }

    public void OnEnable()
    {
        cam = Camera.main;

        tr = GetComponent<Transform>();
        skill = tr.parent.GetComponent<ActiveSkill>();

        OnProjectileInit();
        //StartCoroutine(CheckProjectileLifetime());
    }

    public void RegisterListener()
    {
        BattleEventManager.instance.onBattlePause += OnBattlePause;
        BattleEventManager.instance.onBattleResume += OnBattleResume;
    }

    public void DeregisterListener()
    {
        BattleEventManager.instance.onBattlePause -= OnBattlePause;
        BattleEventManager.instance.onBattleResume -= OnBattleResume;
    }

    private IEnumerator CheckProjectileLifetime()
    {
        yield return new WaitForSeconds(projectileDuration);
        StartCoroutine(Destroy());
    }

    protected IEnumerator Destroy()
    {
        yield return null;
        Destroy(this);
    }

    public abstract void OnBattlePause();

    public abstract void OnBattleResume();

    protected Transform FindRandomTarget()
    {
        if (BattleManager.instance.liveEnemyDict.Count == 0) return BattleManager.instance.GetRandomTransform();

        Transform target;
        int cnt = 0;
        while (cnt < 5)
        {
            int randIdx = Random.Range(0, BattleManager.instance.liveEnemyDict.Count);
            target = BattleManager.instance.liveEnemyDict.ElementAt(randIdx).Value.transform;
            if (IsInViewport(target))
            {
                return target;
            }
            cnt++;
        }
        return BattleManager.instance.GetRandomTransform();
    }

    private void OnTriggerEnter2D(Collider2D col)
    {
        if (col.gameObject.CompareTag("Enemy") || col.gameObject.CompareTag("Treasure"))
        {
            OnObjHit(col.GetComponent<IHittable>());
        }
    }

    protected abstract void OnProjectileInit();

    protected abstract void OnObjHit(IHittable hitObj);

    protected bool IsInViewport(Transform target)
    {
        Vector2 targetViewportPos = cam.WorldToViewportPoint(target.position);
        if (0 <= targetViewportPos.x && targetViewportPos.x <= 1 && 0 <= targetViewportPos.y && targetViewportPos.y <= 1)
        {
            return true;
        }
        return false;
    }

    protected void OnDestroy()
    {
        DeregisterListener();
    }
}