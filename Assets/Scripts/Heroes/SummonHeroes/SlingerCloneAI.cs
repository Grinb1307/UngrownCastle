using UnityEngine;
using UnityEngine.Rendering;
using UngrownCastle.Enemies;

[RequireComponent(typeof(SortingGroup))]
public class SlingerCloneAI : MonoBehaviour
{
    [Header("Combat")]
    [SerializeField] private float damage = 15f;
    [SerializeField] private float attackRange = 12f;
    [SerializeField] private float attackSpeed = 1f;
    [SerializeField] private GameObject spearPrefab;
    [SerializeField] private Sprite spearSprite;
    [SerializeField] private Transform firePoint;

    [Header("Target")]
    [SerializeField] private LayerMask enemyLayer;
    [SerializeField] private float retargetInterval = 0.2f;

    [Header("Lifetime")]
    [SerializeField] private float duration = 15f;

    [Header("Depth Sorting")]
    [SerializeField] private SortingGroup sortingGroup;
    [SerializeField] private int sortingOrderBase = 1000;
    [SerializeField] private int sortingPrecision = 100;
    [SerializeField] private float sortingYOffset = -0.6f;

    private Enemy targetEnemy;
    private float attackTimer;
    private float retargetTimer;
    private float remainingDuration;
    private bool hasLifetime;

    public void Initialize(
        float cloneDamage,
        float cloneAttackSpeed,
        float cloneAttackRange,
        LayerMask targetEnemyLayer,
        GameObject projectilePrefab,
        Sprite projectileSprite,
        float lifeDuration)
    {
        damage = Mathf.Max(0f, cloneDamage);
        attackSpeed = Mathf.Max(0.1f, cloneAttackSpeed);
        attackRange = Mathf.Max(0.5f, cloneAttackRange);
        enemyLayer = targetEnemyLayer;

        if (projectilePrefab != null)
        {
            spearPrefab = projectilePrefab;
        }

        if (projectileSprite != null)
        {
            spearSprite = projectileSprite;
        }

        duration = Mathf.Max(0f, lifeDuration);
        if (duration > 0f)
        {
            hasLifetime = true;
            remainingDuration = duration;
        }
        else
        {
            hasLifetime = false;
            remainingDuration = 0f;
        }
    }

    private void Reset()
    {
        AutoAssignComponents();
    }

    private void OnValidate()
    {
        AutoAssignComponents();
    }

    private void Awake()
    {
        AutoAssignComponents();

        if (sortingGroup == null)
        {
            sortingGroup = gameObject.AddComponent<SortingGroup>();
        }

        if (duration > 0f && !hasLifetime)
        {
            hasLifetime = true;
            remainingDuration = duration;
        }

        UpdateSortingOrder();
    }

    private void AutoAssignComponents()
    {
        if (sortingGroup == null)
        {
            sortingGroup = GetComponent<SortingGroup>();
        }
    }

    private void LateUpdate()
    {
        UpdateSortingOrder();
    }

    private void UpdateSortingOrder()
    {
        if (sortingGroup == null)
            return;

        float footY = transform.position.y + sortingYOffset;
        sortingGroup.sortingOrder = sortingOrderBase - Mathf.RoundToInt(footY * sortingPrecision);
    }

    private void Update()
    {
        if (hasLifetime)
        {
            remainingDuration -= Time.deltaTime;

            if (remainingDuration <= 0f)
            {
                Destroy(gameObject);
                return;
            }
        }

        UpdateNearestTarget();

        if (targetEnemy == null)
            return;

        UpdateFacing();

        if (attackSpeed <= 0f)
            return;

        attackTimer += Time.deltaTime;

        if (attackTimer >= 1f / attackSpeed)
        {
            ThrowSpear();
            attackTimer = 0f;
        }
    }

    private bool IsTargetValid(Enemy enemy)
    {
        return enemy != null && !enemy.IsDead && enemy.gameObject.activeInHierarchy;
    }

    private void UpdateNearestTarget()
    {
        if (!IsTargetValid(targetEnemy))
        {
            targetEnemy = FindNearestEnemy();
            retargetTimer = 0f;
            return;
        }

        retargetTimer += Time.deltaTime;
        if (retargetTimer < retargetInterval)
            return;

        retargetTimer = 0f;
        targetEnemy = FindNearestEnemy();
    }

    private Enemy FindNearestEnemy()
    {
        Collider2D[] enemies = Physics2D.OverlapCircleAll(
            transform.position,
            attackRange,
            enemyLayer
        );

        Enemy nearest = null;
        float minSqrDistance = Mathf.Infinity;
        Vector2 selfPos = transform.position;

        foreach (Collider2D col in enemies)
        {
            if (col == null)
                continue;

            Enemy enemyComponent = col.GetComponentInParent<Enemy>();
            if (!IsTargetValid(enemyComponent))
                continue;

            float sqrDist = ((Vector2)enemyComponent.transform.position - selfPos).sqrMagnitude;
            if (sqrDist < minSqrDistance)
            {
                minSqrDistance = sqrDist;
                nearest = enemyComponent;
            }
        }

        return nearest;
    }

    private void UpdateFacing()
    {
        if (targetEnemy == null)
            return;

        float direction = targetEnemy.transform.position.x - transform.position.x;
        if (Mathf.Abs(direction) <= 0.05f)
            return;

        Vector3 scale = transform.localScale;
        scale.x = Mathf.Abs(scale.x) * (direction > 0f ? 1f : -1f);
        transform.localScale = scale;
    }

    private void ThrowSpear()
    {
        if (spearPrefab == null || !IsTargetValid(targetEnemy))
            return;

        Vector3 spawnPos = firePoint != null ? firePoint.position : transform.position;

        GameObject spearObj = Instantiate(
            spearPrefab,
            spawnPos,
            Quaternion.identity
        );

        SpearProjectile spear = spearObj.GetComponent<SpearProjectile>();
        if (spear != null)
        {
            spear.SetSprite(spearSprite);
            spear.Initialize(targetEnemy.transform.position, damage);
        }
    }

    private void OnDrawGizmosSelected()
    {
        Gizmos.color = Color.cyan;
        Gizmos.DrawWireSphere(transform.position, attackRange);
    }
}
