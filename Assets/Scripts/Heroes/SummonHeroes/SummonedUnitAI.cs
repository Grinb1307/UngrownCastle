using UnityEngine;
using UnityEngine.Rendering;
using UngrownCastle.Enemies;

[RequireComponent(typeof(Animator))]
[RequireComponent(typeof(SortingGroup))]
public abstract class SummonedUnitAI : MonoBehaviour
{
    [Header("Combat")]
    [SerializeField] protected float damage = 30f;
    [SerializeField] protected float attackRange = 1.5f;
    [SerializeField] protected float attackSpeed = 0.7f;
    [SerializeField] protected float attackHitBuffer = 0.35f;
    [SerializeField] protected float attackFallbackDuration = 1.0f;

    [Header("Movement")]
    [SerializeField] protected float moveSpeed = 2f;
    [SerializeField] protected float detectionRange = 10f;

    [Header("Target")]
    [SerializeField] protected LayerMask enemyLayer;
    [SerializeField] protected float retargetInterval = 0.2f;
    [SerializeField] protected float retargetSwitchMargin = 0.15f;

    [Header("Animation")]
    [SerializeField] protected Animator animator;
    [SerializeField] protected string walkParameter = "IsMoving";
    [SerializeField] protected string attackTrigger = "Attack";

    [Header("Depth Sorting")]
    [SerializeField] protected SortingGroup sortingGroup;
    [SerializeField] protected int sortingOrderBase = 1000;
    [SerializeField] protected int sortingPrecision = 100;
    [SerializeField] protected float sortingYOffset = -0.75f;

    protected Enemy targetEnemy;
    protected bool isAttacking;

    private bool isWalking;
    private bool hasDealtDamageThisAttack;
    private float nextAttackTime;
    private float attackFallbackEndTime;
    private float retargetTimer;
    private int walkParameterHash;
    private int attackTriggerHash;

    public float Damage => damage;
    public float AttackRange => attackRange;
    public float AttackSpeed => attackSpeed;
    public float MoveSpeed => moveSpeed;
    public float DetectionRange => detectionRange;
    public Enemy CurrentTarget => targetEnemy;
    public bool IsAttacking => isAttacking;

    public void SetDamage(float newDamage)
    {
        damage = Mathf.Max(0f, newDamage);
    }

    protected virtual void Reset()
    {
        AutoAssignComponents();
    }

    protected virtual void OnValidate()
    {
        AutoAssignComponents();

        damage = Mathf.Max(0f, damage);
        attackRange = Mathf.Max(0.1f, attackRange);
        attackSpeed = Mathf.Max(0.05f, attackSpeed);
        moveSpeed = Mathf.Max(0f, moveSpeed);
        detectionRange = Mathf.Max(attackRange, detectionRange);
        retargetInterval = Mathf.Max(0.05f, retargetInterval);
        retargetSwitchMargin = Mathf.Max(0f, retargetSwitchMargin);
    }

    protected virtual void Awake()
    {
        AutoAssignComponents();

        if (sortingGroup == null)
        {
            sortingGroup = gameObject.AddComponent<SortingGroup>();
        }

        walkParameterHash = Animator.StringToHash(walkParameter);
        attackTriggerHash = Animator.StringToHash(attackTrigger);

        UpdateSortingOrder();
    }

    protected void AutoAssignComponents()
    {
        if (animator == null)
        {
            animator = GetComponent<Animator>();
        }

        if (sortingGroup == null)
        {
            sortingGroup = GetComponent<SortingGroup>();
        }
    }

    protected virtual void LateUpdate()
    {
        UpdateSortingOrder();
    }

    protected virtual void UpdateSortingOrder()
    {
        if (sortingGroup == null)
            return;

        float footY = transform.position.y + sortingYOffset;
        sortingGroup.sortingOrder = sortingOrderBase - Mathf.RoundToInt(footY * sortingPrecision);
    }

    protected virtual void OnDisable()
    {
        isAttacking = false;
        isWalking = false;
        hasDealtDamageThisAttack = false;
        retargetTimer = 0f;
    }

    protected virtual void Update()
    {
        if (isAttacking && Time.time >= attackFallbackEndTime)
        {
            if (!hasDealtDamageThisAttack)
            {
                ApplyAttackDamage();
            }

            EndAttack();
        }

        UpdateNearestTarget();

        if (targetEnemy == null)
        {
            SetWalking(false);
            return;
        }

        if (isAttacking)
        {
            SetWalking(false);
            return;
        }

        UpdateFacing();

        float distance = Vector2.Distance(
            transform.position,
            targetEnemy.transform.position
        );

        if (distance > attackRange)
        {
            MoveToTarget();
        }
        else
        {
            SetWalking(false);

            if (Time.time >= nextAttackTime)
            {
                StartAttack();
            }
        }
    }

    protected bool IsTargetValid(Enemy enemy)
    {
        return enemy != null && !enemy.IsDead && enemy.gameObject.activeInHierarchy;
    }

