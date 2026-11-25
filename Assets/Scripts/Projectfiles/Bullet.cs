using UnityEngine;
using UngrownCastle.Enemies;

[RequireComponent(typeof(Rigidbody2D))]
[RequireComponent(typeof(Collider2D))]
public class Bullet : MonoBehaviour
{
    [Header("Projectile Settings")]
    [SerializeField] private float lifeTime = 5f;

    private Rigidbody2D rb;
    private Transform target;
    private float damage;
    private bool hasHit;

    private void Awake()
    {
        rb = GetComponent<Rigidbody2D>();
    }

    // Khởi tạo và bắn đạn về phía Enemy
    public void Initialize(
        Transform enemy,
        float attackDamage,
        float speed)
    {
        target = enemy;
        damage = attackDamage;

        if (target == null)
        {
            Destroy(gameObject);
            return;
        }

        // Tính hướng từ đạn đến Enemy
        Vector2 direction =
            ((Vector2)target.position -
             (Vector2)transform.position).normalized;

        // Cho đạn bay
        rb.linearVelocity = direction * speed;

        // Xoay sprite theo hướng bay
        float angle = Mathf.Atan2(
            direction.y,
            direction.x
        ) * Mathf.Rad2Deg;

        transform.rotation = Quaternion.Euler(
            0f, 0f, angle
        );

        Destroy(gameObject, lifeTime);
    }

    // Va chạm với Enemy khi Collider bật Is Trigger
    private void OnTriggerEnter2D(Collider2D other)
    {
        HitTarget(other);
    }

    // Va chạm với Enemy khi Collider không bật Is Trigger
    private void OnCollisionEnter2D(Collision2D collision)
    {
        HitTarget(collision.collider);
    }

    private void HitTarget(Collider2D other)
    {
        if (hasHit || target == null)
            return;

        Transform hitTransform = other.transform;

        bool isTarget =
            hitTransform == target ||
            hitTransform.IsChildOf(target) ||
            target.IsChildOf(hitTransform);

        if (!isTarget)
            return;

        Enemy enemy = other.GetComponentInParent<Enemy>();

        if (enemy == null)
            return;

        hasHit = true;

        enemy.TakeDamage(damage);

        Destroy(gameObject);
    }
}