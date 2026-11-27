using System.IO;
using UnityEditor;
using UnityEngine;
using UngrownCastle.Enemies;

namespace UngrownCastle.Editor
{
    public static class EnemyPrefabGenerator
    {
        private const string PrefabOutputDir = "Assets/Prefabs/Enemies";
        private const string SpriteBaseDir = "Assets/Sprites/Enemies";
        private const string CommonDir = "Assets/Sprites/Enemies/Common";

        [MenuItem("Tools/UngrownCastle/Generate All Enemy Prefabs (Full Limbs)")]

        public static void GenerateAllEnemies()
        {
            if (!Directory.Exists(PrefabOutputDir))
            {
                Directory.CreateDirectory(PrefabOutputDir);
            }

            try
            {
                AssetDatabase.StartAssetEditing();

                CreateSlimePrefab();
                CreateZombiePrefab();
                CreateKoboldPrefab();
                CreateCannibalPrefab();
                CreateOrkPrefab();
                CreateTrollPrefab();
                CreateHarpyPrefab();
                CreateTaurenPrefab();

                AssetDatabase.SaveAssets();
                AssetDatabase.Refresh();
            }
            finally
            {
                AssetDatabase.StopAssetEditing();
            }
        }

        private static Sprite LoadSprite(string path)
        {
            Sprite s = AssetDatabase.LoadAssetAtPath<Sprite>(path);
            if (s == null)
            {
                Texture2D tex = AssetDatabase.LoadAssetAtPath<Texture2D>(path);
                if (tex != null)
                {
                    s = Sprite.Create(tex, new Rect(0, 0, tex.width, tex.height), new Vector2(0.5f, 0.5f), 100f);
                }
            }
            return s;
        }

        private static GameObject CreateShadow(GameObject parent, float scale = 0.5f)
        {
            GameObject shadowObj = new GameObject("Shadow");
            shadowObj.transform.SetParent(parent.transform, false);
            shadowObj.transform.localPosition = new Vector3(0, -0.05f, 0);
            shadowObj.transform.localScale = new Vector3(scale, scale * 0.7f, 1f);

            SpriteRenderer sr = shadowObj.AddComponent<SpriteRenderer>();
            sr.sprite = LoadSprite($"{CommonDir}/Shadow.png");
            sr.color = new Color(0, 0, 0, 0.4f);
            sr.sortingOrder = 0;
            return shadowObj;
        }

        private static GameObject CreatePart(GameObject parent, string name, string spritePath, Vector3 localPos, int sortingOrder, float scale = 1f, Vector3 localRot = default)
        {
            GameObject partObj = new GameObject(name);
            partObj.transform.SetParent(parent.transform, false);
            partObj.transform.localPosition = localPos;
            partObj.transform.localScale = Vector3.one * scale;
            if (localRot != default)
            {
                partObj.transform.localRotation = Quaternion.Euler(localRot);
            }

            SpriteRenderer sr = partObj.AddComponent<SpriteRenderer>();
            sr.sprite = LoadSprite(spritePath);
            sr.sortingOrder = sortingOrder;
            return partObj;
        }

        private static void CreateSlimePrefab()
        {
            GameObject root = new GameObject("Slime");
            CreateShadow(root, 0.55f);

            GameObject visual = new GameObject("Visual");
            visual.transform.SetParent(root.transform, false);

            GameObject body = CreatePart(visual, "Body", $"{SpriteBaseDir}/Slime/Slime.png", new Vector3(0, 0.17f, 0), 2);

            CircleCollider2D col = root.AddComponent<CircleCollider2D>();
            col.offset = new Vector2(0, 0.17f);
            col.radius = 0.22f;

            EnemyWalkAnimation anim = root.AddComponent<EnemyWalkAnimation>();
            anim.SetupSlime(visual.transform);

            Enemy enemy = root.AddComponent<Enemy>();
            enemy.InitializeStats("Slime", hp: 10, speed: 0.8f, damage: 5, range: 0.5f, gold: 10, exp: 10);

            SavePrefab(root, "Slime");
        }

