
using UnityEngine;

public class Archer : BaseHeroes
{
    [Header("Archer Skill")]
    [SerializeField] private int arrowCount = 5;
    [SerializeField] private float spreadAngle = 40f;

    // Chỉ sử dụng skill khi có mục tiêu
    protected override bool CanUseSkill()
    {
        return target != null;
    }

    public override void UseSkill()
    {
        float angleStep =
            spreadAngle / (arrowCount - 1);

        float startAngle = -spreadAngle / 2f;

        for (int i = 0; i < arrowCount; i++)
        {
            float angle =
                startAngle + angleStep * i;

            ShootArrow(angle);
        }
    }
}
