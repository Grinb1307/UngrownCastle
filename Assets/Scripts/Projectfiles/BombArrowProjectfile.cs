using UnityEngine;

[RequireComponent(typeof(Rigidbody2D))]
public class BombArrowProjectile : MonoBehaviour
{
    private Rigidbody2D rb;
    private bool hasExploded = false;

    [Header("Physics")]
    [SerializeField] private float gravityScale = 1.5f;
    [SerializeField] private float rotationOffset = -90f;

    [Header("Explosion")]
    [SerializeField] private GameObject explosionPrefab;

    [Header("Target")]
    [SerializeField] private LayerMask targetLayers;

    private void Awake()
    {
        rb = GetComponent<Rigidbody2D>();

        rb.gravityScale = gravityScale;
        rb.freezeRotation = true;
        rb.linearDamping = 0f;
    }

    public void Launch(Vector2 targetPosition, float flightTime)
    {
        Vector2 startPosition = transform.position;
        Vector2 distance = targetPosition - startPosition;

        Vector2 gravity = Physics2D.gravity * gravityScale;

        Vector2 velocity =
            (distance - 0.5f * gravity *
            flightTime * flightTime) / flightTime;

        rb.linearVelocity = velocity;
    }

    private void Update()
    {
        if (hasExploded) return;

        Vector2 velocity = rb.linearVelocity;

        if (velocity.sqrMagnitude > 0.01f)
        {
            float angle = Mathf.Atan2(
                velocity.y, velocity.x
            ) * Mathf.Rad2Deg;

            transform.rotation = Quaternion.Euler(
                0f, 0f, angle + rotationOffset
            );
        }
    }

    // Dành cho Collider Is Trigger
    private void OnTriggerEnter2D(Collider2D other)
    {
        CheckHit(other);
    }

    // Dành cho Collider không bật Is Trigger
    private void OnCollisionEnter2D(Collision2D collision)
    {
        CheckHit(collision.collider);
    }

    private void CheckHit(Collider2D other)
    {
        if (hasExploded || other == null)
            return;

        // Kiểm tra layer của Collider và các object cha
        Transform current = other.transform;

        bool isTarget = false;

        while (current != null)
        {
            int layer = current.gameObject.layer;

            if ((targetLayers.value & (1 << layer)) != 0)
            {
                isTarget = true;
                break;
            }

            // Kiểm tra tên Layer nếu chưa cấu hình LayerMask
            string layerName = LayerMask.LayerToName(layer);

            if (layerName == "Enemy" || layerName == "Dummy")
            {
                isTarget = true;
                break;
            }

            current = current.parent;
        }

        if (!isTarget)
            return;

        Explode(other);
    }

    private void Explode(Collider2D other)
    {
        if (hasExploded)
            return;

        hasExploded = true;

        Debug.Log("BombArrow đã chạm mục tiêu: " + other.name);

        Vector2 hitPosition =
            other.ClosestPoint(transform.position);

        if (explosionPrefab != null)
        {
            Instantiate(
                explosionPrefab,
                hitPosition,
                Quaternion.identity
            );
        }
        else
        {
            Debug.LogError("Chưa gán Explosion Prefab!");
        }

        Destroy(gameObject);
    }
}