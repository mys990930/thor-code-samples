using System;
using System.Collections.Generic;
using UnityEngine;

public class ObjectPool : MonoBehaviour, IPool
{
    public GameObject prefab;
    public Stack<GameObject> pooledObjects = new Stack<GameObject>();
    public int maxCount;
    private readonly HashSet<GameObject> ownedObjects = new HashSet<GameObject>();
    private readonly HashSet<GameObject> pooledObjectSet = new HashSet<GameObject>();
    private readonly HashSet<GameObject> returningObjects = new HashSet<GameObject>();
    private bool isClearing;
    private bool isDisabling;

    public void Init(GameObject prefab, int cnt, int maxCount)
    {
        if (prefab == null || prefab.GetComponent<IObjectPoolable>() == null)
            throw new ArgumentException("Pool prefab must implement IObjectPoolable.", nameof(prefab));
        if (maxCount < 0 || cnt < 0 || cnt > maxCount)
            throw new ArgumentOutOfRangeException(nameof(cnt), "Initial count must be between zero and maxCount.");
        PruneDestroyedObjects();
        if (isClearing || ownedObjects.Count != 0)
            throw new InvalidOperationException("Clear the pool before initializing it again.");

        this.prefab = prefab;
        this.maxCount = maxCount;
        for (int i = 0; i < cnt; i++)
        {
            Release(Create());
        }
    }

    public GameObject Create()
    {
        PruneDestroyedObjects();
        if (isClearing)
            throw new InvalidOperationException("Cannot create an object while clearing the pool.");
        if (ownedObjects.Count >= maxCount)
            throw new ArgumentOutOfRangeException(nameof(maxCount), "Pool reached its maximum count.");
        if (prefab == null || prefab.GetComponent<IObjectPoolable>() == null)
            throw new InvalidOperationException("Pool prefab must implement IObjectPoolable.");

        var newObj = Instantiate(prefab);
        newObj.transform.SetParent(transform, false);
        ownedObjects.Add(newObj);
        try
        {
            newObj.GetComponent<IObjectPoolable>().InitializePool(this);
        }
        catch
        {
            Remove(newObj);
            throw;
        }
        return newObj;
    }

    public GameObject Get()
    {
        if (TryGet(out GameObject obj)) return obj;
        throw new ArgumentOutOfRangeException(nameof(maxCount), $"Pool '{name}' has no available object for '{prefab?.name}'.");
    }

    public bool TryGet(out GameObject obj)
    {
        obj = null;
        if (isClearing) return false;
        PruneDestroyedObjects();

        if (!TryPopPooledObject(out obj) && !TryRecoverInactiveObject(out obj))
        {
            if (ownedObjects.Count >= maxCount) return false;
            obj = Create();
            pooledObjectSet.Remove(obj);
        }

        if (obj != null && ownedObjects.Contains(obj)) obj.SetActive(true);
        if (obj != null && ownedObjects.Contains(obj) && obj.activeSelf && !pooledObjectSet.Contains(obj))
            return true;

        obj = null;
        return false;
    }

    public void Release(GameObject obj)
    {
        if (isClearing || obj == null || !ownedObjects.Contains(obj) ||
            pooledObjectSet.Contains(obj) || !returningObjects.Add(obj)) return;

        pooledObjectSet.Add(obj);
        try
        {
            obj.SetActive(false);
            if (obj != null && ownedObjects.Contains(obj) && pooledObjectSet.Contains(obj) && !obj.activeSelf)
                pooledObjects.Push(obj);
            else
                pooledObjectSet.Remove(obj);
        }
        finally
        {
            returningObjects.Remove(obj);
        }
    }

    private bool TryPopPooledObject(out GameObject obj)
    {
        while (pooledObjects.TryPop(out obj))
        {
            if (obj == null || !ownedObjects.Contains(obj) || returningObjects.Contains(obj)) continue;
            if (!pooledObjectSet.Remove(obj)) continue;
            if (obj.activeSelf) continue;
            return true;
        }
        obj = null;
        return false;
    }

    private bool TryRecoverInactiveObject(out GameObject obj)
    {
        foreach (GameObject target in ownedObjects)
        {
            if (target != null && !target.activeSelf && !pooledObjectSet.Contains(target) && !returningObjects.Contains(target))
            {
                obj = target;
                return true;
            }
        }
        obj = null;
        return false;
    }

    private void PruneDestroyedObjects()
    {
        ownedObjects.RemoveWhere(obj => obj == null);
        pooledObjectSet.RemoveWhere(obj => obj == null || !ownedObjects.Contains(obj));
        returningObjects.RemoveWhere(obj => obj == null || !ownedObjects.Contains(obj));
    }

    public void Remove(GameObject obj)
    {
        if (isClearing || ReferenceEquals(obj, null) || !ownedObjects.Remove(obj)) return;
        pooledObjectSet.Remove(obj);
        returningObjects.Remove(obj);
        if (obj != null) Destroy(obj);
    }

    public void Disable()
    {
        if (isClearing || isDisabling) return;
        isDisabling = true;
        try
        {
            foreach (GameObject target in new List<GameObject>(ownedObjects))
            {
                if (target == null || !ownedObjects.Contains(target) || !target.activeSelf) continue;
                target.GetComponent<IObjectPoolable>().Disable();
                Release(target);
            }
        }
        finally
        {
            isDisabling = false;
        }
    }

    public void Clear()
    {
        if (isClearing) return;
        isClearing = true;
        try
        {
            var targets = new List<GameObject>(ownedObjects);
            ownedObjects.Clear();
            pooledObjects.Clear();
            pooledObjectSet.Clear();
            returningObjects.Clear();
            foreach (GameObject target in targets)
            {
                if (target != null) Destroy(target);
            }
        }
        finally
        {
            isClearing = false;
        }
    }
}
