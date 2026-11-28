using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UngrownCastle.Enemies;

public class LightningMage : MonoBehaviour
{
    private bool isAttacking = false;

    public Transform Stick;
    public GameObject LightningBall;

    private Vector3 idleLocalPos = new Vector3(-0.21f, 0.09f, 0.06f);
    private Vector3 idleEulerAngles = new Vector3(0f, 0f, 0f);
    private Vector3 isAttackingPos = new Vector3(-0.435f, -0.0025f, 0.06f);
    private Vector3 isAttackingEulerAngles = new Vector3(0f, 0f, -100f);

    public float StickEndZ = -100f;     // Góc Z cuối cùng khi vung trượng chém xuống
    public float StickDuration = 0.15f; // Thời gian vung trượng (giây)
    public float recoverDelay = 0.1f;   // Thời gian dừng nhẹ sau khi đánh trước khi thu trượng

    [Header("Target")]
    [SerializeField] protected float attackRange = 8f;
    [SerializeField] protected LayerMask enemyLayer;

    [Header("Combat & Skill")]
    [SerializeField] private float attackInterval = 1.5f;
    [SerializeField] private float attackDamage = 15f;
    [SerializeField] private float skillDamage = 40f;
    [SerializeField] private float skillRadius = 3f;
    [SerializeField] private float cooldownTime = 6f;

    [Header("Ultimate - Thunderstorm")]
    public LightningMageUltimate ultimateSkill;
    public GameObject lightningBoltPrefab;

    [Header("Skill Targeting")]
    [SerializeField] private GameObject targetingCirclePrefab;
    [SerializeField] private GameObject targetingCircleObject;

    [Header("Ultimate Dim Effect")]
    [SerializeField] private float dimAlpha = 0.65f;
    [SerializeField] private float dimFadeOutDuration = 0.25f;
    [SerializeField] private int dimSortingOrder = 9;

    protected Transform target;
    private float targetTimer;
    private float attackTimer = 0f;
    private bool isReadyToUse = true;
    private Transform spawnPoint;

    private bool isSelectingArea = false;
    private bool waitingForMouseRelease = false;
    private float savedTimeScale = 1f;
    private GameObject dimOverlayObject;
    private SpriteRenderer dimOverlayRenderer;
    private Coroutine dimFadeCoroutine;

    void Awake()
    {
        if (enemyLayer.value == 0)
        {
            int mask = LayerMask.GetMask("Enemy");
            enemyLayer = mask != 0 ? (LayerMask)mask : (LayerMask)(~0);
        }

        EnsureBoxCollider2D();

        if (ultimateSkill == null)
        {
            ultimateSkill = GetComponent<LightningMageUltimate>();
        }
        if (ultimateSkill == null)
        {
            ultimateSkill = gameObject.AddComponent<LightningMageUltimate>();
        }
        if (ultimateSkill != null && ultimateSkill.lightningBoltPrefab == null && lightningBoltPrefab != null)
        {
            ultimateSkill.lightningBoltPrefab = lightningBoltPrefab;
        }

        if (targetingCircleObject == null && targetingCirclePrefab != null)
        {
            targetingCircleObject = Instantiate(targetingCirclePrefab);
        }
        if (targetingCircleObject == null)
        {
            targetingCircleObject = CreateFallbackTargetingCircle();
        }
        if (targetingCircleObject != null)
        {
            targetingCircleObject.SetActive(false);
        }

        EnsureDimOverlay();
    }

    private void EnsureDimOverlay()
    {
        if (dimOverlayObject != null) return;

        dimOverlayObject = new GameObject("LightningUltimateDimOverlay");
        dimOverlayRenderer = dimOverlayObject.AddComponent<SpriteRenderer>();
        Texture2D tex = Texture2D.whiteTexture;
        dimOverlayRenderer.sprite = Sprite.Create(
            tex,
            new Rect(0, 0, tex.width, tex.height),
            new Vector2(0.5f, 0.5f),
            tex.width
        );
        dimOverlayRenderer.color = new Color(0f, 0f, 0f, dimAlpha);
        dimOverlayRenderer.sortingOrder = dimSortingOrder;
        dimOverlayObject.SetActive(false);
    }

