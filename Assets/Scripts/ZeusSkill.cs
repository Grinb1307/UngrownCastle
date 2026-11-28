using UnityEngine;
using System.Collections;
public class ZeusSkill : MonoBehaviour
{

    [Header("Cài đặt hiệu ứng")]
    public float duration = 0.2f;      // Thời gian hoàn thành hiệu ứng sét đánh (giây)
    public float hideDelay = 0.3f;     // Thời gian lưu lại trên màn hình trước khi tắt

    private Transform[] childTransforms;
    private Vector3[] targetLocalPositions; // Lưu vị trí gốc của các con
    private Vector3[] targetScales;        // Lưu kích thước gốc của các con

    private float timer = 0f;
    private bool isAnimating = false;

    void Awake()
    {
        int childCount = transform.childCount;
        childTransforms = new Transform[childCount];
        targetLocalPositions = new Vector3[childCount];
        targetScales = new Vector3[childCount];

        for (int i = 0; i < childCount; i++)
        {
            Transform child = transform.GetChild(i);
            childTransforms[i] = child;
            targetLocalPositions[i] = child.localPosition;
            targetScales[i] = child.localScale;
        }
    }

    public void TriggerSkill()
    {
        // Dừng coroutine ẩn trước đó nếu có
        StopAllCoroutines();

        // Bật GameObject cha lên
        gameObject.SetActive(true);

        for (int i = 0; i < childTransforms.Length; i++)
        {
            if (childTransforms[i] == null) continue;

            // Bật từng con lên
            childTransforms[i].gameObject.SetActive(true);

            // THIẾT LẬP TRẠNG THÁI BAN ĐẦU (Biến mất / Lệch lên trên)
            // Dịch vị trí Y ban đầu cao hơn vị trí gốc một đoạn bằng đúng chiều cao của nó (hoặc tùy chỉnh hệ số)
            Vector3 startPos = targetLocalPositions[i];
            startPos.y += targetScales[i].y; // Đẩy điểm xuất phát dịch lên trên theo chiều cao tia sét
            childTransforms[i].localPosition = startPos;

            // Scale Y ban đầu bằng 0 (chưa xuất hiện)
            childTransforms[i].localScale = new Vector3(targetScales[i].x, 0f, targetScales[i].z);
        }

        // Bắt đầu chạy hiệu ứng
        timer = 0f;
        isAnimating = true;
    }

    void Update()
    {
        if (!isAnimating) return;

        timer += Time.deltaTime;
        float progress = Mathf.Clamp01(timer / duration); // Chạy từ 0 đến 1

        // Nội suy đồng loạt vị trí và kích thước của các tia sét con
        for (int i = 0; i < childTransforms.Length; i++)
        {
            if (childTransforms[i] == null) continue;

            // 1. Tính toán vị trí xuất phát lệch lên trên so với vị trí đích
            Vector3 startPos = targetLocalPositions[i];
            startPos.y += targetScales[i].y;

            // Dịch chuyển vị trí từ trên xuống vị trí đích gốc
            childTransforms[i].localPosition = Vector3.Lerp(startPos, targetLocalPositions[i], progress);

            // 2. Kéo dài kích thước Y từ 0 lên kích thước gốc để hiện toàn bộ
            float currentY = Mathf.Lerp(0f, targetScales[i].y, progress);
            childTransforms[i].localScale = new Vector3(targetScales[i].x, currentY, targetScales[i].z);
        }

        // Khi hiệu ứng chạy xong
        if (progress >= 1f)
        {
            isAnimating = false;

            // Đảm bảo các con khớp chính xác 100% vị trí và scale gốc
            for (int i = 0; i < childTransforms.Length; i++)
            {
                if (childTransforms[i] != null)
                {
                    childTransforms[i].localPosition = targetLocalPositions[i];
                    childTransforms[i].localScale = targetScales[i];
                }
            }

            // Chờ một chút rồi ẩn đi để tái sử dụng hoặc dùng Destroy(gameObject) nếu muốn xóa luôn
            StartCoroutine(HideAfterDelay());
        }
    }

    IEnumerator HideAfterDelay()
    {
        yield return new WaitForSeconds(hideDelay);

        foreach (Transform child in childTransforms)
        {
            if (child != null) child.gameObject.SetActive(false);
        }
        gameObject.SetActive(false);
    }
}
