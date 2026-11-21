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
    private bool exploded;

    private void Awake()
    {
        rb = GetComponent<Rigidbody2D>();
    }

    public void Launch(Vector2 targetPosition)
    {
        // Gia tốc trọng trường thực tế của Rigidbody2D
        float gravity = Physics2D.gravity.y * rb.gravityScale;

        Vector2 startPosition = rb.position;
        Vector2 displacement = targetPosition - startPosition;

        // Tính vận tốc ban đầu để TNT đến đúng mục tiêu
        float vx = displacement.x / flightTime;

        float vy = (
            displacement.y -
            0.5f * gravity * flightTime * flightTime
        ) / flightTime;

        rb.linearVelocity = new Vector2(vx, vy);

        // Tạo chuyển động xoay tự nhiên
        rb.angularVelocity = spinSpeed;
    }

    private void OnCollisionEnter2D(Collision2D collision)
    {
        Explode();
    }

    private void OnTriggerEnter2D(Collider2D other)
    {
        Explode();
    }

    private void Explode()
    {
        if (exploded)
            return;

        exploded = true;

        rb.linearVelocity = Vector2.zero;
        rb.angularVelocity = 0f;
        rb.simulated = false;

        if (explosionPrefab != null)
        {
            GameObject explosion = Instantiate(
                explosionPrefab,
                transform.position,
                Quaternion.identity
            );

            TNTExplosion effect =
                explosion.GetComponent<TNTExplosion>();

            if (effect != null)
            {
                effect.Initialize(damage, explosionRadius);
            }
        }

        Destroy(gameObject);
    }
}