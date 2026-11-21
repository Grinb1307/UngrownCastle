
using UnityEngine;

public class DummyTarget : MonoBehaviour
{
    [SerializeField] private float health = 1000f;

    public void TakeDamage(float damage)
    {
        health -= damage;

        Debug.Log("Dummy nhận damage: " + damage);
        Debug.Log("HP còn lại: " + health);

        if (health <= 0)
        {
            Debug.Log("Dummy đã bị tiêu diệt!");
            Destroy(gameObject);
        }
    }
}
