
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

        Debug.Log("Druid summoned Ent!");
    }

    public override void UseSkill()
    {
        // Skill: Cường hóa đòn đánh tiếp theo của Ent
        if (ent != null)
        {
            ent.EmpowerNextAttack();
        }
    }
}
