
using UnityEngine;
using UngrownCastle.Enemies;

public class EntAI : MonoBehaviour
{
    [Header("Combat")]
    [SerializeField] private float damage = 30f;
    [SerializeField] private float attackRange = 1.5f;
    [SerializeField] private float attackCooldown = 1.5f;

    [Header("Movement")]
    [SerializeField] private float moveSpeed = 2f;
    [SerializeField] private float detectionRange = 10f;

    [Header("Target")]
    [SerializeField] private LayerMask enemyLayer;

    [Header("Animation")]
    [SerializeField] private Animator animator;

    [SerializeField] private string walkParameter = "IsMoving";
    [SerializeField] private string attackTrigger = "Attack";

    private Enemy targetEnemy;

    private bool isAttacking;
    private bool empowered;

    private float attackTimer;

    private void Awake()
    {
        if (animator == null)
            animator = GetComponent<Animator>();
    }

    private void Update()
    {
        FindTarget();

        if (targetEnemy == null)
        {
            SetWalking(false);
            return;
        }

        float distance = Vector2.Distance(
            transform.position,
            targetEnemy.transform.position
        );

        UpdateFacing();

        // Enemy còn ở xa
        if (distance > attackRange)
        {
            if (isAttacking)
                return;

            MoveToTarget();
        }
        else
        {
            SetWalking(false);

            // Đã đến tầm đánh
            if (!isAttacking && Time.time >= attackTimer)
            {
                StartAttack();
            }
        }
    }

    // Tìm Enemy gần nhất
    private void FindTarget()
    {
        if (targetEnemy != null)
            return;

        Collider2D[] enemies =
            Physics2D.OverlapCircleAll(
                transform.position,
                detectionRange,
                enemyLayer
            );

        float minDistance = Mathf.Infinity;

        foreach (Collider2D enemy in enemies)
        {
            Enemy enemyComponent =
                enemy.GetComponentInParent<Enemy>();

            if (enemyComponent == null)
                continue;

            float distance = Vector2.Distance(
                transform.position,
                enemyComponent.transform.position
            );

            if (distance < minDistance)
            {
                minDistance = distance;
                targetEnemy = enemyComponent;
            }
        }
    }

    // Di chuyển đến Enemy
    private void MoveToTarget()
    {
        SetWalking(true);

        transform.position = Vector2.MoveTowards(
            transform.position,
            targetEnemy.transform.position,
            moveSpeed * Time.deltaTime
        );
    }

    // Xoay mặt về phía Enemy
    private void UpdateFacing()
    {
        float direction =
            targetEnemy.transform.position.x - transform.position.x;

        Vector3 scale = transform.localScale;

        scale.x = Mathf.Abs(scale.x) * (direction >= 0 ? 1 : -1);

        transform.localScale = scale;
    }

    // Bắt đầu tấn công
    private void StartAttack()
    {
        isAttacking = true;

        attackTimer = Time.time + attackCooldown;

        SetWalking(false);

        animator.SetTrigger(attackTrigger);
    }

    // Gọi bằng Animation Event tại frame gây sát thương
    public void ApplyAttackDamage()
    {
        if (targetEnemy == null)
            return;

        float distance = Vector2.Distance(
            transform.position,
            targetEnemy.transform.position
        );

        if (distance > attackRange + 0.3f)
            return;

        float finalDamage = empowered
            ? damage * 2f
            : damage;

        targetEnemy.TakeDamage(finalDamage);

        empowered = false;
    }

    // Gọi bằng Animation Event ở frame cuối animation Attack
    public void EndAttack()
    {
        isAttacking = false;
    }

    // Skill của Druid
    public void EmpowerNextAttack()
    {
        empowered = true;
    }

    private void SetWalking(bool value)
    {
        if (animator != null)
            animator.SetBool(walkParameter, value);
    }

    private void OnDrawGizmosSelected()
    {
        Gizmos.color = Color.yellow;
        Gizmos.DrawWireSphere(transform.position, detectionRange);

        Gizmos.color = Color.red;
        Gizmos.DrawWireSphere(transform.position, attackRange);
    }
}
