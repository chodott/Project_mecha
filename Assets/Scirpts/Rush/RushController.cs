using Unity.Netcode;
using UnityEngine;

public class RushController : NetworkBehaviour
{
    [SerializeField] private float _launchForce = 15f;
    [SerializeField] private float _fallSpeed = 5f;

    private RushStateMachine _rushStateMachine;
    private Animator _animator;
    private float _targetY = 0f; 
    public bool IsNetowrked => NetworkManager.Singleton != null && NetworkManager.Singleton.IsListening;

    protected void Start()
    {
        _animator = GetComponent<Animator>();
        _rushStateMachine = new RushStateMachine(this);
        _rushStateMachine.ChangeState<RushFallState>();
    }

    protected void Update()
    {
        _rushStateMachine.Update();
    }

    private void OnTriggerStay2D(Collider2D collision)
    {
        _rushStateMachine.OnTriggerStay(collision);
    }

    public void ChangeState<T>() where T : RushBaseState
    {
        _rushStateMachine.ChangeState<T>();
    }

    public void PlayAnimation(int animHash, float crossFadeTime = 0f)
    {
        _animator.CrossFade(animHash, crossFadeTime);
    }

    public void CheckPlayerJump(Collider2D collision)
    {
        if (!IsServer && IsNetowrked) return;

        var player = collision.GetComponent<PlayerController>();
        if (player != null)
        {
            player.OnSuperJump(_launchForce);

            //PlayRushAnimServerRpc();
        }
    }

    public void FallDown()
    {
        transform.position += Vector3.down * _fallSpeed * Time.deltaTime;
    }

    public bool IsLandingComplete()
    {
        if (transform.position.y <= _targetY)
        {
            // 위치 보정 (바닥에 딱 붙이기)
            Vector3 finalPos = transform.position;
            finalPos.y = _targetY;
            transform.position = finalPos;
            return true;
        }
        else
        {
            return false;
        }
    }
}
