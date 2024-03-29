﻿using System.Collections;
using UnityEngine;

[CreateAssetMenu(menuName = "State Machine/Enemy/Attack")]
public class Attack : StateSO
{
    // This is a wrong implementation, this should be done along side the animation
    [Header("Attack Settings")]
    public float attackAfter = 1f;
    
    public override IState Initialize(MonoMachine machine) => new AttackHandler(machine, this);
}

public class AttackHandler : StateHandler<Attack>
{
    private Coroutine _attackRoutine;
    
    public AttackHandler(MonoMachine machine, Attack data) : base(machine, data)
    {
        machine.Agent.isStopped = true;
        _attackRoutine = machine.StartCoroutine(AttackRoutine());
    }

    public override void Exit()
    {
        base.Exit();
        if (_attackRoutine != null)
            _machine.StopCoroutine(_attackRoutine);
    }

    protected virtual IEnumerator AttackRoutine()
    {
        yield return new WaitForSeconds(_data.attackAfter);
        _machine.AttackObject.Target = _machine.Target;
        _machine.AttackObject.BeginAttack();
        
        _attackRoutine = null;
    }
}