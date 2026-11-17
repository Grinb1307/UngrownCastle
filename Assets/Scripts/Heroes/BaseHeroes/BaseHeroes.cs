
using UnityEngine;

public abstract class BaseHeroes : Heroes
{
    [Header("Bow Attack")]
    [SerializeField] protected GameObject arrowPrefab;
    [SerializeField] protected Transform bowTransform;
    [SerializeField] protected Transform firePoint;

    // Cơ chế tấn công chung của nhóm BaseHeroes
    protected override void UpdateAim()
    {
        if (target == null || bowTransform == null)
            return;

        Vector2 direction =
            (Vector2)target.position -
            (Vector2)bowTransform.position;

        float angle = Mathf.Atan2(
            direction.y,
            direction.x
        ) * Mathf.Rad2Deg;

        // Xoay cây cung về phía Enemy
        bowTransform.rotation = Quaternion.Euler(
            0f, 0f, angle - 90f
        );
    }
    protected override void PerformAttack()
    {
        if (target == null)
            return;

        ShootArrow(0f);
    }

    protected void ShootArrow(float angleOffset)
    {
        // Không có Enemy thì không bắn
        if (target == null)
            return;

        if (arrowPrefab == null || firePoint == null)
            return;

        // Lấy vị trí Enemy tại thời điểm bắn
        Vector2 targetPosition = target.position;

        GameObject arrow = Instantiate(
            arrowPrefab,
            firePoint.position,
            Quaternion.identity
        );

        ArrowProjectile projectile =
            arrow.GetComponent<ArrowProjectile>();

        if (projectile != null)
        {
            projectile.Initialize(
                targetPosition,
                CalculateDamage(),
                angleOffset
            );
        }
    }

}
