using System;using System.Collections.Generic;
using UnityEngine;

public abstract class StateSO : ScriptableObject
{
    public List<StateTransition> transitions;
    
    public abstract IState Initialize(MonoMachine machine);
}

public class StateHandler<TState> : IState where TState : StateSO
{
    protected readonly MonoMachine _machine;
    protected readonly TState _data;
    
    public StateHandler(MonoMachine machine, TState stateSo)
    {
        _machine = machine;
        _data = stateSo;

        stateSo.transitions.ForEach(transition => transition.Start(_machine));
    }

    public virtual void Update()
    {
        
    }

    public virtual void FixedUpdate()
    {
        
    }

    public virtual void Exit()
    {
        _data.transitions.ForEach(transition => transition.Stop());
    }

    public string Name => _data.name;
}

public interface IState
{
    void Update();
    void FixedUpdate();
    void Exit();
    string Name { get; }
}

public class NamedAction
{
    public string name;
    public Action action;
}