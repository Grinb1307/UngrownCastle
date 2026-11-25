using UnityEngine;

public class IceMage : IceHeroes
{
    [Header("Ice Mage Skill")]
    [SerializeField] private GameObject iceSpikePrefab;

    [SerializeField] private float skillRadius = 3f;
    [SerializeField] private float skillDamage = 30f;
    [SerializeField] private float slowMultiplier = 0.5f;
    [SerializeField] private float slowDuration = 3f;
    [SerializeField] private float skillLifetime = 2f;

    private bool isSelectingArea;

    // Kiểm tra điều kiện sử dụng skill
    protected override bool CanUseSkill()
    {
        return iceSpikePrefab != null && !isSelectingArea;
    }

    // Bắt đầu chọn vị trí sử dụng skill
    public override void UseSkill()
    {
        isSelectingArea = true;

        Debug.Log("Ice Mage: Hãy chọn vị trí sử dụng skill!");
    }

    protected override void Update()
    {
        base.Update();

        if (!isSelectingArea)
            return;

        // Click chuột trái để chọn vị trí
        if (Input.GetMouseButtonDown(0))
        {
            CastIceSkill();
        }

        // Nhấn chuột phải để hủy chọn
        if (Input.GetMouseButtonDown(1))
        {
            isSelectingArea = false;

            // Hoàn lại Mana nếu hủy skill
            currentMana = maxMana;

            Debug.Log("Đã hủy chọn vị trí skill!");
        }
    }

    // Tạo vùng băng tại vị trí được chọn
    private void CastIceSkill()
    {
        if (Camera.main == null)
            return;

        Vector3 mousePosition =
            Camera.main.ScreenToWorldPoint(Input.mousePosition);

        Vector2 skillPosition = new Vector2(
            mousePosition.x,
            mousePosition.y
        );

        GameObject iceZone = Instantiate(
            iceSpikePrefab,
            skillPosition,
            Quaternion.identity
        );

        IceSpikeZone zone = iceZone.GetComponent<IceSpikeZone>();

        if (zone != null)
        {
            zone.Initialize(
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
            Debug.LogError("Ice Spike Prefab thiếu IceSpikeZone!");
            Destroy(iceZone);
        }

        isSelectingArea = false;

        Debug.Log("Ice Mage sử dụng Ice Skill!");
    }

    private void OnDisable()
    {
        isSelectingArea = false;
    }
}