using Unity.Netcode;
using UnityEngine;

public class RushController : NetworkBehaviour
{
    [SerializeField] private Animator _animator;
    [SerializeField] private float _launchForce = 15f;
    [SerializeField] private float _fallSpeed = 5f;
    [SerializeField] private float _yOffset = 0.5f;

    private RushStateMachine _rushStateMachine;
    private float _targetY = 0f; 
    public bool IsNetowrked => NetworkManager.Singleton != null && NetworkManager.Singleton.IsListening;

    protected void Awake()
    {
        _rushStateMachine = new RushStateMachine(this);
        _rushStateMachine.AddState(new RushFallState());
        _rushStateMachine.AddState(new RushLandingState());
        _rushStateMachine.AddState(new RushIdleState());
        _rushStateMachine.AddState(new RushUsedState());
    }

    protected void Start()
    {
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

    public AnimatorStateInfo GetAnimStateInfo()
    {
        return _animator.GetCurrentAnimatorStateInfo(0);
    }

    public bool CheckPlayerJump(Collider2D collision)
    {

        var player = collision.GetComponent<PlayerController>();
        if (player != null)
        {
            player.OnSuperJump(_launchForce);
            return true; 
            //PlayRushAnimServerRpc();
        }
        return false;
    }

    public void RequestSpawn(Vector3 targetPos)
    {
        _rushStateMachine.OnRespawn(targetPos);
    }
    
    public void SpawnToTarget(Vector3 targetPos)
    {
        transform.position = new Vector3(targetPos.x, 5f, 0);
        _targetY = targetPos.y + _yOffset;
        gameObject.SetActive(true);
    }

    public void FallDown()
    {
        transform.position += Vector3.down * _fallSpeed * Time.deltaTime;
    }

    public bool IsLandingComplete()
    {
        if (transform.position.y <= _targetY)
        {
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
