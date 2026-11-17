
using UnityEngine;
using System.Collections;

public class Archer : BaseHeroes
{
    [Header("Rapid Fire Skill")]
    [SerializeField] private float attackSpeedMultiplier = 2f;
    [SerializeField] private float skillDuration = 6f;

    private float originalAttackSpeed;
    private Coroutine skillCoroutine;

    public override void UseSkill()
    {
        // Không cho kích hoạt chồng skill
        if (skillCoroutine != null)
            return;

        // Lưu tốc độ bắn ban đầu
        originalAttackSpeed = attackSpeed;

        skillCoroutine = StartCoroutine(RapidFire());
    }

    private IEnumerator RapidFire()
    {
        Debug.Log("Archer activated Rapid Fire!");

        // Tăng tốc độ bắn
        attackSpeed = originalAttackSpeed *
                      attackSpeedMultiplier;

        // Duy trì skill trong 6 giây
        yield return new WaitForSeconds(skillDuration);

        // Khôi phục tốc độ bắn
        attackSpeed = originalAttackSpeed;

        skillCoroutine = null;

    }
}
