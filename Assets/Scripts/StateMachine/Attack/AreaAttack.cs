using System;
using System.Collections;
using UnityEngine;

public class AreaAttack : EnemyAttack
{
    [Header("Area Attack Settings")]
    [SerializeField] private SpriteRenderer spriteRenderer;
    [SerializeField] private Collider collider;
    [SerializeField] private float colliderLifeTime = 0.2f;

    private Coroutine _routine;

    private void Start()
    {
        spriteRenderer.enabled = false;
        collider.enabled = false;
    }

    public override void BeginAttack()
    {
        if (_routine != null)
            StopCoroutine(_routine);
        StartCoroutine(LifeTime());
    }

    private IEnumerator LifeTime()
    {
        spriteRenderer.enabled = true;
        collider.enabled = true;
        yield return new WaitForSeconds(colliderLifeTime);
        spriteRenderer.enabled = false;
        collider.enabled = false;
        onOver?.Invoke();
    }

    private void OnTriggerEnter(Collider other)
    {
        if (!IsValidTarget(other.gameObject, out IDamageable damageable) 
            || damageable.IsInInvincibilityRecovery())
            return;

        Attack(other.gameObject, damageable);
    }
}