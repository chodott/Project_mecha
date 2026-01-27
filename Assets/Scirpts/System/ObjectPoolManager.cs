using System.Collections.Generic;
using Unity.Netcode;
using Unity.VisualScripting;
using UnityEngine;

public class ObjectPoolManager : MonoBehaviour
{
    [Header("Networking Pool Settings")]
    [SerializeField] private List<NetworkPoolable> networkedPrefabsToPool;

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

    private void Start()
    {
        if(NetworkManager.Singleton != null)
        {
            RegisterNetworkPool();
        }
    }

    private void RegisterNetworkPool()
    {
        foreach (var prefab in networkedPrefabsToPool)
        {
            NetworkManager.Singleton.PrefabHandler.AddHandler(
                prefab.gameObject,
                new PooledNetworkObjectHandler(prefab, this)
                );
        }
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
            newObj.PoolKey = _prefab.gameObject.name;
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
        string key = prefab.gameObject.name;

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

    private Dictionary<string, object> _pools = new Dictionary<string, object>();

    public T Get<T>(T prefab) where T : MonoBehaviour, IPoolable
    {
        string key = prefab.gameObject.name;
        if (!_pools.ContainsKey(key))
        {
            _pools[key] = new ComponentPool<T>(prefab);
            
        }

        if(_pools[key] is not ComponentPool<T>)
        {
            Debug.LogError($"Pool type mismatch for key: {key}");
            return null;
        }
        var pool = _pools[key] as ComponentPool<T>;

        return pool.Get();
    }

    public void Release<T>(T obj) where T : MonoBehaviour, IPoolable
    {
        string key = obj.PoolKey;
        if(_pools.ContainsKey(key) == false)
        {
            Debug.LogWarning($"No pool found for key: {key}. Destroying object.");
            Destroy(obj.gameObject);
            return;
        }

        var pool = _pools[key] as ComponentPool<T>;
        pool.Release(obj);
    }
}