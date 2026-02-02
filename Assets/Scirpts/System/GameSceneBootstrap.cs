using Unity.Netcode;
using UnityEngine;

public class GameSceneBootstrap : MonoBehaviour
{
    [SerializeField] private GameObject _playerPrefab;
    void Start()
    {
        if(!NetworkManager.Singleton.IsServer)
        {
            return;
        }

        NetworkManager.Singleton.NetworkConfig.PlayerPrefab = _playerPrefab;

        foreach(var clientID in NetworkManager.Singleton.ConnectedClientsIds)
        {
            var player = Instantiate(_playerPrefab);
            player.GetComponent<NetworkObject>()
                .SpawnAsPlayerObject(clientID);
        }
    }
}
