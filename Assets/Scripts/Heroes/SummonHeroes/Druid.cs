using UnityEngine;

public class Druid : SummonHeroes
{
    private EntAI ent;

    protected override void Start()
    {
        base.Start();

        if (summonedUnit != null)
        {
            ent = summonedUnit.GetComponent<EntAI>();
        }
    }

    protected override void Summon()
    {
        base.Summon();

        if (summonedUnit != null)
        {
            ent = summonedUnit.GetComponent<EntAI>();
        }

        Debug.Log("Druid summoned Ent!");
    }

    protected override bool CanUseSkill()
    {
        return ent != null && !ent.IsEmpowered;
    }

    public override void UseSkill()
    {
        
    }
}
