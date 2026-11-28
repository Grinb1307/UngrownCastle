using UnityEngine;

public class GolemAI : SummonedUnitAI
{
    protected override void Reset()
    {
        base.Reset();

        damage = 50f;
        attackRange = 1.7f;
        attackSpeed = 0.55f;
        moveSpeed = 1.3f;
        detectionRange = 10f;
        attackFallbackDuration = 1.05f;
    }
}
