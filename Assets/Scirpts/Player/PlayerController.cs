using System;
using System.Collections;
using Unity.Netcode;
using UnityEngine;
using UnityEngine.InputSystem;

public class PlayerController : NetworkBehaviour, IDamageable
{
    #region SerializeField
    [SerializeField]
    private Animator _animator;
    [SerializeField]
    private SpriteRenderer _spriteRenderer;

    [SerializeField]
    private float _stunDuration = 1f;

    #endregion

    #region
    private PlayerInput _playerInput;
    private InputAction _attackAction;
    private PlayerAttack _playerAttack;
    private PlayerMovement _playerMovement;
    private PlayerSummonHandler _playerSummonHandler;
    private PlayerStateMachine _stateMachine;
    private float _moveInput;
    private float _direction = -1;
    #endregion

    #region Properties
    public Vector2 Velocity => _playerMovement.Velocity;
    public bool IsTouchCeiling => _playerMovement.IsTouchCeiling();
    public bool IsGrounded => _playerMovement.IsGrounded();


    #endregion

    #region NetworkVariables
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
    #endregion

    #region Unity Method
    private void Awake()
    {
        _playerAttack = GetComponent<PlayerAttack>();
        _playerMovement = GetComponent<PlayerMovement>();
        _playerInput = GetComponent<PlayerInput>();
        _playerSummonHandler = GetComponent<PlayerSummonHandler>();
    }

    private void OnEnable()
    {
        if (IsSpawned == true)
        {
            BindLocalEvents();
        }

        EventBus.OnWin += HandleGameWin;
        EventBus.OnLose += HandleGameLosed;
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
    #endregion



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

    #region Network Function
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
    #endregion

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


    #region Player Input Events
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
    #endregion

    private void HandleGameWin()
    {
        _stateMachine.OnGameEnded(true);
    }

    private void HandleGameLosed()
    {
        _stateMachine.OnGameEnded(false);
    }


    #region Movement Method
    public void ApplyMovement()
    {
        _playerMovement.ApplyMovement(_moveInput);
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
        _playerMovement.Jump();
    }

    public void SuperJump()
    {
        _playerMovement.SuperJump();
    }
    #endregion

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

    public void TrySpawnRush()
    {
        _playerSummonHandler.TrySpawnRush(transform.position);
    }

    public AnimatorStateInfo GetAnimStateInfo()
    {
        return _animator.GetCurrentAnimatorStateInfo(0);
    }

    #region IDamagable Interface
    public void TakeDamage(float damageAmount)
    {
        ApplyStun(_stunDuration);
    }
    #endregion
}
