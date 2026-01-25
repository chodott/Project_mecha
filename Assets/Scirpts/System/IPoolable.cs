using UnityEngine;

public interface IPoolable
{
    public int PoolKey { get; set; }

    void OnSpawn();
    void OnDespawn();
}