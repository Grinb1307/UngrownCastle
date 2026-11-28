using System.Collections;
using UnityEngine;
using UngrownCastle.Enemies;

public class Succubus : MonoBehaviour
{
    [Header("Tham chiếu Object con")]
    public Transform clawTransform;        // Kéo object 'skill' vào đây
    public Transform bodyTransform;        // Kéo object 'body' vào đây

    [Header("Thông số đánh thường")]
    public float attackInterval = 1.5f;    // Thời gian hồi giữa mỗi lần cào
    public float slashDuration = 0.18f;    // Thời gian thực hiện 1 nét cào
    public float slashDistance = 0.6f;     // Quãng đường trượt từ trên-trái xuống dưới-phải
    public float slashAngle = 70f;         // Góc nghiêng chéo bên trái 70 độ

    [Header("Target")]
    [SerializeField] protected float attackRange = 8f;
    [SerializeField] protected LayerMask enemyLayer;

    [Header("Combat")]
    [SerializeField] private float attackDamage = 12f;

    protected Transform target;
    private float targetTimer;

    private Vector3 baseScale;
    private Vector3 startWorldPos;
    private Vector3 endWorldPos;
    private float attackTimer = 0f;
    private float animTimer = 0f;
    private bool isSlashing = false;

    void Awake()
    {
        Application.targetFrameRate = 60;
        if (clawTransform == null) clawTransform = transform.Find("skill");
        if (bodyTransform == null) bodyTransform = transform.Find("body");

        if (clawTransform != null)
        {
            baseScale = clawTransform.localScale;
            clawTransform.gameObject.SetActive(false);
        }

        if (enemyLayer.value == 0)
        {
            int mask = LayerMask.GetMask("Enemy");
            enemyLayer = mask != 0 ? (LayerMask)mask : (LayerMask)(~0);
        }

        EnsureBoxCollider2D();
    }

    private void EnsureBoxCollider2D()
    {
        if (GetComponent<Collider2D>() != null) return;
        BoxCollider2D col = gameObject.AddComponent<BoxCollider2D>();
        SpriteRenderer[] renderers = GetComponentsInChildren<SpriteRenderer>(false);
        if (renderers != null && renderers.Length > 0)
        {
            Bounds bounds = renderers[0].bounds;
            for (int i = 1; i < renderers.Length; i++)
            {
                if (renderers[i] != null) bounds.Encapsulate(renderers[i].bounds);
            }
            col.offset = transform.InverseTransformPoint(bounds.center);
            Vector3 lossy = transform.lossyScale;
            col.size = new Vector2(
                Mathf.Abs(lossy.x) > 0.0001f ? bounds.size.x / Mathf.Abs(lossy.x) : bounds.size.x,
                Mathf.Abs(lossy.y) > 0.0001f ? bounds.size.y / Mathf.Abs(lossy.y) : bounds.size.y
            );
        }
        else
        {
            col.size = new Vector2(1.5f, 1.5f);
        }
    }

    void Update()
    {
        FindTarget();

        if (clawTransform == null || bodyTransform == null) return;

        // 1. Đếm nhịp tự động đánh thường khi có Enemy trong tầm
        if (!isSlashing)
        {
            if (target == null) return;

            attackTimer += Time.deltaTime;
            if (attackTimer >= attackInterval)
            {
                attackTimer = 0f;
                PerformClawAttack();
            }
        }
        // 2. Diễn hoạt nét cào chéo từ trên-trái xuống dưới-phải và nhỏ dần theo chiều dọc
        else
        {
            animTimer += Time.deltaTime;
            float progress = Mathf.Clamp01(animTimer / slashDuration);

            // Trượt vị trí dọc theo góc nghiêng 70 độ (từ trái-trên sang phải-dưới)
            clawTransform.position = Vector3.Lerp(startWorldPos, endWorldPos, progress);

            // Làm nhỏ dần theo chiều dọc (trục Y giảm từ kích thước gốc về 0)
            float currentScaleY = Mathf.Lerp(baseScale.y, 0f, progress);
            clawTransform.localScale = new Vector3(baseScale.x, currentScaleY, baseScale.z);

            // Kết thúc nét cào
            if (progress >= 1f)
            {
                isSlashing = false;
                clawTransform.gameObject.SetActive(false);
            }
        }
    }

    protected void FindTarget()
    {
        // Giữ nguyên mục tiêu hiện tại cho đến khi Enemy chết hoặc ra khỏi tầm đánh
        if (target != null)
        {
            Enemy currentEnemy = target.GetComponentInParent<Enemy>();
            if (currentEnemy != null && !currentEnemy.IsDead &&
                Vector2.Distance(transform.position, target.position) <= attackRange)
            {
                return;
            }
            target = null;
            targetTimer = 0.25f; // Tìm ngay Enemy gần nhất tiếp theo
        }

        targetTimer += Time.deltaTime;
        if (targetTimer < 0.25f) return;
        targetTimer = 0f;

        Collider2D[] enemies = Physics2D.OverlapCircleAll(
            transform.position,
            attackRange,
            enemyLayer
        );

        Transform nearest = null;
        float minDistance = Mathf.Infinity;

        foreach (Collider2D col in enemies)
        {
            if (col == null) continue;
            Enemy enemyComp = col.GetComponentInParent<Enemy>();
            if (enemyComp == null || enemyComp.IsDead) continue;

            float distance = Vector2.Distance(transform.position, enemyComp.transform.position);
            if (distance < minDistance)
            {
                minDistance = distance;
                nearest = enemyComp.transform;
            }
        }

        target = nearest;
    }

    void PerformClawAttack()
    {
        // Bật vết cào và xoay góc chéo bên trái 70 độ
        clawTransform.gameObject.SetActive(true);
        clawTransform.localRotation = Quaternion.Euler(0f, 0f, slashAngle);

        // Lấy vị trí của Enemy gần nhất làm điểm giữa của vết cào (hoặc fallback về body)
        Vector3 centerPos = target != null ? target.position : bodyTransform.position;

        // Tính hướng trượt theo góc xoay hiện tại của vết cào (từ trên-trái xuống dưới-phải)
        Vector3 slashDirection = -clawTransform.up;

        startWorldPos = centerPos - slashDirection * (slashDistance * 0.5f);
        endWorldPos = centerPos + slashDirection * (slashDistance * 0.5f);

        // Đặt trạng thái khởi đầu
        clawTransform.position = startWorldPos;
        clawTransform.localScale = baseScale;

        // Gây sát thương lên Enemy mục tiêu
        if (target != null)
        {
            Enemy enemy = target.GetComponentInParent<Enemy>();
            if (enemy != null && !enemy.IsDead)
            {
                enemy.TakeDamage(attackDamage);
            }
        }

        animTimer = 0f;
        isSlashing = true;
    }
}
