using System.Collections.Generic;
using UnityEngine;

public abstract class StateSO : ScriptableObject
{
    public List<StateTransition> transitions;
    public string animationName;
    
    public abstract IState Initialize(MonoMachine machine);
}

public class StateHandler<TState> : IState where TState : StateSO
{
    protected List<StateTransition> _transitions;
    
    protected readonly MonoMachine _machine;
    protected readonly TState _data;
    
    public StateHandler(MonoMachine machine, TState stateSo)
    {
        _machine = machine;
        _data = stateSo;

        _transitions = new List<StateTransition>();
        stateSo.transitions.ForEach(transition =>
        {
            var newItem = new StateTransition(transition);
            newItem.Start(machine);
            _transitions.Add(newItem);
        });
        
        _machine.Animator.SetTrigger(stateSo.animationName);
    }

    public virtual void Update()
    {
        
    }

    public virtual void FixedUpdate()
    {
        
    }

    public virtual void Exit() => _transitions.ForEach(transition => transition.Stop());

    public string Name => _data.name;
}

public interface IState
{
    void Update();
    void FixedUpdate();
    void Exit();
    string Name { get; }
}