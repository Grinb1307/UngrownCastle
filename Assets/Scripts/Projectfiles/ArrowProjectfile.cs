
using UnityEngine;

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

    private void Awake()
    {
        rb = GetComponent<Rigidbody2D>();
    }

    public void Initialize(
        Vector2 targetPosition,
        float arrowDamage,
        float angleOffset)
    {
        damage = arrowDamage;

        // Vị trí xuất phát
        Vector2 startPosition = transform.position;

        // Khoảng cách đến mục tiêu
        Vector2 distance = targetPosition - startPosition;

        // Thời gian bay dựa trên khoảng cách ngang
        float flightTime = Mathf.Max(
            Mathf.Abs(distance.x) / horizontalSpeed,
            0.15f
        );

        // Gia tốc trọng trường thực tế
        float gravity = Physics2D.gravity.y *
                        rb.gravityScale;

        // Tính vận tốc ban đầu
        float vx = distance.x / flightTime;

        float vy = (
            distance.y -
            0.5f * gravity * flightTime * flightTime
        ) / flightTime;

        // Xoay góc bổ sung cho Multi Shot
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

        // Lấy vận tốc thực tế của mũi tên
        Vector2 velocity = rb.linearVelocity;

        // Tính góc bay
        float angle = Mathf.Atan2(
            velocity.y,
            velocity.x
        ) * Mathf.Rad2Deg;

        // Xoay sprite theo hướng bay
        transform.rotation = Quaternion.Euler(0,0,angle + 120f);
    }

    private void OnTriggerEnter2D(Collider2D collision)
    {
        DummyTarget dummy =
            collision.GetComponent<DummyTarget>();

        if (dummy != null)
        {
            dummy.TakeDamage(damage);
            Destroy(gameObject);
        }
    }
}
