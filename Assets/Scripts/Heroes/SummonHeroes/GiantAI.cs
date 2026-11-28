using UnityEngine;

public class GiantAI : SummonedUnitAI
{
    [Header("Lifetime")]
    [SerializeField] private float duration = 15f;

    private float remainingDuration;
    private bool hasLifetime;

    public float RemainingDuration => remainingDuration;

    protected override void Reset()
    {
        base.Reset();

        damage = 40f;
        attackRange = 1.6f;
        attackSpeed = 0.8f;
        moveSpeed = 1.7f;
        detectionRange = 10f;
        attackFallbackDuration = 0.9f;
        sortingYOffset = -0.95f;
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

        if (duration > 0f)
        {
            hasLifetime = true;
            remainingDuration = duration;
        }
    }

    public void Initialize(float giantDamage, float lifeDuration)
    {
        SetDamage(giantDamage);
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
}
