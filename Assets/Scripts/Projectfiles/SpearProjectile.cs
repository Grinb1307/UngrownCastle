using UnityEngine;
using UngrownCastle.Enemies;

[RequireComponent(typeof(Rigidbody2D))]
[RequireComponent(typeof(Collider2D))]
public class SpearProjectile : MonoBehaviour
{
    [Header("Spear Physics")]
    [SerializeField] private float horizontalSpeed = 10f;
    [SerializeField] private float minFlightTime = 0.35f;
    [SerializeField] private float maxFlightTime = 1.4f;
    [SerializeField] private float extraArcHeight = 0.8f;
    [SerializeField] private float rotationOffset = 0f;
    [SerializeField] private float lifeTime = 5f;

    [Header("Visual")]
    [SerializeField] private SpriteRenderer spriteRenderer;

    private Rigidbody2D rb;
    private float damage;
    private bool initialized;
    private bool hasHit;

    private void Awake()
    {
        rb = GetComponent<Rigidbody2D>();
        rb.collisionDetectionMode = CollisionDetectionMode2D.Continuous;

        if (rb.gravityScale <= 0f)
        {
            rb.gravityScale = 1f;
        }

        if (spriteRenderer == null)
        {
            spriteRenderer = GetComponent<SpriteRenderer>();
        }
    }

    public void SetSprite(Sprite sprite)
    {
        if (sprite != null && spriteRenderer != null)
        {
            spriteRenderer.sprite = sprite;
        }
    }

    // Ném ngọn giáo theo quỹ đạo parabol vật lý rơi đúng vị trí mục tiêu
    public void Initialize(Vector2 targetPosition, float spearDamage, float angleOffset = 0f)
    {
        damage = spearDamage;

        if (rb == null)
        {
            rb = GetComponent<Rigidbody2D>();
        }

        if (rb.gravityScale <= 0f)
        {
            rb.gravityScale = 1f;
        }

        Vector2 startPosition = transform.position;
        Vector2 displacement = targetPosition - startPosition;

        float gravityAbs = Mathf.Max(Mathf.Abs(Physics2D.gravity.y * rb.gravityScale), 0.1f);
        float minTimeForArc = extraArcHeight > 0f
            ? Mathf.Sqrt(8f * extraArcHeight / gravityAbs) * 0.65f
            : 0f;

        float rawFlightTime = Mathf.Max(
            Mathf.Abs(displacement.x) / Mathf.Max(horizontalSpeed, 0.1f),
            minTimeForArc
        );

        float flightTime = Mathf.Clamp(rawFlightTime, minFlightTime, maxFlightTime);

        float gravity = Physics2D.gravity.y * rb.gravityScale;
        float vx = displacement.x / flightTime;
        float vy = (displacement.y - 0.5f * gravity * flightTime * flightTime) / flightTime;

        Vector2 velocity = new Vector2(vx, vy);

        if (Mathf.Abs(angleOffset) > 0.001f)
        {
            velocity = Quaternion.Euler(0f, 0f, angleOffset) * velocity;
        }

        rb.linearVelocity = velocity;
        UpdateRotation(velocity);

        initialized = true;
        Destroy(gameObject, lifeTime);
    }

    private void FixedUpdate()
    {
        if (!initialized || hasHit)
            return;

        UpdateRotation(rb.linearVelocity);
    }

    private void UpdateRotation(Vector2 velocity)
    {
        if (velocity.sqrMagnitude < 0.001f)
            return;

        float angle = Mathf.Atan2(velocity.y, velocity.x) * Mathf.Rad2Deg;
        transform.rotation = Quaternion.Euler(0f, 0f, angle + rotationOffset);
    }

    private void OnTriggerEnter2D(Collider2D collision)
    {
        if (!initialized || hasHit)
            return;

        Enemy enemy = collision.GetComponentInParent<Enemy>();

        if (enemy != null)
        {
            if (enemy.IsDead)
                return;

            hasHit = true;
            enemy.TakeDamage(damage);
            Destroy(gameObject);
            return;
        }

        DummyTarget dummy = collision.GetComponentInParent<DummyTarget>();

        if (dummy != null)
        {
            hasHit = true;
            dummy.TakeDamage(damage);
            Destroy(gameObject);
        }
    }
}