        private static void CreateZombiePrefab()
        {
            GameObject root = new GameObject("Zombie");
            CreateShadow(root, 0.45f);

            GameObject visual = new GameObject("Visual");
            visual.transform.SetParent(root.transform, false);

            string stick = $"{CommonDir}/MonsterStick.png";

            // 1. Back limbs (SortingOrder: 1)
            GameObject leftArm = CreatePart(visual, "LeftArm", stick, new Vector3(-0.08f, 0.20f, 0), 1, 1f, new Vector3(0, 0, 20f));
            GameObject leftLeg = CreatePart(visual, "LeftLeg", stick, new Vector3(-0.05f, 0.12f, 0), 1, 1f);

            // 2. Torso (SortingOrder: 2)
            GameObject body = CreatePart(visual, "Body", $"{SpriteBaseDir}/Zombie/ZombieBody.png", new Vector3(0, 0.18f, 0), 2);

            // 3. Head (SortingOrder: 3)
            GameObject head = CreatePart(visual, "Head", $"{SpriteBaseDir}/Zombie/ZombieHead.png", new Vector3(-0.02f, 0.38f, 0), 3);

            // 4. Front limbs (SortingOrder: 4)
            GameObject rightLeg = CreatePart(visual, "RightLeg", stick, new Vector3(0.05f, 0.12f, 0), 3, 1f);
            GameObject rightArm = CreatePart(visual, "RightArm", stick, new Vector3(0.08f, 0.20f, 0), 4, 1f, new Vector3(0, 0, -25f));

            // 5. Weapon held in RightArm (SortingOrder: 5)
            CreatePart(rightArm, "Weapon", $"{SpriteBaseDir}/Zombie/MonsterStick.png", new Vector3(-0.05f, -0.08f, 0), 5, 1f, new Vector3(0, 0, 45f));

            BoxCollider2D col = root.AddComponent<BoxCollider2D>();
            col.offset = new Vector2(0, 0.25f);
            col.size = new Vector2(0.35f, 0.5f);

            EnemyWalkAnimation anim = root.AddComponent<EnemyWalkAnimation>();
            anim.SetupBiped(visual.transform, head.transform, leftLeg.transform, rightLeg.transform, leftArm.transform, rightArm.transform);

            Enemy enemy = root.AddComponent<Enemy>();
            enemy.InitializeStats("Zombie", hp: 12, speed: 1.0f, damage: 6, range: 0.5f, gold: 11, exp: 12);

            SavePrefab(root, "Zombie");
        }

        private static void CreateKoboldPrefab()
        {
            GameObject root = new GameObject("Kobold");
            CreateShadow(root, 0.5f);

            GameObject visual = new GameObject("Visual");
            visual.transform.SetParent(root.transform, false);

            string stick = $"{CommonDir}/MonsterStick.png";

            // Limbs
            GameObject leftArm = CreatePart(visual, "LeftArm", stick, new Vector3(-0.08f, 0.20f, 0), 1, 1f, new Vector3(0, 0, 25f));
            GameObject leftLeg = CreatePart(visual, "LeftLeg", stick, new Vector3(-0.05f, 0.12f, 0), 1, 1f);

            GameObject body = CreatePart(visual, "Body", $"{SpriteBaseDir}/Kobold/KoboldBody.png", new Vector3(0, 0.18f, 0), 2);
            GameObject head = CreatePart(visual, "Head", $"{SpriteBaseDir}/Kobold/KoboldHead.png", new Vector3(-0.02f, 0.38f, 0), 3);

            GameObject rightLeg = CreatePart(visual, "RightLeg", stick, new Vector3(0.05f, 0.12f, 0), 3, 1f);
            GameObject rightArm = CreatePart(visual, "RightArm", stick, new Vector3(0.08f, 0.20f, 0), 4, 1f, new Vector3(0, 0, -25f));

            // Shield on LeftArm
            CreatePart(leftArm, "Shield", $"{SpriteBaseDir}/Kobold/KoboldShield.png", new Vector3(0.02f, -0.08f, 0), 4);

            // Club in RightArm
            CreatePart(rightArm, "Club", $"{SpriteBaseDir}/Kobold/KoboldClub.png", new Vector3(-0.08f, -0.06f, 0), 5, 1f, new Vector3(0, 0, 45f));

            BoxCollider2D col = root.AddComponent<BoxCollider2D>();
            col.offset = new Vector2(0, 0.25f);
            col.size = new Vector2(0.4f, 0.5f);

            EnemyWalkAnimation anim = root.AddComponent<EnemyWalkAnimation>();
            anim.SetupBiped(visual.transform, head.transform, leftLeg.transform, rightLeg.transform, leftArm.transform, rightArm.transform);

            Enemy enemy = root.AddComponent<Enemy>();
            enemy.InitializeStats("Kobold", hp: 15, speed: 1.2f, damage: 7, range: 0.5f, gold: 12, exp: 14);

            SavePrefab(root, "Kobold");
        }

