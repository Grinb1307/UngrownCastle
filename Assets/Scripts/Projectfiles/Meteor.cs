using UnityEngine;

public class Meteor : MonoBehaviour
{
    [SerializeField] private float fallSpeed = 15f;
    [SerializeField] private float explosionRadius = 1.5f;

    private Vector2 targetPosition;
    private float damage;

    private bool initialized;

    public void Initialize(
        Vector2 targetPosition,
        float damage)
    {
        this.targetPosition = targetPosition;
        this.damage = damage;

        initialized = true;
    }

    private void Update()
    {
        if (!initialized)
            return;

        transform.position = Vector2.MoveTowards(
            transform.position,
            targetPosition,
            fallSpeed * Time.deltaTime
        );

        if (Vector2.Distance(
            transform.position,
            targetPosition) < 0.05f)
        {
            Explode();
        }
    }

    private void Explode()
    {
        Collider2D[] enemies =
            Physics2D.OverlapCircleAll(
                targetPosition,
                explosionRadius
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

        Destroy(gameObject);
    }
}