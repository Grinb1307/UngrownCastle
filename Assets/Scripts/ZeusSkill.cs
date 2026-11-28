using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ZeusSkill : MonoBehaviour
{
    [Header("Cài đặt hiệu ứng")]
    public float duration = 0.2f;      // Thời gian hoàn thành hiệu ứng sét đánh (giây)
    public float hideDelay = 0.3f;     // Thời gian lưu lại trên màn hình trước khi tắt
    public float boltYOffset = 1.0f;   // Độ lệch Y để chân tia sét chạm đúng vị trí Enemy

    private Transform[] childTransforms;
    private Vector3[] targetLocalPositions; // Lưu vị trí gốc của các con
    private Vector3[] targetScales;         // Lưu kích thước gốc của các con
    private Vector3[] animStartLocalPositions;
    private Vector3[] animEndLocalPositions;

    private float timer = 0f;
    private bool isAnimating = false;

    void Awake()
    {
        EnsureInitialized();
    }

    private void EnsureInitialized()
    {
        if (childTransforms != null) return;

        int childCount = transform.childCount;
        childTransforms = new Transform[childCount];
        targetLocalPositions = new Vector3[childCount];
        targetScales = new Vector3[childCount];
        animStartLocalPositions = new Vector3[childCount];
        animEndLocalPositions = new Vector3[childCount];

        for (int i = 0; i < childCount; i++)
        {
            Transform child = transform.GetChild(i);
            childTransforms[i] = child;
            targetLocalPositions[i] = child.localPosition;
            targetScales[i] = child.localScale;
            animEndLocalPositions[i] = targetLocalPositions[i];
            animStartLocalPositions[i] = targetLocalPositions[i] + new Vector3(0f, targetScales[i].y, 0f);
        }
    }

    public void TriggerSkill()
    {
        StopAllCoroutines();
        gameObject.SetActive(true);
        EnsureInitialized();

        if (childTransforms == null || childTransforms.Length == 0) return;

        for (int i = 0; i < childTransforms.Length; i++)
        {
            if (childTransforms[i] == null) continue;

            childTransforms[i].gameObject.SetActive(true);

            Vector3 endPos = targetLocalPositions[i];
            Vector3 startPos = endPos + new Vector3(0f, targetScales[i].y, 0f);

            animEndLocalPositions[i] = endPos;
            animStartLocalPositions[i] = startPos;

            childTransforms[i].localPosition = startPos;
            childTransforms[i].localScale = new Vector3(targetScales[i].x, 0f, targetScales[i].z);
        }

        timer = 0f;
        isAnimating = true;
    }

    public void TriggerSkillOnTargets(List<Transform> targets)
    {
        StopAllCoroutines();
        gameObject.SetActive(true);
        EnsureInitialized();

        if (childTransforms == null || childTransforms.Length == 0) return;

        for (int i = 0; i < childTransforms.Length; i++)
        {
            if (childTransforms[i] == null) continue;

            if (targets != null && i < targets.Count && targets[i] != null)
            {
                childTransforms[i].gameObject.SetActive(true);

                Vector3 worldTarget = new Vector3(
                    targets[i].position.x,
                    targets[i].position.y + boltYOffset,
                    childTransforms[i].position.z
                );
                Vector3 localEnd = transform.InverseTransformPoint(worldTarget);
                Vector3 localStart = localEnd + new Vector3(0f, targetScales[i].y, 0f);

                animEndLocalPositions[i] = localEnd;
                animStartLocalPositions[i] = localStart;

                childTransforms[i].localPosition = localStart;
                childTransforms[i].localScale = new Vector3(targetScales[i].x, 0f, targetScales[i].z);
            }
            else
            {
                childTransforms[i].gameObject.SetActive(false);
            }
        }

        timer = 0f;
        isAnimating = true;
    }

    void Update()
    {
        if (!isAnimating || childTransforms == null) return;

        timer += Time.deltaTime;
        float progress = Mathf.Clamp01(timer / Mathf.Max(duration, 0.001f));

        for (int i = 0; i < childTransforms.Length; i++)
        {
            if (childTransforms[i] == null || !childTransforms[i].gameObject.activeSelf) continue;

            childTransforms[i].localPosition = Vector3.Lerp(animStartLocalPositions[i], animEndLocalPositions[i], progress);

            float currentY = Mathf.Lerp(0f, targetScales[i].y, progress);
            childTransforms[i].localScale = new Vector3(targetScales[i].x, currentY, targetScales[i].z);
        }

        if (progress >= 1f)
        {
            isAnimating = false;

            for (int i = 0; i < childTransforms.Length; i++)
            {
                if (childTransforms[i] != null && childTransforms[i].gameObject.activeSelf)
                {
                    childTransforms[i].localPosition = animEndLocalPositions[i];
                    childTransforms[i].localScale = targetScales[i];
                }
            }

            StartCoroutine(HideAfterDelay());
        }
    }

    IEnumerator HideAfterDelay()
    {
        yield return new WaitForSeconds(hideDelay);

        if (childTransforms != null)
        {
            foreach (Transform child in childTransforms)
            {
                if (child != null) child.gameObject.SetActive(false);
            }
        }
        gameObject.SetActive(false);
    }
}