        private static void CreateCannibalPrefab()
        {
            GameObject root = new GameObject("Cannibal");
            CreateShadow(root, 0.45f);

            GameObject visual = new GameObject("Visual");
            visual.transform.SetParent(root.transform, false);

            string stick = $"{CommonDir}/MonsterStick.png";

            GameObject leftArm = CreatePart(visual, "LeftArm", stick, new Vector3(-0.08f, 0.20f, 0), 1, 1f, new Vector3(0, 0, 25f));
            GameObject leftLeg = CreatePart(visual, "LeftLeg", stick, new Vector3(-0.05f, 0.12f, 0), 1, 1f);

            GameObject body = CreatePart(visual, "Body", $"{SpriteBaseDir}/Cannibal/CannibalBody.png", new Vector3(0, 0.18f, 0), 2);
            GameObject head = CreatePart(visual, "Head", $"{SpriteBaseDir}/Cannibal/CannibalHead.png", new Vector3(-0.02f, 0.38f, 0), 3);

            GameObject rightLeg = CreatePart(visual, "RightLeg", stick, new Vector3(0.05f, 0.12f, 0), 3, 1f);
            GameObject rightArm = CreatePart(visual, "RightArm", stick, new Vector3(0.08f, 0.20f, 0), 4, 1f, new Vector3(0, 0, -25f));

            CreatePart(rightArm, "Weapon", $"{SpriteBaseDir}/Zombie/MonsterStick.png", new Vector3(-0.05f, -0.08f, 0), 5, 1f, new Vector3(0, 0, 40f));

            BoxCollider2D col = root.AddComponent<BoxCollider2D>();
            col.offset = new Vector2(0, 0.25f);
            col.size = new Vector2(0.35f, 0.5f);

            EnemyWalkAnimation anim = root.AddComponent<EnemyWalkAnimation>();
            anim.SetupBiped(visual.transform, head.transform, leftLeg.transform, rightLeg.transform, leftArm.transform, rightArm.transform);

            Enemy enemy = root.AddComponent<Enemy>();
            enemy.InitializeStats("Cannibal", hp: 12, speed: 1.5f, damage: 8, range: 0.5f, gold: 12, exp: 14);

            SavePrefab(root, "Cannibal");
        }

        private static void CreateOrkPrefab()
        {
            GameObject root = new GameObject("Ork");
            CreateShadow(root, 0.5f);

            GameObject visual = new GameObject("Visual");
            visual.transform.SetParent(root.transform, false);

            string stick = $"{CommonDir}/MonsterStick.png";

            GameObject leftArm = CreatePart(visual, "LeftArm", stick, new Vector3(-0.08f, 0.22f, 0), 1, 1.1f, new Vector3(0, 0, 25f));
            GameObject leftLeg = CreatePart(visual, "LeftLeg", stick, new Vector3(-0.06f, 0.13f, 0), 1, 1.1f);

            GameObject body = CreatePart(visual, "Body", $"{SpriteBaseDir}/Ork/OrkBody.png", new Vector3(0, 0.20f, 0), 2);
            GameObject head = CreatePart(visual, "Head", $"{SpriteBaseDir}/Ork/OrkHead.png", new Vector3(0, 0.40f, 0), 3);
            CreatePart(head, "Helmet", $"{SpriteBaseDir}/Ork/OrkHelmet.png", new Vector3(0, 0.04f, 0), 4);

            GameObject rightLeg = CreatePart(visual, "RightLeg", stick, new Vector3(0.06f, 0.13f, 0), 3, 1.1f);
            GameObject rightArm = CreatePart(visual, "RightArm", stick, new Vector3(0.08f, 0.22f, 0), 4, 1.1f, new Vector3(0, 0, -25f));

            // Battleaxe in RightArm
            CreatePart(rightArm, "Axe", $"{SpriteBaseDir}/Ork/OrkAxe.png", new Vector3(-0.1f, -0.06f, 0), 5, 1f, new Vector3(0, 0, 45f));

            BoxCollider2D col = root.AddComponent<BoxCollider2D>();
            col.offset = new Vector2(0, 0.28f);
            col.size = new Vector2(0.42f, 0.56f);

            EnemyWalkAnimation anim = root.AddComponent<EnemyWalkAnimation>();
            anim.SetupBiped(visual.transform, head.transform, leftLeg.transform, rightLeg.transform, leftArm.transform, rightArm.transform);

            Enemy enemy = root.AddComponent<Enemy>();
            enemy.InitializeStats("Ork", hp: 20, speed: 1.4f, damage: 8, range: 0.5f, gold: 5, exp: 5);

            SavePrefab(root, "Ork");
        }

