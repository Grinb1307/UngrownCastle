
using UnityEngine;
using System.Collections.Generic;
using UngrownCastle.Enemies;

public class Meteor : MonoBehaviour
{
    [Header("Meteor Settings")]
    [SerializeField] private float fallSpeed = 15f;
    [SerializeField] private float explosionRadius = 1.5f;

    private Vector2 targetPosition;
    private float damage;

    private bool initialized;
    private bool hasExploded;

    public void Initialize(
        Vector2 targetPosition,
        float damage)
    {
        this.targetPosition = targetPosition;
        this.damage = damage;

        initialized = true;
    }

    private void Update()
    {
        if (!initialized || hasExploded)
            return;

        // Thiên thạch rơi xuống vị trí mục tiêu
        transform.position = Vector2.MoveTowards(
            transform.position,
            targetPosition,
            fallSpeed * Time.deltaTime
        );

        // Kiểm tra đã chạm đất chưa
        if (Vector2.Distance(
            transform.position,
            targetPosition) < 0.05f)
        {
            Explode();
        }
    }

    private void Explode()
    {
        if (hasExploded)
            return;

        hasExploded = true;

        // Tìm tất cả Collider trong vùng nổ
        Collider2D[] enemies =
            Physics2D.OverlapCircleAll(
                targetPosition,
                explosionRadius
            );

        // Tránh gây sát thương nhiều lần cho cùng một Enemy
        HashSet<Enemy> damagedEnemies = new HashSet<Enemy>();

        foreach (Collider2D col in enemies)
        {
            Enemy enemy =
                col.GetComponentInParent<Enemy>();

            if (enemy == null || enemy.IsDead)
                continue;

            // Kiểm tra Enemy đã nhận sát thương chưa
            if (damagedEnemies.Contains(enemy))
                continue;

            damagedEnemies.Add(enemy);

            // Gây sát thương
            enemy.TakeDamage(damage);

            Debug.Log(
                $"Meteor hit {enemy.EnemyName} for {damage} damage!"
            );
        }

        Debug.Log(
            $"Meteor exploded at {targetPosition}. " +
            $"Hit {damagedEnemies.Count} enemies."
        );

        Destroy(gameObject);
    }

    // Hiển thị vùng sát thương trong Scene
    private void OnDrawGizmosSelected()
    {
        Gizmos.color = Color.red;

        Gizmos.DrawWireSphere(
            targetPosition,
            explosionRadius
        );
    }
}
