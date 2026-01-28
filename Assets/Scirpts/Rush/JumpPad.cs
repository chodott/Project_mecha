using Unity.Netcode;
using UnityEngine;

public class JumpPad : NetworkBehaviour
{
    [SerializeField] private float _launchForce = 15f;
    public bool IsNetowrked => NetworkManager.Singleton != null && NetworkManager.Singleton.IsListening;

    private void OnTriggerStay2D(Collider2D collision)
    {
        if (!IsServer && IsNetowrked) return;

        var player = collision.GetComponent<PlayerController>();
        if (player != null)
        {
            player.OnSuperJump(_launchForce);

            //PlayRushAnimServerRpc();
        }
    }
}
