using System.Collections;
using UnityEngine;
using UngrownCastle.Enemies;

public class AssassinKnifeAttack : MonoBehaviour
{
    private Transform knife;

    [Header("Thông số từ Inspector của Knife")]
    public Vector3 idleLocalPos = new Vector3(-2.864f, 2.235f, 9.994007f);
    public Vector3 idleEulerAngles = new Vector3(0f, 0f, -180f);

    public Vector3 readyAttackLocalPos = new Vector3(-2.76f, 2.522f, 9.994007f);
    public Vector3 readyAttackEulerAngles = new Vector3(0f, -180f, -200f);

    [Header("Cài đặt cú chém & ném")]
    public float slashEndZ = -200f;     // Góc Z cuối cùng khi vung dao
    public float slashDuration = 0.15f; // Thời gian vung dao (giây)
    public float recoverDelay = 0.1f;   // Thời gian dừng nhẹ sau khi ném trước khi thu tay
    [SerializeField] private Vector3 throwForwardOffset = new Vector3(0.35f, -0.15f, 0f);

    [Header("Target & Combat")]
    [SerializeField] protected float attackRange = 8f;
    [SerializeField] protected LayerMask enemyLayer;
    [SerializeField] private float attackInterval = 1.2f;
    [SerializeField] private float attackDamage = 15f;

    [Header("Knife Throw")]
    [SerializeField] private GameObject knifeProjectilePrefab;
    [SerializeField] private float knifeHorizontalSpeed = 8f;

    protected Transform target;
    private float targetTimer;
    private float attackTimer = 0f;
    private bool isAttacking = false;

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
        Application.targetFrameRate = 60;
        knife = transform.Find("Knife");
        if (knife != null)
        {
            // Đặt đúng thông số ban đầu
            knife.localPosition = idleLocalPos;
            knife.GetComponent<SpriteRenderer>().flipX = false;
            knife.localRotation = Quaternion.Euler(idleEulerAngles);
        }

        Transform autoAttackObj = transform.Find("autoattack");
        if (autoAttackObj != null)
        {
            autoAttackObj.gameObject.SetActive(false);
        }
    }

    void Update()
    {
        FindTarget();

        if (target != null && !isAttacking && knife != null)
        {
            attackTimer += Time.deltaTime;
            if (attackTimer >= attackInterval)
            {
                attackTimer = 0f;
                StartCoroutine(AttackRoutine());
            }
        }
    }

    protected void FindTarget()
    {
        Vector2 searchCenter = knife != null ? (Vector2)knife.position : (Vector2)transform.position;

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

    IEnumerator AttackRoutine()
    {
        isAttacking = true;

        Quaternion idleRot = Quaternion.Euler(idleEulerAngles);
        Quaternion startSlashRot = Quaternion.Euler(readyAttackEulerAngles);
        Quaternion endSlashRot = Quaternion.Euler(readyAttackEulerAngles.x, readyAttackEulerAngles.y, slashEndZ);
        Vector3 throwEndLocalPos = readyAttackLocalPos + throwForwardOffset;
        knife.GetComponent<SpriteRenderer>().flipX = false;

        if (knife != null)
        {
            // 1. Đưa dao lên vị trí chuẩn bị ném mượt mà bằng Position + Rotation
            float windupTime = Mathf.Max(slashDuration * 0.4f, 0.02f);
            float elapsed = 0f;
            while (elapsed < windupTime)
            {
                elapsed += Time.deltaTime;
                float t = Mathf.Clamp01(elapsed / windupTime);
                knife.localPosition = Vector3.Lerp(idleLocalPos, readyAttackLocalPos, t);
                knife.localRotation = Quaternion.Slerp(idleRot, startSlashRot, t);
                yield return null;
            }
            knife.localPosition = readyAttackLocalPos;
            knife.localRotation = startSlashRot;

            // 2. Vung tay đẩy vị trí dao về phía trước để ném
            float throwTime = Mathf.Max(slashDuration * 0.6f, 0.02f);
            elapsed = 0f;

            while (elapsed < throwTime)
            {
                elapsed += Time.deltaTime;
                float t = Mathf.Clamp01(elapsed / throwTime);
                knife.localPosition = Vector3.Lerp(readyAttackLocalPos, throwEndLocalPos, t);
                knife.localRotation = Quaternion.Slerp(startSlashRot, endSlashRot, t);
                yield return null;
            }
            knife.localPosition = throwEndLocalPos;
            knife.localRotation = endSlashRot;
        }

        // Ném 1 knife theo quỹ đạo Parabol tới vị trí Enemy mục tiêu
        if (target != null)
        {
            Enemy enemy = target.GetComponentInParent<Enemy>();
            if (enemy != null && !enemy.IsDead)
            {
                ThrowKnife(enemy.transform, attackDamage);
            }
        }

        // Chờ một nhịp ngắn ở cuối động tác ném
        yield return new WaitForSeconds(recoverDelay);

        if (knife != null)
        {
            // 3. Thu dao về lại vị trí nghỉ ban đầu
            float returnTime = Mathf.Max(slashDuration * 0.5f, 0.02f);
            float elapsed = 0f;
            Vector3 currentLocalPos = knife.localPosition;
            Quaternion currentLocalRot = knife.localRotation;
            while (elapsed < returnTime)
            {
                elapsed += Time.deltaTime;
                float t = Mathf.Clamp01(elapsed / returnTime);
                knife.localPosition = Vector3.Lerp(currentLocalPos, idleLocalPos, t);
                knife.localRotation = Quaternion.Slerp(currentLocalRot, idleRot, t);
                yield return null;
            }
            knife.localPosition = idleLocalPos;
            knife.localRotation = idleRot;
        }

        isAttacking = false;
    }

    private void ThrowKnife(Transform targetEnemy, float dmg)
    {
        if (targetEnemy == null) return;

        Vector3 rawPos = knife != null ? knife.position : transform.position;
        Vector3 spawnPos = new Vector3(rawPos.x, rawPos.y, 0f);
        GameObject knifeObj;

        if (knifeProjectilePrefab != null)
        {
            knifeObj = Instantiate(knifeProjectilePrefab, spawnPos, Quaternion.identity);
        }
        else if (knife != null)
        {
            knifeObj = Instantiate(knife.gameObject, spawnPos, Quaternion.identity);
            Vector3 lossy = knife.lossyScale;
            knifeObj.transform.localScale = new Vector3(Mathf.Abs(lossy.x), Mathf.Abs(lossy.y), 1f);
        }
        else
        {
            return;
        }

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
}