    private void SetDimActive(bool active)
    {
        if (dimFadeCoroutine != null)
        {
            StopCoroutine(dimFadeCoroutine);
            dimFadeCoroutine = null;
        }

        EnsureDimOverlay();
        if (dimOverlayObject == null) return;

        if (active)
        {
            Camera cam = Camera.main;
            if (cam != null)
            {
                dimOverlayObject.transform.position = new Vector3(cam.transform.position.x, cam.transform.position.y, 0f);
                float height = cam.orthographic ? cam.orthographicSize * 2f + 4f : 24f;
                float width = cam.orthographic ? height * cam.aspect + 4f : 40f;
                dimOverlayObject.transform.localScale = new Vector3(width, height, 1f);
            }
            else
            {
                dimOverlayObject.transform.position = new Vector3(transform.position.x, transform.position.y, 0f);
                dimOverlayObject.transform.localScale = new Vector3(40f, 24f, 1f);
            }

            if (dimOverlayRenderer != null)
            {
                dimOverlayRenderer.sortingOrder = dimSortingOrder;
                dimOverlayRenderer.color = new Color(0f, 0f, 0f, dimAlpha);
            }
            dimOverlayObject.SetActive(true);
        }
        else
        {
            dimOverlayObject.SetActive(false);
        }
    }

    private void OnThunderstormFinished()
    {
        if (dimFadeCoroutine != null)
        {
            StopCoroutine(dimFadeCoroutine);
        }
        dimFadeCoroutine = StartCoroutine(FadeOutDimRoutine());
    }

    private IEnumerator FadeOutDimRoutine()
    {
        if (dimOverlayObject == null || dimOverlayRenderer == null || !dimOverlayObject.activeSelf)
        {
            dimFadeCoroutine = null;
            yield break;
        }

        float startAlpha = dimOverlayRenderer.color.a;
        float elapsed = 0f;
        while (elapsed < dimFadeOutDuration)
        {
            if (dimOverlayObject == null || dimOverlayRenderer == null) break;
            elapsed += Time.unscaledDeltaTime;
            float alpha = Mathf.Lerp(startAlpha, 0f, Mathf.Clamp01(elapsed / Mathf.Max(dimFadeOutDuration, 0.001f)));
            dimOverlayRenderer.color = new Color(0f, 0f, 0f, alpha);
            yield return null;
        }

        if (dimOverlayObject != null)
        {
            dimOverlayObject.SetActive(false);
        }
        dimFadeCoroutine = null;
    }

    private GameObject CreateFallbackTargetingCircle()
    {
        GameObject circle = new GameObject("LightningTargetingCircle");
        SpriteRenderer sr = circle.AddComponent<SpriteRenderer>();
        int size = 128;
        Texture2D tex = new Texture2D(size, size, TextureFormat.RGBA32, false);
        Color clear = new Color(0f, 0f, 0f, 0f);
        Color ringColor = new Color(1f, 0.92f, 0.2f, 0.85f);
        Vector2 center = new Vector2(size * 0.5f, size * 0.5f);
        float outerR = size * 0.48f;
        float innerR = size * 0.42f;
        for (int y = 0; y < size; y++)
        {
            for (int x = 0; x < size; x++)
            {
                float d = Vector2.Distance(new Vector2(x, y), center);
                tex.SetPixel(x, y, (d <= outerR && d >= innerR) ? ringColor : clear);
            }
        }
        tex.Apply();
        sr.sprite = Sprite.Create(tex, new Rect(0, 0, size, size), new Vector2(0.5f, 0.5f), size);
        sr.sortingOrder = 4;
        return circle;
    }

    private void EnsureBoxCollider2D()
    {
        if (GetComponent<Collider2D>() != null) return;
        BoxCollider2D col = gameObject.AddComponent<BoxCollider2D>();
        SpriteRenderer[] renderers = GetComponentsInChildren<SpriteRenderer>(false);
        if (renderers != null && renderers.Length > 0)
        {
            Bounds bounds = renderers[0].bounds;
            for (int i = 1; i < renderers.Length; i++)
            {
                if (renderers[i] != null) bounds.Encapsulate(renderers[i].bounds);
            }
            col.offset = transform.InverseTransformPoint(bounds.center);
            Vector3 lossy = transform.lossyScale;
            col.size = new Vector2(
                Mathf.Abs(lossy.x) > 0.0001f ? bounds.size.x / Mathf.Abs(lossy.x) : bounds.size.x,
                Mathf.Abs(lossy.y) > 0.0001f ? bounds.size.y / Mathf.Abs(lossy.y) : bounds.size.y
            );
        }
        else
        {
            col.size = new Vector2(1.5f, 1.5f);
        }
    }

