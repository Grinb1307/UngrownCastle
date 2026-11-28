using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UngrownCastle.Enemies;

namespace UngrownCastle.Wave
{
    [Serializable]
    public class WaveSpawnItem
    {
        public GameObject enemyPrefab;
        public int count = 5;
        public float spawnInterval = 1.2f;
    }

    [Serializable]
    public class WaveData
    {
        public string waveName = "Wave 1";
        public List<WaveSpawnItem> spawnItems = new List<WaveSpawnItem>();
        public float delayBeforeNextWave = 5f;
    }

    public class WaveSpawner : MonoBehaviour
    {
        [Header("Spawn Settings")]
        [SerializeField] private Transform spawnPoint;
        [SerializeField] private float defaultSpawnX = 10f;
        [SerializeField] private float groundY = 0f;
        [SerializeField] private float flyingY = 1.5f;

        [Header("Waves Configuration")]
        [SerializeField] private List<WaveData> waves = new List<WaveData>();
        [SerializeField] private bool autoStartNextWave = false;

        [Header("Enemy Prefab Library")]
        [SerializeField] private GameObject slimePrefab;
        [SerializeField] private GameObject zombiePrefab;
        [SerializeField] private GameObject koboldPrefab;
        [SerializeField] private GameObject cannibalPrefab;
        [SerializeField] private GameObject orkPrefab;
        [SerializeField] private GameObject trollPrefab;
        [SerializeField] private GameObject harpyPrefab;
        [SerializeField] private GameObject taurenPrefab;

        private int currentWaveIndex = 0;
        private bool isWaveRunning = false;
        private List<GameObject> activeEnemies = new List<GameObject>();

        public int CurrentWave => currentWaveIndex + 1;
        public bool IsWaveRunning => isWaveRunning;

        private void Start()
        {
            // Auto configure default waves if list is empty
            if (waves == null || waves.Count == 0)
            {
                SetupDefaultWaves();
            }

            // Auto-start wave 1 if enabled
            if (autoStartNextWave && waves.Count > 0)
            {
                StartNextWave();
            }
        }

        private void SetupDefaultWaves()
        {
            waves = new List<WaveData>();

            // Wave 1: Slimes
            if (slimePrefab != null)
            {
                WaveData w1 = new WaveData { waveName = "Wave 1 - Slime Invasion" };
                w1.spawnItems.Add(new WaveSpawnItem { enemyPrefab = slimePrefab, count = 6, spawnInterval = 1.2f });
                waves.Add(w1);
            }

            // Wave 2: Slimes + Zombies
            if (zombiePrefab != null)
            {
                WaveData w2 = new WaveData { waveName = "Wave 2 - The Walking Dead" };
                if (slimePrefab != null) w2.spawnItems.Add(new WaveSpawnItem { enemyPrefab = slimePrefab, count = 4, spawnInterval = 1.0f });
                w2.spawnItems.Add(new WaveSpawnItem { enemyPrefab = zombiePrefab, count = 5, spawnInterval = 1.2f });
                waves.Add(w2);
            }

            // Wave 3: Zombies + Kobolds
            if (koboldPrefab != null)
            {
                WaveData w3 = new WaveData { waveName = "Wave 3 - Kobold Patrol" };
                if (zombiePrefab != null) w3.spawnItems.Add(new WaveSpawnItem { enemyPrefab = zombiePrefab, count = 5, spawnInterval = 1.0f });
                w3.spawnItems.Add(new WaveSpawnItem { enemyPrefab = koboldPrefab, count = 4, spawnInterval = 1.2f });
                waves.Add(w3);
            }

            // Wave 4: Orks & Trolls
            if (orkPrefab != null)
            {
                WaveData w4 = new WaveData { waveName = "Wave 4 - Ork Vanguard" };
                w4.spawnItems.Add(new WaveSpawnItem { enemyPrefab = orkPrefab, count = 6, spawnInterval = 1.0f });
                if (trollPrefab != null) w4.spawnItems.Add(new WaveSpawnItem { enemyPrefab = trollPrefab, count = 2, spawnInterval = 2.0f });
                waves.Add(w4);
            }

            // Wave 5: Harpies (Fly) & Boss Tauren
            if (taurenPrefab != null)
            {
                WaveData w5 = new WaveData { waveName = "Wave 5 - Tauren Chieftain" };
                if (harpyPrefab != null) w5.spawnItems.Add(new WaveSpawnItem { enemyPrefab = harpyPrefab, count = 4, spawnInterval = 1.5f });
                w5.spawnItems.Add(new WaveSpawnItem { enemyPrefab = taurenPrefab, count = 1, spawnInterval = 1.0f });
                waves.Add(w5);
            }
        }

        [ContextMenu("Start Next Wave")]
        public void StartNextWave()
        {
            if (isWaveRunning)
            {
                Debug.LogWarning("Wave is already running!");
                return;
            }

            if (currentWaveIndex >= waves.Count)
            {
                Debug.Log("All waves completed!");
                return;
            }

            StartCoroutine(SpawnWaveRoutine(waves[currentWaveIndex]));
        }

        private IEnumerator SpawnWaveRoutine(WaveData wave)
        {
            isWaveRunning = true;
            Debug.Log($"=== Starting {wave.waveName} ===");

            foreach (var item in wave.spawnItems)
            {
                if (item.enemyPrefab == null) continue;

                for (int i = 0; i < item.count; i++)
                {
                    SpawnEnemy(item.enemyPrefab);
                    yield return new WaitForSeconds(item.spawnInterval);
                }
            }

            // Wait until all enemies in wave are killed
            while (activeEnemies.Count > 0)
            {
                activeEnemies.RemoveAll(e => e == null);
                yield return new WaitForSeconds(0.5f);
            }

            Debug.Log($"=== {wave.waveName} CLEARED! ===");
            isWaveRunning = false;
            currentWaveIndex++;

            if (autoStartNextWave && currentWaveIndex < waves.Count)
            {
                yield return new WaitForSeconds(wave.delayBeforeNextWave);
                StartNextWave();
            }
        }

        private void SpawnEnemy(GameObject prefab)
        {
            // Base X from spawnPoint (or defaultSpawnX fallback)
            float baseX = spawnPoint != null ? spawnPoint.position.x : defaultSpawnX;

            // Check if flying enemy
            bool isFlying = false;
            Enemy enemyComp = prefab.GetComponent<Enemy>();
            if (enemyComp != null)
            {
                isFlying = enemyComp.IsFlying;
            }

            // Y: flying enemies go higher, ground enemies use groundY
            float spawnY = isFlying ? flyingY : groundY;

            // Slight X variation so enemies don't all stack at same X
            float offsetX = UnityEngine.Random.Range(-0.15f, 0.15f);

            Vector3 pos = new Vector3(baseX + offsetX, spawnY, 0f);

            GameObject enemy = Instantiate(prefab, pos, Quaternion.identity);

            activeEnemies.Add(enemy);
            Debug.Log($"[WaveSpawner] Spawned {prefab.name} at {pos}");
        }
    }
}
