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

    public virtual void OnRespawn(Vector3 targetPos) { }
}


public class RushFallState : RushBaseState
{
    public override void Enter(RushController owner)
    {
        base.Enter(owner);
        _controller.PlayAnimation(RushAnim.Spawn);
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
        AnimatorStateInfo info =  _controller.GetAnimStateInfo();
        if(info.IsName("Landing") && info.normalizedTime >= 0.95f)
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
        _controller.PlayAnimation(RushAnim.Idle);
    }

    public override void OnTriggerStay(Collider2D collision)
    {
        bool result = _controller.CheckPlayerJump(collision);
        //if(result == true)
        //{
        //    _controller.ChangeState<RushUsedState>();
        //}
    }

    public override void OnRespawn(Vector3 targetPos) 
    {
        _controller.SpawnToTarget(targetPos);
        _controller.ChangeState<RushFallState>();
    }

}

public class RushUsedState : RushBaseState
{
    public override void Enter(RushController owner)
    {
        base.Enter(owner);
        _controller.PlayAnimation(RushAnim.Used);
    }

    public override void OnRespawn(Vector3 targetPos)
    {
        _controller.SpawnToTarget(targetPos);
        _controller.ChangeState<RushFallState>();
    }
}
