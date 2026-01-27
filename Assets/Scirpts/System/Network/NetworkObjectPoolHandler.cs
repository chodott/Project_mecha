using Unity.Netcode;
using UnityEngine;

public class PooledNetworkObjectHandler : INetworkPrefabInstanceHandler
{
    private NetworkPoolable _prefab;
    private ObjectPoolManager _poolManager;
    public PooledNetworkObjectHandler(NetworkPoolable poolablePrefab, ObjectPoolManager poolManager)
    {
        _prefab = poolablePrefab;
        _poolManager = poolManager;
    }

    public void Destroy(NetworkObject networkObject)
    {
        if(networkObject.TryGetComponent<NetworkPoolable>(out var poolableObject))
        {
            _poolManager.Release<NetworkPoolable>(poolableObject);
        }
        else
        {
            Debug.Log("Is Not PoolableNetworkObject!!!!!");
        }
    }

    public NetworkObject Instantiate(ulong ownerClientId, Vector3 position, Quaternion rotation)
    {
        var newObject = _poolManager.Get<NetworkPoolable>(_prefab);
        newObject.transform.SetPositionAndRotation(position, rotation);
        return newObject.GetComponent<NetworkObject>();
    }
}
