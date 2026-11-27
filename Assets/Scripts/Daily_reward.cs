using System;
using UnityEngine;
using UnityEngine.UI;

public enum RewardType
{
    Coin,
    Diamond,
    Item
}

[Serializable]
public class DailyRewardDefinition
{
    public RewardType type;

    [Min(1)]
    public int amount = 1;

    // Chỉ dùng khi type là Item.
    public string itemId;
}

public class Daily_reward : MonoBehaviour
{
    [Header("References")]
    [SerializeField] private Player player;
    [SerializeField] private GameManager gameManager;

    [Header("Days: index 0 = Day 1")]
    [SerializeField] private Button[] dayButtons =
        new Button[6];

    [SerializeField] private DailyRewardDefinition[] rewards =
        new DailyRewardDefinition[6];

#if UNITY_EDITOR
    [Header("Chỉ dùng kiểm thử trong Editor")]
    [SerializeField] private int debugDayOffset;
#endif

    private bool ready;
    private int displayedDay;
    private float nextDateCheck;

    private int GetToday()
    {
        DateTime date = DateTime.UtcNow.AddHours(7);

#if UNITY_EDITOR
        date = date.AddDays(debugDayOffset);
#endif

        return date.Year * 10000
             + date.Month * 100
             + date.Day;
    }

    private void Start()
    {
        if (player == null ||
            !player.IsReady ||
            gameManager == null ||
            dayButtons == null ||
            rewards == null ||
            gameManager.CheckinImage == null)
        {
            Debug.LogError(
                "Daily reward chưa được gán đủ references.",
                this
            );
            return;
        }

        if (dayButtons.Length != Player.CycleLength ||
            rewards.Length != Player.CycleLength ||
            gameManager.CheckinImage.Length != Player.CycleLength)
        {
            Debug.LogError(
                "Buttons, Rewards và CheckinImage phải có 6 phần tử.",
                this
            );
            return;
        }

        for (int i = 0; i < Player.CycleLength; i++)
        {
            if (dayButtons[i] == null ||
                rewards[i] == null ||
                gameManager.CheckinImage[i] == null)
            {
                Debug.LogError(
                    $"Thiếu cấu hình của Day {i + 1}.",
                    this
                );
                return;
            }
        }

        ready = true;
        RefreshUI();
    }

    private void Update()
    {
        if (!ready || Time.unscaledTime < nextDateCheck)
            return;

        nextDateCheck = Time.unscaledTime + 1f;

        // Cập nhật nếu người chơi đang mở game qua 00:00.
        if (GetToday() != displayedDay)
            RefreshUI();
    }

    private void OnEnable()
    {
        if (ready)
            RefreshUI();
    }

    public void ClaimDay(int dayIndex)
    {
        if (!ready)
            return;

        if (dayIndex < 0 || dayIndex >= rewards.Length)
            return;

        bool success = player.TryClaimDailyReward(
            dayIndex,
            GetToday(),
            rewards[dayIndex]
        );

        if (!success)
        {
            Debug.Log(
                "Chưa thể nhận ngày này, đã nhận hôm nay, "
                + "hoặc cấu hình thưởng chưa hợp lệ."
            );
        }

        RefreshUI();
    }

    public void RefreshUI()
    {
        if (!ready)
            return;

        int today = GetToday();
        int claimedDays = player.GetClaimedDays(today);

        for (int i = 0; i < Player.CycleLength; i++)
        {
            gameManager.CheckinImage[i].SetActive(
                i < claimedDays
            );

            dayButtons[i].interactable =
                player.CanClaimDay(i, today);
        }

        displayedDay = today;
    }
}