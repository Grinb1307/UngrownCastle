using UnityEngine;

public class LightningBallScript : MonoBehaviour
{
    public float speed = 5f;
    public float angle = 135f;
    public float lifetime = 5f;
    public float start = 0f;

    public Transform SpawnPoint;
    public Vector2 diagonalDirection = new Vector2(1f, -1f);
    private Vector2 direction;

    void Start()
    {
        transform.position = SpawnPoint.position;
    }
    private void Update()
    {
        Attack();
    }

    void Attack()
    {
        start += Time.deltaTime;
        if(start > lifetime)
        {
            gameObject.SetActive(false);
            start = 0f;
        }
        Vector3 moveDir = new Vector2(diagonalDirection.x, diagonalDirection.y).normalized;

        // Di chuyển object
        gameObject.transform.position += moveDir * speed * Time.deltaTime;
    }


}
