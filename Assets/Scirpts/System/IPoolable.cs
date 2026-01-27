using UnityEngine;

public interface IPoolable
{
    public string PoolKey { get; set; }

    void OnSpawn();
    void OnDespawn();
}