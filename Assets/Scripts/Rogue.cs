using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UngrownCastle.Enemies;

public class Rogue : MonoBehaviour
{
    [Header("Tham chiếu Object con")]
    public Transform knifeTransform;

    [Header("Target")]
    [SerializeField] protected float attackRange = 8f;
    [SerializeField] protected LayerMask enemyLayer;

    [Header("Thông số từ Inspector của Knife")]
    public Vector3 idleLocalPos = new Vector3(-0.243f, 0.244f, -0.025948687f);
    public Vector3 idleEulerAngles = new Vector3(0f, 0f, 90.75f);

    public Vector3 readyAttackLocalPos = new Vector3(-0.18f, 0.36f, -0.025948687f);
    public Vector3 readyAttackEulerAngles = new Vector3(0f, 0f, 90f);
    public Vector3 AttackPos = new Vector3(-0.08f, 0.20f, -0.025948687f);

    [Header("Cài đặt cú chém & ném")]
    public float slashEndZ = 41.67f;     // Góc Z cuối cùng khi vung dao chém/ném xuống
    public float slashDuration = 0.15f;  // Thời gian vung dao (giây)
    public float recoverDelay = 0.1f;    // Thời gian dừng nhẹ sau khi ném trước khi thu dao

    [Header("Combat Stats")]
    [SerializeField] private float attackDamage = 12f;
    [SerializeField] private float attackInterval = 1.2f;
    [SerializeField] private float critChance = 0.3f;
    [SerializeField] private float critMultiplier = 2f;

    [Header("Active Skill & Cooldown")]
    [SerializeField] private float cooldownTime = 6f;
    [SerializeField] private float skillDamageMultiplier = 3f;
    [SerializeField] private float skillRadius = 2.5f;

    [Header("Knife Throw")]
    [SerializeField] private GameObject knifeProjectilePrefab;
    [SerializeField] private float knifeHorizontalSpeed = 8f;

    protected Transform target;
    private float targetTimer;
    private float attackTimer = 0f;
    private bool isAttacking = false;
    private bool isReadyToUse = true;
    private Transform autoAttackTemplate;

