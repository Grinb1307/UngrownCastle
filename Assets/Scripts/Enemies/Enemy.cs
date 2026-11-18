using System.Collections;
using UnityEngine;

namespace UngrownCastle.Enemies
{
    public enum EnemyState
    {
        Moving,
        Attacking,
        Dead
    }

    [SelectionBase]
    public class Enemy : MonoBehaviour
    {
        [Header("Enemy Info")]
        [SerializeField] private string enemyName = "Monster";
        [SerializeField] private float maxHp = 10f;
        [SerializeField] private float moveSpeed = 1f;
        [SerializeField] private float attackDamage = 5f;
        [SerializeField] private float attackRange = 0.6f;
        [SerializeField] private float attackSpeed = 1f; // seconds per attack
        [SerializeField] private int goldReward = 10;
        [SerializeField] private int expReward = 10;
        [SerializeField] private bool isFlying = false;

        [Header("Target & Movement")]
        [SerializeField] private float castleStopX = 1.0f; // Position where castle stands
        [SerializeField] private Transform targetCastle;

        [Header("Components")]
        [SerializeField] private EnemyWalkAnimation walkAnimation;
        [SerializeField] private SpriteRenderer[] renderersToFlash;

        // Current runtime stats
        private float currentHp;
        private EnemyState state = EnemyState.Moving;
        private float nextAttackTime;
        private Collider2D enemyCollider;
        private Coroutine flashCoroutine;

        public string EnemyName => enemyName;
        public float CurrentHp => currentHp;
        public float MaxHp => maxHp;
        public float MoveSpeed => moveSpeed;
        public EnemyState State => state;
        public bool IsDead => state == EnemyState.Dead;

        private void Awake()
        {
            currentHp = maxHp;
            enemyCollider = GetComponent<Collider2D>();
            if (walkAnimation == null)
            {
                walkAnimation = GetComponent<EnemyWalkAnimation>();
            }

            if (renderersToFlash == null || renderersToFlash.Length == 0)
            {
                renderersToFlash = GetComponentsInChildren<SpriteRenderer>();
            }
        }

        private void Start()
        {
            // If target castle is not explicitly assigned, find by tag or default to X = 0
            if (targetCastle == null)
            {
                GameObject castleObj = GameObject.FindGameObjectWithTag("Castle");
                if (castleObj != null)
                {
                    targetCastle = castleObj.transform;
                }
            }
        }

        private void Update()
        {
            if (state == EnemyState.Dead) return;

            float targetX = targetCastle != null ? targetCastle.position.x : castleStopX;
            float distanceToCastle = transform.position.x - targetX;

            if (distanceToCastle <= attackRange)
            {
                // Reached Castle - switch to attack
                if (state != EnemyState.Attacking)
                {
                    state = EnemyState.Attacking;
                    if (walkAnimation != null)
                    {
                        walkAnimation.SetAttacking(true);
                    }
                }

                if (Time.time >= nextAttackTime)
                {
                    PerformAttack();
                    nextAttackTime = Time.time + attackSpeed;
                }
            }
            else
            {
                // Move towards castle (leftwards)
                if (state != EnemyState.Moving)
                {
                    state = EnemyState.Moving;
                    if (walkAnimation != null)
                    {
                        walkAnimation.SetAttacking(false);
                    }
                }

                transform.position += Vector3.left * (moveSpeed * Time.deltaTime);
            }
        }

        private void PerformAttack()
        {
            if (walkAnimation != null)
            {
                walkAnimation.TriggerAttackSwing();
            }

            // In GrowCastle, monsters hit the Castle
            // You can call Castle.Instance.TakeDamage(attackDamage);
            Debug.Log($"[{enemyName}] attacks Castle for {attackDamage} damage!");
        }

        public void TakeDamage(float damage)
        {
            if (state == EnemyState.Dead) return;

            currentHp -= damage;

            // Flash visual effect
            if (flashCoroutine != null)
            {
                StopCoroutine(flashCoroutine);
            }
            flashCoroutine = StartCoroutine(FlashRoutine());

            if (currentHp <= 0f)
            {
                Die();
            }
        }

        private void Die()
        {
            state = EnemyState.Dead;

            if (enemyCollider != null)
            {
                enemyCollider.enabled = false;
            }

            if (walkAnimation != null)
            {
                walkAnimation.OnDeath();
            }

            Debug.Log($"[{enemyName}] died! +{goldReward} Gold, +{expReward} Exp");
            StartCoroutine(FadeAndDestroyRoutine());
        }

        private IEnumerator FlashRoutine()
        {
            if (renderersToFlash != null)
            {
                foreach (var sr in renderersToFlash)
                {
                    if (sr != null) sr.color = Color.red;
                }
            }

            yield return new WaitForSeconds(0.1f);

            if (renderersToFlash != null)
            {
                foreach (var sr in renderersToFlash)
                {
                    if (sr != null) sr.color = Color.white;
                }
            }
        }

        private IEnumerator FadeAndDestroyRoutine()
        {
            float elapsed = 0f;
            float duration = 0.5f;

            while (elapsed < duration)
            {
                elapsed += Time.deltaTime;
                float alpha = Mathf.Lerp(1f, 0f, elapsed / duration);

                if (renderersToFlash != null)
                {
                    foreach (var sr in renderersToFlash)
                    {
                        if (sr != null)
                        {
                            Color c = sr.color;
                            c.a = alpha;
                            sr.color = c;
                        }
                    }
                }

                yield return null;
            }

            Destroy(gameObject);
        }

        public void InitializeStats(string name, float hp, float speed, float damage, float range, int gold, int exp, bool flying = false)
        {
            enemyName = name;
            maxHp = hp;
            currentHp = hp;
            moveSpeed = speed;
            attackDamage = damage;
            attackRange = range;
            goldReward = gold;
            expReward = exp;
            isFlying = flying;
        }
    }
}
