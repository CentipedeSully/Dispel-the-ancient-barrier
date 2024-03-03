using Unity.Mathematics;
using UnityEngine;

public class ShootAttack : EnemyAttack
{
    [Header("Shoot Attack Settings")]
    [SerializeField] private Transform spawnTransform;
    [SerializeField] private ShootAttackProjectile bulletPrefeb;
    
    public override void BeginAttack()
    {
        GameObject instance = Instantiate(bulletPrefeb.gameObject, spawnTransform.position, Quaternion.identity);
        ShootAttackProjectile projectile = instance.GetComponent<ShootAttackProjectile>();
        projectile.Setup(this, Target.position);

        Invoke(nameof(ForceOver), 1);
    }

    private void ForceOver() => onOver?.Invoke();
}
