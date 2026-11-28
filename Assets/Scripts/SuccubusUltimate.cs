using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UngrownCastle.Enemies;

public class SuccubusUltimate : MonoBehaviour
{
    [Header("Tham chiếu các Object con")]
    public Transform ultimateTransform;  // Kéo object 'Ultimate' vào đây
    public Transform heartTransform;     // Kéo object 'Heart' vào đây
    public Transform bodyTransform;      // Kéo object 'body' vào đây (để lấy chuẩn vị trí)

    [Header("Cài đặt thời gian và hiệu ứng")]
    public float ultimateCooldown = 6f;  // Thời gian hồi chiêu (6 giây)
    public float ultimateDuration = 0.3f;// Thời gian Ultimate mọc dài từ dưới lên
    public float heartDuration = 0.25f;  // Thời gian Heart bung to ra
    public float holdDuration = 0.5f;    // Thời gian giữ cả 2 hiển thị trước khi tắt hoàn toàn
    public float dropOffset = 1.0f;      // Khoảng cách thụt xuống bên dưới body để mọc lên

    [Header("Target & Damage")]
    [SerializeField] protected float attackRange = 8f;
    [SerializeField] protected LayerMask enemyLayer;
    [SerializeField] private float skillDamage = 30f;
    [SerializeField] private float skillRadius = 3f;

    protected Transform target;
    private float targetTimer;

    private Vector3 ultimateTargetPos;
    private Vector3 ultimateTargetScale;
    private Vector3 heartTargetScale;
    private bool isReadyToUse = true;
    private bool isExecuting = false;

    void Awake()
    {
        Application.targetFrameRate = 60;
        if (ultimateTransform == null) ultimateTransform = transform.Find("Ultimate");
        if (heartTransform == null) heartTransform = transform.Find("Heart");
        if (bodyTransform == null) bodyTransform = transform.Find("body");

        if (ultimateTransform != null)
        {
            ultimateTargetPos = ultimateTransform.localPosition;
            ultimateTargetScale = ultimateTransform.localScale;
            ultimateTransform.gameObject.SetActive(false);
        }

        if (heartTransform != null)
        {
            heartTargetScale = heartTransform.localScale;
            heartTransform.gameObject.SetActive(false);
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
    }

    void OnMouseDown()
    {
        if (!isReadyToUse || isExecuting) return;

        TriggerUltimateSequence();
    }

    protected void FindTarget()
    {
        if (target != null)
        {
            Enemy currentEnemy = target.GetComponentInParent<Enemy>();
            if (currentEnemy == null || currentEnemy.IsDead ||
                Vector2.Distance(transform.position, target.position) > attackRange)
            {
                target = null;
            }
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

    private void DealAreaDamage(Vector2 center, float radius, float dmg)
    {
        Collider2D[] hits = Physics2D.OverlapCircleAll(center, radius, enemyLayer);
        HashSet<Enemy> affectedEnemies = new HashSet<Enemy>();
        foreach (Collider2D hit in hits)
        {
            if (hit == null) continue;
            Enemy enemy = hit.GetComponentInParent<Enemy>();
            if (enemy == null || enemy.IsDead || affectedEnemies.Contains(enemy)) continue;
            affectedEnemies.Add(enemy);
            enemy.TakeDamage(dmg);
        }
    }

    void TriggerUltimateSequence()
    {
        isReadyToUse = false;
        isExecuting = true;
        StopAllCoroutines();
        StartCoroutine(RunUltimateAnimation());
    }

    IEnumerator RunUltimateAnimation()
    {
        // ==========================================
        // BƯỚC 1: ULTIMATE MỌC TỪ DƯỚI LÊN
        // ==========================================
        if (ultimateTransform != null)
        {
            ultimateTransform.gameObject.SetActive(true);

            // Đặt vị trí xuất phát thấp hơn bên dưới (tạo cảm giác từ phía sau/dưới trồi lên)
            Vector3 startPos = ultimateTargetPos;
            startPos.y -= dropOffset;
            ultimateTransform.localPosition = startPos;

            // Scale Y ban đầu bằng 0
            ultimateTransform.localScale = new Vector3(ultimateTargetScale.x, 0f, ultimateTargetScale.z);

            float elapsed = 0f;
            while (elapsed < ultimateDuration)
            {
                elapsed += Time.deltaTime;
                float progress = Mathf.Clamp01(elapsed / ultimateDuration);

                // Nội suy vị trí trồi lên và kéo dài trục Y
                ultimateTransform.localPosition = Vector3.Lerp(startPos, ultimateTargetPos, progress);
                float currentY = Mathf.Lerp(0f, ultimateTargetScale.y, progress);
                ultimateTransform.localScale = new Vector3(ultimateTargetScale.x, currentY, ultimateTargetScale.z);

                yield return null;
            }

            // Đảm bảo khớp chuẩn 100% khi kết thúc bước 1
            ultimateTransform.localPosition = ultimateTargetPos;
            ultimateTransform.localScale = ultimateTargetScale;
        }

        // Gây sát thương vùng lên Enemy gần nhất (hoặc tại vị trí Ultimate)
        Vector2 damageCenter = target != null
            ? (Vector2)target.position
            : (ultimateTransform != null ? (Vector2)ultimateTransform.position : (Vector2)transform.position);
        DealAreaDamage(damageCenter, skillRadius, skillDamage);

        // ==========================================
        // BƯỚC 2: HEART BUNG TO DẦN TẠI VỊ TRÍ ĐÓ
        // ==========================================
        if (heartTransform != null)
        {
            heartTransform.gameObject.SetActive(true);

            // Đặt Heart nằm đúng vị trí của Ultimate và scale ban đầu bằng 0 (biến mất)
            if (ultimateTransform != null)
            {
                heartTransform.position = ultimateTransform.position;
            }
            heartTransform.localScale = Vector3.zero;

            float elapsed = 0f;
            while (elapsed < heartDuration)
            {
                elapsed += Time.deltaTime;
                float progress = Mathf.Clamp01(elapsed / heartDuration);

                // Phóng to dần Heart từ 0 đến kích thước chuẩn
                heartTransform.localScale = Vector3.Lerp(Vector3.zero, heartTargetScale, progress);

                yield return null;
            }

            heartTransform.localScale = heartTargetScale;
        }

        // ==========================================
        // BƯỚC 3: GIỮ HIỆN THỊ TRONG CHỐC LÁT RỒI TẮT
        // ==========================================
        yield return new WaitForSeconds(holdDuration);

        if (ultimateTransform != null) ultimateTransform.gameObject.SetActive(false);
        if (heartTransform != null) heartTransform.gameObject.SetActive(false);

        isExecuting = false;

        // Hồi chiêu
        float timer = 0f;
        while (timer < ultimateCooldown)
        {
            timer += Time.deltaTime;
            yield return null;
        }

        isReadyToUse = true;
    }
}
