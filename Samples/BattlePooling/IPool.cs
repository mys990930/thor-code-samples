using System.Collections.Generic;
using UnityEngine;

public interface IPool
{
    public void Init(GameObject prefab, int cnt, int maxCount);
    public GameObject Create();
    public GameObject Get();
    public bool TryGet(out GameObject obj);
    public void Release(GameObject obj);
    public void Remove(GameObject obj);
    public void Disable();
    public void Clear();
}
