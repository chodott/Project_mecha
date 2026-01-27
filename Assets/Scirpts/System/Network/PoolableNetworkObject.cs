using Unity.Netcode;
using UnityEngine;

public abstract class NetworkPoolable : NetworkBehaviour, IPoolable
{
    public string PoolKey { get; set; }
    public abstract void OnSpawn();
    public abstract void OnDespawn();
}