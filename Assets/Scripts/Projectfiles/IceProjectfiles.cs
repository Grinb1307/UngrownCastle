using UnityEngine;

[RequireComponent(typeof(Rigidbody2D))]
[RequireComponent(typeof(Collider2D))]
public class IceProjectile : MonoBehaviour
{
    [SerializeField] private float lifeTime = 5f;

    private Rigidbody2D rb;
    private Transform target;
    private float damage;
    private bool hasHit;

    private void Awake()
    {
        rb = GetComponent<Rigidbody2D>();
    }

    public void Initialize(
        Transform enemy,
        float attackDamage,
        float speed)
    {
        target = enemy;
        damage = attackDamage;

        if (target == null)
            return;

        Vector2 direction =
            ((Vector2)target.position - (Vector2)transform.position).normalized;

        rb.linearVelocity = direction * speed;

        float angle = Mathf.Atan2(
            direction.y,
            direction.x
        ) * Mathf.Rad2Deg;

        transform.rotation = Quaternion.Euler(0f, 0f, angle);

        Destroy(gameObject, lifeTime);
    }

    private void OnTriggerEnter2D(Collider2D other)
    {
        if (hasHit || target == null)
            return;

        Transform hit = other.transform;

        if (hit != target &&
            !hit.IsChildOf(target) &&
            !target.IsChildOf(hit))
            return;

        hasHit = true;

        other.SendMessageUpwards(
            "TakeDamage",
            damage,
            SendMessageOptions.DontRequireReceiver
        );

        Destroy(gameObject);
    }
}