using System;
using UnityEngine;

public class ShootAttackProjectile : MonoBehaviour
{
    [SerializeField] private float speed = 20;
    [SerializeField] private float lifeSpan = 5;

    private Vector3 _targetDirection;
    private ShootAttack _owner;

    private void Update()
    {
        transform.Translate(-_targetDirection * (speed * Time.deltaTime));
    }

    public void Setup(ShootAttack owner, Vector3 targetPos)
    {
        _owner = owner;
        targetPos.y = transform.position.y;
        _targetDirection = (transform.position - targetPos).normalized;
        transform.rotation.SetLookRotation(_targetDirection);
        
        Invoke(nameof(End), lifeSpan);
    }

    private void End() => Destroy(gameObject);

    private void OnTriggerEnter(Collider other)
    {
        if (!_owner.IsValidTarget(other.gameObject, out IDamageable damageable) 
            || damageable.IsInInvincibilityRecovery())
            return;
        
        _owner.Attack(other.gameObject, damageable);
        Destroy(gameObject);
    }
}