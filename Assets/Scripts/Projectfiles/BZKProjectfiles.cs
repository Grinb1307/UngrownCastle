
using UnityEngine;

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

    private Rigidbody2D rb;
    private Transform target;

    private bool initialized;
    private bool hasHit;

    private void Awake()
    {
        rb = GetComponent<Rigidbody2D>();
    }

    public void Initialize(Transform enemy, float attackDamage)
    {
        target = enemy;
        damage = attackDamage;
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

        // Gây sát thương
        DummyTarget enemy =
            other.GetComponentInParent<DummyTarget>();

        if (enemy != null)
        {
            enemy.TakeDamage(damage);
        }

        // Dừng vật lý
        rb.linearVelocity = Vector2.zero;
        rb.angularVelocity = 0f;
        rb.simulated = false;

        // Hủy đạn
        Destroy(gameObject);
    }
}
