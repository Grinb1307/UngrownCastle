using UnityEngine;

public class AngelCloneAI : SummonedUnitAI
{
    [Header("Lifetime")]
    [SerializeField] private float duration = 15f;

    [Header("Flight Offset")]
    [SerializeField] private float approachVerticalOffset = 0f;

    private float remainingDuration;
    private bool hasLifetime;

    public float RemainingDuration => remainingDuration;

    protected override void Reset()
    {
        base.Reset();

        damage = 22.5f;
        attackRange = 1.4f;
        attackSpeed = 1.2f;
        attackHitBuffer = 0.4f;
        attackFallbackDuration = 0.65f;
        moveSpeed = 4.2f;
        detectionRange = 30f;
        sortingYOffset = -0.45f;
        duration = 15f;
    }

    protected override void OnValidate()
    {
        base.OnValidate();

        duration = Mathf.Max(0f, duration);
    }

    protected override void Awake()
    {
        base.Awake();

        if (duration > 0f && !hasLifetime)
        {
            hasLifetime = true;
            remainingDuration = duration;
        }
    }

    public void Initialize(
        float cloneDamage,
        float lifeDuration,
        LayerMask targetEnemyLayer,
        float verticalOffset = 0f)
    {
        SetDamage(cloneDamage);
        enemyLayer = targetEnemyLayer;
        approachVerticalOffset = verticalOffset;

        duration = Mathf.Max(0f, lifeDuration);
        if (duration > 0f)
        {
            hasLifetime = true;
            remainingDuration = duration;
        }
        else
        {
            hasLifetime = false;
            remainingDuration = 0f;
        }
    }

    protected override void Update()
    {
        if (hasLifetime)
        {
            remainingDuration -= Time.deltaTime;

            if (remainingDuration <= 0f)
            {
                Destroy(gameObject);
                return;
            }
        }

        base.Update();
    }

    protected override void MoveToTarget()
    {
        if (!IsTargetValid(targetEnemy))
        {
            SetWalking(false);
            return;
        }

        SetWalking(true);

        Vector2 destination = (Vector2)targetEnemy.transform.position + new Vector2(0f, approachVerticalOffset);
        transform.position = Vector2.MoveTowards(
            transform.position,
            destination,
            moveSpeed * Time.deltaTime
        );
    }
}
