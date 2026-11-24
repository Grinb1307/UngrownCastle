using UnityEngine;
using System.Collections;

public class LightningMage : MonoBehaviour
{
    // Start is called once before the first execution of Update after the MonoBehaviour is created
    private bool isAttacking = false;

    public Transform Stick;

    public GameObject LightningBall;

    private Vector3 idleLocalPos = new Vector3(-0.21f, 0.09f, 0.06f);

    private Vector3 idleEulerAngles = new Vector3(0f, 0f, 0f);

    private Vector3 isAttackingPos = new Vector3(-0.435f, -0.0025f, 0.06f);

    private Vector3 isAttackingEulerAngles = new Vector3(0f, 0f, -100f);

    public float StickEndZ = -100f;     // Góc Z cuối cùng khi vung dao chém xuống
    public float StickDuration = 0.15f; // Thời gian vung dao (giây)
    public float recoverDelay = 0.1f;   // Thời gian dừng nhẹ sau khi chém trước khi thu dao

    void Start()
    {
        Application.targetFrameRate = 60;
        Stick = transform.Find("Stick");
        if (Stick != null)
        {
            // Đặt đúng thông số ban đầu như Ảnh 1
            Stick.localPosition = idleLocalPos;
            Stick.localRotation = Quaternion.Euler(idleEulerAngles);
        }
    }

    // Update is called once per frame
    void Update()
    {
        if (Input.GetMouseButtonDown(0) && !isAttacking && Stick != null)
        {
            StartCoroutine(AttackRoutine());
        }
    }

    IEnumerator AttackRoutine()
    {
        isAttacking = true;
        // 1. Đưa dao vào điểm bắt đầu tấn công (Ảnh 2)
        Stick.localPosition = isAttackingPos;
        Quaternion startSlashRot = Quaternion.Euler(isAttackingEulerAngles);
        Quaternion endSlashRot = Quaternion.Euler(isAttackingEulerAngles.x, isAttackingEulerAngles.y, StickEndZ);
        Stick.localRotation = startSlashRot;

        // 2. Vung dao chém xuống mượt mà bằng Quaternion.Slerp
        float elapsed = 0f;
        while (elapsed < StickDuration)
        {
            elapsed += Time.deltaTime;
            Stick.localRotation = Quaternion.Slerp(startSlashRot, endSlashRot, elapsed / StickDuration);
            yield return null;
        }
        Stick.localRotation = endSlashRot;

        // Tạo quả cầu LightningBall tại vị trí cuối cùng của dao
        Vector3 lightningBallPosition = Stick.position;
        Quaternion lightningBallRotation = Quaternion.Euler(0f, 0f, 0f);
        GameObject lightningBall = Instantiate(LightningBall, lightningBallPosition, lightningBallRotation);
        LightningBallScript ballScript = lightningBall.GetComponent<LightningBallScript>();
        if (ballScript != null)
        {
            ballScript.Initialize(lightningBallPosition, 45f, 5f);
        }

        // Chờ một nhịp ngắn ở cuối đòn chém
        yield return new WaitForSeconds(recoverDelay);

        // 3. Thu dao về lại vị trí nghỉ ban đầu (Ảnh 1)
        Stick.localPosition = idleLocalPos;
        Stick.localRotation = Quaternion.Euler(idleEulerAngles);
        isAttacking = false;
    }
}
