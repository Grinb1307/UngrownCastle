using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UngrownCastle.Enemies;

public class IceSocceress : IceHeroes
{
    [Header("Ice Rain Sprites")]
    [SerializeField] private Sprite[] iceRainSprites;

    [Header("Ice Rain Settings")]
    [SerializeField] private int rainDropCount = 20;
    [SerializeField] private float rainSpawnHeight = 5f;
    [SerializeField] private float rainFallSpeed = 12f;
    [SerializeField] private float rainDuration = 1.2f;
    [SerializeField] private int sortingOrder = 5;

    [Header("Ice Rain Appearance")]
    [SerializeField]
    private Vector2 iceScaleRange =
        new Vector2(0.8f, 1.2f);

    [SerializeField] private float randomRotation = 15f;

    [Header("Skill Stats")]
    [SerializeField] private float skillRadius = 3f;
    [SerializeField] private float skillDamage = 30f;
    [SerializeField] private float freezeDuration = 3f;

    [Header("Skill Targeting")]
    [SerializeField] private GameObject targetingCircleObject;

    private bool isSelectingArea;
    private bool waitingForMouseRelease;

    private void Awake()
    {
        if (targetingCircleObject != null)
            targetingCircleObject.SetActive(false);
    }

    protected override bool CanUseSkill()
    {
        return iceRainSprites != null
            && iceRainSprites.Length > 0
            && targetingCircleObject != null
            && !isSelectingArea;
    }

    // Bắt đầu chọn vùng sử dụng skill
    public override void UseSkill()
    {
        isSelectingArea = true;
        waitingForMouseRelease = true;

        targetingCircleObject.SetActive(true);

        targetingCircleObject.transform.localScale =
            Vector3.one * (skillRadius * 2f);

        Debug.Log("Ice Socceress: Chọn vùng để gọi mưa băng!");
    }

    protected override void Update()
    {
        // Giữ nguyên hoạt động đánh thường của IceHeroes
        base.Update();

        if (!isSelectingArea)
            return;

        // Chờ thả chuột sau khi click vào Hero
        if (waitingForMouseRelease)
        {
            if (!Input.GetMouseButton(0))
                waitingForMouseRelease = false;

            return;
        }

        UpdateTargetingPosition();

        // Click trái để xác nhận vùng
        if (Input.GetMouseButtonDown(0))
        {
            CastIceRain();
        }

        // Click phải để hủy
        if (Input.GetMouseButtonDown(1))
        {
            CancelSkill();
        }
    }

    private Vector2 GetMouseWorldPosition()
    {
        Camera cam = Camera.main;

        if (cam == null)
            return transform.position;

        Vector3 mousePosition = Input.mousePosition;
        mousePosition.z = -cam.transform.position.z;

        Vector3 worldPosition =
            cam.ScreenToWorldPoint(mousePosition);

        return new Vector2(worldPosition.x, worldPosition.y);
    }

    private void UpdateTargetingPosition()
    {
        Vector2 position = GetMouseWorldPosition();

        targetingCircleObject.transform.position =
            new Vector3(position.x, position.y, 0f);
    }

    // Xác nhận vùng và thi triển skill
    private void CastIceRain()
    {
        Vector2 skillPosition = GetMouseWorldPosition();

        // Gây sát thương và đóng băng Enemy trong vùng
        ApplyIceEffect(skillPosition);

        // Tạo hiệu ứng mưa băng
        StartCoroutine(SpawnIceRain(skillPosition));

        FinishSkillSelection();

        Debug.Log("Ice Socceress: Đã sử dụng Mưa Băng!");
    }

    // Tìm Enemy trong vùng và áp dụng hiệu ứng
    private void ApplyIceEffect(Vector2 position)
    {
        Collider2D[] hits = Physics2D.OverlapCircleAll(
            position,
            skillRadius,
            enemyLayer
        );

        // Tránh gây sát thương nhiều lần cho Enemy có nhiều Collider
        HashSet<Enemy> affectedEnemies = new HashSet<Enemy>();

        foreach (Collider2D hit in hits)
        {
            Enemy enemy = hit.GetComponentInParent<Enemy>();

            if (enemy == null || enemy.IsDead)
                continue;

            if (affectedEnemies.Contains(enemy))
                continue;

            affectedEnemies.Add(enemy);

            enemy.TakeDamage(skillDamage);
            enemy.Freeze(freezeDuration);
        }

        Debug.Log($"Mưa Băng trúng {affectedEnemies.Count} Enemy!");
    }

    // Tạo các sprite băng rơi từ trên trời
    private IEnumerator SpawnIceRain(Vector2 center)
    {
        float delay = rainDuration / Mathf.Max(rainDropCount, 1);

        for (int i = 0; i < rainDropCount; i++)
        {
            Vector2 offset = Random.insideUnitCircle * skillRadius;

            Vector2 landingPosition = center + offset;

            CreateIceDrop(landingPosition);

            yield return new WaitForSeconds(delay);
        }
    }

    private void CreateIceDrop(Vector2 landingPosition)
    {
        Sprite sprite = iceRainSprites[
            Random.Range(0, iceRainSprites.Length)
        ];

        GameObject iceDrop = new GameObject("IceRainDrop");

        SpriteRenderer sr = iceDrop.AddComponent<SpriteRenderer>();

        sr.sprite = sprite;
        sr.sortingOrder = sortingOrder;

        float scale = Random.Range(
            iceScaleRange.x,
            iceScaleRange.y
        );

        iceDrop.transform.localScale =
            new Vector3(scale, scale, 1f);

        float angle = Random.Range(
            -randomRotation,
            randomRotation
        );

        iceDrop.transform.rotation =
            Quaternion.Euler(0f, 0f, angle);

        Vector2 startPosition =
            landingPosition + Vector2.up * rainSpawnHeight;

        iceDrop.transform.position = startPosition;

        StartCoroutine(FallIceDrop(iceDrop, landingPosition));
    }

    // Điều khiển sprite băng rơi xuống
    private IEnumerator FallIceDrop(
        GameObject iceDrop,
        Vector2 landingPosition)
    {
        while (iceDrop != null)
        {
            iceDrop.transform.position = Vector2.MoveTowards(
                iceDrop.transform.position,
                landingPosition,
                rainFallSpeed * Time.deltaTime
            );

            float distance = Vector2.Distance(
                iceDrop.transform.position,
                landingPosition
            );

            if (distance < 0.05f)
                break;

            yield return null;
        }

        if (iceDrop != null)
            Destroy(iceDrop);
    }

    private void CancelSkill()
    {
        FinishSkillSelection();

        // Hoàn lại mana nếu người chơi hủy
        currentMana = maxMana;

        Debug.Log("Đã hủy Mưa Băng!");
    }

    private void FinishSkillSelection()
    {
        isSelectingArea = false;
        waitingForMouseRelease = false;

        if (targetingCircleObject != null)
            targetingCircleObject.SetActive(false);
    }

    private void OnDisable()
    {
        FinishSkillSelection();
    }
}