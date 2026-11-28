using System;
using System.Collections;
using System.Collections.Generic;

using UnityEngine;
using UngrownCastle.Enemies;

public class LightningMageUltimate : MonoBehaviour
{
    [Header("Thunderstorm Settings")]
    public GameObject lightningBoltPrefab;
    public int strikeCount = 6;
    public float strikeInterval = 0.08f;
    public float stormRadius = 3.5f;
    public float strikeHitRadius = 1.2f;
    public float strikeDamage = 35f;
    public float boltStrikeDuration = 0.12f;
    public float boltFadeDuration = 0.18f;
    public float boltHeightOffset = 1.2f;
    public float cooldownTime = 6f;

    private Coroutine stormCoroutine;

    public void CastThunderstorm(Vector2 stormCenter, LayerMask enemyLayer, Action onComplete = null)
    {
        if (stormCoroutine != null)
        {
            StopCoroutine(stormCoroutine);
        }
        stormCoroutine = StartCoroutine(ThunderstormRoutine(stormCenter, enemyLayer, onComplete));
    }

    private IEnumerator ThunderstormRoutine(Vector2 stormCenter, LayerMask enemyLayer, Action onComplete)
    {
        // Gom toàn bộ Enemy đang nằm trong vòng mục tiêu và gây sát thương chắc chắn lên tất cả
        Collider2D[] hits = Physics2D.OverlapCircleAll(stormCenter, stormRadius, enemyLayer);
        List<Enemy> enemiesInCircle = new List<Enemy>();
        HashSet<Enemy> seen = new HashSet<Enemy>();

        foreach (Collider2D hit in hits)
        {
            if (hit == null) continue;
            Enemy enemy = hit.GetComponentInParent<Enemy>();
            if (enemy == null || enemy.IsDead || seen.Contains(enemy)) continue;
            seen.Add(enemy);
            enemiesInCircle.Add(enemy);
        }

        // Tất cả Enemy nằm trong vòng đều bị sét đánh trúng
        foreach (Enemy enemy in enemiesInCircle)
        {
            if (enemy != null && !enemy.IsDead)
            {
                enemy.TakeDamage(strikeDamage);
            }
        }

        int totalBolts = Mathf.Max(strikeCount, 1);
        for (int i = 0; i < totalBolts; i++)
        {
            Vector2 strikePos;
            if (enemiesInCircle.Count > 0)
            {
                Enemy targetEnemy = enemiesInCircle[i % enemiesInCircle.Count];
                Vector2 baseEnemyPos = targetEnemy != null ? (Vector2)targetEnemy.transform.position : stormCenter;
                Vector2 jitter = i >= enemiesInCircle.Count ? UnityEngine.Random.insideUnitCircle * 0.15f : Vector2.zero;
                strikePos = baseEnemyPos + jitter;
            }
            else
            {
                strikePos = stormCenter + UnityEngine.Random.insideUnitCircle * (stormRadius * 0.75f);
            }

            StartCoroutine(SpawnAndAnimateBolt(strikePos));
            yield return new WaitForSeconds(strikeInterval);
        }

        // Chờ tia sét cuối cùng chạy xong animation đánh xuống + mờ dần rồi mới tắt nền tối
        yield return new WaitForSeconds(boltStrikeDuration + boltFadeDuration);

        stormCoroutine = null;
        onComplete?.Invoke();
    }

    private IEnumerator SpawnAndAnimateBolt(Vector2 strikePos)
    {
        GameObject boltObj;

        if (lightningBoltPrefab != null)
        {
            boltObj = Instantiate(
                lightningBoltPrefab,
                new Vector3(strikePos.x, strikePos.y + boltHeightOffset, 0f),
                Quaternion.identity
            );
        }
        else
        {
            boltObj = CreateFallbackBolt(strikePos);
        }

        if (boltObj == null) yield break;

        SpriteRenderer[] renderers = boltObj.GetComponentsInChildren<SpriteRenderer>(true);
        foreach (SpriteRenderer sr in renderers)
        {
            if (sr != null && sr.sortingOrder < 11)
            {
                sr.sortingOrder = 11;
            }
        }

        Vector3 targetScale = boltObj.transform.localScale;
        Vector3 endPos = new Vector3(strikePos.x, strikePos.y + boltHeightOffset, 0f);
        Vector3 startPos = endPos + new Vector3(0f, targetScale.y * 1.2f, 0f);

        boltObj.transform.position = startPos;
        boltObj.transform.localScale = new Vector3(targetScale.x, 0f, targetScale.z);

        // 1. Tia sét vàng giáng nhanh từ trên trời xuống
        float elapsed = 0f;
        while (elapsed < boltStrikeDuration)
        {
            if (boltObj == null) yield break;
            elapsed += Time.deltaTime;
            float t = Mathf.Clamp01(elapsed / Mathf.Max(boltStrikeDuration, 0.001f));
            boltObj.transform.position = Vector3.Lerp(startPos, endPos, t);
            boltObj.transform.localScale = new Vector3(targetScale.x, Mathf.Lerp(0f, targetScale.y, t), targetScale.z);
            yield return null;
        }

        if (boltObj == null) yield break;
        boltObj.transform.position = endPos;
        boltObj.transform.localScale = targetScale;

        // 2. Mờ dần rồi biến mất
        Color[] baseColors = new Color[renderers.Length];
        for (int i = 0; i < renderers.Length; i++)
        {
            baseColors[i] = renderers[i] != null ? renderers[i].color : Color.white;
        }

        elapsed = 0f;
        while (elapsed < boltFadeDuration)
        {
            if (boltObj == null) yield break;
            elapsed += Time.deltaTime;
            float alpha = Mathf.Lerp(1f, 0f, elapsed / Mathf.Max(boltFadeDuration, 0.001f));
            for (int i = 0; i < renderers.Length; i++)
            {
                if (renderers[i] != null)
                {
                    Color c = baseColors[i];
                    c.a = baseColors[i].a * alpha;
                    renderers[i].color = c;
                }
            }
            yield return null;
        }

        if (boltObj != null)
        {
            Destroy(boltObj);
        }
    }

    private GameObject CreateFallbackBolt(Vector2 strikePos)
    {
        GameObject obj = new GameObject("YellowLightningBolt");
        obj.transform.position = new Vector3(strikePos.x, strikePos.y + boltHeightOffset, 0f);
        obj.transform.localScale = new Vector3(0.4f, 2.8f, 1f);

        Texture2D tex = Texture2D.whiteTexture;
        Sprite sprite = Sprite.Create(tex, new Rect(0, 0, tex.width, tex.height), new Vector2(0.5f, 0.5f), 4f);
        SpriteRenderer sr = obj.AddComponent<SpriteRenderer>();
        sr.sprite = sprite;
        sr.color = new Color(1f, 0.92f, 0.15f, 1f);
        sr.sortingOrder = 11;
        return obj;
    }
}