    void Awake()
    {
        if (knifeTransform == null)
        {
            knifeTransform = transform.Find("knife");
        }
        if (knifeTransform != null)
        {
            knifeTransform.localPosition = idleLocalPos;
            knifeTransform.localRotation = Quaternion.Euler(idleEulerAngles);
        }

        autoAttackTemplate = transform.Find("autoattack");
        if (autoAttackTemplate != null)
        {
            autoAttackTemplate.gameObject.SetActive(false);
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

        if (!isAttacking)
        {
            if (target != null)
            {
                if (knifeTransform != null)
                {
                    knifeTransform.localPosition = readyAttackLocalPos;
                    knifeTransform.localRotation = Quaternion.Euler(readyAttackEulerAngles);
                }

                attackTimer += Time.deltaTime;
                if (attackTimer >= attackInterval)
                {
                    attackTimer = 0f;
                    StartCoroutine(AttackRoutine(false));
                }
            }
            else
            {
                attackTimer = 0f;
                if (knifeTransform != null)
                {
                    knifeTransform.localPosition = idleLocalPos;
                    knifeTransform.localRotation = Quaternion.Euler(idleEulerAngles);
                }
            }
        }
    }

    void OnMouseDown()
    {
        if (!isReadyToUse || isAttacking) return;

        isReadyToUse = false;
        StartCoroutine(AttackRoutine(true));
        StartCoroutine(SkillCooldownRoutine());
    }

    protected void FindTarget()
    {
        Vector2 searchCenter = knifeTransform != null ? (Vector2)knifeTransform.position : (Vector2)transform.position;

        // Giữ nguyên mục tiêu hiện tại cho đến khi Enemy chết hoặc ra khỏi tầm đánh
        if (target != null)
        {
            Enemy currentEnemy = target.GetComponentInParent<Enemy>();
            if (currentEnemy != null && !currentEnemy.IsDead &&
                Vector2.Distance(searchCenter, target.position) <= attackRange)
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
            searchCenter,
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

            float distance = Vector2.Distance(searchCenter, enemyComp.transform.position);
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

    IEnumerator AttackRoutine(bool isSkill)
    {
        isAttacking = true;

        if (knifeTransform != null)
        {
            // 1. Đưa dao vào điểm bắt đầu tấn công
            knifeTransform.localPosition = readyAttackLocalPos;
            Quaternion startSlashRot = Quaternion.Euler(readyAttackEulerAngles);
            Quaternion endSlashRot = Quaternion.Euler(readyAttackEulerAngles.x, readyAttackEulerAngles.y, slashEndZ);
            knifeTransform.localRotation = startSlashRot;

            // 2. Vung dao chém/ném từ readyAttackLocalPos -> AttackPos và startSlashRot -> endSlashRot
            float elapsed = 0f;
            while (elapsed < slashDuration)
            {
                elapsed += Time.deltaTime;
                float t = Mathf.Clamp01(elapsed / Mathf.Max(slashDuration, 0.001f));
                knifeTransform.localPosition = Vector3.Lerp(readyAttackLocalPos, AttackPos, t);
                knifeTransform.localRotation = Quaternion.Slerp(startSlashRot, endSlashRot, t);
                yield return null;
            }
            knifeTransform.localPosition = AttackPos;
            knifeTransform.localRotation = endSlashRot;
        }

        if (isSkill)
        {
            Vector2 hitCenter = target != null ? (Vector2)target.position : (Vector2)transform.position;
            DealAreaDamage(hitCenter, skillRadius, attackDamage * skillDamageMultiplier);
        }
        else
        {
            if (target != null)
            {
                Enemy enemy = target.GetComponentInParent<Enemy>();
                if (enemy != null && !enemy.IsDead)
                {
                    float dmg = Random.value < critChance ? attackDamage * critMultiplier : attackDamage;
                    ThrowKnife(enemy.transform, dmg);
                }
            }
        }

        // Chờ một nhịp ngắn ở cuối đòn ném
        yield return new WaitForSeconds(recoverDelay);

        if (knifeTransform != null)
        {
            // 3. Nếu còn Enemy thì giữ tư thế ReadyAttack, ngược lại thu dao về Idle
            if (target != null)
            {
                knifeTransform.localPosition = readyAttackLocalPos;
                knifeTransform.localRotation = Quaternion.Euler(readyAttackEulerAngles);
            }
            else
            {
                knifeTransform.localPosition = idleLocalPos;
                knifeTransform.localRotation = Quaternion.Euler(idleEulerAngles);
            }
        }

        isAttacking = false;
    }

    private void ThrowKnife(Transform targetEnemy, float dmg)
    {
        if (targetEnemy == null) return;

        Vector3 rawPos = knifeTransform != null ? knifeTransform.position : transform.position;
        Vector3 spawnPos = new Vector3(rawPos.x, rawPos.y, 0f);
        GameObject knifeObj;

        if (knifeProjectilePrefab != null)
        {
            knifeObj = Instantiate(knifeProjectilePrefab, spawnPos, Quaternion.identity);
        }
        else if (autoAttackTemplate != null)
        {
            knifeObj = Instantiate(autoAttackTemplate.gameObject, spawnPos, Quaternion.identity);
            Vector3 lossy = autoAttackTemplate.lossyScale;
            knifeObj.transform.localScale = new Vector3(Mathf.Abs(lossy.x), Mathf.Abs(lossy.y), 1f);
        }
        else if (knifeTransform != null)
        {
            knifeObj = Instantiate(knifeTransform.gameObject, spawnPos, Quaternion.identity);
            Vector3 lossy = knifeTransform.lossyScale;
            knifeObj.transform.localScale = new Vector3(Mathf.Abs(lossy.x), Mathf.Abs(lossy.y), 1f);
        }
        else
        {
            return;
        }

        // Luôn bật SetActive(true) cho dao bay ra kể cả khi template ban đầu bị SetActive(false)
        knifeObj.SetActive(true);

        SpriteRenderer sr = knifeObj.GetComponentInChildren<SpriteRenderer>(true);
        if (sr != null)
        {
            sr.enabled = true;
            sr.sortingOrder = 5;
        }

        KnifeProjectile proj = knifeObj.GetComponent<KnifeProjectile>();
        if (proj == null) proj = knifeObj.AddComponent<KnifeProjectile>();
        proj.Initialize(targetEnemy, dmg, knifeHorizontalSpeed);
    }

    IEnumerator SkillCooldownRoutine()
    {
        yield return new WaitForSeconds(cooldownTime);
        isReadyToUse = true;
    }
}
