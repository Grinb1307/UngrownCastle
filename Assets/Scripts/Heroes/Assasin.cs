using System.Collections;
using UnityEngine;

public class AssassinKnifeAttack : MonoBehaviour
{
    private Transform knife;

    [Header("Thông số từ Inspector của Knife")]
    public Vector3 idleLocalPos = new Vector3(4.864f, 3.2f, 9.994007f);
    public Vector3 idleEulerAngles = new Vector3(0f, 0f, -3.18f);

    public Vector3 readyAttackLocalPos = new Vector3(4.864f, 3.3338f, 9.994007f);
    public Vector3 readyAttackEulerAngles = new Vector3(0f, -180f, -20f);

    [Header("Cài đặt cú chém")]
    public float slashEndZ = -110f;     // Góc Z cuối cùng khi vung dao chém xuống
    public float slashDuration = 0.15f; // Thời gian vung dao (giây)
    public float recoverDelay = 0.1f;   // Thời gian dừng nhẹ sau khi chém trước khi thu dao

    private bool isAttacking = false;

    void Start()
    {
        Application.targetFrameRate = 60;
        knife = transform.Find("Knife");
        if (knife != null)
        {
            // Đặt đúng thông số ban đầu như Ảnh 1
            knife.localPosition = idleLocalPos;
            knife.localRotation = Quaternion.Euler(idleEulerAngles);
        }
    }

    void Update()
    {
        if (Input.GetMouseButtonDown(0) && !isAttacking && knife != null)
        {
            StartCoroutine(AttackRoutine());
        }
    }

    IEnumerator AttackRoutine()
    {
        isAttacking = true;

        // 1. Đưa dao vào điểm bắt đầu tấn công (Ảnh 2)
        knife.localPosition = readyAttackLocalPos;
        Quaternion startSlashRot = Quaternion.Euler(readyAttackEulerAngles);
        Quaternion endSlashRot = Quaternion.Euler(readyAttackEulerAngles.x, readyAttackEulerAngles.y, slashEndZ);
        knife.localRotation = startSlashRot;

        // 2. Vung dao chém xuống mượt mà bằng Quaternion.Slerp
        float elapsed = 0f;
        while (elapsed < slashDuration)
        {
            elapsed += Time.deltaTime;
            knife.localRotation = Quaternion.Slerp(startSlashRot, endSlashRot, elapsed / slashDuration);
            yield return null;
        }
        knife.localRotation = endSlashRot;

        // Chờ một nhịp ngắn ở cuối đòn chém
        yield return new WaitForSeconds(recoverDelay);

        // 3. Thu dao về lại vị trí nghỉ ban đầu (Ảnh 1)
        knife.localPosition = idleLocalPos;
        knife.localRotation = Quaternion.Euler(idleEulerAngles);

        isAttacking = false;
    }
}