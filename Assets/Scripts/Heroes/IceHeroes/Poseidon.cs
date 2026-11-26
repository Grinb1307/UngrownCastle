
using UnityEngine;
using System.Collections.Generic;
using UngrownCastle.Enemies;

public class Poseidon : IceHeroes
{
    [Header("Water Slash")]
    [SerializeField] private GameObject waterSlashPrefab;
    [SerializeField] private float slashRange = 5f;
    [SerializeField] private int maxTargets = 5;
    [SerializeField] private float slashLifetime = 0.5f;

    // Đánh thường
    protected override void PerformAttack()
    {
        SlashEnemies();
    }
    public override void UseSkill()
    {

    }
    private void SlashEnemies()
    {
        if (waterSlashPrefab == null)
        {
            Debug.LogError("Poseidon thiếu Water Slash Prefab!");
            return;
        }

        // Tìm tất cả Enemy đang tồn tại
        Enemy[] allEnemies = FindObjectsByType<Enemy>(
            FindObjectsSortMode.None
        );

        List<Enemy> targets = new List<Enemy>();

        foreach (Enemy enemy in allEnemies)
        {
            if (enemy == null || enemy.IsDead)
                continue;

            float distance = Vector2.Distance(
                transform.position,
                enemy.transform.position
            );

            if (distance <= slashRange)
            {
                targets.Add(enemy);
            }
        }

        // Sắp xếp Enemy theo khoảng cách gần nhất
        targets.Sort((a, b) =>
        {
            float distanceA = Vector2.Distance(
                transform.position,
                a.transform.position
            );

            float distanceB = Vector2.Distance(
                transform.position,
                b.transform.position
            );

            return distanceA.CompareTo(distanceB);
        });

        // Chém tối đa 5 Enemy
        int hitCount = Mathf.Min(maxTargets, targets.Count);

        for (int i = 0; i < hitCount; i++)
        {
            Enemy enemy = targets[i];

            if (enemy == null || enemy.IsDead)
                continue;

            // Tạo Water Slash ngay trên Enemy
            GameObject slash = Instantiate(
                waterSlashPrefab,
                enemy.transform.position,
                Quaternion.identity
            );

            // Gây sát thương trực tiếp
            enemy.TakeDamage(CalculateDamage());

            // Hủy hiệu ứng sau một khoảng thời gian
            Destroy(slash, slashLifetime);
        }
    }
}
