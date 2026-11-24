using System.Collections;
using UnityEngine;

public class SuccubusUltimate : MonoBehaviour
{
    [Header("Tham chiếu các Object con")]
    public Transform ultimateTransform;  // Kéo object 'Ultimate' vào đây
    public Transform heartTransform;     // Kéo object 'Heart' vào đây
    public Transform bodyTransform;      // Kéo object 'body' vào đây (để lấy chuẩn vị trí)

    [Header("Cài đặt thời gian và hiệu ứng")]
    public float ultimateCooldown = 6f;  // Cứ 6 giây kích hoạt 1 lần
    public float ultimateDuration = 0.3f;// Thời gian Ultimate mọc dài từ dưới lên
    public float heartDuration = 0.25f;  // Thời gian Heart bung to ra
    public float holdDuration = 0.5f;    // Thời gian giữ cả 2 hiển thị trước khi tắt hoàn toàn
    public float dropOffset = 1.0f;      // Khoảng cách thụt xuống bên dưới body để mọc lên

    private Vector3 ultimateTargetPos;
    private Vector3 ultimateTargetScale;
    private Vector3 heartTargetScale;
    private float timer = 0f;

    void Awake()
    {
        if (ultimateTransform == null) ultimateTransform = transform.Find("Ultimate");
        if (heartTransform == null) heartTransform = transform.Find("Heart");
        if (bodyTransform == null) bodyTransform = transform.Find("body");

        if (ultimateTransform != null)
        {
            ultimateTargetPos = ultimateTransform.localPosition;
            ultimateTargetScale = ultimateTransform.localScale;
            ultimateTransform.gameObject.SetActive(false);
        }

        if (heartTransform != null)
        {
            heartTargetScale = heartTransform.localScale;
            heartTransform.gameObject.SetActive(false);
        }
    }

    void Update()
    {
        timer += Time.deltaTime;
        if (timer >= ultimateCooldown)
        {
            timer = 0f;
            TriggerUltimateSequence();
        }
    }

    void TriggerUltimateSequence()
    {
        StopAllCoroutines();
        StartCoroutine(RunUltimateAnimation());
    }

    IEnumerator RunUltimateAnimation()
    {
        // ==========================================
        // BƯỚC 1: ULTIMATE MỌC TỪ DƯỚI LÊN
        // ==========================================
        if (ultimateTransform != null)
        {
            ultimateTransform.gameObject.SetActive(true);

            // Đặt vị trí xuất phát thấp hơn bên dưới (tạo cảm giác từ phía sau/dưới trồi lên)
            Vector3 startPos = ultimateTargetPos;
            startPos.y -= dropOffset;
            ultimateTransform.localPosition = startPos;

            // Scale Y ban đầu bằng 0
            ultimateTransform.localScale = new Vector3(ultimateTargetScale.x, 0f, ultimateTargetScale.z);

            float elapsed = 0f;
            while (elapsed < ultimateDuration)
            {
                elapsed += Time.deltaTime;
                float progress = Mathf.Clamp01(elapsed / ultimateDuration);

                // Nội suy vị trí trồi lên và kéo dài trục Y
                ultimateTransform.localPosition = Vector3.Lerp(startPos, ultimateTargetPos, progress);
                float currentY = Mathf.Lerp(0f, ultimateTargetScale.y, progress);
                ultimateTransform.localScale = new Vector3(ultimateTargetScale.x, currentY, ultimateTargetScale.z);

                yield return null;
            }

            // Đảm bảo khớp chuẩn 100% khi kết thúc bước 1
            ultimateTransform.localPosition = ultimateTargetPos;
            ultimateTransform.localScale = ultimateTargetScale;
        }

        // ==========================================
        // BƯỚC 2: HEART BUNG TO DẦN TẠI VỊ TRÍ ĐÓ
        // ==========================================
        if (heartTransform != null)
        {
            heartTransform.gameObject.SetActive(true);

            // Đặt Heart nằm đúng vị trí của Ultimate và scale ban đầu bằng 0 (biến mất)
            heartTransform.position = ultimateTransform.position;
            heartTransform.localScale = Vector3.zero;

            float elapsed = 0f;
            while (elapsed < heartDuration)
            {
                elapsed += Time.deltaTime;
                float progress = Mathf.Clamp01(elapsed / heartDuration);

                // Phóng to dần Heart từ 0 đến kích thước chuẩn
                heartTransform.localScale = Vector3.Lerp(Vector3.zero, heartTargetScale, progress);

                yield return null;
            }

            heartTransform.localScale = heartTargetScale;
        }

        // ==========================================
        // BƯỚC 3: GIỮ HIỆN THỊ TRONG CHỐC LÁT RỒI TẮT
        // ==========================================
        yield return new WaitForSeconds(holdDuration);

        if (ultimateTransform != null) ultimateTransform.gameObject.SetActive(false);
        if (heartTransform != null) heartTransform.gameObject.SetActive(false);
    }
}