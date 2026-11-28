using System.Collections.Generic;
using UnityEngine;
using UngrownCastle.Enemies;

public class GolemAI : SummonedUnitAI
{
    [Header("Golem Skill - Heavy Slam")]
    [SerializeField] private float heavySlamMultiplier = 2f;
    [SerializeField] private float heavySlamRadius = 2.2f;

    private bool heavySlamReady;

    public bool IsHeavySlamReady => heavySlamReady;
    public float HeavySlamMultiplier => heavySlamMultiplier;
    public float HeavySlamRadius => heavySlamRadius;

    protected override void Reset()
    {
        base.Reset();

        damage = 50f;
        attackRange = 1.7f;
        attackSpeed = 0.55f;
        moveSpeed = 1.3f;
        detectionRange = 10f;
        attackFallbackDuration = 1.05f;
        heavySlamMultiplier = 2f;
        heavySlamRadius = 2.2f;
    }

    protected override void OnValidate()
    {
        base.OnValidate();

        heavySlamMultiplier = Mathf.Max(1f, heavySlamMultiplier);
        heavySlamRadius = Mathf.Max(0.1f, heavySlamRadius);
    }

    // Skill cơ bản của Golem Master: cường hóa đòn đánh kế tiếp thành đòn đập diện rộng
    public virtual void ActivateHeavySlam()
    {
        heavySlamReady = true;
    }

    protected override void DealAttackDamage(Enemy target)
    {
        if (!heavySlamReady)
        {
            target.TakeDamage(damage);
            return;
        }

        heavySlamReady = false;
        PerformHeavySlam(target);
    }

    // Hook dễ mở rộng cho các skill của Golem trong tương lai
    protected virtual void PerformHeavySlam(Enemy primaryTarget)
    {
        float slamDamage = damage * heavySlamMultiplier;
        Vector2 impactPoint = primaryTarget.transform.position;

        HashSet<Enemy> damagedEnemies = new HashSet<Enemy>();

        primaryTarget.TakeDamage(slamDamage);
        damagedEnemies.Add(primaryTarget);

        Collider2D[] hits = Physics2D.OverlapCircleAll(
            impactPoint,
            heavySlamRadius,
            enemyLayer
        );

        foreach (Collider2D col in hits)
        {
            if (col == null)
                continue;

            Enemy nearbyEnemy = col.GetComponentInParent<Enemy>();

            if (!IsTargetValid(nearbyEnemy) || damagedEnemies.Contains(nearbyEnemy))
                continue;

            damagedEnemies.Add(nearbyEnemy);
            nearbyEnemy.TakeDamage(slamDamage);
        }

        Debug.Log($"Golem Heavy Slam hit {damagedEnemies.Count} enemies for {slamDamage} damage!");
    }

    protected override void OnDrawGizmosSelected()
    {
        base.OnDrawGizmosSelected();

        Gizmos.color = new Color(1f, 0.5f, 0f, 0.8f);
        Gizmos.DrawWireSphere(transform.position, heavySlamRadius);
    }
}
