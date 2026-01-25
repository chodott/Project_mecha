using System;
using System.Collections;
using TMPro;
using UnityEngine;
using UnityEngine.Animations;
using UnityEngine.InputSystem;
using UnityEngine.Rendering.Universal;

public class PlayerController : MonoBehaviour
{
    private float _moveInput;
    private float _direction = -1;

    [SerializeField]
    private Animator _animator;
    [SerializeField]
    private SpriteRenderer _spriteRenderer;
    [SerializeField]
    private Rigidbody2D _rigidBody;

    private PlayerInput _playerInput;
    private InputAction _attackAction;
    private PlayerAttack _playerAttack;

    public Vector2 Velocity { get { return _rigidBody.linearVelocity; } }

    //Stat
    [SerializeField]
    private float _movingSpeed = 5f;
    [SerializeField]
    private float _jumpForce = 3f;


    //Collision
    [SerializeField] private LayerMask _groundLayer;    // 바닥 레이어
    [SerializeField] private Transform _groundCheckPos; // 발밑에 배치한 빈 오브젝트
    [SerializeField] private Vector2 _groundCheckSize = new Vector2(0.3f, 0.1f); // 박스 크기

    private PlayerStateMachine _stateMachine;


    private void Awake()
    {
        _stateMachine = new PlayerStateMachine(this);
        _playerAttack = GetComponent<PlayerAttack>();

        _playerInput = GetComponent<PlayerInput>();
        _attackAction = _playerInput.actions["Attack"];

    }

    private void OnEnable()
    {
        _attackAction.started += OnAttackStarted;
        _attackAction.canceled += OnAttackCanceled;
    }

    private void OnDisable()
    {
        _attackAction.started -= OnAttackStarted;
        _attackAction.canceled -= OnAttackCanceled;
    }

    protected void Start()
    {
        _stateMachine.AddState(new PlayerIdleState());
        _stateMachine.AddState(new PlayerRunState());
        _stateMachine.AddState(new PlayerJumpState());
        _stateMachine.AddState(new PlayerFallState());
        _stateMachine.AddState(new PlayerLandingState());


        ChangeState<PlayerIdleState>();
    }

    protected void Update()
    {
        _stateMachine.Update();
        _stateMachine.OnMove(_moveInput);
    }



    //Input System
    private void OnMove(InputValue value)
    {
        _moveInput = value.Get<float>();
        if (_moveInput != 0)
        {
            _spriteRenderer.flipX = _moveInput > 0;
            _direction = Mathf.Sign(_moveInput);
        }
    }

    private void OnJump(InputValue value)
    {
        _stateMachine.OnJump();
    }

    private void OnAttackStarted(InputAction.CallbackContext context)
    {
        _playerAttack.StartCharging();
    }

    private void OnAttackCanceled(InputAction.CallbackContext context)
    {
        _playerAttack.Fire(_direction);
    }



    public void ChangeState<T>() where T : PlayerBaseState
    {
        _stateMachine.ChangeState<T>();
    }

    public void PlayAnimation(int animHash, float crossFadeTime = 0.1f)
    {
        _animator.CrossFade(animHash, crossFadeTime);
    }

    public void Move()
    {
        _rigidBody.linearVelocityX = _movingSpeed * _moveInput;
    }

    public void Jump()
    {
        _rigidBody.linearVelocityY = _jumpForce;
    }

    public bool IsGrounded()
    {
        return Physics2D.OverlapBox(_groundCheckPos.position, _groundCheckSize, 0, _groundLayer);
    }

    public AnimatorStateInfo GetAnimStateInfo()
    {
        return _animator.GetCurrentAnimatorStateInfo(0);
    }
}
