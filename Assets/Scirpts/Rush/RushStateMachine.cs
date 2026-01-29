using UnityEngine;

public class RushStateMachine : StateMachine<RushController>
{
    private RushBaseState _curRushState;
    public RushStateMachine(RushController owner) : base(owner) { }

    public override void ChangeState<T>()
    {
        base.ChangeState<T>();
        // cache
        _curRushState = _curState as RushBaseState;
       
    }

    public virtual void OnRespawn(Vector3 targetPos)
    {
        if (_curRushState == null)
        {
            ChangeState<RushIdleState>();
        }
        _curRushState.OnRespawn(targetPos);
    }

    public virtual void OnTriggerStay(Collider2D collision) => _curRushState?.OnTriggerStay(collision);

}
