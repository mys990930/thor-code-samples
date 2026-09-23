using Assets.HeroEditor.Common.Scripts.CharacterScripts;

public class AnimationAdapter : IAnimator
{
    public Character charaSprite;
    
    public AnimationAdapter(Character charaSprite)
    {
        this.charaSprite = charaSprite;
    }

    public void SetAttack()
    {
        //Upper body animations; check partial class Character from CharacterAnimation.cs
        charaSprite.Slash();
    }

    public void SetMovement(CharaMovement movement)
    {
        //Lower body animations; check CharacterState enum
        switch (movement)
        {
            case CharaMovement.IDLE:
                charaSprite.SetState(CharacterState.Idle);
                break;
            case CharaMovement.WALK:
                charaSprite.SetState(CharacterState.Walk);
                break;
        }
    }
    public void Pause()
    {
        charaSprite.Animator.speed = 0;
    }

    public void Resume()
    {
        charaSprite.Animator.speed = 1;
    }
    public void ShowDeathAnim()
    {
        charaSprite.SetState(CharacterState.DeathF);
    }
    public void Reset()
    {
        charaSprite.ResetAnimation();
    }
}