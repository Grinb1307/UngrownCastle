using System.Collections;
using UnityEngine;
using UngrownCastle.Enemies;
public interface ISlowable
{
    void ApplySlow(float multiplier, float duration);
}

public class IceSpikeZone : MonoBehaviour
{
    private float explosionRadius;
    private float damage;
    private float slowMultiplier;
    private float slowDuration;
    private float lifeTime;
    private LayerMask enemyLayer;

    private Vector3 originalScale;

    public void Initialize(
        float radius,
        float skillDamage,
        float slow,
        float duration,
        float lifetime,
        LayerMask layer)
    {
        explosionRadius = radius;
        damage = skillDamage;
        slowMultiplier = slow;
        slowDuration = duration;
        lifeTime = lifetime;
        enemyLayer = layer;

        originalScale = transform.localScale;

        transform.localScale = Vector3.zero;

        StartCoroutine(GrowSpikes());
        ApplyEffect();

        Destroy(gameObject, lifeTime);
    }

    // Gai băng mọc lên
    private IEnumerator GrowSpikes()
    {
        float timer = 0f;
        float growDuration = 0.25f;

        while (timer < growDuration)
        {
            timer += Time.deltaTime;

            float progress = Mathf.Clamp01(timer / growDuration);

            transform.localScale =
                originalScale * progress;

            yield return null;
        }

        transform.localScale = originalScale;
    }

    // Áp dụng hiệu ứng lên Enemy trong vùng
    private void ApplyEffect()
    {
        Collider2D[] enemies = Physics2D.OverlapCircleAll(
            transform.position,
            explosionRadius,
            enemyLayer
        );

        foreach (Collider2D enemyCollider in enemies)
        {
            UngrownCastle.Enemies.Enemy enemyComponent =
                enemyCollider.GetComponentInParent<UngrownCastle.Enemies.Enemy>();

            if (enemyComponent == null)
                continue;

            // Gây sát thương
            enemyComponent.TakeDamage(damage);

            // Làm chậm và đổi màu xanh băng
            enemyComponent.ApplySlow(
                slowMultiplier,
                slowDuration
            );
        }
    }
}