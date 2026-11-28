
using UnityEngine;

public class Elizabeth : IceHeroes
{
    [Header("Elizabeth Skill")]
    [SerializeField] private float critDamageBonus = 2f;
    [Header("Elizabeth Weapon")]
    [SerializeField] private Transform flintlock;

    [SerializeField] private float rotationOffset = 0f;

    private bool skillActivated;
    protected override void UpdateAim()
    {
        // Giữ nguyên chức năng ngắm của IceHeroes
        base.UpdateAim();

        if (target == null || flintlock == null)
            return;

        // Hướng từ súng đến Enemy
        Vector2 direction =
            (Vector2)target.position - (Vector2)flintlock.position;

        float angle = Mathf.Atan2(
            direction.y,
            direction.x
        ) * Mathf.Rad2Deg;

        // Xoay khẩu Flintlock
        flintlock.rotation = Quaternion.Euler(
            0f,
            0f,
            angle + rotationOffset
        );
    }

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
