using UnityEngine;
using UngrownCastle.Enemies;

public class KnifeProjectile : MonoBehaviour
{
    [Header("Knife Settings")]
    [SerializeField] private float horizontalSpeed = 10f;
    [SerializeField] private float arcHeightFactor = 0.22f;
    [SerializeField] private float minArcHeight = 0.4f;
    [SerializeField] private float maxArcHeight = 2.0f;
    [SerializeField] private float rotationOffset = 0f;
    [SerializeField] private float lifeTime = 5f;

    private Rigidbody2D rb;
    private Transform targetTransform;
    private Vector2 startPos;
    private Vector2 lastKnownTargetPos;
    private float flightDuration;
    private float arcHeight;
    private float elapsed;
    private float damage;
    private bool initialized;
    private bool hasHit;

    private void Awake()
    {
        SetupPhysicsComponents();
    }

    private void SetupPhysicsComponents()
    {
        if (rb == null)
        {
            rb = GetComponent<Rigidbody2D>();
            if (rb == null) rb = gameObject.AddComponent<Rigidbody2D>();
        }

        // Dùng Kinematic để bám sát đường cong Parabol tới mục tiêu (kể cả Enemy bay), không rơi xuống đất
        rb.bodyType = RigidbodyType2D.Kinematic;
        rb.simulated = true;
        rb.collisionDetectionMode = CollisionDetectionMode2D.Continuous;
        rb.gravityScale = 0f;
        rb.freezeRotation = true;
        rb.linearVelocity = Vector2.zero;
        rb.angularVelocity = 0f;

        transform.position = new Vector3(transform.position.x, transform.position.y, 0f);

        SpriteRenderer sr = GetComponentInChildren<SpriteRenderer>(true);
        if (sr != null && sr.sortingOrder < 5)
        {
            sr.sortingOrder = 5;
        }

        Collider2D col = GetComponent<Collider2D>();
        if (col == null) col = gameObject.AddComponent<BoxCollider2D>();
        col.isTrigger = true;
    }

    private Vector2 GetTargetAimPoint(Transform t)
    {
        if (t == null) return lastKnownTargetPos;

        Collider2D col = t.GetComponentInChildren<Collider2D>();
        if (col != null && col.enabled)
        {
            return col.bounds.center;
        }
        return t.position;
    }

    public void Initialize(
        Transform target,
        float knifeDamage,
        float speed = -1f,
        float customArcHeight = -1f)
    {
        if (speed > 0f) horizontalSpeed = speed;

        SetupPhysicsComponents();

        targetTransform = target;
        damage = knifeDamage;
        startPos = transform.position;
        lastKnownTargetPos = target != null
            ? GetTargetAimPoint(target)
            : startPos + Vector2.right * 4f;

        float dist = Vector2.Distance(startPos, lastKnownTargetPos);
        flightDuration = Mathf.Clamp(dist / Mathf.Max(horizontalSpeed, 0.1f), 0.18f, 1.2f);
        arcHeight = customArcHeight > 0f
            ? customArcHeight
            : Mathf.Clamp(dist * arcHeightFactor, minArcHeight, maxArcHeight);

        elapsed = 0f;
        hasHit = false;
        initialized = true;

        AlignToVelocity((lastKnownTargetPos - startPos) + Vector2.up * (4f * arcHeight));

        Destroy(gameObject, lifeTime);
    }

    public void Initialize(
        Vector2 targetPosition,
        float knifeDamage,
        float speed = -1f,
        float customGravityScale = -1f)
    {
        if (speed > 0f) horizontalSpeed = speed;

        SetupPhysicsComponents();

        targetTransform = null;
        damage = knifeDamage;
        startPos = transform.position;
        lastKnownTargetPos = targetPosition;

        float dist = Vector2.Distance(startPos, lastKnownTargetPos);
        flightDuration = Mathf.Clamp(dist / Mathf.Max(horizontalSpeed, 0.1f), 0.18f, 1.2f);
        arcHeight = Mathf.Clamp(dist * arcHeightFactor, minArcHeight, maxArcHeight);

        elapsed = 0f;
        hasHit = false;
        initialized = true;

        AlignToVelocity((lastKnownTargetPos - startPos) + Vector2.up * (4f * arcHeight));

        Destroy(gameObject, lifeTime);
    }

    private void Update()
    {
        if (!initialized || hasHit)
            return;

        if (targetTransform != null)
        {
            Enemy e = targetTransform.GetComponentInParent<Enemy>();
            if (e != null && !e.IsDead)
            {
                lastKnownTargetPos = GetTargetAimPoint(targetTransform);
            }
        }

        elapsed += Time.deltaTime;
        float t = Mathf.Clamp01(elapsed / Mathf.Max(flightDuration, 0.05f));

        Vector2 linearPos = Vector2.Lerp(startPos, lastKnownTargetPos, t);
        float yArc = 4f * arcHeight * t * (1f - t);
        Vector2 nextPos = new Vector2(linearPos.x, linearPos.y + yArc);

        Vector2 delta = nextPos - (Vector2)transform.position;
        AlignToVelocity(delta);

        transform.position = new Vector3(nextPos.x, nextPos.y, 0f);

        if (t >= 1f)
        {
            HitTargetAndDestroy();
        }
    }

    private void AlignToVelocity(Vector2 velocity)
    {
        if (velocity.sqrMagnitude < 0.00001f)
            return;

        float angle = Mathf.Atan2(velocity.y, velocity.x) * Mathf.Rad2Deg;
        transform.rotation = Quaternion.Euler(0f, 0f, angle + rotationOffset);
    }

    private void HitTargetAndDestroy()
    {
        if (hasHit) return;
        hasHit = true;

        if (targetTransform != null)
        {
            Enemy enemy = targetTransform.GetComponentInParent<Enemy>();
            if (enemy != null && !enemy.IsDead)
            {
                enemy.TakeDamage(damage);
            }
            else
            {
                DummyTarget dummy = targetTransform.GetComponentInParent<DummyTarget>();
                if (dummy != null) dummy.TakeDamage(damage);
            }
        }

        Destroy(gameObject);
    }

    private void OnTriggerEnter2D(Collider2D collision)
    {
        if (!initialized || hasHit || collision == null)
            return;

        // Nếu đang khóa 1 mục tiêu cụ thể, chỉ kích nổ khi chạm đúng mục tiêu đó (tránh bị chắn bởi Enemy khác trước mặt Enemy bay)
        if (targetTransform != null)
        {
            Transform hitT = collision.transform;
            bool isLockedTarget =
                hitT == targetTransform ||
                hitT.IsChildOf(targetTransform) ||
                targetTransform.IsChildOf(hitT);

            if (!isLockedTarget)
                return;

            Enemy lockedEnemy = collision.GetComponentInParent<Enemy>();
            if (lockedEnemy != null && !lockedEnemy.IsDead)
            {
                hasHit = true;
                lockedEnemy.TakeDamage(damage);
                Destroy(gameObject);
                return;
            }

            DummyTarget lockedDummy = collision.GetComponentInParent<DummyTarget>();
            if (lockedDummy != null)
            {
                hasHit = true;
                lockedDummy.TakeDamage(damage);
                Destroy(gameObject);
            }
            return;
        }

        Enemy enemy = collision.GetComponentInParent<Enemy>();
        if (enemy != null && !enemy.IsDead)
        {
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
