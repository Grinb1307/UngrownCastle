using System.Collections;
using System.Collections.Generic;
using UnityEngine;
public class Zeus : MonoBehaviour
{
    private bool isAttacking = false;

    public Transform Hammer;


    private Vector3 idleLocalPos = new Vector3(-1.44f, 0.121f, -0.002693f);

    private Vector3 idleEulerAngles = new Vector3(0f, 0f, -60f);

    private Vector3 isAttackingPos = new Vector3(-1.159f, 0.012f, -0.002693f);

    private Vector3 isAttackingEulerAngles = new Vector3(0f, 0f, -100f);

    public GameObject Skill;

    public float HammerEndZ = 160f;     // Góc Z cuối cùng khi vung dao chém xuống
    public float HammerDuration = 0.15f; // Thời gian vung dao (giây)
    public float recoverDelay = 0.1f;   // Thời gian dừng nhẹ sau khi chém trước khi thu dao
    // Start is called once before the first execution of Update after the MonoBehaviour is created

    public float count_timer = 0f;
    void Start()
    {
        Application.targetFrameRate = 60;
        Hammer = transform.Find("Hammer");
        if (Hammer != null)
        {
            // Đặt đúng thông số ban đầu như Ảnh 1
            Hammer.localPosition = idleLocalPos;
            Hammer.localRotation = Quaternion.Euler(idleEulerAngles);
        }
    }

    // Update is called once per frame
    void Update()
    {
        if (Input.GetMouseButtonDown(0) && !isAttacking && Hammer != null)
        {
            StartCoroutine(AttackRoutine());
        }
    }

    IEnumerator AttackRoutine()
    {
        isAttacking = true;
        // 1. Đưa dao vào điểm bắt đầu tấn công (Ảnh 2)
        Hammer.localPosition = isAttackingPos;
        Quaternion startSlashRot = Quaternion.Euler(isAttackingEulerAngles);
        Quaternion endSlashRot = Quaternion.Euler(isAttackingEulerAngles.x, isAttackingEulerAngles.y, HammerEndZ);
        Hammer.localRotation = startSlashRot;

        // 2. Vung dao chém xuống mượt mà bằng Quaternion.Slerp
        float elapsed = 0f;
        while (elapsed < HammerDuration)
        {
            elapsed += Time.deltaTime;
            Hammer.localRotation = Quaternion.Slerp(startSlashRot, endSlashRot, elapsed / HammerDuration);
            yield return null;
        }
        Hammer.localRotation = endSlashRot;


        if(count_timer < 2f)
        {
            ZeusSkill controller = Skill.GetComponent<ZeusSkill>();
            if (controller != null)
            {
                controller.TriggerSkill(); // Lúc này các con mới setactive(true) và chạy hiệu ứng
            }
            count_timer += Time.deltaTime;
        }
        count_timer = 0f; 
        // Chờ một nhịp ngắn ở cuối đòn chém
        yield return new WaitForSeconds(recoverDelay);

        // 3. Thu dao về lại vị trí nghỉ ban đầu (Ảnh 1)
        Hammer.localPosition = idleLocalPos;
        Hammer.localRotation = Quaternion.Euler(idleEulerAngles);
        isAttacking = false;
    }
}
