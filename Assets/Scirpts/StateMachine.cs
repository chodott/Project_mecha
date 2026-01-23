using System.Collections.Generic;
using Unity.VisualScripting;
using UnityEngine;

public interface IState<in TOwner>
{
    public void Enter(TOwner owner);
    public void Exit();
    public void Update();
}

public class StateMachine<TOwner> where TOwner : MonoBehaviour
{
    private TOwner _owner;
    protected IState<TOwner> _curState;

    private Dictionary<System.Type, IState<TOwner>> _stateDicitonary = new();

    public StateMachine(TOwner owner)
    {
        _owner = owner;
    }

    public void AddState(IState<TOwner> state)
    {
        _stateDicitonary[state.GetType()] = state;
    }

    public virtual void ChangeState<T>() where T : IState<TOwner>
    {
        var type = typeof(T);
        if(_stateDicitonary.TryGetValue(type, out var nextState)) 
        {
            _curState?.Exit();
            _curState = nextState;
            _curState.Enter(_owner);
        }
 
    }

    public void Update()
    {
        _curState?.Update();
    }


    public void Reset()
    {
        _curState?.Exit();
        _curState = null;
    }
}