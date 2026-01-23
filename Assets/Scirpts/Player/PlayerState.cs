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
}


public class PlayerIdleState : PlayerBaseState
{

    public override void OnMove(float x) 
    {
        _controller.ChangeState<PlayerRunState>();
    }
}


public class PlayerRunState: PlayerBaseState
{
    public override void Update()
    {
        _controller.Move();
    }
}
