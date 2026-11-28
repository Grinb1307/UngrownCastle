using UnityEngine;

public class TinyGiant : SummonHeroes
{
    [Header("Tiny Giant Skill")]
    [SerializeField] private float giantDamageMultiplier = 6f;
    [SerializeField] private float giantDuration = 15f;

    private GiantAI giant;

    protected override void Start()
    {
        // Tiny Giant không đánh thường và chỉ triệu hồi Giant khi đủ mana & người chơi click
        attackSpeed = 0f;
    }

    protected override bool CanUseSkill()
    {
        return summonPrefab != null;
    }

    public override void UseSkill()
    {
        Summon();
    }

    protected override void Summon()
    {
        base.Summon();

        if (summonedUnit == null)
            return;

        giant = summonedUnit.GetComponent<GiantAI>();

        float giantDamage = damage * giantDamageMultiplier;

        if (giant != null)
        {
            giant.Initialize(giantDamage, giantDuration);
        }

        Debug.Log($"Tiny Giant summoned Giant! Damage: {giantDamage}, Duration: {giantDuration}s");
    }
}
