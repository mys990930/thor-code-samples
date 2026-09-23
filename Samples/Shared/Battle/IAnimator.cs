using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public interface IAnimator
{
    public void SetMovement(CharaMovement movement);
    public void SetAttack();
    public void Pause();
    public void Resume();
    public void ShowDeathAnim();
    public void Reset();
}
