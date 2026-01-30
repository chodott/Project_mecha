using System;
using Unity.Collections.LowLevel.Unsafe;
using UnityEngine;
using UnityEngine.EventSystems;

public abstract class PlayerBaseState : IState<PlayerController>
{
    protected PlayerController _controller;

    public virtual void Enter(PlayerController owner)
    {
        _controller = owner;
    }

    public virtual void Exit()
    {
        _controller = null;
    }

    public virtual void Update() { }

    public virtual void OnMove(float direction)
    {
        if (Mathf.Abs(direction) > 0.01f)
        {
            _controller.ChangeMoveDirection(direction);
        }
    }
    public virtual void OnJump() { }

    protected void PlayAnim(int animHash, float crossFadeTime = 0)
    {
        _controller.PlayAnimation(animHash, crossFadeTime);
    }

    public virtual void FixedUpdate() { }

    public virtual void OnHit() { }

    public virtual void OnEndedHit() { }

    public virtual void OnStartCharging()
    {
        _controller.StartCharging();
    }
    public virtual void OnEndedCharging()
    {
        _controller.Fire();
    }

    public virtual void OnSuperJump() { }
    public virtual void OnCallRush()
    {
        _controller.TrySpawnRush();
    }

}


public class PlayerIdleState : PlayerBaseState
{
    public override void Enter(PlayerController owner)
    {
        base.Enter(owner);
        PlayAnim(PlayerAnim.Idle);
    }

    public override void OnMove(float direction)
    {
        if (Mathf.Abs(direction) > 0.01f)
        {
            _controller.ChangeMoveDirection(direction);
            _controller.ChangeState<PlayerRunState>();
        }
    }

    public override void OnJump()
    {
        _controller.ChangeState<PlayerJumpState>();
    }

    public override void OnHit()
    {
        _controller.ChangeState<PlayerStunState>();
    }
}


public class PlayerRunState : PlayerBaseState
{
    public override void Enter(PlayerController owner)
    {
        base.Enter(owner);
        PlayAnim(PlayerAnim.Run);
    }

    public override void OnMove(float direction)
    {
        if (Mathf.Abs(direction) < 0.01f)
        {
            if (Mathf.Abs(_controller.Velocity.x) < 0.01f)
            {
                _controller.ChangeState<PlayerIdleState>();
                return;
            }
        }

        base.OnMove(direction);
    }

    public override void FixedUpdate()
    {
        _controller.ApplyMovement();
    }

    public override void OnJump()
    {
        _controller.ChangeState<PlayerJumpState>();
    }

    public override void OnHit()
    {
        _controller.ChangeState<PlayerStunState>();
    }
}

public class PlayerJumpState : PlayerBaseState
{
    private bool _needJump = true;
    public override void Enter(PlayerController owner)
    {
        base.Enter(owner);
        PlayAnim(PlayerAnim.Jump);
        _needJump = true;

    }

    public override void Update()
    {
        _controller.IsTouchCeiling();

        if (_controller.Velocity.y < -0.1f)
        {
            _controller.ChangeState<PlayerFallState>();
        }
    }

    public override void FixedUpdate()
    {
        if (_needJump == true)
        {
            _controller.Jump();
            _needJump = false;
        }
        _controller.ApplyMovement();
    }

    public override void OnHit()
    {
        _controller.ChangeState<PlayerStunState>();
    }
}

public class PlayerFallState : PlayerBaseState
{
    private bool _isSuperJumping = false;
    public override void Update()
    {
        if(_isSuperJumping && _controller.IsTouchCeiling())
        {
            _isSuperJumping = false;
        }

        if (_controller.IsGrounded() == true)
        {
            _controller.ChangeState<PlayerLandingState>();
        }
    }

    public override void FixedUpdate()
    {
        _controller.ApplyMovement();
    }

    public override void OnHit()
    {
        _controller.ChangeState<PlayerStunState>();
    }

    public override void OnSuperJump()
    {
        _controller.SuperJump();
        _isSuperJumping = true;
    }
}

public class PlayerLandingState : PlayerBaseState
{
    public override void Enter(PlayerController owner)
    {
        base.Enter(owner);
        PlayAnim(PlayerAnim.Landing);
    }
    public override void Update()
    {
        var stateInfo = _controller.GetAnimStateInfo();
        if (stateInfo.shortNameHash == PlayerAnim.Landing && stateInfo.normalizedTime >= 0.9f)
        {
            _controller.ChangeState<PlayerRunState>();
        }
    }

    public override void FixedUpdate()
    {
        _controller.ApplyMovement();
    }

    public override void OnHit()
    {
        _controller.ChangeState<PlayerStunState>();
    }
}


public class PlayerStunState : PlayerBaseState
{
    public override void Enter(PlayerController owner)
    {
        base.Enter(owner);
        PlayAnim(PlayerAnim.Stun);
        _controller.BreakCharging();
    }

    public override void OnMove(float direction)
    {
        //Do nothing Input
    }

    public override void OnEndedHit()
    {
        _controller.ChangeState<PlayerIdleState>();
    }

    public override void OnStartCharging()
    {
        //Do nothing Input
    }

    public override void OnEndedCharging()
    {
        _controller.BreakCharging();
    }
}