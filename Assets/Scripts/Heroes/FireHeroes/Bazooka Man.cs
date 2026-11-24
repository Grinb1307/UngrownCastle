
using System.Collections;
using UnityEngine;

public class BazookaMan : FireHeroes
{
    [Header("Bazooka Projectile")]
    [SerializeField] private GameObject bazookaPrefab;

    [Header("Normal Attack")]
    [SerializeField] private float projectileFlightTime = 0.8f;

    [Header("Bazooka Skill")]
    [SerializeField] private int skillShotCount = 10;
    [SerializeField] private float skillFireInterval = 0.1f;
    [SerializeField] private float skillDuration = 5f;

    [Header("Bazooka Rotation")]
    [SerializeField] private Transform bazookaPivot;
    [SerializeField] private float rotationOffset = -90f;

    private bool isUsingSkill;
    private Coroutine skillCoroutine;

    protected override void UpdateAim()
    {
        if (target == null || bazookaPivot == null)
            return;

        // Xoay khẩu Bazooka theo hướng Enemy
        Vector2 direction =
            (target.position - bazookaPivot.position).normalized;

        aimDirection = direction;

        float angle = Mathf.Atan2(
            direction.y,
            direction.x
        ) * Mathf.Rad2Deg;

        if (bazookaPivot != null)
        {
            bazookaPivot.rotation = Quaternion.Euler(
                0f,
                0f,
                angle + rotationOffset
            );
        }
       
    }

    // Đánh thường: bắn một viên đạn theo parabol
    protected override void PerformAttack()
    {
        ShootBazooka();
    }

    // Điều kiện sử dụng skill
    protected override bool CanUseSkill()
    {
        return target != null &&
               bazookaPrefab != null &&
               firePoint != null &&
               !isUsingSkill;
    }

    // Skill: bắn liên tục 10 viên
    public override void UseSkill()
    {
        if (!CanUseSkill())
            return;

        skillCoroutine = StartCoroutine(SkillBurst());
    }

    private IEnumerator SkillBurst()
    {
        isUsingSkill = true;

        for (int i = 0; i < skillShotCount; i++)
        {
            if (target == null)
                break;

            ShootBazooka();

            if (i < skillShotCount - 1)
                yield return new WaitForSeconds(skillFireInterval);
        }

        isUsingSkill = false;
        skillCoroutine = null;
    }

    private void ShootBazooka()
    {
        if (target == null || bazookaPrefab == null)
            return;

        Vector3 spawnPosition = firePoint.position;

        GameObject projectile = Instantiate(
            bazookaPrefab,
            spawnPosition,
            Quaternion.identity
        );

        BazookaProjectile projectileScript =
            projectile.GetComponent<BazookaProjectile>();

        if (projectileScript == null)
            projectileScript =
                projectile.GetComponentInChildren<BazookaProjectile>();

        if (projectileScript == null)
        {
            Debug.LogError(
                "BZK Projectile thiếu BazookaProjectile! Hãy gắn script vào Prefab."
            );

            Destroy(projectile);
            return;
        }

        projectileScript.Initialize(
            target,
            CalculateDamage()
        );

        Rigidbody2D rb =
            projectile.GetComponent<Rigidbody2D>();

        if (rb == null)
        {
            Debug.LogError("BZK Projectile thiếu Rigidbody2D!");
            Destroy(projectile);
            return;
        }

        // Tính vận tốc để viên đạn bay theo quỹ đạo parabol
        Vector2 displacement =
            (Vector2)target.position - (Vector2)spawnPosition;

        float gravity =
            Physics2D.gravity.y * rb.gravityScale;

        float flightTime =
            Mathf.Max(0.1f, projectileFlightTime);

        float vx = displacement.x / flightTime;

        float vy = (
            displacement.y -
            0.5f * gravity * flightTime * flightTime
        ) / flightTime;

        Vector2 velocity = new Vector2(vx, vy);

        rb.linearVelocity = velocity;

        projectileScript.SetVelocity(velocity);
    }

    private void OnDisable()
    {
        if (skillCoroutine != null)
        {
            StopCoroutine(skillCoroutine);
            skillCoroutine = null;
        }

        isUsingSkill = false;
    }
}
