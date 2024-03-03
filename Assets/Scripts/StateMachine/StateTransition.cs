using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[Serializable]
public class StateTransition
{
    public StateSO state;

    public List<StateEvent> stateEvents;
    
    public ConditionalsComparison agentDistanceComparison;
    public float agentDistance;
    public ConditionalsComparison targetDistanceComparison;
    public float targetDistance;
    public bool waitThenSetState;
    public float waitThenSetStateTime = 0;

    private MonoMachine _machine;
    private bool _invokedOnce;

    protected List<Coroutine> _routines = new List<Coroutine>();

    public StateTransition(StateTransition transition)
    {
        state = transition.state;
        stateEvents = transition.stateEvents;
        agentDistanceComparison = transition.agentDistanceComparison;
        agentDistance = transition.agentDistance;
        targetDistanceComparison = transition.targetDistanceComparison;
        targetDistance = transition.targetDistance;
        waitThenSetState = transition.waitThenSetState;
        waitThenSetStateTime = transition.waitThenSetStateTime;
    }

    public void Start(MonoMachine machine)
    {
        _invokedOnce = false;
        
        _machine = machine;
        _routines.Add(machine.StartCoroutine(CheckComparisons()));
        _routines.Add(machine.StartCoroutine(WaitThenSetState()));
        stateEvents.ForEach(Listen);
    }

    public void Stop()
    {
        _routines.ForEach(routine =>
        {
            if (routine != null)
                _machine.StopCoroutine(routine);
        });
        _routines.Clear();
        
        stateEvents.ForEach(StopListening);
    }

    public void SetState()
    {
        if (_invokedOnce)
            return;

        _invokedOnce = true;
        _machine.SetState(state);
    }

    private void Listen(StateEvent stateEvent)
    {
        switch (stateEvent)
        {
            case StateEvent.ON_FINISH_ATTACK:
                _machine.AttackObject.onOver.AddListener(SetState);
                break;
        }
    }

    private void StopListening(StateEvent stateEvent)
    {
        switch (stateEvent)
        {
            case StateEvent.ON_FINISH_ATTACK:
                _machine.AttackObject.onOver.RemoveListener(SetState);
                break;
        }
    }
    
    
    public virtual void SetState<T>(T _) => SetState();

    private IEnumerator CheckFor(Func<bool> condition)
    {
        while (true)
        {
            yield return new WaitUntil(condition);
            SetState();
            yield return null;
        }
    }

    private IEnumerator CheckComparisons()
    {
        yield return null;
        yield return new WaitUntil(() => agentDistanceComparison.Check(_machine.Agent.remainingDistance, agentDistance)
            && targetDistanceComparison.Check(Vector3.Distance(_machine.transform.position, _machine.Target.position), targetDistance));
        SetState();
    }

    private IEnumerator WaitThenSetState()
    {
        if (!waitThenSetState)
            yield break;

        yield return new WaitForSeconds(waitThenSetStateTime);
        SetState();
    }
}

public static class ConditionalChecking
{
    /// <param name="valueToEvaluate">
    /// Value should be defaulted to true.
    /// </param>
    /// <returns></returns>
    public static bool Check(this ConditionalsBool conditionalsBool, bool valueToEvaluate)
    {
        return conditionalsBool switch
        {
            ConditionalsBool.HALT => false,
            ConditionalsBool.TRUE => valueToEvaluate,
            ConditionalsBool.FALSE => !valueToEvaluate,
            _ => true
        };
    }
        
    public static bool Check(this ConditionalsComparison conditionalsComparison, float valueToEvaluate, float comparedValue)
    {
        return conditionalsComparison switch
        {
            ConditionalsComparison.HALT => false,
            ConditionalsComparison.GREATER_THAN => valueToEvaluate > comparedValue,
            ConditionalsComparison.LESS_THAN => valueToEvaluate < comparedValue,
            ConditionalsComparison.EQUAL => Mathf.Approximately(valueToEvaluate, comparedValue),
            _ => true
        };
    }
}
    
public enum ConditionalsBool
{
    IGNORE,
    TRUE,
    FALSE,
    HALT
}
    
public enum ConditionalsComparison
{
    IGNORE,
    GREATER_THAN,
    LESS_THAN,
    EQUAL,
    HALT
}

public enum StateEvent
{
    ON_FINISH_ATTACK
}