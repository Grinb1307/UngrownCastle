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
        [SerializeField] public float castleStopX = -3.77f; // Position where castle stands
        [SerializeField] public Transform targetCastle;

        [Header("Components")]
        [SerializeField] private EnemyWalkAnimation walkAnimation;
        [SerializeField] private SpriteRenderer[] renderersToFlash;
        [Header("Ice Effects")]
        private float slowMultiplier = 1f;
        private Coroutine slowCoroutine;
        private Coroutine freezeCoroutine;

        private bool isFrozen;
        private bool isFlashing;

        private readonly Color frozenColor = new Color(
            0.4f, 0.8f, 1f, 1f
        );

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
        public bool IsFlying => isFlying;
        [Header("Training Dummy")]
        [SerializeField] private bool isTrainingDummy = false;

        private void Awake()
        {
            currentHp = maxHp;
            enemyCollider = GetComponent<Collider2D>();

            // Auto-find EnemyWalkAnimation anywhere on this prefab
            if (walkAnimation == null)
            {
                walkAnimation = GetComponentInChildren<EnemyWalkAnimation>(true);
            }
            if (walkAnimation == null)
            {
                walkAnimation = GetComponent<EnemyWalkAnimation>();
            }

            // Auto-collect all SpriteRenderers in children for flash effect
            if (renderersToFlash == null || renderersToFlash.Length == 0)
            {
                renderersToFlash = GetComponentsInChildren<SpriteRenderer>(true);
            }
        }

        private void Start()
        {
            if (isTrainingDummy)
                return;
            // If target castle is not explicitly assigned, find by tag or default to X = 0
            if (targetCastle == null)
            {
                GameObject castleObj = GameObject.FindGameObjectWithTag("Castle");
                if (castleObj != null)
                {
                    targetCastle = castleObj.transform;
                }
                else
                {
                    Debug.LogWarning($"[{enemyName}] No GameObject with tag 'Castle' found. Using castleStopX={castleStopX} as target.");
                }
            }
            // NOTE: Facing direction is handled by WaveSpawner on Instantiate
        }

        private void Update()
        {
            if (state == EnemyState.Dead || isTrainingDummy)
                return;

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

                float currentSpeed = moveSpeed * slowMultiplier;

                transform.position += Vector3.left *
                    (currentSpeed * Time.deltaTime);
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
            isFlashing = true;
            UpdateEnemyColor();

            yield return new WaitForSeconds(0.1f);

            isFlashing = false;
            flashCoroutine = null;

            UpdateEnemyColor();
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
        // Áp dụng hiệu ứng làm chậm
        public void ApplySlow(float multiplier, float duration)
        {
            if (state == EnemyState.Dead)
                return;

            if (slowCoroutine != null)
                StopCoroutine(slowCoroutine);

            slowCoroutine = StartCoroutine(
                SlowRoutine(multiplier, duration)
            );

            // Đồng thời chuyển màu Enemy thành xanh băng
            ApplyFreezeColor(duration);
        }

        // Coroutine làm chậm
        private IEnumerator SlowRoutine(float multiplier, float duration)
        {
            slowMultiplier = Mathf.Clamp(multiplier, 0f, 1f);

            yield return new WaitForSeconds(duration);

            slowMultiplier = 1f;
            slowCoroutine = null;
        }
        // Áp dụng hiệu ứng đóng băng
        private void ApplyFreezeColor(float duration)
        {
            if (freezeCoroutine != null)
                StopCoroutine(freezeCoroutine);

            freezeCoroutine = StartCoroutine(
                FreezeColorRoutine(duration)
            );
        }

        // Coroutine đổi màu xanh
        private IEnumerator FreezeColorRoutine(float duration)
        {
            isFrozen = true;
            UpdateEnemyColor();

            yield return new WaitForSeconds(duration);

            isFrozen = false;
            freezeCoroutine = null;

            UpdateEnemyColor();
        }

        // Cập nhật màu Enemy
        private void UpdateEnemyColor()
        {
            if (renderersToFlash == null)
                return;

            Color currentColor;

            if (isFlashing)
                currentColor = Color.red;
            else if (isFrozen)
                currentColor = frozenColor;
            else
                currentColor = Color.white;

            foreach (SpriteRenderer sr in renderersToFlash)
            {
                if (sr != null)
                {
                    currentColor.a = sr.color.a;
                    sr.color = currentColor;
                }
            }
        }
    }
}
