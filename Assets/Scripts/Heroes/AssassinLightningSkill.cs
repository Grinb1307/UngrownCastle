using System.Collections;
using UnityEngine;

public class AssassinLightningSkill : MonoBehaviour
{
    [Header("Cài đặt Tia điện")]
    public GameObject lightningPrefab;   // Kéo Prefab tia điện vào đây
    public Transform spawnPoint;         // Kéo object con SpawnPoint vào đây

    [Header("Thông số phóng và thu nhỏ")]
    public Vector3 maxScale = new Vector3(5f, 5f, 1f);
    public float expandDuration = 0.08f;
    public float shrinkDuration = 0.25f;
    [Header("Góc xoay bổ sung")]
    public float rotationOffsetZ = 315f; // Nhập số độ bạn muốn xoay ở đây

    // Tại hàm Instantiate trong CastLightningRoutine(), sửa thành:


    private bool isCasting = false;
    private Collider2D myCollider;

    void Start()
    {
        myCollider = GetComponent<Collider2D>();
        if (spawnPoint == null)
        {
            Transform knife = transform.Find("Knife");
            spawnPoint = knife != null ? knife : transform;
        }
    }

    void Update()
    {
        // Kiểm tra khi người chơi nhấn chuột trái
        if (Input.GetMouseButtonDown(0) && !isCasting && lightningPrefab != null)
        {
            // Bắn một tia Raycast từ vị trí chuột ngoài màn hình vào không gian 2D trong Scene
            Vector2 mousePos = Camera.main.ScreenToWorldPoint(Input.mousePosition);
            RaycastHit2D hit = Physics2D.Raycast(mousePos, Vector2.zero);

            // Nếu tia Raycast trúng đúng Collider của chính nhân vật này
            if (hit.collider != null && hit.collider == myCollider)
            {
                StartCoroutine(CastLightningRoutine());
            }
        }
    }

    IEnumerator CastLightningRoutine()
    {
        isCasting = true;

        // Ép tọa độ Z về 0 để chắc chắn tia sét không bị ẩn sau Camera
        Vector3 spawnPos = new Vector3(spawnPoint.position.x, spawnPoint.position.y, 0f);
        Quaternion customRot = spawnPoint.rotation * Quaternion.Euler(0, 0, rotationOffsetZ);
        GameObject lightning = Instantiate(lightningPrefab, spawnPos, customRot);
        lightning.transform.localScale = Vector3.zero;

        // Phóng to
        float elapsed = 0f;
        while (elapsed < expandDuration)
        {
            elapsed += Time.deltaTime;
            float t = elapsed / expandDuration;
            lightning.transform.localScale = Vector3.Lerp(Vector3.zero, maxScale, t);
            yield return null;
        }
        lightning.transform.localScale = maxScale;

        // Thu nhỏ
        elapsed = 0f;
        while (elapsed < shrinkDuration)
        {
            elapsed += Time.deltaTime;
            float t = elapsed / shrinkDuration;
            lightning.transform.localScale = Vector3.Lerp(maxScale, Vector3.zero, t);
            yield return null;
        }

        Destroy(lightning);
        isCasting = false;
    }
}