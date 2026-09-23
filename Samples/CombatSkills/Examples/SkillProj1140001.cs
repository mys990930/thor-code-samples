using System.Collections;
using Unity.Collections.LowLevel.Unsafe;
using Unity.VisualScripting;
using UnityEngine;

public class SkillProj1140001 : SkillProjectile
{
    public ParticleSystem particle;
    public CapsuleCollider2D col;

    protected override void OnProjectileInit()
    {
        particle = transform.GetComponent<ParticleSystem>();
        col = transform.GetComponent<CapsuleCollider2D>();
        AudioManager.instance.PlaySoundEffectOneShot(SkillSoundType.skill0001);
        StartCoroutine(TrackParticleFinish());
        //GetComponent<CapsuleCollider2D>().enabled = false;
    }

    protected override void OnObjHit(IHittable hitObj)
    {
        shouldDestroy = true;
        hitObj.Hit(skill.DMG, true, (isDead) => { });
        //StartCoroutine(Destroy());
    }

    public override void OnBattlePause()
    {
        particle.Pause();
    }

    public override void OnBattleResume()
    {
        particle.Play();
    }

    private IEnumerator TrackParticleFinish()
    {
        int frame = 0;
        float particleDurationInSec = 0.3f;
        while (frame < GameManager.fps * particleDurationInSec)
        {
            yield return BattleManager.instance.waitPause;
            frame++;
        }
        col.enabled = false;
    }

    public void OnParticleSystemStopped()
    {
        //col.enabled = false;
    }
}