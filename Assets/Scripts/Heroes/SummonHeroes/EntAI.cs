using UnityEngine;
using UngrownCastle.Enemies;

public class EntAI : SummonedUnitAI
{
    [Header("Ent Skill")]
    [SerializeField] private float empoweredDamageMultiplier = 2f;

    private bool empowered;

    public bool IsEmpowered => empowered;

    protected override void Reset()
    {
        base.Reset();

        damage = 30f;
        attackRange = 1.5f;
        attackSpeed = 0.7f;
        moveSpeed = 2f;
        detectionRange = 10f;
        attackFallbackDuration = 0.85f;
    }

    protected override void DealAttackDamage(Enemy target)
    {
        float finalDamage = empowered
            ? damage * empoweredDamageMultiplier
            : damage;

        target.TakeDamage(finalDamage);
        empowered = false;
    }

    // Skill của Druid
    public void EmpowerNextAttack()
    {
        empowered = true;
    }
}
