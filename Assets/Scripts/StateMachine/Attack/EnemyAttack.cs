using System.Collections.Generic;
using Cinemachine;
using UnityEngine;
using UnityEngine.Events;

public abstract class EnemyAttack : MonoBehaviour
{
    [Header("Damage Utilities")]
    [SerializeField, Min(0)] protected float damage = 20;
    [SerializeField, Min(0)] protected float stunDuration = .25f;
    [Tooltip("This is an impulse force")]
    [SerializeField, Min(0)] protected float pushBackForce = 2000f;
    
    [Header("Collision-filtering utilities")]
    [SerializeField, TagField] protected List<string> validTags;
    
    [Header("Events")]
    [SerializeField] public UnityEvent onOver;
    
    public Transform Target { get; set; }
    
    public abstract void BeginAttack();

    public virtual bool IsValidTarget(GameObject target, out IDamageable damageable)
    {
        damageable = null;
        return validTags.Contains(target.tag) && target.TryGetComponent(out damageable);
    }
    
    public void Attack(GameObject target, IDamageable damageable)
    {
        damageable.ModifyHealth(-damage);
        damageable.SufferStun(stunDuration);

        if (target.TryGetComponent<Rigidbody>(out var targetRB))
        {
            //Calculate the direction that's towards the touched object. NORMALIZE it
            Vector3 pushDirection = (target.transform.position - transform.position).normalized;

            //Apply the push
            targetRB.AddForce(pushDirection * pushBackForce * Time.deltaTime, ForceMode.Impulse);
        }
    }
    
}