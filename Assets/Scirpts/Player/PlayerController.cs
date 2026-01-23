using UnityEngine;
using UnityEngine.Animations;
using UnityEngine.InputSystem;

public class PlayerController : MonoBehaviour
{
    enum EPlayerState
    {
        idle,
        run,
        jump
    }
    private float _moveInput;
    private EPlayerState _curState;


    [SerializeField]
    private Animator _animator;
    [SerializeField]
    private SpriteRenderer _spriteRenderer;

    protected void Start()
    {
        _animator = GetComponent<Animator>();
    }

    protected void LateUpdate()
    {
        if (_moveInput != 0)
        {
            _curState = EPlayerState.run;
        }
        else
        {
            _curState = EPlayerState.idle;
        }
    }


    private void OnMove(InputValue value)
    {
        _moveInput = value.Get<float>();
        _animator.SetBool("Direction", _moveInput != 0);

        if (_moveInput != 0)
        {
            _spriteRenderer.flipX = _moveInput > 0;
        }
    }
}
