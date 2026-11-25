
using UnityEngine;

public class Elizabeth : IceHeroes
{
    [Header("Elizabeth Skill")]
    [SerializeField] private float critDamageBonus = 2f;

    private bool skillActivated;

    // Không cần Enemy để kích hoạt skill
    protected override bool CanUseSkill()
    {
        return !skillActivated;
    }

    // Skill: Tăng Crit Damage vĩnh viễn
    public override void UseSkill()
    {
        if (skillActivated)
            return;

        // Tăng Crit Damage
        critMultiplier += critDamageBonus;

        skillActivated = true;

        Debug.Log(
            $"Elizabeth kích hoạt skill! Crit Damage hiện tại: x{critMultiplier}"
        );
    }
}
