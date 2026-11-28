using UnityEngine;
using UngrownCastle.Enemies;

public class LightningBallScript : MonoBehaviour
{
    public float speed = 5f;
    public float angle = 135f;
    public float lifetime = 5f;
    public float start = 0f;

    public Transform SpawnPoint;
    public Vector2 diagonalDirection = new Vector2(1f, -1f);
    private Vector2 direction;

    private float damage = 15f;
    private Transform targetEnemy;
    private bool isClone = false;
    private bool hasHit = false;

    void Start()
    {
        if (SpawnPoint != null && !isClone)
        {
            transform.position = SpawnPoint.position;
        }
    }

    public void Initialize(Transform target, Vector3 spawnPos, float attackDamage)
    {
        isClone = true;
        targetEnemy = target;
        damage = attackDamage;
        transform.position = spawnPos;
        start = 0f;
        hasHit = false;

        if (targetEnemy != null)
        {
            diagonalDirection = ((Vector2)targetEnemy.position - (Vector2)spawnPos).normalized;
        }
    }

    private void Update()
    {
        Attack();
    }

    void Attack()
    {
        start += Time.deltaTime;
        if (start > lifetime)
        {
            if (isClone)
            {
                Destroy(gameObject);
                return;
            }
            gameObject.SetActive(false);
            start = 0f;
        }

        if (isClone && targetEnemy != null)
        {
            diagonalDirection = ((Vector2)targetEnemy.position - (Vector2)transform.position).normalized;
        }

        Vector3 moveDir = new Vector2(diagonalDirection.x, diagonalDirection.y).normalized;

        // Di chuyển object
        gameObject.transform.position += moveDir * speed * Time.deltaTime;

        if (isClone && !hasHit && targetEnemy != null)
        {
            if (Vector2.Distance(transform.position, targetEnemy.position) <= 0.6f)
            {
                Enemy enemy = targetEnemy.GetComponentInParent<Enemy>();
                if (enemy != null && !enemy.IsDead)
                {
                    enemy.TakeDamage(damage);
                }
                hasHit = true;
                Destroy(gameObject);
            }
        }
    }

    private void OnTriggerEnter2D(Collider2D other)
    {
        if (!isClone || hasHit || other == null) return;

        Enemy enemy = other.GetComponentInParent<Enemy>();
        if (enemy != null && !enemy.IsDead)
        {
            enemy.TakeDamage(damage);
            hasHit = true;
            Destroy(gameObject);
        }
    }
}
