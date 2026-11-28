using UnityEngine;

public class Lisa : SummonHeroes
{
    [Header("Lisa Auto Summon")]
    [SerializeField] private int skeletonCount = 2;
    [SerializeField] private float autoSummonInterval = 12f;
    [SerializeField] private bool summonOnStart = true;

    [Header("Skeleton Stats")]
    [SerializeField] private float skeletonDamageMultiplier = 3f;
    [SerializeField] private float skeletonDuration = 15f;
    [SerializeField] private float skeletonScaleMultiplier = 0.8f;
    [SerializeField] private float spawnHorizontalSpread = 0.35f;
    [SerializeField] private float spawnVerticalSpread = 0.45f;

    private float autoSummonTimer;

    protected override void Start()
    {
        // Lisa không có đánh thường, tự động triệu hồi 2 Skeleton sau mỗi 12s
        attackSpeed = 0f;
        autoSummonTimer = summonOnStart ? 0f : autoSummonInterval;
    }

    protected override void Update()
    {
        base.Update();

        if (summonPrefab == null || autoSummonInterval <= 0f)
            return;

        autoSummonTimer -= Time.deltaTime;
        if (autoSummonTimer <= 0f)
        {
            UseSkill();
            autoSummonTimer = autoSummonInterval;
        }
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
            Debug.LogWarning($"{gameObject.name}: Missing Skeleton Prefab!");
            return;
        }

        Vector3 spawnOrigin = GetLisaSpawnOrigin();
        int count = Mathf.Max(1, skeletonCount);
        float skeletonDamage = damage * skeletonDamageMultiplier;
        Vector3 lisaScale = transform.lossyScale;
        float scaleRatio = Mathf.Max(0.05f, skeletonScaleMultiplier);

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
                Mathf.Abs(lisaScale.x) * scaleRatio,
                Mathf.Abs(lisaScale.y) * scaleRatio,
                Mathf.Abs(lisaScale.z) > 0f ? Mathf.Abs(lisaScale.z) : 1f
            );

            SkeletonAI skeletonAI = summonedUnit.GetComponent<SkeletonAI>();
            if (skeletonAI != null)
            {
                float approachOffsetY = normalizedIndex * 0.2f;
                skeletonAI.Initialize(skeletonDamage, skeletonDuration, enemyLayer, approachOffsetY);
            }
        }

        Debug.Log($"Lisa summoned {count} Skeletons! Damage: {skeletonDamage}, Duration: {skeletonDuration}s");
    }

    private Vector3 GetLisaSpawnOrigin()
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