    void Start()
    {
        Application.targetFrameRate = 60;
        if (Stick == null) Stick = transform.Find("Stick");
        if (Stick != null)
        {
            // Đặt đúng thông số ban đầu như Ảnh 1
            Stick.localPosition = idleLocalPos;
            Stick.localRotation = Quaternion.Euler(idleEulerAngles);
        }

        spawnPoint = transform.Find("SpawnPoint");
        if (LightningBall != null && LightningBall.transform.IsChildOf(transform))
        {
            LightningBall.SetActive(false);
        }
    }

    void Update()
    {
        // Khi đang chọn vùng Ultimate: đóng băng thời gian và chỉ cho phép di chuyển vòng bằng chuột
        if (isSelectingArea)
        {
            if (waitingForMouseRelease)
            {
                if (!Input.GetMouseButton(0))
                {
                    waitingForMouseRelease = false;
                }
                return;
            }

            UpdateTargetingPosition();

            // Click trái để xác nhận vị trí đánh 6 tia sét vàng
            if (Input.GetMouseButtonDown(0))
            {
                Vector2 skillPosition = GetMouseWorldPosition();
                FinishSkillTargeting(keepDim: true);

                isReadyToUse = false;
                StartCoroutine(CastUltimateRoutine(skillPosition));
                StartCoroutine(SkillCooldownRoutine());
            }
            // Click phải để hủy và khôi phục thời gian
            else if (Input.GetMouseButtonDown(1))
            {
                FinishSkillTargeting(keepDim: false);
            }

            return;
        }

        FindTarget();

        if (target != null && !isAttacking)
        {
            attackTimer += Time.deltaTime;
            if (attackTimer >= attackInterval)
            {
                attackTimer = 0f;
                StartCoroutine(AttackRoutine());
            }
        }
    }

    void OnMouseDown()
    {
        if (!isReadyToUse || isSelectingArea) return;

        BeginSkillTargeting();
    }

    private void BeginSkillTargeting()
    {
        isSelectingArea = true;
        waitingForMouseRelease = true;

        savedTimeScale = Time.timeScale > 0f ? Time.timeScale : 1f;
        Time.timeScale = 0f;

        SetDimActive(true);

        if (targetingCircleObject != null)
        {
            SpriteRenderer[] circleRenderers = targetingCircleObject.GetComponentsInChildren<SpriteRenderer>(true);
            foreach (SpriteRenderer sr in circleRenderers)
            {
                if (sr != null)
                {
                    sr.sortingOrder = dimSortingOrder + 1;
                }
            }

            targetingCircleObject.SetActive(true);
            targetingCircleObject.transform.localScale = Vector3.one * (skillRadius * 0.5f);
            UpdateTargetingPosition();
        }
    }

    private Vector2 GetMouseWorldPosition()
    {
        Camera cam = Camera.main;
        if (cam == null) return transform.position;

        Vector3 mousePosition = Input.mousePosition;
        mousePosition.z = -cam.transform.position.z;

        Vector3 worldPosition = cam.ScreenToWorldPoint(mousePosition);
        return new Vector2(worldPosition.x, worldPosition.y);
    }

    private void UpdateTargetingPosition()
    {
        if (targetingCircleObject == null) return;

        Vector2 pos = GetMouseWorldPosition();
        targetingCircleObject.transform.position = new Vector3(pos.x, pos.y, 0f);
    }

    private void FinishSkillTargeting(bool keepDim = false)
    {
        isSelectingArea = false;
        waitingForMouseRelease = false;
        Time.timeScale = savedTimeScale;

        if (targetingCircleObject != null)
        {
            targetingCircleObject.SetActive(false);
        }

        if (!keepDim)
        {
            SetDimActive(false);
        }
    }

