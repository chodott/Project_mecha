using Unity.Netcode;
using UnityEngine;

public class CameraScroller : NetworkBehaviour
{
    [SerializeField] private float _scrollSpeed =0.0001f;

    NetworkVariable<float> _currentY = new NetworkVariable<float>(0f);

    public override void OnNetworkSpawn()
    {
        _currentY.OnValueChanged += UpdateCameraPosition;
    }

    private void LateUpdate()
    {
        if(!IsServer)
        {
            return;
        }


        _currentY.Value += _scrollSpeed * Time.deltaTime;
    }

    private void UpdateCameraPosition(float previousValue, float newValue)
    {

        transform.position = new Vector3(
          transform.position.x,
          newValue,
          transform.position.z
      );
    }
}
