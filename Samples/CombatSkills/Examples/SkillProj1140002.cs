using System.Collections;
using System.Collections.Generic;
using System.Linq;
using Unity.Collections.LowLevel.Unsafe;
using UnityEngine;

public class SkillProj1140002 : SkillProjectile
{
    private Vector2 randomStartPoint;
    private float speed = 1;

    protected override void OnProjectileInit()
    {
        tr.position = new Vector2(1.5f, 1.5f);
        StartCoroutine(ProjectileAction());
    }

    protected override void OnObjHit(IHittable hitObj)
    {
        hitObj.Hit(skill.DMG, true, (isDead) => { });
    }

    public override void OnBattlePause()
    {
    }

    public override void OnBattleResume()
    {
    }
    public void SetSpeed(float speed)
    {
        this.speed = speed;
    }
    private IEnumerator ProjectileAction()
    {
        bool movingDown = false;
        while (true)
        {
            AudioManager.instance.PlaySoundEffectOneShot(SkillSoundType.skill0002);
            movingDown = !movingDown;

            if (movingDown) randomStartPoint = new Vector2(Random.Range(1, 1.5f), Random.Range(1, 1.5f));
            else randomStartPoint = new Vector2(Random.Range(0, -0.5f), Random.Range(0, -0.5f));
            randomStartPoint = cam.ViewportToWorldPoint(randomStartPoint);
            tr.position = randomStartPoint;

            int cnt = 0;
            Transform target = FindRandomTarget();
            Vector2 dir = new Vector2((target.position.x - tr.position.x), (target.position.y - tr.position.y));
            dir.Normalize();
            dir *= GameManager.frameConst * 0.3f; //frame const 확인하기
            dir *= speed;

            tr.rotation = Quaternion.Euler(0, 0, (3.925f + Mathf.Atan2(dir.y, dir.x)) * 57.3f);
            while (cnt < 50)
            {
                tr.position = new Vector2(tr.position.x + dir.x, tr.position.y + dir.y); 
                cnt++;
                yield return BattleManager.instance.waitPause;
            }
            if (BattleManager.instance.battleMode == BattleMode.eIdleMode)
            {
                yield return new WaitForSeconds(2.5f);
            }
            else
            {
                yield return new WaitForSeconds(0.5f);
            }
            yield return BattleManager.instance.waitPause;
        }
    }
}