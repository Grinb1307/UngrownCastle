using System;
using System.Collections.Generic;
using TMPro;
using UnityEngine;

[Serializable]
public class InventoryEntry
{
    public string itemId;
    public int quantity;
}

[Serializable]
public class PlayerSaveData
{
    public int coin = 1000;
    public int diamond = 100;

    public List<InventoryEntry> inventory =
        new List<InventoryEntry>();

    // Số ô đã nhận trong chu kỳ: 0 đến 6.
    public int claimedDays;

    // Ví dụ: 20261127.
    public int lastClaimDay;
}

[Serializable]
public class InventorySlotView
{
    public string itemId;

    // Object con chứa icon và số lượng.
    public GameObject content;
    public TMP_Text quantityText;
}

public class Player : MonoBehaviour
{
    private const string SaveKey = "UngrownCastle.Player.v1";
    public const int CycleLength = 6;

    [Header("Currency UI")]
    [SerializeField] private TMP_Text coinText;
    [SerializeField] private TMP_Text diamondText;

    [Header("Inventory UI")]
    [SerializeField] private InventorySlotView[] inventorySlots;

    [Header("Runtime data")]
    [SerializeField] private PlayerSaveData data =
        new PlayerSaveData();

    public bool IsReady { get; private set; }

    public int Coin => data.coin;
    public int Diamond => data.diamond;
    public int LastClaimDay => data.lastClaimDay;

    private void Awake()
    {
        if (PlayerPrefs.HasKey(SaveKey))
        {
            try
            {
                string json = PlayerPrefs.GetString(SaveKey);

                JsonUtility.FromJsonOverwrite(json, data);
            }
            catch (Exception exception)
            {
                Debug.LogError(
                    "Không đọc được dữ liệu người chơi: "
                    + exception.Message,
                    this
                );

                // Không ghi đè save lỗi bằng dữ liệu mới.
                return;
            }
        }

        if (data.inventory == null)
            data.inventory = new List<InventoryEntry>();

        IsReady = true;
    }

    private void Start()
    {
        if (IsReady)
            RefreshUI();
    }

    // Số seal cần hiển thị trong ngày hiện tại.
    public int GetClaimedDays(int today)
    {
        bool completedCycle =
            data.claimedDays >= CycleLength;

        bool reachedNewDay =
            today > data.lastClaimDay;

        if (completedCycle && reachedNewDay)
            return 0;

        return data.claimedDays;
    }

    public bool CanClaimDay(int dayIndex, int today)
    {
        if (!IsReady)
            return false;

        if (dayIndex < 0 || dayIndex >= CycleLength)
            return false;

        // Chặn nhận lại cùng ngày và khi đồng hồ quay lùi.
        if (today <= data.lastClaimDay)
            return false;

        return dayIndex == GetClaimedDays(today);
    }

    public int GetItemCount(string itemId)
    {
        InventoryEntry entry = data.inventory.Find(
            item => item.itemId == itemId
        );

        return entry == null ? 0 : entry.quantity;
    }

    public bool TryClaimDailyReward(
        int dayIndex,
        int today,
        DailyRewardDefinition reward)
    {
        if (!CanClaimDay(dayIndex, today))
            return false;

        if (reward == null || reward.amount <= 0)
            return false;

        switch (reward.type)
        {
            case RewardType.Coin:
                if (data.coin > int.MaxValue - reward.amount)
                    return false;

                data.coin += reward.amount;
                break;

            case RewardType.Diamond:
                if (data.diamond > int.MaxValue - reward.amount)
                    return false;

                data.diamond += reward.amount;
                break;

            case RewardType.Item:
                if (string.IsNullOrWhiteSpace(reward.itemId))
                    return false;

                InventoryEntry entry = data.inventory.Find(
                    item => item.itemId == reward.itemId
                );

                if (entry == null)
                {
                    data.inventory.Add(new InventoryEntry
                    {
                        itemId = reward.itemId,
                        quantity = reward.amount
                    });
                }
                else
                {
                    if (entry.quantity >
                        int.MaxValue - reward.amount)
                    {
                        return false;
                    }

                    entry.quantity += reward.amount;
                }

                break;

            default:
                return false;
        }

        // Chỉ ghi nhận tiến độ sau khi cộng thưởng thành công.
        data.claimedDays = dayIndex + 1;
        data.lastClaimDay = today;

        Save();
        RefreshUI();

        return true;
    }

    private void Save()
    {
        string json = JsonUtility.ToJson(data);

        PlayerPrefs.SetString(SaveKey, json);
        PlayerPrefs.Save();
    }

    public void RefreshUI()
    {
        if (!IsReady)
            return;

        if (coinText != null)
            coinText.text = data.coin.ToString("N0");

        if (diamondText != null)
            diamondText.text = data.diamond.ToString("N0");

        if (inventorySlots == null)
            return;

        foreach (InventorySlotView slot in inventorySlots)
        {
            if (slot == null)
                continue;

            int quantity = GetItemCount(slot.itemId);

            if (slot.content != null)
                slot.content.SetActive(quantity > 0);

            if (slot.quantityText != null)
                slot.quantityText.text = quantity.ToString();
        }
    }
}