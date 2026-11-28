using UnityEngine;
using UngrownCastle.Enemies;

public class Slinger : SummonHeroes
{
    [Header("Slinger Spear Attack")]
    [SerializeField] private GameObject spearPrefab;
    [SerializeField] private Sprite spearSprite;
    [SerializeField] private Transform firePoint;

    [Header("Slinger Skill - Summon Clones")]
    [SerializeField] private int cloneCount = 5;
    [SerializeField] private float cloneDuration = 15f;
    [SerializeField] private float cloneDamageMultiplier = 2.5f;
    [SerializeField] private float cloneScaleMultiplier = 0.8f;
    [SerializeField] private float cloneAttackSpeed = 1f;
    [SerializeField] private float cloneVerticalSpacing = 0.55f;
    [SerializeField] private float cloneHorizontalSpread = 0.25f;

    protected override void Start()
    {
        // Slinger có đánh thường ném giáo nên giữ nguyên attackSpeed, chỉ triệu hồi clone khi dùng skill
    }

    protected override void FindTarget()
    {
        Collider2D[] enemies = Physics2D.OverlapCircleAll(
            transform.position,
            attackRange,
            enemyLayer
        );

        Transform nearest = null;
        float minSqrDistance = Mathf.Infinity;
        Vector2 selfPos = transform.position;

        foreach (Collider2D col in enemies)
        {
            if (col == null)
                continue;

            Enemy enemyComponent = col.GetComponentInParent<Enemy>();
            if (enemyComponent == null || enemyComponent.IsDead || !enemyComponent.gameObject.activeInHierarchy)
                continue;

            float sqrDist = ((Vector2)enemyComponent.transform.position - selfPos).sqrMagnitude;
            if (sqrDist < minSqrDistance)
            {
                minSqrDistance = sqrDist;
                nearest = enemyComponent.transform;
            }
        }

        target = nearest;
    }

    // Đánh thường: ném ngọn giáo theo quỹ đạo vòng cung vật lý
    protected override void PerformAttack()
    {
        if (target == null || spearPrefab == null)
            return;

        Enemy enemy = target.GetComponentInParent<Enemy>();
        if (enemy != null && enemy.IsDead)
        {
            target = null;
            return;
        }

        Vector3 spawnPosition = firePoint != null
            ? firePoint.position
            : transform.position;

        GameObject spearObj = Instantiate(
            spearPrefab,
            spawnPosition,
            Quaternion.identity
        );

        SpearProjectile spear = spearObj.GetComponent<SpearProjectile>();
        if (spear != null)
        {
            spear.SetSprite(spearSprite);
            spear.Initialize(target.position, CalculateDamage());
        }
    }

    protected override bool CanUseSkill()
    {
        return summonPrefab != null;
    }

    // Skill: triệu hồi 5 Slinger clone ném giáo vào Enemy
    public override void UseSkill()
    {
        Summon();
    }

    protected override void Summon()
    {
        if (summonPrefab == null)
        {
            Debug.LogWarning($"{gameObject.name}: Missing Slinger Clone Prefab!");
            return;
        }

        Vector3 basePosition = summonPoint != null
            ? summonPoint.position
            : transform.position;

        int count = Mathf.Max(1, cloneCount);

        for (int i = 0; i < count; i++)
        {
            float yOffset = (i - (count - 1) * 0.5f) * cloneVerticalSpacing;
            float xOffset = (i % 2 == 0 ? 1f : -1f) * cloneHorizontalSpread * 0.5f;

            Vector3 spawnPos = new Vector3(
                basePosition.x + xOffset,
                basePosition.y + yOffset,
                basePosition.z
            );

            summonedUnit = Instantiate(
                summonPrefab,
                spawnPos,
                Quaternion.identity
            );

            Vector3 slingerScale = transform.lossyScale;
            float scaleRatio = Mathf.Max(0.05f, cloneScaleMultiplier);
            summonedUnit.transform.localScale = new Vector3(
                Mathf.Abs(slingerScale.x) * scaleRatio,
                Mathf.Abs(slingerScale.y) * scaleRatio,
                Mathf.Abs(slingerScale.z) > 0f ? Mathf.Abs(slingerScale.z) : 1f
            );

            SlingerCloneAI cloneAI = summonedUnit.GetComponent<SlingerCloneAI>();
            if (cloneAI != null)
            {
                cloneAI.Initialize(
                    damage * cloneDamageMultiplier,
                    cloneAttackSpeed,
                    attackRange,
                    enemyLayer,
                    spearPrefab,
                    spearSprite,
                    cloneDuration
                );
            }
        }

        Debug.Log($"Slinger summoned {count} Slinger clones!");
    }
}
