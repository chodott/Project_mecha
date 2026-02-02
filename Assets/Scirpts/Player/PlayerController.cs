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
    private PlayerSummonHandler _playerSummonHandler;

    public Vector2 Velocity { get { return _rigidBody.linearVelocity; } }

    //Stat
    [SerializeField]
    private float _movingSpeed = 5f;
    [SerializeField]
    private float _jumpForce = 3f;
    [SerializeField]
    private float _superJumpForce = 10f;
    [SerializeField]
    private float _stunDuration = 1f;
    [SerializeField]
    private float _gravity = 9.81f;


    //Collision
    [SerializeField] private LayerMask _groundLayer;    // 바닥 레이어
    [SerializeField] private Transform _groundCheckPos; // 발밑에 배치한 빈 오브젝트
    [SerializeField] private Vector2 _groundCheckSize = new Vector2(0.3f, 0.1f); // 박스 크기
    [SerializeField] private Transform _ceilCheckPos;   // 머리 위에 배치한 빈 오브젝트
    [SerializeField] private Vector2 _ceilCheckSize = new Vector2(0.3f, 0.1f);   // 박스 크기

    private PlayerStateMachine _stateMachine;
    private float _verticalVelocity;

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
        _playerSummonHandler = GetComponent<PlayerSummonHandler>();
    }

    private void OnEnable()
    {
        if (IsSpawned == true)
        {
            BindLocalEvents();
        }

        EventBus.OnGameEnded += HandleGameEnd;
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
        _stateMachine.AddState(new PlayerWinState());

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
            SendInputServerRpc(
                _moveInput,
                _playerInput.actions["Jump"].WasPressedThisFrame(),
                _playerInput.actions["CallRush"].WasPressedThisFrame()
                );
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
        _spriteRenderer.flipX = _isFacingRight.Value;

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

        if (rushInput)
        {
            _stateMachine.OnCallRush();
        }
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

    private void OnCallRush(InputValue value)
    {
        if (!IsOwner && IsNetworked())
        {
            return;
        }

        _stateMachine.OnCallRush();
    }

    private void OnStartCharging(InputAction.CallbackContext context)
    {
        _stateMachine.OnStartCharging();
    }

    private void OnEndedCharging(InputAction.CallbackContext context)
    {
        _stateMachine.OnEndedCharging();
    }

    private void HandleGameEnd(GameResultArgs args)
    {
        bool isWinner = NetworkManager.LocalClientId == args.WinnerID;
        _stateMachine.OnGameEnded(isWinner);
    }

    private void ApplyGravity()
    {
        if (IsGrounded() && _verticalVelocity <= 0)
        {
            _verticalVelocity = 0.0f;
        }
        else
        {
            _verticalVelocity -= _gravity * Time.deltaTime;
        }
    }

    public void ApplyMovement()
    {
        ApplyGravity();
        float xVelocity = _moveInput * _movingSpeed;
        _rigidBody.linearVelocity = new Vector2(xVelocity, _verticalVelocity);
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
        Debug.Log($"State Changed to {typeof(T).Name}");
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
            if (IsOwner && IsNetworked())
            {
                _isFacingRight.Value = direction > 0;
            }
            _spriteRenderer.flipX = isRight;
            _direction = Mathf.Sign(direction);
        }
    }

    public void Jump()
    {
        _verticalVelocity = _jumpForce;
    }

    public void SuperJump()
    {
        _verticalVelocity = _superJumpForce;
    }

    public void OnSuperJump()
    {
        _stateMachine.OnSuperJump();
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

    public bool IsTouchCeiling()
    {
        if(Physics2D.OverlapBox(_ceilCheckPos.position, _ceilCheckSize, 0, _groundLayer))
        {
            _verticalVelocity = 0;
            _rigidBody.linearVelocity = new Vector2(_rigidBody.linearVelocity.x, 0);
            return true;
        }
        return false;
    }

    private void OnDrawGizmos()
    {
        Gizmos.color = Color.red;
        Gizmos.DrawWireCube(_groundCheckPos.position, _groundCheckSize);
        Gizmos.DrawWireCube(_ceilCheckPos.position, _ceilCheckSize);
    }

    public void TrySpawnRush()
    {
        _playerSummonHandler.TrySpawnRush(transform.position);
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
