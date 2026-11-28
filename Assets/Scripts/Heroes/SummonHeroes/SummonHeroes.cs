
using UnityEngine;

public abstract class SummonHeroes : Heroes
{
    [Header("Summon Settings")]
    [SerializeField] protected GameObject summonPrefab;
    [SerializeField] protected Transform summonPoint;

    protected GameObject summonedUnit;

    protected virtual void Start()
    {
        // Summon Heroes không có đánh thường
        attackSpeed = 0f;

        // Triệu hồi ngay khi bắt đầu
        Summon();
    }

    protected virtual void Summon()
    {
        if (summonPrefab == null)
        {
            Debug.LogWarning($"{gameObject.name}: Missing Summon Prefab!");
            return;
        }

        Vector3 spawnPosition = summonPoint != null
            ? summonPoint.position
            : transform.position;

        summonedUnit = Instantiate(
            summonPrefab,
            spawnPosition,
            Quaternion.identity
        );
    }

    // Mặc định Summon Heroes không đánh thường (lớp con như Slinger có thể override)
    protected override void PerformAttack()
    {
    }
}
