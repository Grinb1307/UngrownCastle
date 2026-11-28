using UnityEngine;

public class GolemMaster : SummonHeroes
{
    private GolemAI golem;

    protected override void Start()
    {
        base.Start();

        if (summonedUnit != null)
        {
            golem = summonedUnit.GetComponent<GolemAI>();
        }
    }

    protected override void Summon()
    {
        base.Summon();

        if (summonedUnit != null)
        {
            golem = summonedUnit.GetComponent<GolemAI>();
        }

        Debug.Log("Golem Master summoned Golem!");
    }

    protected override bool CanUseSkill()
    {
        return golem != null && !golem.IsHeavySlamReady;
    }

    public override void UseSkill()
    {
        if (golem == null)
            return;

        golem.ActivateHeavySlam();
        Debug.Log("Golem Master activated Golem Heavy Slam!");
    }
}
