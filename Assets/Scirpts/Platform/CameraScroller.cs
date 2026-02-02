using Unity.Netcode;
using UnityEngine;

public class CameraScroller : NetworkBehaviour
{
    [SerializeField] private float _scrollSpeed =0.0001f;

    NetworkVariable<float> _currentY = new NetworkVariable<float>(0f);

    private bool _isScrolling = false;

    private void LateUpdate()
    {
        if (!IsServer)
        {
            return;
        }

        if (_isScrolling)
        {
            _currentY.Value += _scrollSpeed * Time.deltaTime;
        }
    }

    private void OnEnable()
    {
        EventBus.OnChangedGamePhase += HandleGamePhase;
    }

    private void OnDisable()
    {
        EventBus.OnChangedGamePhase -= HandleGamePhase;
    }

    public override void OnNetworkSpawn()
    {
        _currentY.OnValueChanged += UpdateCameraPosition;
    }

    private void UpdateCameraPosition(float previousValue, float newValue)
    {

        transform.position = new Vector3(
          transform.position.x,
          newValue,
          transform.position.z
      );
    }

    private void HandleGamePhase(GamePhase gameStatus)
    {
        switch (gameStatus)
        { 
            case GamePhase.Playing:
                _isScrolling = true;
                break;

            default:
                _isScrolling = false; 
                break;
        }
    }
}
