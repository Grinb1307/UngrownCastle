
using UnityEngine;
using System.Collections;

public class Ignis : FireHeroes
{
    [SerializeField] protected Transform bowTransform;
    protected override void UpdateAim()
    {
        if (target == null || bowTransform == null)
            return;

        Vector2 direction =
            (Vector2)target.position -
            (Vector2)bowTransform.position;

        float angle = Mathf.Atan2(
            direction.y,
            direction.x
        ) * Mathf.Rad2Deg;

        // Xoay cây cung về phía Enemy
        bowTransform.rotation = Quaternion.Euler(
            0f, 0f, angle
        );
    }
    protected override void PerformAttack()
    {
        ThrowBombArrow();
    }
    public override void UseSkill()
    {

    }
    private void ThrowBombArrow()
    {
        if (target == null || ProjectilePrefab == null || firePoint == null)
            return;

        Collider2D enemyCollider = target.GetComponent<Collider2D>();

        Vector2 targetPosition = enemyCollider != null
            ? enemyCollider.bounds.center
            : (Vector2)target.position;

        GameObject projectile = Instantiate(
            ProjectilePrefab,
            firePoint.position,
            Quaternion.identity
        );

        BombArrowProjectile arrow =
            projectile.GetComponent<BombArrowProjectile>();

        if (arrow != null)
        {
            arrow.Launch(targetPosition, 0.7f);
        }
        else
        {
            Debug.LogError("BombArrow thiếu BombArrowProjectile!");
            Destroy(projectile);
        }
    }
}
