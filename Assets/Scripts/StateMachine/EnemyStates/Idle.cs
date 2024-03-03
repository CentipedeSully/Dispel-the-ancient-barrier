using UnityEngine;

[CreateAssetMenu(menuName = "State Machine/Enemy/Idle")]
public class Idle : StateSO
{
    public override IState Initialize(MonoMachine machine) => new IdleHandler(machine, this);
}

public class IdleHandler : StateHandler<Idle>
{
    public IdleHandler(MonoMachine machine, Idle data) : base(machine, data)
    {
        machine.Agent.isStopped = true;
    }
}