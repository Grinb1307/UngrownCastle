using UnityEngine;

public abstract class IceHeroes : Heroes
{
    [Header("Ice Attack")]
    [SerializeField] protected GameObject iceProjectilePrefab;
    [SerializeField] protected Transform firePoint;
    [SerializeField] protected float projectileSpeed = 10f;

    protected Vector2 aimDirection = Vector2.right;

    // Xác định hướng ngắm
    protected override void UpdateAim()
    {
        if (target == null || firePoint == null)
            return;

        aimDirection =
            ((Vector2)target.position - (Vector2)firePoint.position).normalized;
    }

    // Đánh thường
    protected override void PerformAttack()
    {
        ShootIceProjectile();
    }

    protected virtual void ShootIceProjectile()
    {
        if (target == null ||
            iceProjectilePrefab == null ||
            firePoint == null)
            return;

        GameObject projectile = Instantiate(
            iceProjectilePrefab,
            firePoint.position,
            Quaternion.identity
        );

        IceProjectile ice =
            projectile.GetComponent<IceProjectile>();

        if (ice != null)
        {
            ice.Initialize(
                target,
                CalculateDamage(),
                projectileSpeed
            );
        }
        else
        {
            Debug.LogError("Ice Projectile thiếu IceProjectile!");
            Destroy(projectile);
        }
    }
}