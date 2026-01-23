using UnityEngine;
using UnityEngine.Animations;
using UnityEngine.InputSystem;

public class PlayerController : MonoBehaviour
{
    private float _moveInput;

    [SerializeField]
    private Animator _animator;
    [SerializeField]
    private SpriteRenderer _spriteRenderer;
    [SerializeField]
    private Rigidbody2D _rigidBody;


    [SerializeField]
    private float _movingSpeed = 5f;


    private PlayerStateMachine _stateMachine;

    private void Awake()
    {
        _stateMachine = new PlayerStateMachine(this);
    }
    protected void Start()
    {
        _stateMachine.AddState(new PlayerIdleState());
        _stateMachine.AddState(new PlayerRunState());

        ChangeState<PlayerIdleState>();
    }

    protected void LateUpdate()
    {
        _stateMachine.Update();
    }


    private void OnMove(InputValue value)
    {
        _moveInput = value.Get<float>();
        _animator.SetBool("Direction", _moveInput != 0);

        if (_moveInput != 0)
        {
            _spriteRenderer.flipX = _moveInput > 0;
        }

        _stateMachine.OnMove(_moveInput);
    }

    public void ChangeState<T>() where T: PlayerBaseState
    {
        _stateMachine.ChangeState<T>();
    }

    public void Move()
    {
        _rigidBody.linearVelocityX = _movingSpeed * _moveInput;
    }
}
