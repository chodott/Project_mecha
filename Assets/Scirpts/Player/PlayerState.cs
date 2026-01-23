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
}


public class PlayerIdleState : PlayerBaseState
{
    public override void Enter(PlayerController owner)
    {
        base.Enter(owner);
        PlayAnim(PlayerAnim.Idle, 0);
    }

    public override void OnMove(float x) 
    {
        if (Mathf.Abs(x) > 0.01f) 
        {
            _controller.ChangeState<PlayerRunState>();
        }
    }
}


public class PlayerRunState: PlayerBaseState
{
    public override void Enter(PlayerController owner)
    {
        base.Enter(owner);
        PlayAnim(PlayerAnim.Run);
    }

    public override void OnMove(float x)
    {
        if(Mathf.Abs(x) < 0.01f)
        {
            if(Mathf.Abs(_controller.Velocity) < 0.01f)
            {
                _controller.ChangeState<PlayerIdleState>();
                return;
            }
        }

        _controller.Move();
    }
}
