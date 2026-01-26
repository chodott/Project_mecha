using UnityEngine;

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

    public virtual void Update(){}

    public virtual void OnMove(float x) { }
    public virtual void OnJump() { }

    protected void PlayAnim(int animHash, float crossFadeTime = 0.1f)
    {
        _controller.PlayAnimation(animHash, crossFadeTime);
    }

    public virtual void FixedUpdate() { }
}


public class PlayerIdleState : PlayerBaseState
{
    public override void Enter(PlayerController owner)
    {
        base.Enter(owner);
        PlayAnim(PlayerAnim.Idle, 0.1f);
    }

    public override void OnMove(float x) 
    {
        if (Mathf.Abs(x) > 0.01f) 
        {
            _controller.ChangeState<PlayerRunState>();
        }
    }

    public override void OnJump()
    {
        _controller.ChangeState<PlayerJumpState>();
    }
}


public class PlayerRunState: PlayerBaseState
{
    public override void Enter(PlayerController owner)
    {
        base.Enter(owner);
        PlayAnim(PlayerAnim.Run, 0.1f);
    }

    public override void OnMove(float x)
    {
        if(Mathf.Abs(x) < 0.01f)
        {
            if(Mathf.Abs(_controller.Velocity.x) < 0.01f)
            {
                _controller.ChangeState<PlayerIdleState>();
                return;
            }
        }

        _controller.Move();
    }

    public override void OnJump()
    {
        _controller.ChangeState<PlayerJumpState>();
    }
}

public class PlayerJumpState : PlayerBaseState
{
    public override void Enter(PlayerController owner)
    {
        base.Enter(owner);
        PlayAnim(PlayerAnim.Jump);

        _controller.Jump();
    }

    public override void Update()
    {
        if(_controller.Velocity.y < -0.1f)
        {
            _controller.ChangeState<PlayerFallState>();
        }
    }
}

public class PlayerFallState : PlayerBaseState
{
    public override void Update()
    {
        if(_controller.IsGrounded() == true)
        {
            _controller.ChangeState<PlayerLandingState>();
        }
    }
}

public class PlayerLandingState : PlayerBaseState
{
    public override void Enter(PlayerController owner)
    {
        base.Enter(owner);
        PlayAnim(PlayerAnim.Landing, 0.1f);
    }
    public override void Update()
    {
        var stateInfo = _controller.GetAnimStateInfo();
        if(stateInfo.shortNameHash == PlayerAnim.Landing && stateInfo.normalizedTime >= 0.9f)
        {
            if (Mathf.Abs(_controller.Velocity.x) < 0.1f)
            {
                _controller.ChangeState<PlayerIdleState>();
            }
            else
            {
                _controller.ChangeState<PlayerRunState>();
            }
        }
    }
}
