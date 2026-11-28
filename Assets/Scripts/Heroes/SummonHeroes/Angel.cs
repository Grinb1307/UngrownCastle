using UnityEngine;

public class Angel : SummonHeroes
{
    [Header("Angel Skill - Summon Clones")]
    [SerializeField] private int cloneCount = 5;
    [SerializeField] private float cloneDamageMultiplier = 1.5f;
    [SerializeField] private float cloneDuration = 15f;
    [SerializeField] private float cloneScaleMultiplier = 0.8f;
    [SerializeField] private float spawnHorizontalSpread = 0.35f;
    [SerializeField] private float spawnVerticalSpread = 0.35f;

    protected override void Start()
    {
        // Angel không có đánh thường và chỉ triệu hồi 5 Angel Clone khi đầy mana & người chơi click
        attackSpeed = 0f;
    }

    protected override bool CanUseSkill()
    {
        return summonPrefab != null;
    }

    public override void UseSkill()
    {
        Summon();
    }

    protected override void Summon()
    {
        if (summonPrefab == null)
        {
            Debug.LogWarning($"{gameObject.name}: Missing Angel Clone Prefab!");
            return;
        }

        Vector3 spawnOrigin = GetAngelSpawnOrigin();
        int count = Mathf.Max(1, cloneCount);
        float cloneDamage = damage * cloneDamageMultiplier;
        Vector3 angelScale = transform.lossyScale;
        float scaleRatio = Mathf.Max(0.05f, cloneScaleMultiplier);

        for (int i = 0; i < count; i++)
        {
            float normalizedIndex = count > 1 ? (i - (count - 1) * 0.5f) : 0f;
            float yOffset = normalizedIndex * spawnVerticalSpread;
            float xOffset = (i % 2 == 0 ? 1f : -1f) * spawnHorizontalSpread * 0.5f;

            Vector3 spawnPos = new Vector3(
                spawnOrigin.x + xOffset,
                spawnOrigin.y + yOffset,
                spawnOrigin.z
            );

            summonedUnit = Instantiate(
                summonPrefab,
                spawnPos,
                Quaternion.identity
            );

            summonedUnit.transform.localScale = new Vector3(
                Mathf.Abs(angelScale.x) * scaleRatio,
                Mathf.Abs(angelScale.y) * scaleRatio,
                Mathf.Abs(angelScale.z) > 0f ? Mathf.Abs(angelScale.z) : 1f
            );

            AngelCloneAI cloneAI = summonedUnit.GetComponent<AngelCloneAI>();
            if (cloneAI != null)
            {
                float approachOffsetY = normalizedIndex * 0.14f;
                cloneAI.Initialize(cloneDamage, cloneDuration, enemyLayer, approachOffsetY);
            }
        }

        Debug.Log($"Angel summoned {count} Angel clones! Damage: {cloneDamage}, Duration: {cloneDuration}s");
    }

    // Ưu tiên vị trí hiển thị thực tế của Angel (tránh lệch khi child sprite có offset so với root)
    private Vector3 GetAngelSpawnOrigin()
    {
        if (summonPoint != null)
            return summonPoint.position;

        SpriteRenderer[] renderers = GetComponentsInChildren<SpriteRenderer>();
        if (renderers != null && renderers.Length > 0)
        {
            Bounds combined = renderers[0].bounds;
            for (int i = 1; i < renderers.Length; i++)
            {
                combined.Encapsulate(renderers[i].bounds);
            }

            return new Vector3(combined.center.x, combined.center.y, transform.position.z);
        }

        return transform.position;
    }
}
