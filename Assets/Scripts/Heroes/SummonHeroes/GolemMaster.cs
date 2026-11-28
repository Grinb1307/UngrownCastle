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
        return false;
    }

    public override void UseSkill()
    {
    }
}
