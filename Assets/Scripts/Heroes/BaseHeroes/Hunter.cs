
using UnityEngine;
using System.Collections;

public class Hunter : BaseHeroes
{
    [Header("Rapid Fire Skill")]
    [SerializeField] private float attackSpeedMultiplier = 2f;
    [SerializeField] private float attackDamageMultiplier = 5f;
    [SerializeField] private float skillDuration = 5f;

    private float originalAttackSpeed;
    private float originalDamage;
    private Coroutine skillCoroutine;

    public override void UseSkill()
    {
        // Không cho kích hoạt chồng skill
        if (skillCoroutine != null)
            return;

        // Lưu tốc độ bắn ban đầu
        originalAttackSpeed = attackSpeed;
        originalDamage = damage;

        skillCoroutine = StartCoroutine(RapidFire());
    }

    private IEnumerator RapidFire()
    {
        Debug.Log("Hunter activated Rapid Fire!");

        // Tăng tốc độ bắn
        attackSpeed = originalAttackSpeed *
                      attackSpeedMultiplier;
        damage *= attackDamageMultiplier;
        // Duy trì skill trong 6 giây
        yield return new WaitForSeconds(skillDuration);

        // Khôi phục tốc độ bắn
        attackSpeed = originalAttackSpeed;
        damage = originalDamage;

        skillCoroutine = null;

    }
}
