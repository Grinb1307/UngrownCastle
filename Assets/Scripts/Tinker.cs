using System.Collections;
using UnityEngine;

public class Tinker : MonoBehaviour
{
    [Header("Cài đặt vị trí xuất hiện trên cao")]
    public float spawnOffsetX = 4.0f;      // Khoảng cách ngang so với Tinker để bom xuất hiện
    public float spawnOffsetY = 3.5f;      // Độ cao xuất hiện ở trên trời so với Tinker
    public float fallSpeed = 8.0f;         // Tốc độ rơi của bom theo trục Y mỗi giây
    public float groundLevelY = -1.5f;     // Độ cao mặt đất (khi rơi đến đây bom sẽ biến mất)

    [Header("Hồi chiêu")]
    public float cooldownTime = 6.0f;      // Thời gian hồi chiêu (6 giây)

    private Transform[] bombs;             // Mảng chứa 7 quả bom
    private Vector3[] initialLocalPositions; // Lưu vị trí gốc của các quả bom
    private bool isReadyToUse = true;      // Mặc định ban đầu vào game xài được luôn
    private bool isFalling = false;

    void Awake()
    {
        Application.targetFrameRate = 60;
        // Tự động tìm tất cả các object con có chứa chữ "skill" trong tên để gom thành 7 quả bom
        System.Collections.Generic.List<Transform> bombList = new System.Collections.Generic.List<Transform>();

        foreach (Transform child in transform)
        {
            if (child.name.Contains("skill"))
            {
                bombList.Add(child);
            }
        }

        bombs = bombList.ToArray();
        initialLocalPositions = new Vector3[bombs.Length];

        // Lưu vị trí gốc và ẩn tất cả bom đi lúc đầu (SetActive(false))
        for (int i = 0; i < bombs.Length; i++)
        {
            initialLocalPositions[i] = bombs[i].localPosition;
            bombs[i].gameObject.SetActive(false);
        }
    }

    // Sự kiện khi người chơi nhấn chuột vào Tinker
    void OnMouseDown()
    {
        // Nếu chưa hồi xong hoặc bom đang rơi thì không nhận click
        if (!isReadyToUse || isFalling) return;

        StartCoroutine(TriggerBombSkillSequence());
    }

    IEnumerator TriggerBombSkillSequence()
    {
        isReadyToUse = false; // Khóa skill, bắt đầu tính cooldown sau khi dùng
        isFalling = true;

        Vector3 tinkerPos = transform.position;

        // 1. Đặt 7 quả bom xuất hiện trên cao (lệch X, Y so với Tinker)
        for (int i = 0; i < bombs.Length; i++)
        {
            if (bombs[i] == null) continue;

            // Tính vị trí xuất hiện trên trời (rải lệch vị trí X một chút cho tự nhiên dựa trên vị trí gốc của chúng)
            float randomOffsetX = Random.Range(-1.5f, 1.5f);
            Vector3 spawnWorldPos = new Vector3(tinkerPos.x + spawnOffsetX + randomOffsetX, tinkerPos.y + spawnOffsetY, tinkerPos.z);

            bombs[i].position = spawnWorldPos;
            bombs[i].gameObject.SetActive(true); // Bật bom lên
        }

        // 2. Vòng lặp cho bom rơi xuống đất theo từng khung hình
        bool allLanded = false;
        while (!allLanded)
        {
            allLanded = true;

            for (int i = 0; i < bombs.Length; i++)
            {
                if (bombs[i] == null || !bombs[i].gameObject.activeSelf) continue;

                // Giảm dần độ cao Y theo khung hình
                Vector3 currentPos = bombs[i].position;
                currentPos.y -= fallSpeed * Time.deltaTime;
                bombs[i].position = currentPos;

                // Kiểm tra nếu rơi chạm mặt đất (groundLevelY)
                if (currentPos.y <= groundLevelY)
                {
                    bombs[i].gameObject.SetActive(false); // Chạm đất thì biến mất
                }
                else
                {
                    // Nếu vẫn còn ít nhất 1 quả chưa chạm đất thì tiếp tục chạy vòng lặp
                    allLanded = false;
                }
            }

            yield return null; // Chờ sang frame tiếp theo
        }

        isFalling = false;

        // ==========================================
        // BẮT ĐẦU ĐẾM NGƯỢC 6 GIÂY (COOLDOWN)
        // ==========================================
        float timer = 0f;
        while (timer < cooldownTime)
        {
            timer += Time.deltaTime;
            yield return null;
        }

        // Hết 6 giây, skill sẵn sàng được bấm lại
        isReadyToUse = true;
    }
}