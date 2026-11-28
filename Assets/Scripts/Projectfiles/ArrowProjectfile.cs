
using UnityEngine;
using UngrownCastle.Enemies;

[RequireComponent(typeof(Rigidbody2D))]
[RequireComponent(typeof(Collider2D))]
public class ArrowProjectile : MonoBehaviour
{
    [Header("Arrow Settings")]
    [SerializeField] private float horizontalSpeed = 8f;
    [SerializeField] private float lifeTime = 5f;

    private Rigidbody2D rb;
    private float damage;
    private bool initialized;
    private bool hasHit;

    private void Awake()
    {
        rb = GetComponent<Rigidbody2D>();

        // Giảm nguy cơ xuyên qua Enemy khi bay nhanh
        rb.collisionDetectionMode =
            CollisionDetectionMode2D.Continuous;
    }

    public void Initialize(
        Vector2 targetPosition,
        float arrowDamage,
        float angleOffset)
    {
        damage = arrowDamage;

        Vector2 startPosition = transform.position;
        Vector2 distance = targetPosition - startPosition;

        float flightTime = Mathf.Max(
            Mathf.Abs(distance.x) / horizontalSpeed,
            0.15f
        );

        float gravity = Physics2D.gravity.y *
                        rb.gravityScale;

        float vx = distance.x / flightTime;

        float vy = (
            distance.y -
            0.5f * gravity * flightTime * flightTime
        ) / flightTime;

        Vector2 velocity = new Vector2(vx, vy);

        velocity = Quaternion.Euler(
            0f, 0f, angleOffset
        ) * velocity;

        rb.linearVelocity = velocity;

        initialized = true;

        Destroy(gameObject, lifeTime);
    }

    private void FixedUpdate()
    {
        if (!initialized)
            return;

        Vector2 velocity = rb.linearVelocity;

        if (velocity.sqrMagnitude < 0.001f)
            return;

        float angle = Mathf.Atan2(
            velocity.y,
            velocity.x
        ) * Mathf.Rad2Deg;

        transform.rotation = Quaternion.Euler(
            0f, 0f, angle + 120f
        );
    }

    private void OnTriggerEnter2D(Collider2D collision)
    {
        if (!initialized || hasHit)
            return;

        // Tìm Enemy kể cả khi Collider nằm ở object con
        Enemy enemy =
            collision.GetComponentInParent<Enemy>();

        if (enemy != null)
        {
            hasHit = true;

            enemy.TakeDamage(damage);

            Debug.Log(
                $"Arrow gây {damage} damage cho {enemy.EnemyName}"
            );

            Destroy(gameObject);
            return;
        }

        // Hỗ trợ DummyTarget nếu vẫn còn sử dụng
        DummyTarget dummy =
            collision.GetComponentInParent<DummyTarget>();

        if (dummy != null)
        {
            hasHit = true;

            dummy.TakeDamage(damage);

            Debug.Log($"Arrow gây {damage} damage cho Dummy");

            Destroy(gameObject);
        }
    }
}
