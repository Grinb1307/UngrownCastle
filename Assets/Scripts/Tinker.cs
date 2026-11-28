using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UngrownCastle.Enemies;

public class Tinker : MonoBehaviour
{
    [Header("Cài đặt vị trí xuất hiện trên cao")]
    public float spawnOffsetX = 4.0f;      // Khoảng cách ngang so với Tinker để bom xuất hiện
    public float spawnOffsetY = 3.5f;      // Độ cao xuất hiện ở trên trời so với Tinker
    public float fallSpeed = 8.0f;         // Tốc độ rơi của bom theo trục Y mỗi giây
    public float groundLevelY = -1.5f;     // Độ cao mặt đất (khi rơi đến đây bom sẽ biến mất)

    [Header("Hồi chiêu")]
    public float cooldownTime = 6.0f;      // Thời gian hồi chiêu (6 giây)

    [Header("Target & Damage")]
    [SerializeField] protected float attackRange = 10f;
    [SerializeField] protected LayerMask enemyLayer;
    [SerializeField] private float bombDamage = 25f;
    [SerializeField] private float bombExplosionRadius = 1.5f;

    protected Transform target;
    private float targetTimer;

    private Transform[] bombs;             // Mảng chứa 7 quả bom
    private Vector3[] initialLocalPositions; // Lưu vị trí gốc của các quả bom
    private bool isReadyToUse = true;      // Mặc định ban đầu vào game xài được luôn
    private bool isFalling = false;

    void Awake()
    {
        Application.targetFrameRate = 60;
        // Tự động tìm tất cả các object con có chứa chữ "skill" trong tên để gom thành 7 quả bom
        List<Transform> bombList = new List<Transform>();

        foreach (Transform child in transform)
        {
            if (child.name.Contains("skill"))
            {
                bombList.Add(child);
            }
        }

        bombs = bombList.ToArray();
        initialLocalPositions = new Vector3[bombs.Length];

        // Lưu vị trí gốc và ẩn tất cả bom đi lúc đầu (SetActive(false))
        for (int i = 0; i < bombs.Length; i++)
        {
            initialLocalPositions[i] = bombs[i].localPosition;
            bombs[i].gameObject.SetActive(false);
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

    // Sự kiện khi người chơi nhấn chuột vào Tinker
    void OnMouseDown()
    {
        // Nếu chưa hồi xong hoặc bom đang rơi thì không nhận click
        if (!isReadyToUse || isFalling) return;

        StartCoroutine(TriggerBombSkillSequence());
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

    IEnumerator TriggerBombSkillSequence()
    {
        isReadyToUse = false; // Khóa skill, bắt đầu tính cooldown sau khi dùng
        isFalling = true;

        Vector3 tinkerPos = transform.position;
        float centerX = target != null ? target.position.x : (tinkerPos.x + spawnOffsetX);
        float targetGroundY = target != null ? target.position.y : groundLevelY;

        // 1. Đặt 7 quả bom xuất hiện trên cao (rải quanh vị trí Enemy gần nhất hoặc offset mặc định)
        for (int i = 0; i < bombs.Length; i++)
        {
            if (bombs[i] == null) continue;

            float randomOffsetX = Random.Range(-1.5f, 1.5f);
            Vector3 spawnWorldPos = new Vector3(centerX + randomOffsetX, tinkerPos.y + spawnOffsetY, tinkerPos.z);

            bombs[i].position = spawnWorldPos;
            bombs[i].gameObject.SetActive(true); // Bật bom lên
        }

        // 2. Vòng lặp cho bom rơi xuống đất theo từng khung hình
        bool allLanded = false;
        while (!allLanded)
        {
            allLanded = true;

            for (int i = 0; i < bombs.Length; i++)
            {
                if (bombs[i] == null || !bombs[i].gameObject.activeSelf) continue;

                // Giảm dần độ cao Y theo khung hình
                Vector3 currentPos = bombs[i].position;
                currentPos.y -= fallSpeed * Time.deltaTime;
                bombs[i].position = currentPos;

                // Kiểm tra nếu rơi chạm mặt đất
                if (currentPos.y <= targetGroundY)
                {
                    DealAreaDamage(currentPos, bombExplosionRadius, bombDamage);
                    bombs[i].gameObject.SetActive(false); // Chạm đất thì nổ gây damage rồi biến mất
                }
                else
                {
                    // Nếu vẫn còn ít nhất 1 quả chưa chạm đất thì tiếp tục chạy vòng lặp
                    allLanded = false;
                }
            }

            yield return null; // Chờ sang frame tiếp theo
        }

        isFalling = false;

        // ==========================================
        // BẮT ĐẦU ĐẾM NGƯỢC 6 GIÂY (COOLDOWN)
        // ==========================================
        float timer = 0f;
        while (timer < cooldownTime)
        {
            timer += Time.deltaTime;
            yield return null;
        }

        // Hết 6 giây, skill sẵn sàng được bấm lại
        isReadyToUse = true;
    }
}
