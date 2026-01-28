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
            _attackAction.started -= OnStartCharging;
            _attackAction.canceled -= OnEndedCharging;
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
        if (!IsServer && IsNetworked() && !IsOwner)
        {
            return;
        }

        _stateMachine.OnMove(_moveInput);
        _stateMachine.Update();


        if (IsOwner)
        {
            SendInputServerRpc(_moveInput, _playerInput.actions["Jump"].WasPressedThisFrame(), false);
        }
    }

    protected void FixedUpdate()
    {
        if (!IsServer && IsNetworked() && !IsOwner)
        {
            return;
        }
        _stateMachine.FixedUpdate();
    }

    private void BindLocalEvents()
    {
        _attackAction = _playerInput.actions["Attack"];
        _attackAction.started += OnStartCharging;
        _attackAction.canceled += OnEndedCharging;
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

    //Network Fucntions
    private bool IsNetworked() => NetworkManager.Singleton != null && NetworkManager.Singleton.IsListening;

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

    [ServerRpc]
    private void SendInputServerRpc(float moveInput, bool jumpInput, bool rushInput)
    {
        ApplyInputs(moveInput, jumpInput, rushInput);
    }

    private void ApplyInputs(float moveInput, bool jumpInput, bool rushInput)
    {
        _moveInput = moveInput;
        _stateMachine.OnMove(moveInput);

        if (jumpInput)
        {
            _stateMachine.OnJump();
        }

        //Do Rush Input Later
    }



    public void ApplyStun(float duration)
    {
        if (IsServer == false || _isStunned.Value == true)
        {
            return;
        }

        StartCoroutine(StunCoroutine(duration));
    }


    //Input System
    private void OnMove(InputValue value)
    {
        if (!IsOwner && IsNetworked())
        {
            return;
        }

        _moveInput = value.Get<float>();
    }

    private void OnJump(InputValue value)
    {
        if (!IsOwner && IsNetworked())
        {
            return;
        }

        _stateMachine.OnJump();
    }

    private void OnStartCharging(InputAction.CallbackContext context)
    {
        _stateMachine.OnStartCharging();
    }

    private void OnEndedCharging(InputAction.CallbackContext context)
    {
        _stateMachine.OnEndedCharging();
    }

    private void UpdateFireLayer(FireState state)
    {
        if (IsOwner == true && IsNetworked() == true)
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

        yield return new WaitForSeconds(duration);

        _isStunned.Value = false;
    }


    public void ChangeState<T>() where T : PlayerBaseState
    {
        _stateMachine.ChangeState<T>();
    }

    public void PlayAnimation(int animHash, float crossFadeTime = 0.1f)
    {
        if (IsOwner && IsNetworked())
        {
            _curAnimHash.Value = animHash;
        }
        _animator.CrossFade(animHash, crossFadeTime);
    }

    public void ChangeMoveDirection(float direction)
    {
        if (direction != 0)
        {
            bool isRight = direction > 0;
            if(IsOwner && IsNetworked())
            {
                _isFacingRight.Value = direction > 0;
            }
            _spriteRenderer.flipX = isRight;
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

    public void SuperJump(float force)
    {
        _rigidBody.linearVelocityY = force;
    }

    public void OnSuperJump(float force)
    {
        _stateMachine.OnSuperJump(force);
    }

    public void Fire()
    {
        _playerAttack.Fire(_direction);
    }

    public void StartCharging()
    {
        _playerAttack.StartCharging();
    }

    public void BreakCharging()
    {
        _playerAttack.BreakCharging();
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
