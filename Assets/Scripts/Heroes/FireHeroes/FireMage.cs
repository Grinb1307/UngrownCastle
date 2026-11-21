using UnityEngine;
using System.Collections;

public class FireMage : FireHeroes
{
    [Header("Meteor Skill")]
    [SerializeField] private GameObject meteorPrefab;

    [SerializeField] private int meteorCount = 8;

    [SerializeField] private float meteorSpacing = 1.5f;

    [SerializeField] private float meteorSpawnHeight = 8f;

    [SerializeField] private float meteorDelay = 0.3f;

    [SerializeField] private float meteorDamageMultiplier = 3f;

    protected override void PerformAttack()
    {
        // Đánh thường: Bắn cầu lửa
        ShootFireball();
    }

    protected override bool CanUseSkill()
    {
        return target != null && meteorPrefab != null;
    }

    public override void UseSkill()
    {
        if (target == null || meteorPrefab == null)
            return;

        StartCoroutine(MeteorShower());
    }

    private IEnumerator MeteorShower()
    {
        // Lưu vị trí mục tiêu tại thời điểm sử dụng skill
        Vector2 center = target.position;

        // Rơi lần lượt từ trái sang phải
        for (int i = 0; i < meteorCount; i++)
        {
            // Tính vị trí theo hàng ngang
            float offset = (i - (meteorCount - 1) / 2f)
                           * meteorSpacing;

            Vector2 impactPosition = new Vector2(
                center.x + offset,
                center.y
            );

            // Vị trí xuất hiện trên trời
            Vector3 spawnPosition = new Vector3(
                impactPosition.x,
                impactPosition.y + meteorSpawnHeight,
                0f
            );

            // Tạo thiên thạch
            GameObject meteor = Instantiate(
                meteorPrefab,
                spawnPosition,
                Quaternion.identity
            );

            Meteor meteorScript =
                meteor.GetComponent<Meteor>();

            if (meteorScript != null)
            {
                meteorScript.Initialize(
                    impactPosition,
                    CalculateDamage() * meteorDamageMultiplier
                );
            }

            // Chờ trước khi gọi thiên thạch tiếp theo
            yield return new WaitForSeconds(meteorDelay);
        }
    }
}