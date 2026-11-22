using UnityEngine;

[RequireComponent(typeof(Rigidbody2D))]
public class TNTProjectile : MonoBehaviour
{
    [Header("Physics")]
    [SerializeField] private float flightTime = 0.8f;
    [SerializeField] private float spinSpeed = 360f;

    [Header("Explosion")]
    [SerializeField] private GameObject explosionPrefab;
    [SerializeField] private float damage = 30f;
    [SerializeField] private float explosionRadius = 2f;

    private Rigidbody2D rb;
    private Transform target;

    private bool exploded;

    private void Awake()
    {
        rb = GetComponent<Rigidbody2D>();
    }

    public void Launch(Transform targetTransform)
    {
        target = targetTransform;

        if (target == null)
            return;

        float gravity = Physics2D.gravity.y * rb.gravityScale;

        Vector2 displacement =
            (Vector2)target.position - rb.position;

        float vx = displacement.x / flightTime;

        float vy = (
            displacement.y -
            0.5f * gravity * flightTime * flightTime
        ) / flightTime;

        rb.linearVelocity = new Vector2(vx, vy);
        rb.angularVelocity = spinSpeed;
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
        if (exploded || target == null)
            return;

        Transform hit = other.transform;

        // Kiểm tra có va chạm đúng mục tiêu không
        bool isTarget =
            hit == target ||
            hit.IsChildOf(target) ||
            target.IsChildOf(hit);

        if (!isTarget)
            return;

        Explode();
    }

    private void Explode()
    {
        if (exploded)
            return;

        exploded = true;

        Vector3 explosionPosition = transform.position;

        // Dừng vật lý
        rb.linearVelocity = Vector2.zero;
        rb.angularVelocity = 0f;
        rb.simulated = false;

        // Gây sát thương diện rộng
        Collider2D[] enemies = Physics2D.OverlapCircleAll(
            explosionPosition,
            explosionRadius
        );

        foreach (Collider2D col in enemies)
        {
            DummyTarget dummy =
                col.GetComponentInParent<DummyTarget>();

            if (dummy != null)
            {
                dummy.TakeDamage(damage);
            }
        }

        // Tạo hiệu ứng nổ
        if (explosionPrefab != null)
        {
            Instantiate(
                explosionPrefab,
                explosionPosition,
                Quaternion.identity
            );
        }
        else
        {
            Debug.LogError("Chưa gán Explosion Prefab!");
        }

        // Hủy TNT ngay lập tức
        Destroy(gameObject);
    }
}