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
    private float _cachedSpeed;
    private float _timer;
    
    public FollowTargetHandler(MonoMachine machine, FollowTarget data) : base(machine, data)
    {
        machine.Agent.isStopped = false;
        _cachedSpeed = machine.Agent.speed;
        machine.Agent.speed = _cachedSpeed * data.speedScale;
        _timer = 0;
    }

    public override void Update()
    {
        base.Update();

        if (_timer > 0)
        {
            _timer -= Time.deltaTime;
            return;
        }

        _timer += _data.updateInterval;
        _machine.Agent.SetDestination(_machine.Target.position);
    }

    public override void Exit()
    {
        base.Exit();
        _machine.Agent.speed = _cachedSpeed;
    }
}