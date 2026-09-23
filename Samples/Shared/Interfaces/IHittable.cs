using System;
using UnityEngine;

public interface IHittable
{
    public bool isDead {  get; set; }
    public void Hit(big initDmg, bool shouldKnockback);
    public void Hit(big initDmg, bool shouldKnockback, Action<bool> callback);
    public Transform GetTransform();
}