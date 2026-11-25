
using UnityEngine;

public class IceSocceress : IceHeroes
{
    [Header("Ice Mage Skill")]
    [SerializeField] private GameObject iceSpikeZonePrefab;

    [Header("Ice Spike Sprites")]
    [SerializeField] private Sprite[] spikeSprites;
    [SerializeField] private int spikeCount = 10;
    [SerializeField] private float spikeVisualRadius = 2.5f;

    [Header("Spike Appearance")]
    [SerializeField]
    private Vector2 spikeScaleRange =
        new Vector2(0.8f, 1.2f);

    [SerializeField] private float randomRotation = 15f;
    [SerializeField] private int sortingOrder = 1;

    [Header("Skill Stats")]
    [SerializeField] private float skillRadius = 3f;
    [SerializeField] private float skillDamage = 30f;
    [SerializeField] private float slowMultiplier = 0.5f;
    [SerializeField] private float slowDuration = 3f;
    [SerializeField] private float skillLifetime = 2f;

    [Header("Skill Targeting")]
    [SerializeField] private GameObject targetingCircleObject;

    private bool isSelectingArea;
    private bool waitingForMouseRelease;

    private void Awake()
    {
        // Ẩn vòng tròn khi bắt đầu game
        if (targetingCircleObject != null)
        {
            targetingCircleObject.SetActive(false);
        }
    }

    protected override bool CanUseSkill()
    {
        return iceSpikeZonePrefab != null
            && spikeSprites != null
            && spikeSprites.Length > 0
            && targetingCircleObject != null
            && !isSelectingArea;
    }

    // Bắt đầu chọn vị trí sử dụng skill
    public override void UseSkill()
    {
        isSelectingArea = true;
        waitingForMouseRelease = true;

        // Hiện vòng tròn
        targetingCircleObject.SetActive(true);

        // Chỉnh kích thước theo bán kính skill
        targetingCircleObject.transform.localScale =
            Vector3.one * (skillRadius * 2f);

        Debug.Log("Ice Socceress: Hãy chọn vùng sử dụng skill!");
    }

    protected override void Update()
    {
        base.Update();

        if (!isSelectingArea)
            return;

        // Chờ thả chuột sau khi click vào Ice Socceress
        if (waitingForMouseRelease)
        {
            if (!Input.GetMouseButton(0))
                waitingForMouseRelease = false;

            return;
        }

        UpdateTargetingPosition();

        // Click trái để xác nhận
        if (Input.GetMouseButtonDown(0))
        {
            CastIceSkill();
        }

        // Click phải để hủy
        if (Input.GetMouseButtonDown(1))
        {
            CancelSkill();
        }
    }

    // Lấy vị trí chuột trong World
    private Vector2 GetMouseWorldPosition()
    {
        Camera cam = Camera.main;

        if (cam == null)
            return transform.position;

        Vector3 mousePosition = Input.mousePosition;
        mousePosition.z = -cam.transform.position.z;

        Vector3 worldPosition =
            cam.ScreenToWorldPoint(mousePosition);

        return new Vector2(
            worldPosition.x,
            worldPosition.y
        );
    }

    // Di chuyển vòng tròn theo chuột
    private void UpdateTargetingPosition()
    {
        Vector2 position = GetMouseWorldPosition();

        targetingCircleObject.transform.position =
            new Vector3(position.x, position.y, 0f);
    }

    // Xác nhận vị trí và thi triển skill
    private void CastIceSkill()
    {
        Vector2 skillPosition = GetMouseWorldPosition();

        GameObject zone = Instantiate(
            iceSpikeZonePrefab,
            skillPosition,
            Quaternion.identity
        );

        CreateSpikeVisuals(zone.transform);

        IceSpikeZone iceZone =
            zone.GetComponent<IceSpikeZone>();

        if (iceZone != null)
        {
            iceZone.Initialize(
                skillRadius,
                skillDamage,
                slowMultiplier,
                slowDuration,
                skillLifetime,
                enemyLayer
            );
        }
        else
        {
            Debug.LogError(
                "Ice Spike Zone Prefab thiếu IceSpikeZone!"
            );

            Destroy(zone);
        }

        FinishSkillSelection();
    }

    // Tạo nhiều thanh gai băng
    private void CreateSpikeVisuals(Transform parent)
    {
        for (int i = 0; i < spikeCount; i++)
        {
            Sprite sprite =
                spikeSprites[i % spikeSprites.Length];

            Vector2 offset =
                Random.insideUnitCircle * spikeVisualRadius;

            GameObject spike = new GameObject("IceSpike");

            spike.transform.SetParent(parent, false);

            spike.transform.localPosition =
                new Vector3(offset.x, offset.y, 0f);

            float angle = Random.Range(
                -randomRotation,
                randomRotation
            );

            spike.transform.localRotation =
                Quaternion.Euler(0f, 0f, angle);

            float scale = Random.Range(
                spikeScaleRange.x,
                spikeScaleRange.y
            );

            spike.transform.localScale =
                new Vector3(scale, scale, 1f);

            SpriteRenderer sr =
                spike.AddComponent<SpriteRenderer>();

            sr.sprite = sprite;
            sr.sortingOrder = sortingOrder;
        }
    }

    // Hủy chọn vùng
    private void CancelSkill()
    {
        FinishSkillSelection();

        // Hoàn lại mana
        currentMana = maxMana;

        Debug.Log("Đã hủy chọn vùng băng!");
    }

    // Kết thúc quá trình chọn vị trí
    private void FinishSkillSelection()
    {
        isSelectingArea = false;
        waitingForMouseRelease = false;

        // Ẩn vòng tròn thay vì Destroy
        if (targetingCircleObject != null)
        {
            targetingCircleObject.SetActive(false);
        }
    }

    private void OnDisable()
    {
        FinishSkillSelection();
    }
}
