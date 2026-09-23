using System.Collections;
using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// 기본공격 (망치로후리기)
/// </summary>
public class Skill1140001 : NormalSkill
{
    private Transform tr;
    private InputManager inputManager;

    private List<SkillProj1140001> projs = new List<SkillProj1140001>();
    private float animDelay = 0.2f;

    protected override void SetSkillCode()
    {
        skillCode = "1140001";
    }

    protected override void OnSkillInit()
    {
        tr = GetComponent<Transform>();
        chara = BattleManager.instance.chara;
        inputManager = InputManager.instance; 

        for (int i = 0; i < tr.childCount; i++)
        {
            projs.Add(tr.GetChild(i).GetComponent<SkillProj1140001>());
        }
        //StopAllCoroutines();
    }

    protected override void ResetSkillAction()
    {
        StopAllCoroutines();
        chara.animator.Reset();
        foreach (SkillProj1140001 p in projs)
        {
            if (p.gameObject.activeSelf)
            {
                p.StopAllCoroutines();
                p.col.enabled = false;
                p.particle.Stop();
            }
        }
    }

    protected override IEnumerator GradeIAction()
    {
        while (true)
        {
            if (!isPaused)
            {
                chara.animator.SetAttack();
                yield return BattleManager.instance.waitPause;
                yield return new WaitForSeconds(animDelay);
                bool lookingRight = inputManager.lookingRight;

                projs[0].GetComponent<ParticleSystemRenderer>().flip = lookingRight ? new Vector2(0, 0) : new Vector2(0, 1);

                float delayDelta = 1 / (1 - animDelay) * 0.1f;

                Vector2 atkTargetPos = new Vector2(
                    (lookingRight ? tr.position.x + 0.2f : tr.position.x - 0.5f), tr.position.y);
                ShowEffect(projs[0], atkTargetPos);

                for (int i = 0; i < 10; i++)
                {
                    yield return BattleManager.instance.waitPause;
                    yield return new WaitForSeconds(delayDelta);
                }
                HideEffect(projs[0]);
            }
            yield return null;
        }
    }

    protected override IEnumerator GradeIIAction()
    {
        while (true)
        {
            if (!isPaused)
            {
                chara.animator.SetAttack();
                yield return BattleManager.instance.waitPause;
                yield return new WaitForSeconds(animDelay);
                bool lookingRight = inputManager.lookingRight;

                projs[0].GetComponent<ParticleSystemRenderer>().flip = lookingRight ? new Vector2(0, 0) : new Vector2(0, 1);
                projs[1].GetComponent<ParticleSystemRenderer>().flip = lookingRight ? new Vector2(0, 1) : new Vector2(0, 0);

                float delayDelta = 1 / (1 - animDelay) * 0.1f;

                Vector2 atkTargetPos = new Vector2(
                    (lookingRight ? tr.position.x + 0.2f : tr.position.x - 0.5f), tr.position.y);
                ShowEffect(projs[0], atkTargetPos);

                for (int i = 0; i < 3; i++)
                {
                    yield return BattleManager.instance.waitPause;
                    yield return new WaitForSeconds(delayDelta);
                }

                Vector2 atkTargetPos2 = new Vector2(
                    (lookingRight ? tr.position.x - 0.5f : tr.position.x + 0.2f), tr.position.y);
                ShowEffect(projs[1], atkTargetPos2);

                for (int i = 0; i < 7; i++)
                {
                    yield return BattleManager.instance.waitPause;
                    yield return new WaitForSeconds(delayDelta);
                }
                HideEffect(projs[0]);
                HideEffect(projs[1]);
            }
            yield return null;
        }
    }