    // Luôn ưu tiên chọn Enemy còn sống gần nhất trong phạm vi phát hiện
    protected virtual void UpdateNearestTarget()
    {
        if (!IsTargetValid(targetEnemy))
        {
            targetEnemy = FindNearestEnemy();
            retargetTimer = 0f;
            return;
        }

        if (isAttacking)
            return;

        retargetTimer += Time.deltaTime;
        if (retargetTimer < retargetInterval)
            return;

        retargetTimer = 0f;

        Enemy nearest = FindNearestEnemy();

        if (nearest == null)
        {
            float currentDist = Vector2.Distance(
                transform.position,
                targetEnemy.transform.position
            );

            if (currentDist > detectionRange)
            {
                targetEnemy = null;
            }

            return;
        }

        if (nearest == targetEnemy)
            return;

        float currentDistance = Vector2.Distance(
            transform.position,
            targetEnemy.transform.position
        );
        float nearestDistance = Vector2.Distance(
            transform.position,
            nearest.transform.position
        );

        if (nearestDistance + retargetSwitchMargin < currentDistance)
        {
            targetEnemy = nearest;
        }
    }

    protected virtual Enemy FindNearestEnemy()
    {
        Collider2D[] enemies = Physics2D.OverlapCircleAll(
            transform.position,
            detectionRange,
            enemyLayer
        );

        Enemy nearest = null;
        float minSqrDistance = Mathf.Infinity;
        Vector2 selfPos = transform.position;

        foreach (Collider2D col in enemies)
        {
            if (col == null)
                continue;

            Enemy enemyComponent = col.GetComponentInParent<Enemy>();

            if (!IsTargetValid(enemyComponent))
                continue;

            Vector2 enemyPos = enemyComponent.transform.position;
            float sqrDistance = (enemyPos - selfPos).sqrMagnitude;

            if (sqrDistance < minSqrDistance)
            {
                minSqrDistance = sqrDistance;
                nearest = enemyComponent;
            }
        }

        return nearest;
    }

    protected virtual void MoveToTarget()
    {
        if (!IsTargetValid(targetEnemy))
        {
            SetWalking(false);
            return;
        }

        SetWalking(true);

        transform.position = Vector2.MoveTowards(
            transform.position,
            targetEnemy.transform.position,
            moveSpeed * Time.deltaTime
        );
    }

    protected virtual void UpdateFacing()
    {
        if (targetEnemy == null)
            return;

        float direction = targetEnemy.transform.position.x - transform.position.x;

        if (Mathf.Abs(direction) <= 0.05f)
            return;

        Vector3 scale = transform.localScale;
        scale.x = Mathf.Abs(scale.x) * (direction > 0f ? 1f : -1f);
        transform.localScale = scale;
    }

    protected virtual void StartAttack()
    {
        isAttacking = true;
        hasDealtDamageThisAttack = false;

        float attackInterval = attackSpeed > 0f ? 1f / attackSpeed : 1.5f;
        nextAttackTime = Time.time + attackInterval;

        SetWalking(false);
        OnAttackStarted();

        if (HasValidAnimator())
        {
            animator.ResetTrigger(attackTriggerHash);
            animator.SetTrigger(attackTriggerHash);
            attackFallbackEndTime = Time.time + Mathf.Max(attackFallbackDuration, 0.5f);
        }
        else
        {
            ApplyAttackDamage();
            EndAttack();
        }
    }

    // Gọi bằng Animation Event tại frame gây sát thương
    public void ApplyAttackDamage()
    {
        if (hasDealtDamageThisAttack)
            return;

        hasDealtDamageThisAttack = true;

        if (!IsTargetValid(targetEnemy))
            return;

        float distance = Vector2.Distance(
            transform.position,
            targetEnemy.transform.position
        );

        if (distance > attackRange + attackHitBuffer)
            return;

        DealAttackDamage(targetEnemy);
    }

    // Gọi bằng Animation Event ở frame cuối animation Attack
    public void EndAttack()
    {
        isAttacking = false;
        hasDealtDamageThisAttack = false;

        if (HasValidAnimator())
        {
            animator.ResetTrigger(attackTriggerHash);
        }

        OnAttackEnded();
    }

    protected virtual void DealAttackDamage(Enemy target)
    {
        target.TakeDamage(damage);
    }

    protected virtual void OnAttackStarted()
    {
    }

    protected virtual void OnAttackEnded()
    {
    }

    protected bool HasValidAnimator()
    {
        return animator != null
            && animator.isActiveAndEnabled
            && animator.runtimeAnimatorController != null;
    }

    protected void SetWalking(bool value)
    {
        if (isWalking == value)
            return;

        isWalking = value;

        if (HasValidAnimator())
        {
            animator.SetBool(walkParameterHash, value);
        }
    }

    protected virtual void OnDrawGizmosSelected()
    {
        Gizmos.color = Color.yellow;
        Gizmos.DrawWireSphere(transform.position, detectionRange);

        Gizmos.color = Color.red;
        Gizmos.DrawWireSphere(transform.position, attackRange);
    }
}