        private static void CreateTrollPrefab()
        {
            GameObject root = new GameObject("Troll");
            CreateShadow(root, 0.8f);

            GameObject visual = new GameObject("Visual");
            visual.transform.SetParent(root.transform, false);

            string stick = $"{CommonDir}/MonsterStick.png";

            // Scaled troll limbs
            GameObject leftArm = CreatePart(visual, "LeftArm", stick, new Vector3(-0.16f, 0.32f, 0), 1, 1.6f, new Vector3(0, 0, 30f));
            GameObject leftLeg = CreatePart(visual, "LeftLeg", stick, new Vector3(-0.10f, 0.18f, 0), 1, 1.6f);

            GameObject body = CreatePart(visual, "Body", $"{SpriteBaseDir}/Troll/TrollBody.png", new Vector3(0, 0.30f, 0), 2, 1.3f);
            GameObject head = CreatePart(visual, "Head", $"{SpriteBaseDir}/Troll/TrollHead.png", new Vector3(-0.02f, 0.62f, 0), 3, 1.3f);

            GameObject rightLeg = CreatePart(visual, "RightLeg", stick, new Vector3(0.10f, 0.18f, 0), 3, 1.6f);
            GameObject rightArm = CreatePart(visual, "RightArm", stick, new Vector3(0.16f, 0.32f, 0), 4, 1.6f, new Vector3(0, 0, -30f));

            // Giant Hammer in RightArm
            CreatePart(rightArm, "Hammer", $"{SpriteBaseDir}/Troll/TrollHammer.png", new Vector3(-0.18f, -0.05f, 0), 5, 1.3f, new Vector3(0, 0, 50f));

            BoxCollider2D col = root.AddComponent<BoxCollider2D>();
            col.offset = new Vector2(0, 0.42f);
            col.size = new Vector2(0.65f, 0.85f);

            EnemyWalkAnimation anim = root.AddComponent<EnemyWalkAnimation>();
            anim.SetupBiped(visual.transform, head.transform, leftLeg.transform, rightLeg.transform, leftArm.transform, rightArm.transform);

            Enemy enemy = root.AddComponent<Enemy>();
            enemy.InitializeStats("Troll", hp: 20, speed: 1.5f, damage: 9, range: 0.5f, gold: 10, exp: 5);

            SavePrefab(root, "Troll");
        }

