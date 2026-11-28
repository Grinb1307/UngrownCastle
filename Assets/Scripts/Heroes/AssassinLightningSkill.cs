using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UngrownCastle.Enemies;

public class AssassinLightningSkill : MonoBehaviour
{
    [Header("Cài đặt Tia điện")]
    public GameObject lightningPrefab;   // Kéo Prefab tia điện vào đây
    public Transform spawnPoint;         // Kéo object con SpawnPoint vào đây

    [Header("Thông số phóng và thu nhỏ")]
    public Vector3 maxScale = new Vector3(5f, 5f, 1f);
    public float expandDuration = 0.08f;
    public float shrinkDuration = 0.25f;

    [Header("Góc xoay bổ sung")]
    public float rotationOffsetZ = 315f; // Nhập số độ bạn muốn xoay ở đây

    [Header("Target & Cooldown")]
    [SerializeField] protected float attackRange = 8f;
    [SerializeField] protected LayerMask enemyLayer;
    [SerializeField] private float cooldownTime = 6f;
    [SerializeField] private float skillDamage = 40f;
    [SerializeField] private float skillRadius = 2.5f;

    protected Transform target;
    private float targetTimer;
    private bool isCasting = false;
    private bool isReadyToUse = true;

    void Awake()
    {
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

    void Start()
    {
        if (spawnPoint == null)
        {
            Transform knife = transform.Find("Knife");
            spawnPoint = knife != null ? knife : transform;
        }
    }

    void Update()
    {
        FindTarget();
    }

    void OnMouseDown()
    {
        if (!isReadyToUse || isCasting || lightningPrefab == null) return;

        isReadyToUse = false;
        StartCoroutine(CastLightningRoutine());
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

    IEnumerator CastLightningRoutine()
    {
        isCasting = true;

        // Ưu tiên giáng sét tại vị trí Enemy gần nhất (nếu có), ngược lại dùng spawnPoint
        Vector3 basePos = target != null
            ? target.position
            : (spawnPoint != null ? spawnPoint.position : transform.position);

        // Ép tọa độ Z về 0 để chắc chắn tia sét không bị ẩn sau Camera
        Vector3 spawnPos = new Vector3(basePos.x, basePos.y, 0f);
        Quaternion customRot = spawnPoint != null
            ? spawnPoint.rotation * Quaternion.Euler(0, 0, rotationOffsetZ)
            : Quaternion.Euler(0, 0, rotationOffsetZ);

        GameObject lightning = Instantiate(lightningPrefab, spawnPos, customRot);
        lightning.transform.localScale = Vector3.zero;

        // Phóng to
        float elapsed = 0f;
        while (elapsed < expandDuration)
        {
            if (lightning == null) break;
            elapsed += Time.deltaTime;
            float t = elapsed / expandDuration;
            lightning.transform.localScale = Vector3.Lerp(Vector3.zero, maxScale, t);
            yield return null;
        }
        if (lightning != null) lightning.transform.localScale = maxScale;

        // Gây sát thương vùng tại tâm sét
        DealAreaDamage(spawnPos, skillRadius, skillDamage);

        // Thu nhỏ
        elapsed = 0f;
        while (elapsed < shrinkDuration)
        {
            if (lightning == null) break;
            elapsed += Time.deltaTime;
            float t = elapsed / shrinkDuration;
            lightning.transform.localScale = Vector3.Lerp(maxScale, Vector3.zero, t);
            yield return null;
        }

        if (lightning != null) Destroy(lightning);
        isCasting = false;

        // Hồi chiêu
        float timer = 0f;
        while (timer < cooldownTime)
        {
            timer += Time.deltaTime;
            yield return null;
        }

        isReadyToUse = true;
    }
}
