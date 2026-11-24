using UnityEngine;

public class LightningBallScript : MonoBehaviour
{
    public float speed = 5f;
    public float angle = 45f;
    public float lifetime = 5f;

    private Vector2 direction;

    public void Initialize(Vector2 startPosition, float angle, float lifetime)
    {
        transform.position = startPosition;
        this.angle = angle;
        this.lifetime = lifetime;
        direction = new Vector2(Mathf.Cos(angle * Mathf.Deg2Rad), Mathf.Sin(angle * Mathf.Deg2Rad));
        Destroy(gameObject, lifetime);
    }

    private void Update()
    {
        transform.Translate(direction * speed * Time.deltaTime);
    }
}
