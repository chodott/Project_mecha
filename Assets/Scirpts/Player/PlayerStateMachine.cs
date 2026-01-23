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

        if( _curPlayerState == null )
        {
            UnityEngine.Debug.LogError($"[PlayerStateMachine] 상태 전환 오류: {typeof(T).Name}은(는) PlayerBaseState를 상속받지 않았습니다. 입력을 처리할 수 없습니다.");
        }
    }

    public void OnMove(float x) => _curPlayerState?.OnMove(x);
    public void OnJump() => _curPlayerState?.OnJump();
}