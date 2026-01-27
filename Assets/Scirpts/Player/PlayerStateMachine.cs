using System.Diagnostics;
using UnityEngine;

public class PlayerStateMachine : StateMachine<PlayerController>
{
    private PlayerBaseState _curPlayerState;

    public PlayerStateMachine(PlayerController owner) : base(owner) { }

    public override void ChangeState<T>()
    {
        base.ChangeState<T>();
        // cache
        _curPlayerState = _curState as PlayerBaseState;
    }

    public void OnMove(float x) => _curPlayerState?.OnMove(x);
    public void OnJump() => _curPlayerState?.OnJump();

    public void OnHit() => _curPlayerState?.OnHit();
    public void OnEndedHit() => _curPlayerState?.OnEndedHit();
}