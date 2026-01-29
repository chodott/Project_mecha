using System;
using Unity.Collections.LowLevel.Unsafe;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.XR;

public abstract class RushBaseState : IState<RushController>
{
    protected RushController _controller;

    public virtual void Enter(RushController owner)
    {
        _controller = owner;
    }

    public virtual void Exit()
    {
        _controller = null;
    }

    public virtual void FixedUpdate() { }

    public virtual void Update() { }

    public virtual void OnTriggerStay(Collider2D collision) { }
}


public class RushFallState : RushBaseState
{
    public override void Enter(RushController owner)
    {
        base.Enter(owner);
    }

    public override void Update()
    {
        //Fall down
        _controller.FallDown();
        if(_controller.IsLandingComplete() == true)
        {
            _controller.ChangeState<RushLandingState>();
        }
    }
  
}

public class RushLandingState : RushBaseState
{
    public override void Enter(RushController owner)
    {
        base.Enter(owner);
        _controller.PlayAnimation(RushAnim.Landing);
    }
    public override void Update()
    {
        //Check Animation End

        if(true)
        {
            _controller.ChangeState<RushIdleState>();
        }
    }
}

public class RushIdleState : RushBaseState
{
    public override void Enter(RushController owner)
    {
        base.Enter(owner);
    }

    public override void OnTriggerStay(Collider2D collision)
    {
        _controller.CheckPlayerJump(_controller.GetComponent<Collider2D>());
    }
}
