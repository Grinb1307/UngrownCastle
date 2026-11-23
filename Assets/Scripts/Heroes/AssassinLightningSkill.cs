using System.Collections;
using UnityEngine;

[RequireComponent(typeof(Collider2D))] // Bắt buộc phải có Collider2D để nhận click chuột vào nhân vật
public class AssassinLightningSkill : MonoBehaviour
{
    [Header("Cài đặt Tia điện")]
    public GameObject lightningPrefab;   // Kéo Prefab tia điện vào đây
    public Transform spawnPoint;         // Điểm xuất phát tia điện (có thể kéo object Knife hoặc tạo 1 Empty Object ở đầu mũi dao)

    [Header("Thông số phóng và thu nhỏ")]
    public Vector3 maxScale = new Vector3(1.5f, 1f, 1f); // Kích thước tối đa khi phóng ra
    public float expandDuration = 0.08f; // Thời gian phóng tia điện đạt cực đại (rất nhanh)
    public float shrinkDuration = 0.25f; // Thời gian tia điện thu nhỏ dần rồi biến mất

    private bool isCasting = false;

    void Start()
    {
        // Nếu chưa gán điểm xuất phát, mặc định lấy vị trí của Knife hoặc chính Assasin
        if (spawnPoint == null)
        {
            Transform knife = transform.Find("Knife");
            spawnPoint = knife != null ? knife : transform;
        }
    }

    // Hàm tự động của Unity: Kích hoạt khi click chuột trái trúng vùng Collider2D của Assasin
    void OnMouseDown()
    {
        if (!isCasting && lightningPrefab != null)
        {
            StartCoroutine(CastLightningRoutine());
        }
    }

    IEnumerator CastLightningRoutine()
    {
        isCasting = true;

        // 1. Tạo tia điện tại điểm xuất phát với kích thước ban đầu bằng 0
        GameObject lightning = Instantiate(lightningPrefab, spawnPoint.position, spawnPoint.rotation);
        lightning.transform.localScale = Vector3.zero;

        // 2. Phóng tia điện ra kích thước tối đa (Expand)
        float elapsed = 0f;
        while (elapsed < expandDuration)
        {
            elapsed += Time.deltaTime;
            float t = elapsed / expandDuration;
            lightning.transform.localScale = Vector3.Lerp(Vector3.zero, maxScale, t);
            yield return null;
        }
        lightning.transform.localScale = maxScale;

        // 3. Thu nhỏ tia điện dần về 0 (Shrink)
        elapsed = 0f;
        while (elapsed < shrinkDuration)
        {
            elapsed += Time.deltaTime;
            float t = elapsed / shrinkDuration;
            // Thu nhỏ mượt mà từ maxScale về Vector3.zero
            lightning.transform.localScale = Vector3.Lerp(maxScale, Vector3.zero, t);
            yield return null;
        }

        // 4. Xóa hẳn tia điện khỏi Scene khi đã thu nhỏ xong
        Destroy(lightning);
        isCasting = false;
    }
}