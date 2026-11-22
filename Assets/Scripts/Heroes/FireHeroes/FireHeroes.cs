using UnityEngine;

public abstract class FireHeroes : Heroes
{
    [Header("Fire Settings")]
    [SerializeField] protected GameObject ProjectilePrefab;
    [SerializeField] protected Transform firePoint;

    [SerializeField] protected float projectileSpeed = 10f;
    [SerializeField] protected float firePointOffset = 0.5f;

    protected Vector2 aimDirection = Vector2.right;

    protected override void UpdateAim()
    {
        if (target == null || firePoint == null)
            return;

        aimDirection = (
            target.position - transform.position
        ).normalized;

        // Chỉ thay đổi hướng FirePoint
        float angle = Mathf.Atan2(
            aimDirection.y,
            aimDirection.x
        ) * Mathf.Rad2Deg;

        firePoint.localRotation = Quaternion.Euler(
            0f, 0f, angle
        );
    }

    protected void ShootFireball()
    {
        if (target == null)
            return;

        if (ProjectilePrefab == null)
        {
            Debug.LogError("Projectile Prefab chưa được gán!");
            return;
        }

        if (firePoint == null)
        {
            Debug.LogError("FirePoint chưa được gán!");
            return;
        }

        GameObject projectile = Instantiate(
            ProjectilePrefab,
            firePoint.position,
            firePoint.rotation
        );

        Fireball fireball =
            projectile.GetComponent<Fireball>();

        if (fireball != null)
        {
            fireball.Initialize(
                target,
                CalculateDamage(),
                projectileSpeed
            );
        }
        else
        {
            Debug.LogError(
                "Fireball Prefab không có script Fireball!"
            );

            Destroy(projectile);
        }
    }
}