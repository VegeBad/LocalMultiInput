using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public abstract class GenericSingleton<T> : MonoBehaviour 
    where T : MonoBehaviour
{
    public static T Instance { get; protected set; }

    protected virtual void InitSingleton()
    {
        if (Instance != null)
        {
            Destroy(Instance.gameObject);
            throw new System.Exception($"{typeof(T)} singleton has been created before, destorying {typeof(T)}");
        }

        Instance = (T)(MonoBehaviour)this;
    }

    protected virtual void UninitSingleton()
    {
        Instance = null;
    }

    protected void KeepSingleton(bool Keep)
    {
        if (Keep) DontDestroyOnLoad(this);
    }

    protected virtual void Awake()
    {
        InitSingleton();
    }

    protected virtual void OnDestroy()
    {
        UninitSingleton();
    }
}
