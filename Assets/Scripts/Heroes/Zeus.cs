using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UngrownCastle.Enemies;

public class Zeus : MonoBehaviour
{
    private bool isAttacking = false;

    public Transform Hammer;

    private Vector3 idleLocalPos = new Vector3(-1.44f, 0.121f, -0.002693f);
    private Vector3 idleEulerAngles = new Vector3(0f, 0f, -60f);
    private Vector3 isAttackingPos = new Vector3(-1.159f, 0.012f, -0.002693f);
    private Vector3 isAttackingEulerAngles = new Vector3(0f, 0f, -100f);

    public GameObject Skill;

    public float HammerEndZ = 160f;     // Góc Z cuối cùng khi vung dao chém xuống
    public float HammerDuration = 0.15f; // Thời gian vung dao (giây)
    public float recoverDelay = 0.1f;   // Thời gian dừng nhẹ sau khi chém trước khi thu dao

    public float count_timer = 0f;

    [Header("Target")]
    [SerializeField] protected float attackRange = 8f;
    [SerializeField] protected LayerMask enemyLayer;

    [Header("Combat & Skill")]
    [SerializeField] private float attackInterval = 1.5f;
    [SerializeField] private float attackDamage = 15f;
    [SerializeField] private int autoAttackMaxTargets = 4;
    [SerializeField] private float skillDamage = 35f;
    [SerializeField] private int skillMaxTargets = 4;
    [SerializeField] private float cooldownTime = 6f;

    protected Transform target;
    private float targetTimer;
    private float attackTimer = 0f;
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
        Application.targetFrameRate = 60;
        if (Hammer == null) Hammer = transform.Find("Hammer");
        if (Hammer != null)
        {
            // Đặt đúng thông số ban đầu như Ảnh 1
            Hammer.localPosition = idleLocalPos;
            Hammer.localRotation = Quaternion.Euler(idleEulerAngles);
        }
    }

    void Update()
    {
        FindTarget();

        if (target != null && !isAttacking)
        {
            attackTimer += Time.deltaTime;
            if (attackTimer >= attackInterval)
            {
                attackTimer = 0f;
                StartCoroutine(AttackRoutine(false));
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

    private List<Transform> FindNearestEnemies(int maxCount)
    {
        Collider2D[] hits = Physics2D.OverlapCircleAll(transform.position, attackRange, enemyLayer);
        HashSet<Enemy> seen = new HashSet<Enemy>();
        List<Enemy> candidates = new List<Enemy>();

        foreach (Collider2D col in hits)
        {
            if (col == null) continue;
            Enemy enemyComp = col.GetComponentInParent<Enemy>();
            if (enemyComp == null || enemyComp.IsDead || seen.Contains(enemyComp)) continue;
            seen.Add(enemyComp);
            candidates.Add(enemyComp);
        }

        candidates.Sort((a, b) =>
        {
            float distA = Vector2.Distance(transform.position, a.transform.position);
            float distB = Vector2.Distance(transform.position, b.transform.position);
            return distA.CompareTo(distB);
        });

        List<Transform> result = new List<Transform>();

        // Ưu tiên đặt target đang khóa lên đầu
        if (target != null)
        {
            Enemy locked = target.GetComponentInParent<Enemy>();
            if (locked != null && !locked.IsDead && seen.Contains(locked))
            {
                result.Add(locked.transform);
                candidates.Remove(locked);
            }
        }

        for (int i = 0; i < candidates.Count && result.Count < maxCount; i++)
        {
            result.Add(candidates[i].transform);
        }

        return result;
    }

    IEnumerator AttackRoutine(bool isSkill)
    {
        isAttacking = true;

        if (Hammer != null)
        {
            // 1. Đưa búa vào điểm bắt đầu tấn công (Ảnh 2)
            Hammer.localPosition = isAttackingPos;
            Quaternion startSlashRot = Quaternion.Euler(isAttackingEulerAngles);
            Quaternion endSlashRot = Quaternion.Euler(isAttackingEulerAngles.x, isAttackingEulerAngles.y, HammerEndZ);
            Hammer.localRotation = startSlashRot;

            // 2. Vung búa chém xuống mượt mà bằng Quaternion.Slerp
            float elapsed = 0f;
            while (elapsed < HammerDuration)
            {
                elapsed += Time.deltaTime;
                Hammer.localRotation = Quaternion.Slerp(startSlashRot, endSlashRot, elapsed / HammerDuration);
                yield return null;
            }
            Hammer.localRotation = endSlashRot;
        }

        if (isSkill)
        {
            List<Transform> targets = FindNearestEnemies(skillMaxTargets);
            ZeusSkill controller = Skill != null ? Skill.GetComponent<ZeusSkill>() : null;

            if (targets.Count > 0)
            {
                if (controller != null)
                {
                    controller.TriggerSkillOnTargets(targets);
                }

                // Mỗi tia sét đánh đúng 1 Enemy riêng biệt
                foreach (Transform t in targets)
                {
                    if (t == null) continue;
                    Enemy e = t.GetComponentInParent<Enemy>();
                    if (e != null && !e.IsDead)
                    {
                        e.TakeDamage(skillDamage);
                    }
                }
            }
            else if (controller != null)
            {
                controller.TriggerSkill();
            }
        }
        else
        {
            // Auto-attack: giáng tối đa 4 tia sét, mỗi tia đánh vào 1 Enemy gần nhất riêng biệt
            List<Transform> targets = FindNearestEnemies(autoAttackMaxTargets);
            if (targets.Count > 0)
            {
                if (Skill != null)
                {
                    ZeusSkill controller = Skill.GetComponent<ZeusSkill>();
                    if (controller != null)
                    {
                        controller.TriggerSkillOnTargets(targets);
                    }
                }

                foreach (Transform t in targets)
                {
                    if (t == null) continue;
                    Enemy enemy = t.GetComponentInParent<Enemy>();
                    if (enemy != null && !enemy.IsDead)
                    {
                        enemy.TakeDamage(attackDamage);
                    }
                }
            }
        }

        count_timer = 0f;
        // Chờ một nhịp ngắn ở cuối đòn chém
        yield return new WaitForSeconds(recoverDelay);

        if (Hammer != null)
        {
            // 3. Thu búa về lại vị trí nghỉ ban đầu (Ảnh 1)
            Hammer.localPosition = idleLocalPos;
            Hammer.localRotation = Quaternion.Euler(idleEulerAngles);
        }
        isAttacking = false;
    }

    IEnumerator SkillCooldownRoutine()
    {
        yield return new WaitForSeconds(cooldownTime);
        isReadyToUse = true;
    }
}
