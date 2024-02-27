using System.Collections;
using UnityEngine;
using UnityEngine.AI;

[CreateAssetMenu(menuName = "State Machine/Enemy/Follow Target")]
public class FollowTarget : StateSO
{
    [Header("Follow Settings")]
    public float speedScale = 1;
    public float updateInterval = 0.1f;

    public override IState Initialize(MonoMachine machine) => new FollowTargetHandler(machine, this);
}

public class FollowTargetHandler : StateHandler<FollowTarget>
{
    private Coroutine _followRoutine;
    private float _cachedSpeed;
    
    public FollowTargetHandler(MonoMachine machine, FollowTarget data) : base(machine, data)
    {
        machine.Agent.isStopped = false;
        _cachedSpeed = machine.Agent.speed;
        machine.Agent.speed = _cachedSpeed * data.speedScale;
        
        _followRoutine = machine.StartCoroutine(FollowRoutine());
    }

    public override void Exit()
    {
        base.Exit();
        _machine.Agent.speed = _cachedSpeed;
        _machine.StopCoroutine(_followRoutine);
    }

    private IEnumerator FollowRoutine()
    {
        NavMeshAgent agent = _machine.Agent;
        
        while (true)
        {
            agent.SetDestination(_machine.Target.position);
            yield return new WaitForSeconds(_data.updateInterval);
        }
        
        // ReSharper disable once IteratorNeverReturns
    }
}