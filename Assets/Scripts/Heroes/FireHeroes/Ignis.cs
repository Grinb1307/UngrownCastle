
using UnityEngine;
using System.Collections;

public class Ignis : FireHeroes
{
    protected override void PerformAttack()
    {
        ThrowBombArrow();
    }
    public override void UseSkill()
    {

    }
    private void ThrowBombArrow()
    {
        if (target == null || ProjectilePrefab == null || firePoint == null)
            return;

        // Lấy vị trí trung tâm Enemy
        Collider2D enemyCollider = target.GetComponent<Collider2D>();

        Vector2 targetPosition = enemyCollider != null
            ? enemyCollider.bounds.center
            : (Vector2)target.position;

        // Tính hướng bắn
        Vector2 direction =
            (targetPosition - (Vector2)firePoint.position).normalized;

        // Tính góc xoay
        float angle = Mathf.Atan2(direction.y, direction.x) * Mathf.Rad2Deg;

        // Tạo mũi tên
        GameObject projectile = Instantiate(
            ProjectilePrefab,
            firePoint.position,
            Quaternion.Euler(0f, 0f, angle)
        );

        Rigidbody2D rb = projectile.GetComponent<Rigidbody2D>();

        if (rb != null)
        {
            // Không cho mũi tên rơi do trọng lực
            rb.gravityScale = 0f;

            // Không để vật lý tự xoay mũi tên
            rb.angularVelocity = 0f;
            rb.freezeRotation = true;

            // Bay thẳng về phía Enemy
            rb.linearVelocity = direction * projectileSpeed;
        }
        else
        {
            Debug.LogError("BombArrow thiếu Rigidbody2D!");
            Destroy(projectile);
        }
    }
}
