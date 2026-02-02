using UnityEngine;

public class PlayerMovement : MonoBehaviour
{
    //Stat
    [SerializeField]
    private float _movingSpeed = 5f;
    [SerializeField]
    private float _jumpForce = 3f;
    [SerializeField]
    private float _superJumpForce = 10f;
    [SerializeField]
    private float _gravity = 9.81f;

    [SerializeField] private Rigidbody2D _rigidBody;
    [SerializeField] private LayerMask _groundLayer;   
    [SerializeField] private Transform _groundCheckPos;
    [SerializeField] private Vector2 _groundCheckSize = new Vector2(0.3f, 0.1f); 
    [SerializeField] private Transform _ceilCheckPos;  
    [SerializeField] private Vector2 _ceilCheckSize = new Vector2(0.3f, 0.1f);


    private float _verticalVelocity;
    public Vector2 Velocity { get { return _rigidBody.linearVelocity; } }

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

    public void ApplyMovement(float moveInput)
    {
        ApplyGravity();
        float xVelocity = moveInput * _movingSpeed;
        _rigidBody.linearVelocity = new Vector2(xVelocity, _verticalVelocity);
    }

    public bool IsGrounded()
    {
        return Physics2D.OverlapBox(_groundCheckPos.position, _groundCheckSize, 0, _groundLayer);
    }

    public bool IsTouchCeiling()
    {
        if (Physics2D.OverlapBox(_ceilCheckPos.position, _ceilCheckSize, 0, _groundLayer))
        {
            _verticalVelocity = 0;
            _rigidBody.linearVelocity = new Vector2(_rigidBody.linearVelocity.x, 0);
            return true;
        }
        return false;
    }

    public void Jump()
    {
        _verticalVelocity = _jumpForce;
    }

    public void SuperJump()
    {
        _verticalVelocity = _superJumpForce;
    }

    #region Debug Method
    private void OnDrawGizmos()
    {
        Gizmos.color = Color.red;
        Gizmos.DrawWireCube(_groundCheckPos.position, _groundCheckSize);
        Gizmos.DrawWireCube(_ceilCheckPos.position, _ceilCheckSize);
    }
    #endregion
}
