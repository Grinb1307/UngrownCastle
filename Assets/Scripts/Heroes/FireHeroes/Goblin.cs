using UnityEngine;

public class Goblin : FireHeroes
{
    [Header("TNT Attack")]
    [SerializeField] private GameObject tntPrefab;

    protected override void PerformAttack()
    {
        ThrowTNT();
    }

    private void ThrowTNT()
    {
        if (target == null || tntPrefab == null || firePoint == null)
            return;

        GameObject tnt = Instantiate(
            tntPrefab,
            firePoint.position,
            Quaternion.identity
        );

        TNTProjectile projectile =
            tnt.GetComponent<TNTProjectile>();

        if (projectile != null)
        {
            projectile.Launch(target.position);
        }
    }

    public override void UseSkill()
    {
        // Skill riêng của Goblin sẽ triển khai sau
    }
}