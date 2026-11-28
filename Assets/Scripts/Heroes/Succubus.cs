using System.Collections;
using UnityEngine;

public class Succubus : MonoBehaviour
{
    [Header("Tham chiếu Object con")]
    public Transform clawTransform;        // Kéo object 'skill' vào đây
    public Transform bodyTransform;        // Kéo object 'body' vào đây

    [Header("Thông số đánh thường")]
    public float attackInterval = 1.5f;    // Thời gian hồi giữa mỗi lần cào
    public float slashDuration = 0.18f;    // Thời gian thực hiện 1 nét cào
    public float slashDistance = 0.6f;     // Quãng đường trượt từ trên-trái xuống dưới-phải
    public float slashAngle = 70f;         // Góc nghiêng chéo bên trái 70 độ

    private Vector3 baseScale;
    private Vector3 startWorldPos;
    private Vector3 endWorldPos;
    private float attackTimer = 0f;
    private float animTimer = 0f;
    private bool isSlashing = false;

    void Awake()
    {
        Application.targetFrameRate = 60;
        if (clawTransform == null) clawTransform = transform.Find("skill");
        if (bodyTransform == null) bodyTransform = transform.Find("body");

        if (clawTransform != null)
        {
            baseScale = clawTransform.localScale;
            clawTransform.gameObject.SetActive(false);
        }
    }

    void Update()
    {
        if (clawTransform == null || bodyTransform == null) return;

        // 1. Đếm nhịp tự động đánh thường
        if (!isSlashing)
        {
            attackTimer += Time.deltaTime;
            if (attackTimer >= attackInterval)
            {
                attackTimer = 0f;
                PerformClawAttack();
            }
        }
        // 2. Diễn hoạt nét cào chéo từ trên-trái xuống dưới-phải và nhỏ dần theo chiều dọc
        else
        {
            animTimer += Time.deltaTime;
            float progress = Mathf.Clamp01(animTimer / slashDuration);

            // Trượt vị trí dọc theo góc nghiêng 70 độ (từ trái-trên sang phải-dưới)
            clawTransform.position = Vector3.Lerp(startWorldPos, endWorldPos, progress);

            // Làm nhỏ dần theo chiều dọc (trục Y giảm từ kích thước gốc về 0)
            float currentScaleY = Mathf.Lerp(baseScale.y, 0f, progress);
            clawTransform.localScale = new Vector3(baseScale.x, currentScaleY, baseScale.z);

            // Kết thúc nét cào
            if (progress >= 1f)
            {
                isSlashing = false;
                clawTransform.gameObject.SetActive(false);
            }
        }
    }

    void PerformClawAttack()
    {
        // Bật vết cào và xoay góc chéo bên trái 70 độ
        clawTransform.gameObject.SetActive(true);
        clawTransform.localRotation = Quaternion.Euler(0f, 0f, slashAngle);

        // Lấy tâm của 'body' làm điểm giữa của vết cào
        Vector3 centerPos = bodyTransform.position;

        // Tính hướng trượt theo góc xoay hiện tại của vết cào (từ trên-trái xuống dưới-phải)
        Vector3 slashDirection = -clawTransform.up;

        // Nếu sprite của bạn nằm ngang thay vì thẳng đứng, hãy đổi dòng trên thành:
        // Vector3 slashDirection = clawTransform.right;

        startWorldPos = centerPos - slashDirection * (slashDistance * 0.5f);
        endWorldPos = centerPos + slashDirection * (slashDistance * 0.5f);

        // Đặt trạng thái khởi đầu
        clawTransform.position = startWorldPos;
        clawTransform.localScale = baseScale;

        animTimer = 0f;
        isSlashing = true;
    }
}