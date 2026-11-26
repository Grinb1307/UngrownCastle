
using UnityEngine;
using UngrownCastle.Enemies;

public class Fireball : MonoBehaviour
{
    private Transform target;
    private float damage;
    private float speed;

    private bool hasHit;

    public void Initialize(
        Transform target,
        float damage,
        float speed)
    {
        this.target = target;
        this.damage = damage;
        this.speed = speed;
    }

    private void Update()
    {
        if (hasHit)
            return;

        if (target == null)
        {
            Destroy(gameObject);
            return;
        }

        // Hướng bay đến Enemy
        Vector2 direction =
            ((Vector2)target.position -
            (Vector2)transform.position).normalized;

        // Di chuyển Fireball
        transform.position +=
            (Vector3)(direction * speed * Time.deltaTime);

        // Xoay Fireball theo hướng bay
        float angle = Mathf.Atan2(
            direction.y,
            direction.x
        ) * Mathf.Rad2Deg;

        transform.rotation = Quaternion.Euler(
            0f,
            0f,
            angle
        );
    }

    private void OnTriggerEnter2D(Collider2D other)
    {
        if (hasHit || target == null)
            return;

        // Kiểm tra có chạm đúng Enemy đang nhắm
        if (other.transform == target ||
            other.transform.IsChildOf(target))
        {
            Enemy enemy =
                other.GetComponentInParent<Enemy>();

            if (enemy != null && !enemy.IsDead)
            {
                hasHit = true;

                // Gây sát thương trực tiếp
                enemy.TakeDamage(damage);

                Debug.Log(
                    $"Fireball hit {enemy.EnemyName} " +
                    $"for {damage} damage!"
                );

                Destroy(gameObject);
            }
        }
    }
}