        private static void CreateHarpyPrefab()
        {
            GameObject root = new GameObject("Harpy");
            CreateShadow(root, 0.4f);

            GameObject visual = new GameObject("Visual");
            visual.transform.SetParent(root.transform, false);
            visual.transform.localPosition = new Vector3(0, 1.5f, 0); // Flying height

            // Back wings
            GameObject wingL = CreatePart(visual, "LeftWing", $"{SpriteBaseDir}/Harpy/HarpyWing1.png", new Vector3(-0.18f, 0.12f, 0), 1);
            GameObject wingR = CreatePart(visual, "RightWing", $"{SpriteBaseDir}/Harpy/HarpyWing2.png", new Vector3(0.16f, 0.12f, 0), 1);

            GameObject body = CreatePart(visual, "Body", $"{SpriteBaseDir}/Harpy/HarpyBody.png", Vector3.zero, 2);
            CreatePart(visual, "Legs", $"{SpriteBaseDir}/Harpy/HarpyLegs.png", new Vector3(0, -0.12f, 0), 2);
            GameObject head = CreatePart(visual, "Head", $"{SpriteBaseDir}/Harpy/HarpyHead.png", new Vector3(0, 0.22f, 0), 3);

            CircleCollider2D col = root.AddComponent<CircleCollider2D>();
            col.offset = new Vector2(0, 1.5f);
            col.radius = 0.32f;

            EnemyWalkAnimation anim = root.AddComponent<EnemyWalkAnimation>();
            anim.SetupFlying(visual.transform, head.transform, wingL.transform, wingR.transform);

            Enemy enemy = root.AddComponent<Enemy>();
            enemy.InitializeStats("Harpy", hp: 15, speed: 1.25f, damage: 4, range: 0.5f, gold: 3, exp: 3, flying: true);

            SavePrefab(root, "Harpy");
        }

        private static void CreateTaurenPrefab()
        {
            GameObject root = new GameObject("Tauren");
            CreateShadow(root, 0.9f);

            GameObject visual = new GameObject("Visual");
            visual.transform.SetParent(root.transform, false);

            // Tauren limbs
            GameObject backLeg = CreatePart(visual, "BackLeg", $"{SpriteBaseDir}/Tauren/TaurenLeg.png", new Vector3(-0.15f, 0.20f, 0), 1, 1.4f);
            GameObject backArm = CreatePart(visual, "BackArm", $"{SpriteBaseDir}/Tauren/TaurenArm.png", new Vector3(-0.25f, 0.38f, 0), 1, 1.4f, new Vector3(0, 0, 30f));
            GameObject tail = CreatePart(visual, "Tail", $"{SpriteBaseDir}/Tauren/TaurenTail.png", new Vector3(0.32f, 0.30f, 0), 1, 1.4f);

            GameObject body = CreatePart(visual, "Body", $"{SpriteBaseDir}/Tauren/TaurenBody.png", new Vector3(0, 0.36f, 0), 2, 1.4f);
            CreatePart(visual, "Drum", $"{SpriteBaseDir}/Tauren/TaurenDrum.png", new Vector3(-0.12f, 0.32f, 0), 3, 1.2f);
            GameObject head = CreatePart(visual, "Head", $"{SpriteBaseDir}/Tauren/TaurenHead.png", new Vector3(-0.06f, 0.72f, 0), 4, 1.4f);

            GameObject frontLeg = CreatePart(visual, "FrontLeg", $"{SpriteBaseDir}/Tauren/TaurenLeg.png", new Vector3(0.12f, 0.20f, 0), 3, 1.4f);
            GameObject frontArm = CreatePart(visual, "FrontArm", $"{SpriteBaseDir}/Tauren/TaurenArm.png", new Vector3(0.20f, 0.38f, 0), 4, 1.4f, new Vector3(0, 0, -25f));

            BoxCollider2D col = root.AddComponent<BoxCollider2D>();
            col.offset = new Vector2(0, 0.52f);
            col.size = new Vector2(0.85f, 1.05f);

            EnemyWalkAnimation anim = root.AddComponent<EnemyWalkAnimation>();
            anim.SetupBiped(visual.transform, head.transform, backLeg.transform, frontLeg.transform, backArm.transform, frontArm.transform, frontArm.transform, tail.transform);

            Enemy enemy = root.AddComponent<Enemy>();
            enemy.InitializeStats("Tauren", hp: 300, speed: 1.3f, damage: 12, range: 0.75f, gold: 100, exp: 50);

            SavePrefab(root, "Tauren");
        }

        private static void SavePrefab(GameObject root, string name)
        {
            string path = $"{PrefabOutputDir}/{name}.prefab";
            PrefabUtility.SaveAsPrefabAsset(root, path);
            Object.DestroyImmediate(root);
        }
    }
}
