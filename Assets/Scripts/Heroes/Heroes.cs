
using UnityEngine;

public abstract class Heroes : MonoBehaviour
{
    [Header("Combat Stats")]
    [SerializeField] protected float damage = 10f;
    [SerializeField] protected float attackSpeed = 1f;
    [SerializeField] protected float critChance = 0.2f;
    [SerializeField] protected float critMultiplier = 2f;

    [Header("Mana")]
    [SerializeField] protected float maxMana = 100f;
    [SerializeField] protected float currentMana = 0f;
    [SerializeField] protected float manaRegen = 5f;

    [Header("Target")]
    [SerializeField] protected float attackRange = 8f;
    [SerializeField] protected LayerMask enemyLayer;

    protected Transform target;

    private float attackTimer;
    private float targetTimer;

    // Kiểm tra Hero đã đầy mana chưa
    public bool IsManaFull()
    {
        return currentMana >= maxMana;
    }


    protected virtual void Update()
    {
        RegenerateMana();

        // Tìm Enemy
        FindTarget();

        // Không có Enemy thì không làm gì
        if (target == null)
        {
            attackTimer = 0f;
            return;
        }

        // Có Enemy thì xoay hướng tấn công
        UpdateAim();

        if (attackSpeed <= 0)
            return;

        attackTimer += Time.deltaTime;

        if (attackTimer >= 1f / attackSpeed)
        {
            PerformAttack();
            attackTimer = 0f;
        }
    }


    // Click chuột vào Hero để sử dụng skill
    private void OnMouseDown()
    {
        if (!IsManaFull())
            return;

        if (!CanUseSkill())
            return;

        // Tiêu hao toàn bộ mana
        currentMana = 0f;

        UseSkill();
    }

    // Điều kiện sử dụng skill riêng của từng Hero
    protected virtual bool CanUseSkill()
    {
        return true;
    }

    // Hồi mana
    protected virtual void RegenerateMana()
    {
        currentMana += manaRegen * Time.deltaTime;

        currentMana = Mathf.Clamp(
            currentMana, 0f, maxMana
        );
    }

    protected virtual void FindTarget()
    {
        targetTimer += Time.deltaTime;

        if (targetTimer < 0.25f)
            return;

        targetTimer = 0f;

        Collider2D[] enemies =
            Physics2D.OverlapCircleAll(
                transform.position,
                attackRange,
                enemyLayer
            );

        Transform nearest = null;
        float minDistance = Mathf.Infinity;

        foreach (Collider2D enemy in enemies)
        {
            float distance = Vector2.Distance(
                transform.position,
                enemy.transform.position
            );

            if (distance < minDistance)
            {
                minDistance = distance;
                nearest = enemy.transform;
            }
        }

        target = nearest;

    }

    protected float CalculateDamage()
    {
        bool critical = Random.value < critChance;

        return critical
            ? damage * critMultiplier
            : damage;
    }
    protected virtual void UpdateAim()
    {
        // Để lớp con tự xử lý hướng ngắm
    }
    protected abstract void PerformAttack();

    public abstract void UseSkill();
}
