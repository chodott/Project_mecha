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

    public Vector2 Velocity { get { return _rigidBody.linearVelocity; } }

    //Stat
    [SerializeField]
    private float _movingSpeed = 5f;
    [SerializeField]
    private float _jumpForce = 3f;
    [SerializeField]
    private float _fireCoolDown = 0.03f;


    //Collision
    [SerializeField] private LayerMask _groundLayer;    // 바닥 레이어
    [SerializeField] private Transform _groundCheckPos; // 발밑에 배치한 빈 오브젝트
    [SerializeField] private Vector2 _groundCheckSize = new Vector2(0.3f, 0.1f); // 박스 크기


    //Bullet
    [SerializeField] GameObject _defaultBullet;
    [SerializeField] GameObject _fullChargeBullet;
    [SerializeField] private float _fullChargeTime;
    private float _curChargeTime;


    private PlayerStateMachine _stateMachine;

    //Shooting
    [SerializeField]
    private Transform _muzzleTransform;
    private bool _isShooting = false;

    private void Awake()
    {
        _stateMachine = new PlayerStateMachine(this);

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
        StillCharge();
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
        if (_isShooting)
        {
            return;
        }

        _isShooting = true;
        _animator.SetLayerWeight(1, 1f);
    }

    private void OnAttackCanceled(InputAction.CallbackContext context)
    {
        StartCoroutine(FirePoseRoutine());

        GameObject launchedMissile;
        if (_curChargeTime >= _fullChargeTime)
        {
            launchedMissile = Instantiate(_fullChargeBullet, _muzzleTransform.position, _muzzleTransform.rotation);
        }
        else
        {
            launchedMissile = Instantiate(_defaultBullet, _muzzleTransform.position, _muzzleTransform.rotation);
        }

        Vector3 directionVector = transform.right * _direction;
        Vector3 muzzlePosition = _muzzleTransform.position;

        float xGap = MathF.Abs(muzzlePosition.x - transform.position.x);

        Vector3 launchPosition = new Vector3(transform.position.x + _direction * xGap , muzzlePosition.y, muzzlePosition.z);
        _curChargeTime = 0;
        launchedMissile.GetComponent<BaseBullet>().Launch(launchPosition, directionVector);

    }

    private IEnumerator FirePoseRoutine()
    {
        yield return new WaitForSeconds(_fireCoolDown);

        _animator.SetLayerWeight(1, 0f);
        _isShooting = false;
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

    public void StillCharge()
    {
        if (_isShooting)
        {
            _curChargeTime += Time.deltaTime;
        }
    }

    public AnimatorStateInfo GetAnimStateInfo()
    {
        return _animator.GetCurrentAnimatorStateInfo(0);
    }
}
