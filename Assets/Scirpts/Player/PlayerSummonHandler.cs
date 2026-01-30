using Unity.Netcode;
using UnityEngine;

public class PlayerSummonHandler : NetworkBehaviour
{
    [SerializeField] private GameObject _rushPrefab;
    [SerializeField] private LayerMask _platformLayer;
    private RushController _rush = null;

    #region Network Variables
    private NetworkVariable<NetworkObjectReference> _rushRef = new();
    #endregion

    #region Network Methods
    private bool IsNetworked() => NetworkManager.Singleton != null && NetworkManager.Singleton.IsListening;

    public override void OnNetworkSpawn()
    {

        _rushRef.OnValueChanged += OnRushReferenceChanged;
        if (IsServer)
        {
            Debug.Log("In Server");
            if (_rush != null)
            {
                return;
            }

            GameObject rush = Instantiate(_rushPrefab);
            NetworkObject netObj = rush.GetComponent<NetworkObject>();
            netObj.Spawn();

            _rushRef.Value = netObj;
            _rush = rush.GetComponent<RushController>();
        }

        else
        {
            Debug.Log("In Local");
            if (_rush != null)
            {
                return;
            }

            if (_rushRef.Value.TryGet(out NetworkObject netObj))
            {
                _rush = netObj.GetComponent<RushController>();
            }
        }
    }

    public override void OnNetworkDespawn()
    {
        _rushRef.OnValueChanged -= OnRushReferenceChanged;
    }

    private void OnRushReferenceChanged(NetworkObjectReference oldRef, NetworkObjectReference newRef)
    {
        if (_rush != null)
        {
            return;
        }
        if (newRef.TryGet(out NetworkObject netObj))
        {
            _rush = netObj.GetComponent<RushController>();
        }
    }


    #endregion

    #region Unity Methods
    void Start()
    {
        if (!IsNetworked())
        {
            GameObject obj = Instantiate(_rushPrefab);
            _rush = obj.GetComponent<RushController>();
        }

    }
    #endregion

    #region Public Methods
    public void TrySpawnRush(Vector3 playerPos)
    {
        Vector3 playerPosition = transform.position;
        RaycastHit2D hit = Physics2D.Raycast(transform.position, Vector2.down, 100f, _platformLayer);
        if (hit.collider != null)
        {
            float targetY = hit.point.y;
            Vector3 targetPos = new Vector3(playerPosition.x, targetY, 0);
            _rush.RequestSpawn(targetPos);
        }
    }
    #endregion
}
