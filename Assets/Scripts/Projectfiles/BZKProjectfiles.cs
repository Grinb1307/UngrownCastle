
using UnityEngine;
using System.Collections.Generic;
using UngrownCastle.Enemies;

[RequireComponent(typeof(Rigidbody2D))]
[RequireComponent(typeof(Collider2D))]
public class BazookaProjectile : MonoBehaviour
{
    [Header("Damage")]
    [SerializeField] private float damage = 10f;

    [Header("Projectile Rotation")]
    [SerializeField] private float rotationOffset = 0f;

    [Header("Lifetime")]
    [SerializeField] private float lifeTime = 5f;

    [Header("Explosion")]
    [SerializeField] private float explosionRadius = 2f;
    [SerializeField] private float explosionDamageMultiplier = 1f;

    [Header("Explosion Effect")]
    [SerializeField] private GameObject explosionPrefab;
    [SerializeField] private float explosionLifetime = 1f;

    private Rigidbody2D rb;
    private Transform target;

    private bool initialized;
    private bool hasHit;

    private void Awake()
    {
        rb = GetComponent<Rigidbody2D>();
    }

    public void Initialize(
        Transform enemy,
        float attackDamage,
        float explosionMultiplier = 1f)
    {
        target = enemy;
        damage = attackDamage;

        explosionDamageMultiplier = explosionMultiplier;

        initialized = true;

        Destroy(gameObject, lifeTime);
    }

    public void SetVelocity(Vector2 velocity)
    {
        rb.linearVelocity = velocity;
        UpdateRotation(velocity);
    }

    private void FixedUpdate()
    {
        if (rb.linearVelocity.sqrMagnitude > 0.01f)
        {
            UpdateRotation(rb.linearVelocity);
        }
    }

    private void UpdateRotation(Vector2 velocity)
    {
        float angle = Mathf.Atan2(
            velocity.y,
            velocity.x
        ) * Mathf.Rad2Deg;

        transform.rotation = Quaternion.Euler(
            0f,
            0f,
            angle + rotationOffset
        );
    }

    private void OnCollisionEnter2D(Collision2D collision)
    {
        CheckHit(collision.collider);
    }

    private void OnTriggerEnter2D(Collider2D other)
    {
        CheckHit(other);
    }

    private void CheckHit(Collider2D other)
    {
        if (!initialized || hasHit || target == null)
            return;

        Transform hit = other.transform;

        bool isTarget =
            hit == target ||
            hit.IsChildOf(target) ||
            target.IsChildOf(hit);

        if (!isTarget)
            return;

        hasHit = true;

        // Vị trí xảy ra vụ nổ
        Vector2 explosionPosition = transform.position;

        // Gây sát thương diện rộng
        Explode(explosionPosition);

        // Dừng vật lý
        rb.linearVelocity = Vector2.zero;
        rb.angularVelocity = 0f;
        rb.simulated = false;

        // Hủy projectile
        Destroy(gameObject);
    }

    private void Explode(Vector2 position)
    {
        // Tạo hiệu ứng nổ
        if (explosionPrefab != null)
        {
            GameObject explosion = Instantiate(
                explosionPrefab,
                position,
                Quaternion.identity
            );

            SpriteRenderer sr = explosion.GetComponent<SpriteRenderer>();

            if (sr != null)
            {
                // Kích thước mong muốn = đường kính vùng damage
                float targetSize = explosionRadius * 1.5f;

                // Kích thước sprite gốc
                Vector2 spriteSize = sr.bounds.size;

                // Tính scale để sprite bằng vùng damage
                explosion.transform.localScale = new Vector3(
                    explosion.transform.localScale.x * targetSize / spriteSize.x,
                    explosion.transform.localScale.y * targetSize / spriteSize.y,
                    1f
                );
            }

            Destroy(explosion, explosionLifetime);
        }

        // Tính damage trong vùng nổ
        Collider2D[] hits = Physics2D.OverlapCircleAll(
            position,
            explosionRadius
        );

        HashSet<Enemy> damagedEnemies = new HashSet<Enemy>();

        foreach (Collider2D hit in hits)
        {
            Enemy enemy = hit.GetComponentInParent<Enemy>();

            if (enemy == null || enemy.IsDead || damagedEnemies.Contains(enemy))
                continue;

            damagedEnemies.Add(enemy);
            enemy.TakeDamage(damage * explosionDamageMultiplier);
        }
    }

    private void OnDrawGizmosSelected()
    {
        Gizmos.color = Color.red;

        Gizmos.DrawWireSphere(
            transform.position,
            explosionRadius
        );
    }
}
