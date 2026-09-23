using UnityEngine;
using UnityEngine.Pool;

public interface IObjectPoolable
{
    public void InitializePool(IPool pool);
    public void Disable();
}