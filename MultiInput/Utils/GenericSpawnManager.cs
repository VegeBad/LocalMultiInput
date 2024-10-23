using System;
using System.Collections.Generic;
using UnityEngine;

public abstract class GenericSpawnManager<T> : GenericSingleton<GenericSpawnManager<T>>
    where T : System.Enum
{
    public SpawnSerialize<T>[] SerializedOb;
    Dictionary<T, SpawnSerialize<T>> SpawnDictionary = new();

    public List<GameObject> SpawnedObjects = new();

    protected override void Awake()
    {
        base.Awake();
        foreach(var SpawnPrefab in SerializedOb)
            SpawnDictionary[SpawnPrefab.SpawnId] = SpawnPrefab;
    }

    public GameObject SpawnObject(T ID, Vector3 Pos, Quaternion Rot)
    {
        GameObject newob = null;
        if (SpawnDictionary.TryGetValue(ID, out SpawnSerialize<T> SpawnPrefab))
        {
            int key = UnityEngine.Random.Range(0, SpawnPrefab.Prefab.Length);
            GameObject c = SpawnPrefab.Prefab[key];
            newob = Instantiate(c, Pos, Rot);
            SpawnedObjects.Add(newob);
        }
        return newob;
    }

    public void DespawnObject(GameObject Despawnob)
    {
        if (SpawnedObjects.Contains(Despawnob))
        {
            SpawnedObjects.Remove(Despawnob);
            Destroy(Despawnob);
        }
        else
            Debug.Log($"{Despawnob} doesn't exist in {SpawnedObjects}");
    }

    public void DespawnEverything()
    {
        foreach(var ob in SpawnedObjects.ToArray())
            DespawnObject(ob);
    }
}

[Serializable]
public struct SpawnSerialize<T> 
    where T : System.Enum
{
    public T SpawnId;
    public GameObject[] Prefab;
}