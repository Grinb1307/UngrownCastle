using UnityEngine;
using System.Collections;

public class TNTExplosion : MonoBehaviour
{
    [Header("Explosion Settings")]
    [SerializeField] private float explosionDuration = 0.6f;

    private float damage;
    private float radius;

    public void Initialize(float damage, float radius)
    {
        this.damage = damage;
        this.radius = radius;

        // Gây sát thương ngay khi phát nổ
        DealDamage();

        // Tự hủy sau khi animation kết thúc
        StartCoroutine(DestroyAfterAnimation());
    }

    private void DealDamage()
    {
        Collider2D[] enemies = Physics2D.OverlapCircleAll(
            transform.position,
            radius
        );

        foreach (Collider2D col in enemies)
        {
            DummyTarget dummy =
                col.GetComponentInParent<DummyTarget>();

            if (dummy != null)
            {
                dummy.TakeDamage(damage);
            }
        }
    }

    private IEnumerator DestroyAfterAnimation()
    {
        yield return new WaitForSeconds(explosionDuration);

        Destroy(gameObject);
    }
}