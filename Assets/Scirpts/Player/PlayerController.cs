using System;
using System.Collections;
using Unity.Netcode;
using UnityEngine;
using UnityEngine.Animations;
using UnityEngine.InputSystem;
using UnityEngine.Rendering.Universal;

public class PlayerController : NetworkBehaviour, IDamageable
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
    [SerializeField]
    private float _stunDuration = 1f;


    //Collision
    [SerializeField] private LayerMask _groundLayer;    // 바닥 레이어
    [SerializeField] private Transform _groundCheckPos; // 발밑에 배치한 빈 오브젝트
    [SerializeField] private Vector2 _groundCheckSize = new Vector2(0.3f, 0.1f); // 박스 크기

    private PlayerStateMachine _stateMachine;

    //Network 
    private NetworkVariable<FireState> _curFireState = new NetworkVariable<FireState>(
        FireState.Idle,
        NetworkVariableReadPermission.Everyone,
        NetworkVariableWritePermission.Owner
        );
    private NetworkVariable<int> _curAnimHash = new NetworkVariable<int>(
        0,
        NetworkVariableReadPermission.Everyone,
        NetworkVariableWritePermission.Owner
        );
    private NetworkVariable<bool> _isFacingRight = new NetworkVariable<bool>(
        false,
        NetworkVariableReadPermission.Everyone,
        NetworkVariableWritePermission.Owner
        );
    private NetworkVariable<bool> _isStunned = new NetworkVariable<bool>(
        false,
        NetworkVariableReadPermission.Everyone,
        NetworkVariableWritePermission.Server);


    private void Awake()
    {
        _playerAttack = GetComponent<PlayerAttack>();
        _playerInput = GetComponent<PlayerInput>();
    }

    private void OnEnable()
    {
        if (IsSpawned == true)
        {
            BindLocalEvents();
        }
    }

    private void OnDisable()
    {
        if (IsSpawned == true)
        {
            _attackAction.started -= OnAttackStarted;
            _attackAction.canceled -= OnAttackCanceled;
        }
    }

    protected void Start()
    {
        _stateMachine = new PlayerStateMachine(this);
        _stateMachine.AddState(new PlayerIdleState());
        _stateMachine.AddState(new PlayerRunState());
        _stateMachine.AddState(new PlayerJumpState());
        _stateMachine.AddState(new PlayerFallState());
        _stateMachine.AddState(new PlayerLandingState());
        _stateMachine.AddState(new PlayerStunState());

        ChangeState<PlayerIdleState>();
    }

    protected void Update()
    {
        if (IsOwner == false)
        {
            return;
        }

        _stateMachine.OnMove(_moveInput);
        _stateMachine.Update();
    }

    protected void FixedUpdate()
    {
        if (IsOwner == false)
        {
            return;
        }
        _stateMachine.FixedUpdate();
    }

    private void BindLocalEvents()
    {
        _attackAction = _playerInput.actions["Attack"];
        _attackAction.started += OnAttackStarted;
        _attackAction.canceled += OnAttackCanceled;
        _playerAttack.OnFireStateChanged += UpdateFireLayer;
    }

    private void BindRemoteEvents()
    {
        _isFacingRight.OnValueChanged += (oldValue, newValue) =>
        {
            _spriteRenderer.flipX = newValue;
        };

        _curAnimHash.OnValueChanged += (oldHash, newHash) =>
        {
            if (!IsOwner)
            {
                _animator.CrossFade(newHash, 0);
            }
        };

        _curFireState.OnValueChanged += (oldState, newState) =>
        {
            UpdateFireLayer(newState);
        };
    }

    //Network
    public override void OnNetworkSpawn()
    {
        if (IsOwner)
        {
            BindLocalEvents();
        }
        else
        {
            BindRemoteEvents();
            _playerInput.enabled = false;
        }

        _isStunned.OnValueChanged += (oldValue, newValue) =>
        {
            if (newValue == true)
            {
                _stateMachine.OnHit();
            }
            else
            {
                _stateMachine.OnEndedHit();
            }
        };
    }

    public void ApplyStun(float duration)
    {
        if (IsServer == false)
        {
            return;
        }

        StartCoroutine(StunCoroutine(duration));
    }


    //Input System
    private void OnMove(InputValue value)
    {
        if (!IsOwner)
        {
            return;
        }

        _moveInput = value.Get<float>();
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

    private void UpdateFireLayer(FireState state)
    {
        if (IsOwner)
        {
            _curFireState.Value = state;
        }

        switch (state)
        {
            case FireState.Idle:
                _animator.SetLayerWeight(1, 0f);
                break;
            case FireState.Charging:
                _animator.SetLayerWeight(1, 1f);
                break;
            case FireState.PostFire:
                //Stop Chrging Effect
                break;
        }
    }

    private IEnumerator StunCoroutine(float duration)
    {
        _isStunned.Value = true;

        // 서버에서 정확히 정해진 시간만큼 대기
        yield return new WaitForSeconds(duration);

        _isStunned.Value = false;
    }


    public void ChangeState<T>() where T : PlayerBaseState
    {
        _stateMachine.ChangeState<T>();
    }

    public void PlayAnimation(int animHash, float crossFadeTime = 0.1f)
    {
        if (animHash == _curAnimHash.Value)
        {
            return;
        }

        _curAnimHash.Value = animHash;
        _animator.CrossFade(animHash, crossFadeTime);
    }

    public void ChangeMoveDirection(float direction)
    {
        if (direction != 0)
        {
            _isFacingRight.Value = direction > 0;
            _spriteRenderer.flipX = _isFacingRight.Value;
            _direction = Mathf.Sign(direction);
        }
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

    public void TakeDamage(float damageAmount)
    {
        ApplyStun(_stunDuration);
    }
}
