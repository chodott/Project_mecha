using System.Collections.Generic;
using Unity.VisualScripting;
using UnityEngine;

public class ObjectPoolManager : MonoBehaviour
{
    public static ObjectPoolManager Instance { get; private set; }
    private void Awake()
    {
        if (Instance != null && Instance != this)
        {
            Destroy(this.gameObject);
            return;
        }
        Instance = this;
        DontDestroyOnLoad(this.gameObject);
    }

    private class ComponentPool<T> where T : MonoBehaviour, IPoolable
    {
        private Queue<T> _queue = new Queue<T>();
        private T _prefab;

        public ComponentPool(T prefab)
        {
            _prefab = prefab;
        }

        public T Get()
        {
            T newObj = (_queue.Count > 0) ? _queue.Dequeue() : Instantiate(_prefab);
            newObj.gameObject.SetActive(true);
            newObj.OnSpawn();
            return newObj;
        }
        public void Release(T obj)
        {
            obj.gameObject.SetActive(false);
            obj.OnDespawn();
            _queue.Enqueue(obj);
        }
    }

    public void PreloadDefault<T>(T prefab, int count) where T : MonoBehaviour, IPoolable
    {
        int key = prefab.gameObject.GetInstanceID();

        if (!_pools.ContainsKey(key))
        {
            _pools.Add(key, new ComponentPool<T>(prefab));
        }

        var pool = _pools[key] as ComponentPool<T>;

        for (int i = 0; i < count; i++)
        {
            T obj = Instantiate(prefab, transform);
            obj.PoolKey = key;
            obj.gameObject.SetActive(false);
            pool.Release(obj);
        }
    }

    private Dictionary<int, object> _pools = new Dictionary<int, object>();

    public T Get<T>(T prefab) where T : MonoBehaviour, IPoolable
    {
        int key = prefab.gameObject.GetInstanceID();
        if (!_pools.ContainsKey(key))
        {
            _pools[key] = new ComponentPool<T>(prefab);
        }
        var pool = _pools[key] as ComponentPool<T>;

        return pool.Get();
    }

    public void Release<T>(T obj) where T : MonoBehaviour, IPoolable
    {
        int key = obj.PoolKey;
        var pool = _pools[key] as ComponentPool<T>;
        pool.Release(obj);
    }
}