    protected override IEnumerator GradeIIIAction()
    {
        projs[0].transform.localScale = new Vector2(1.5f, 1.5f);
        projs[1].transform.localScale = new Vector2(1.5f, 1.5f);
        projs[2].transform.localScale = new Vector2(1.5f, 1.5f);
        while (true)
        {
            if (!isPaused)
            {
                chara.animator.SetAttack();
                yield return BattleManager.instance.waitPause;
                yield return new WaitForSeconds(animDelay);
                bool lookingRight = inputManager.lookingRight;

                projs[0].GetComponent<ParticleSystemRenderer>().flip = lookingRight ? new Vector2(0, 0) : new Vector2(0, 1);
                projs[1].GetComponent<ParticleSystemRenderer>().flip = lookingRight ? new Vector2(0, 1) : new Vector2(0, 0);

                float delayDelta = 1 / (1 - animDelay) * 0.1f;

                Vector2 atkTargetPos = new Vector2(
                    (lookingRight ? tr.position.x + 0.2f : tr.position.x - 0.5f), tr.position.y);
                ShowEffect(projs[0], atkTargetPos);

                for (int i = 0; i < 3; i++)
                {
                    yield return BattleManager.instance.waitPause;
                    yield return new WaitForSeconds(delayDelta);
                }

                Vector2 atkTargetPos2 = new Vector2(
                    (lookingRight ? tr.position.x - 0.5f : tr.position.x + 0.2f), tr.position.y);
                ShowEffect(projs[1], atkTargetPos2);

                for (int i = 0; i < 7; i++)
                {
                    yield return BattleManager.instance.waitPause;
                    yield return new WaitForSeconds(delayDelta);
                }
                HideEffect(projs[0]);
                HideEffect(projs[1]);
            }
            yield return null;
        }
    }

    protected override IEnumerator GradeIVAction()
    {
        while (true)
        {
            if (!isPaused)
            {
                chara.animator.SetAttack();
                yield return BattleManager.instance.waitPause;
                yield return new WaitForSeconds(animDelay);
                bool lookingRight = inputManager.lookingRight;

                projs[0].GetComponent<ParticleSystemRenderer>().flip = lookingRight ? new Vector2(0, 0) : new Vector2(0, 1);
                projs[1].GetComponent<ParticleSystemRenderer>().flip = lookingRight ? new Vector2(0, 1) : new Vector2(0, 0);
                projs[2].GetComponent<ParticleSystemRenderer>().flip = lookingRight ? new Vector2(0, 0) : new Vector2(0, 1);

                float delayDelta = 1 / (1 - animDelay) * 0.1f;

                Vector2 atkTargetPos = new Vector2(
                    (lookingRight ? tr.position.x + 0.2f : tr.position.x - 0.5f), tr.position.y);
                ShowEffect(projs[0], atkTargetPos);

                for (int i = 0; i < 3; i++)
                {
                    yield return BattleManager.instance.waitPause;
                    yield return new WaitForSeconds(delayDelta);
                }

                Vector2 atkTargetPos2 = new Vector2(
                    (lookingRight ? tr.position.x - 0.5f : tr.position.x + 0.2f), tr.position.y);
                ShowEffect(projs[1], atkTargetPos2);

                for (int i = 0; i < 3; i++)
                {
                    yield return BattleManager.instance.waitPause;
                    yield return new WaitForSeconds(delayDelta);
                }

                Vector2 atkTargetPos3 = new Vector2(
                    (lookingRight ? tr.position.x + 0.2f : tr.position.x - 0.5f), tr.position.y);
                ShowEffect(projs[2], atkTargetPos3);

                for (int i = 0; i < 4; i++)
                {
                    yield return BattleManager.instance.waitPause;
                    yield return new WaitForSeconds(delayDelta);
                }
                HideEffect(projs[0]);
                HideEffect(projs[1]);
                HideEffect(projs[2]);
            }
            yield return null;
        }
    }

    private void ShowEffect(SkillProj1140001 proj, Vector2 targetPos)
    {
        proj.gameObject.SetActive(true);
        proj.transform.position = targetPos;
        proj.col.enabled = true;
        proj.particle.Play();
    }
    private void HideEffect(SkillProj1140001 proj)
    {
        proj.gameObject.SetActive(false);
    }
}