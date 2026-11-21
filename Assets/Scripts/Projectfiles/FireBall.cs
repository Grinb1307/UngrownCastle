using UnityEngine;

public class Fireball : MonoBehaviour
{
    private Transform target;
    private float damage;
    private float speed;

    public void Initialize(
        Transform target,
        float damage,
        float speed)
    {
        this.target = target;
        this.damage = damage;
        this.speed = speed;
    }

    private void Update()
    {
        if (target == null)
        {
            Destroy(gameObject);
            return;
        }

        Vector2 direction =
            (target.position - transform.position).normalized;

        transform.position +=
            (Vector3)(direction * speed * Time.deltaTime);

        float angle = Mathf.Atan2(
            direction.y,
            direction.x
        ) * Mathf.Rad2Deg;

        transform.rotation = Quaternion.Euler(
            0, 0, angle
        );
    }

    private void OnTriggerEnter2D(Collider2D other)
    {
        if (target == null)
            return;

        // Kiểm tra có chạm đúng Enemy/Dummy đang nhắm không
        if (other.transform == target ||
            other.transform.IsChildOf(target))
        {
            // Gây sát thương nếu có DummyTarget
            DummyTarget dummy =
                other.GetComponentInParent<DummyTarget>();

            if (dummy != null)
            {
                dummy.TakeDamage(damage);
            }

            // Hủy Fireball
            Destroy(this.gameObject);
        }
    }
}