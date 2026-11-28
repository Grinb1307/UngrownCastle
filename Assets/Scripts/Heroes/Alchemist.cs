using System.Collections;
using UnityEngine;

public class Alchemist : MonoBehaviour
{
    [Header("Tham chiếu các Object con")]
    public Transform firstExplosion;   // Kéo object 'FirstExplosion' vào đây
    public Transform secondExplosion;  // Kéo object 'SecondExplosion' vào đây

    [Header("Cài đặt vị trí xuất hiện")]
    public float spawnOffsetX = 2.0f;  // Khoảng cách theo chiều ngang so với Alchemist
    public float spawnOffsetY = -0.5f; // Thấp hơn một chút so với Alchemist

    [Header("Cài đặt hiệu ứng & Hồi chiêu")]
    public float growDuration = 0.3f;    // Thời gian phình to từ nhỏ lên kích thước chuẩn
    public float holdDuration = 0.5f;    // Thời gian giữ vụ nổ hiển thị
    public float delayBetween = 0.15f;   // Độ trễ giữa 2 vụ nổ
    public float cooldownTime = 6f;      // Thời gian hồi chiêu (6 giây)

    private Vector3 firstTargetScale;
    private Vector3 secondTargetScale;

    private bool isReadyToUse = true;    // Mặc định ban đầu vừa vào game là bấm xài được luôn
    private bool isExecuting = false;

    void Awake()
    {
        Application.targetFrameRate = 60;
        if (firstExplosion == null) firstExplosion = transform.Find("FirstExplosion");
        if (secondExplosion == null) secondExplosion = transform.Find("SecondExplosion");

        if (firstExplosion != null)
        {
            firstTargetScale = firstExplosion.localScale;
            firstExplosion.gameObject.SetActive(false);
        }

        if (secondExplosion != null)
        {
            secondTargetScale = secondExplosion.localScale;
            secondExplosion.gameObject.SetActive(false);
        }
    }

    // ==========================================
    // SỰ KIỆN KHI NGƯỜI CHƠI NHẤN VÀO ALCHEMIST
    // ==========================================
    void OnMouseDown()
    {
        Debug.Log("Oke");
        // Nếu skill chưa hồi xong (đang cooldown) hoặc đang chạy hiệu ứng thì không làm gì cả
        if (!isReadyToUse || isExecuting) return;

        StartCoroutine(TriggerExplosionUltimateSequence());
    }

    IEnumerator TriggerExplosionUltimateSequence()
    {
        isExecuting = true;
        isReadyToUse = false; // Đánh dấu là skill đang trong trạng thái chờ hồi chiêu

        // Xác định vị trí xuất hiện của 2 vụ nổ
        Vector3 basePos = transform.position;
        Vector3 targetPos1 = new Vector3(basePos.x + spawnOffsetX, basePos.y + spawnOffsetY, basePos.z);
        Vector3 targetPos2 = new Vector3(basePos.x + spawnOffsetX + 0.8f, basePos.y + spawnOffsetY - 0.2f, basePos.z);

        // 1. Kích hoạt First Explosion
        if (firstExplosion != null)
        {
            firstExplosion.position = targetPos1;
            firstExplosion.gameObject.SetActive(true);
            StartCoroutine(ScaleAndFadeExplosion(firstExplosion, firstTargetScale));
        }

        yield return new WaitForSeconds(delayBetween);

        // 2. Kích hoạt Second Explosion
        if (secondExplosion != null)
        {
            secondExplosion.position = targetPos2;
            secondExplosion.gameObject.SetActive(true);
            StartCoroutine(ScaleAndFadeExplosion(secondExplosion, secondTargetScale));
        }

        // Chờ cho hiệu ứng nổ chạy xong hoàn toàn
        yield return new WaitForSeconds(growDuration + holdDuration);
        isExecuting = false;

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

    IEnumerator ScaleAndFadeExplosion(Transform explosion, Vector3 targetScale)
    {
        float elapsed = 0f;

        // 1. Lưu lại vị trí đích (vị trí gốc trên màn hình)
        Vector3 targetPos = explosion.position;

        // 2. Thiết lập trạng thái ban đầu: Đặt dịch xuống dưới đất một đoạn và scale về 0
        float dropOffset = 0.8f; // Khoảng cách trồi lên (có thể tăng giảm tùy ý)
        Vector3 startPos = targetPos;
        startPos.y -= dropOffset;

        explosion.position = startPos;
        explosion.localScale = Vector3.zero; // Bắt đầu với kích thước nhỏ nhất

        while (elapsed < growDuration)
        {
            elapsed += Time.deltaTime;
            float progress = Mathf.Clamp01(elapsed / growDuration);

            // Vừa dịch chuyển từ dưới trồi lên, vừa phình to kích thước
            explosion.position = Vector3.Lerp(startPos, targetPos, progress);
            explosion.localScale = Vector3.Lerp(Vector3.zero, targetScale, progress);

            yield return null;
        }

        // Đảm bảo khi kết thúc khớp chính xác 100% vị trí và scale gốc
        explosion.position = targetPos;
        explosion.localScale = targetScale;

        // Giữ hiển thị trong chốc lát rồi tắt đi
        yield return new WaitForSeconds(holdDuration);
        explosion.gameObject.SetActive(false);
    }
}