    protected void FindTarget()
    {
        // Giữ nguyên mục tiêu hiện tại cho đến khi Enemy chết hoặc ra khỏi tầm đánh
        if (target != null)
        {
            Enemy currentEnemy = target.GetComponentInParent<Enemy>();
            if (currentEnemy != null && !currentEnemy.IsDead &&
                Vector2.Distance(transform.position, target.position) <= attackRange)
            {
                return;
            }
            target = null;
            targetTimer = 0.25f; // Tìm ngay Enemy gần nhất tiếp theo
        }

        targetTimer += Time.deltaTime;
        if (targetTimer < 0.25f) return;
        targetTimer = 0f;

        Collider2D[] enemies = Physics2D.OverlapCircleAll(
            transform.position,
            attackRange,
            enemyLayer
        );

        Transform nearest = null;
        float minDistance = Mathf.Infinity;

        foreach (Collider2D col in enemies)
        {
            if (col == null) continue;
            Enemy enemyComp = col.GetComponentInParent<Enemy>();
            if (enemyComp == null || enemyComp.IsDead) continue;

            float distance = Vector2.Distance(transform.position, enemyComp.transform.position);
            if (distance < minDistance)
            {
                minDistance = distance;
                nearest = enemyComp.transform;
            }
        }

        target = nearest;
    }

    IEnumerator AttackRoutine()
    {
        isAttacking = true;

        if (Stick != null)
        {
            Stick.localPosition = isAttackingPos;
            Quaternion startSlashRot = Quaternion.Euler(isAttackingEulerAngles);
            Quaternion endSlashRot = Quaternion.Euler(isAttackingEulerAngles.x, isAttackingEulerAngles.y, StickEndZ);
            Stick.localRotation = startSlashRot;

            float elapsed = 0f;
            while (elapsed < StickDuration)
            {
                elapsed += Time.deltaTime;
                Stick.localRotation = Quaternion.Slerp(startSlashRot, endSlashRot, elapsed / StickDuration);
                yield return null;
            }
            Stick.localRotation = endSlashRot;
        }

        Vector3 firePos = spawnPoint != null ? spawnPoint.position : transform.position;
        if (LightningBall != null && target != null)
        {
            GameObject clone = Instantiate(LightningBall, firePos, Quaternion.identity);
            clone.SetActive(true);

            LightningBallScript ballScript = clone.GetComponent<LightningBallScript>();
            if (ballScript != null)
            {
                ballScript.Initialize(target, firePos, attackDamage);
            }
        }

        yield return new WaitForSeconds(recoverDelay);

        if (Stick != null)
        {
            Stick.localPosition = idleLocalPos;
            Stick.localRotation = Quaternion.Euler(idleEulerAngles);
        }
        isAttacking = false;
    }

    IEnumerator CastUltimateRoutine(Vector2 stormCenter)
    {
        isAttacking = true;

        if (Stick != null)
        {
            Stick.localPosition = isAttackingPos;
            Quaternion startSlashRot = Quaternion.Euler(isAttackingEulerAngles);
            Quaternion endSlashRot = Quaternion.Euler(isAttackingEulerAngles.x, isAttackingEulerAngles.y, StickEndZ);
            Stick.localRotation = startSlashRot;

            float elapsed = 0f;
            while (elapsed < StickDuration)
            {
                elapsed += Time.deltaTime;
                Stick.localRotation = Quaternion.Slerp(startSlashRot, endSlashRot, elapsed / StickDuration);
                yield return null;
            }
            Stick.localRotation = endSlashRot;
        }

        if (ultimateSkill != null)
        {
            ultimateSkill.strikeDamage = skillDamage;
            ultimateSkill.stormRadius = skillRadius;
            ultimateSkill.CastThunderstorm(stormCenter, enemyLayer, OnThunderstormFinished);
        }
        else
        {
            SetDimActive(false);
        }

        yield return new WaitForSeconds(recoverDelay);

        if (Stick != null)
        {
            Stick.localPosition = idleLocalPos;
            Stick.localRotation = Quaternion.Euler(idleEulerAngles);
        }
        isAttacking = false;
    }

    IEnumerator SkillCooldownRoutine()
    {
        yield return new WaitForSeconds(cooldownTime);
        isReadyToUse = true;
    }

    private void OnDisable()
    {
        if (isSelectingArea) FinishSkillTargeting(false);
        else SetDimActive(false);
    }

    private void OnDestroy()
    {
        if (isSelectingArea) FinishSkillTargeting(false);
        else SetDimActive(false);

        if (dimOverlayObject != null)
        {
            Destroy(dimOverlayObject);
        }
    }
}
