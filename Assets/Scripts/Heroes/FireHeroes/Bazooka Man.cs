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
    [SerializeField] private float skillDamageMultiplier = 2f;
    [SerializeField] private float skillExplosionMultiplier = 2f;

    [Header("Bazooka Rotation")]
    [SerializeField] private Transform bazookaPivot;
    [SerializeField] private float rotationOffset = -90f;

    private bool isUsingSkill;
    private Coroutine skillCoroutine;

    private float originalExplosionRadius;

    // Xoay Bazooka theo hướng Enemy
    protected override void UpdateAim()
    {
        if (target == null || bazookaPivot == null)
            return;

        Vector2 direction =
            (target.position - bazookaPivot.position).normalized;

        aimDirection = direction;

        float angle = Mathf.Atan2(
            direction.y,
            direction.x
        ) * Mathf.Rad2Deg;

        bazookaPivot.rotation = Quaternion.Euler(
            0f,
            0f,
            angle + rotationOffset
        );
    }

    // Đánh thường
    protected override void PerformAttack()
    {
        ShootBazooka(false);
    }

    // Kiểm tra điều kiện sử dụng skill
    protected override bool CanUseSkill()
    {
        return target != null &&
               bazookaPrefab != null &&
               firePoint != null &&
               !isUsingSkill;
    }

    // Kích hoạt skill
    public override void UseSkill()
    {
        if (!CanUseSkill())
            return;

        skillCoroutine = StartCoroutine(SkillBurst());
    }

    // Bắn liên tục 10 viên với sức công phá tăng cường
    private IEnumerator SkillBurst()
    {
        isUsingSkill = true;

        for (int i = 0; i < skillShotCount; i++)
        {
            if (target == null)
                break;

            ShootBazooka(true);

            if (i < skillShotCount - 1)
                yield return new WaitForSeconds(skillFireInterval);
        }

        isUsingSkill = false;
        skillCoroutine = null;
    }

    // Tạo và bắn Bazooka
    private void ShootBazooka(bool enhanced)
    {
        if (target == null || bazookaPrefab == null || firePoint == null)
            return;

        Vector3 spawnPosition = firePoint.position;

        GameObject projectile = Instantiate(
            bazookaPrefab,
            spawnPosition,
            Quaternion.identity
        );

        BazookaProjectile projectileScript =
            projectile.GetComponent<BazookaProjectile>();

        Rigidbody2D rb =
            projectile.GetComponent<Rigidbody2D>();

        if (projectileScript == null || rb == null)
        {
            Debug.LogError(
                "Bazooka Prefab cần BazookaProjectile và Rigidbody2D trên GameObject gốc!"
            );

            Destroy(projectile);
            return;
        }

        // Tính sát thương
        float projectileDamage = CalculateDamage();

        if (enhanced)
        {
            projectileDamage *= skillDamageMultiplier;
        }

        // Tăng vùng nổ cho viên đạn khi dùng skill
        projectileScript.Initialize(
            target,
            projectileDamage,
            enhanced ? skillExplosionMultiplier : 1f
        );

        // Tính vận tốc bay parabol
        Vector2 displacement =
            (Vector2)target.position - (Vector2)spawnPosition;

        float gravity =
            Physics2D.gravity.y * rb.gravityScale;

        float flightTime = Mathf.Max(0.1f, projectileFlightTime);

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