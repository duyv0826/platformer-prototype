#if UNITY_EDITOR
using System.Collections.Generic;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;
using Prototype.Core;
using Prototype.Gameplay;
using Prototype.UI;
using Prototype.Audio;
using static Prototype.Editor.SceneSetupPrimitives;

namespace Prototype.Editor
{
    /// <summary>
    /// 一键搭建 2D 像素平台跳跃场景（基于 Kenney Pixel Platformer 素材，CC0）。
    /// 在 Unity 菜单 Prototype/搭建场景 下使用；点击后自动生成并保存 Main / Result 场景，
    /// 并自动把两者注册进 Build Settings（否则运行时按名字加载场景会失败）。
    /// M1 关卡设计：单条地面 + 4 个浮空平台 + 1 个上下移动平台，收集 15 颗红心(5分)与 3 颗蓝宝石(15分)，
    /// 触旗即通关（收集越多星级越高），另有巡逻敌人与火焰陷阱增加挑战。
    /// </summary>
    public static class SceneSetupHelper
    {
        private const string ScenesDir = "Assets/Scenes";
        private const int TargetScore = 60;
        private const int HeartScore = 5;
        private const int GemScore = 15;

        // Kenney 素材索引（见 Assets/Art/PixelPlatformer/Tilesheet 说明）
        private const int TileGrassA = 0, TileGrassB = 1, TileGrassC = 2, TileGrassD = 3;
        private const int TileSoilA = 4, TileSoilB = 5, TileSoilC = 6;
        private const int TileHeart = 44;
        private const int TileGem = 67;
        private const int TileFlag = 111;
        private const int TileCloud = 153;
        private const int TileTree = 125;
        private const int CharHero = 0;
        private const int CharHeroJump = 1;
        private const int CharSlimeA = 11, CharSlimeB = 12;
        private const int CharFireA = 13, CharFireB = 14;
        private const int BgSky = 0, BgMountain = 2;

        // ---------- 菜单入口 ----------

        [MenuItem("Prototype/搭建场景/全部（Main + Result）")]
        public static void BuildAll()
        {
            PixelSprites.EnsureImportSettings();
            BuildMain();
            BuildResult();
            EnsureScenesInBuildSettings();
            Debug.Log("[Prototype] 全部场景已搭建：Main.unity / Result.unity（已注册进 Build Settings）");
        }

        [MenuItem("Prototype/搭建场景/Main（M1 关卡）")]
        public static void BuildMain()
        {
            PixelSprites.EnsureImportSettings();
            EnsureScenesFolder();
            Scene scene = EditorSceneManager.NewScene(NewSceneSetup.EmptyScene, NewSceneMode.Single);

            // ---------- 相机 ----------
            var camGo = CreateCamera(6f, new Color(0.62f, 0.8f, 0.95f));
            camGo.transform.position = new Vector3(22f, 3f, -10f);
            camGo.AddComponent<AudioListener>();

            // ---------- 背景（视差分层） ----------
            CreateBackground(camGo.transform);

            // ---------- 主地面（顶面 y=0，宽 46） ----------
            var ground = new GameObject("Ground");
            ground.layer = LayerMask.NameToLayer("Ground");
            ground.transform.position = new Vector3(22f, -0.25f, 0f);
            AddStripRenderer(ground, new[] { TileGrassA, TileGrassB, TileGrassC, TileGrassD }, 23, 0);
            var soil = new GameObject("Soil");
            soil.transform.SetParent(ground.transform, false);
            soil.transform.localPosition = new Vector3(0f, -0.5f, 0f);
            AddStripRenderer(soil, new[] { TileSoilA, TileSoilB, TileSoilC, TileSoilA }, 23, -1);
            var soil2 = new GameObject("SoilDeep");
            soil2.transform.SetParent(ground.transform, false);
            soil2.transform.localPosition = new Vector3(0f, -1f, 0f);
            AddStripRenderer(soil2, new[] { TileSoilB, TileSoilC, TileSoilA, TileSoilB }, 23, -1);

            var groundCol = ground.AddComponent<BoxCollider2D>();
            groundCol.size = new Vector2(46f, 0.5f);

            // ---------- 浮空平台（Ground 图层，可被地面检测识别） ----------
            CreatePlatform("Platform_A", new Vector3(8.25f, 1.5f, 0f), new[] { TileGrassA, TileGrassB, TileGrassC, TileGrassD, TileGrassA });
            CreatePlatform("Platform_B", new Vector3(14.5f, 2.8f, 0f), new[] { TileGrassA, TileGrassB, TileGrassC, TileGrassD, TileGrassA, TileGrassB });
            CreatePlatform("Platform_C", new Vector3(21.25f, 1.5f, 0f), new[] { TileGrassA, TileGrassB, TileGrassC, TileGrassD, TileGrassA });
            CreatePlatform("Platform_D", new Vector3(28f, 2.8f, 0f), new[] { TileGrassA, TileGrassB, TileGrassC, TileGrassD, TileGrassA, TileGrassB });
            // 上下移动平台（节奏机关：跳上后随平台升降）
            var movePlatform = CreatePlatform("Platform_Move", new Vector3(35f, 2.2f, 0f), new[] { TileGrassA, TileGrassB, TileGrassC, TileGrassD });
            movePlatform.AddComponent<MovingPlatform>();

            // ---------- 边界墙（防走出世界） ----------
            CreateWall("LeftWall", new Vector3(-0.25f, 2.5f, 0f));
            CreateWall("RightWall", new Vector3(44.25f, 2.5f, 0f));

            // ---------- 玩家 ----------
            CreatePlayer(new Vector3(2f, 0.36f, 0f));

            // ---------- 收集物：红心(5分) 15 个 + 蓝宝石(15分) 3 个 ----------
            Vector3[] hearts =
            {
                new Vector3(4f, 0.35f, 0f), new Vector3(10.5f, 0.35f, 0f),
                new Vector3(17f, 0.35f, 0f), new Vector3(23f, 0.35f, 0f),
                new Vector3(30.5f, 0.35f, 0f), new Vector3(36.5f, 0.35f, 0f),
                new Vector3(7.6f, 1.85f, 0f), new Vector3(8.8f, 1.85f, 0f),
                new Vector3(13.6f, 3.15f, 0f), new Vector3(14.6f, 3.15f, 0f),
                new Vector3(15.6f, 3.15f, 0f),
                new Vector3(20.6f, 1.85f, 0f), new Vector3(21.8f, 1.85f, 0f),
                new Vector3(27.2f, 3.15f, 0f), new Vector3(28.2f, 3.15f, 0f)
            };
            int total = hearts.Length;
            for (int i = 0; i < hearts.Length; i++)
            {
                CreateCollectible("Heart_" + i, hearts[i], PixelSprites.Tile(TileHeart), HeartScore);
            }

            Vector3[] gems =
            {
                new Vector3(14.6f, 4.3f, 0f),
                new Vector3(28f, 4.3f, 0f),
                new Vector3(35f, 3.4f, 0f)
            };
            for (int i = 0; i < gems.Length; i++)
            {
                CreateCollectible("Gem_" + i, gems[i], PixelSprites.Tile(TileGem), GemScore);
            }
            total += gems.Length;

            // 关卡配置：运行时注入收集物总数（供结算统计）
            var lvlCfg = new GameObject("LevelConfig");
            var cfg = lvlCfg.AddComponent<LevelConfig>();
            var cfgSO = new SerializedObject(cfg);
            cfgSO.FindProperty("totalCollectibles").intValue = total;
            cfgSO.ApplyModifiedPropertiesWithoutUndo();

            // ---------- 敌人（slime 巡逻） ----------
            CreateSlime("Slime_1", new Vector3(8.5f, 0.33f, 0f), 6.8f, 10.2f);
            CreateSlime("Slime_2", new Vector3(14.8f, 3.13f, 0f), 13.6f, 15.6f);
            CreateSlime("Slime_3", new Vector3(28.2f, 3.13f, 0f), 27f, 29.2f);

            // ---------- 火焰陷阱 ----------
            CreateFire("Fire_1", new Vector3(19.5f, 0.75f, 0f));
            CreateFire("Fire_2", new Vector3(33.5f, 0.75f, 0f));

            // ---------- 终点旗帜（触旗通关） ----------
            CreateFlag(new Vector3(41.5f, 1.1f, 0f));

            // ---------- 相机跟随（锁定 Y=3） ----------
            var follow = camGo.AddComponent<CameraFollow2D>();
            var followSO = new SerializedObject(follow);
            followSO.FindProperty("target").objectReferenceValue = GameObject.Find("Player").transform;
            followSO.FindProperty("lockY").floatValue = 3f;
            followSO.ApplyModifiedPropertiesWithoutUndo();

            // ---------- HUD ----------
            CreateHUD(TargetScore);

            EditorSceneManager.SaveScene(scene, $"{ScenesDir}/Main.unity");
            AssetDatabase.SaveAssets();
            EnsureScenesInBuildSettings();
            Debug.Log("[Prototype] Main 场景已搭建并保存：Assets/Scenes/Main.unity");
        }

        [MenuItem("Prototype/搭建场景/Result（结算画面）")]
        public static void BuildResult()
        {
            EnsureScenesFolder();
            Scene scene = EditorSceneManager.NewScene(NewSceneSetup.EmptyScene, NewSceneMode.Single);

            CreateCamera(5f, new Color(0.2f, 0.32f, 0.42f));

            var canvasGo = CreateCanvas("Result Canvas");

            CreateText(canvasGo.transform, "Title", "通关成功！", TextAnchor.MiddleCenter,
                new Vector2(0.5f, 1f), new Vector2(0.5f, 1f), new Vector2(0.5f, 1f),
                new Vector2(0f, -80f), new Vector2(500f, 60f), 42, new Color(1f, 0.9f, 0.55f));

            var scoreText = CreateText(canvasGo.transform, "FinalScore", "最终分数: 0", TextAnchor.MiddleCenter,
                new Vector2(0.5f, 0.5f), new Vector2(0.5f, 0.5f), new Vector2(0.5f, 0.5f),
                new Vector2(0f, 70f), new Vector2(500f, 50f), 30, Color.white);

            var collectText = CreateText(canvasGo.transform, "CollectInfo", "收集: 0 / 0", TextAnchor.MiddleCenter,
                new Vector2(0.5f, 0.5f), new Vector2(0.5f, 0.5f), new Vector2(0.5f, 0.5f),
                new Vector2(0f, 10f), new Vector2(500f, 40f), 24, new Color(0.85f, 0.9f, 0.95f));

            var starsText = CreateText(canvasGo.transform, "Stars", "☆☆☆", TextAnchor.MiddleCenter,
                new Vector2(0.5f, 0.5f), new Vector2(0.5f, 0.5f), new Vector2(0.5f, 0.5f),
                new Vector2(0f, -30f), new Vector2(300f, 50f), 38, new Color(1f, 0.85f, 0.3f));

            // 再玩一次按钮
            var btnObj = new GameObject("ReplayButton");
            btnObj.transform.SetParent(canvasGo.transform, false);
            var btnImage = btnObj.AddComponent<Image>();
            btnImage.color = new Color(0.31f, 0.82f, 0.77f);
            var btn = btnObj.AddComponent<Button>();
            var btnText = CreateText(btnObj.transform, "Text", "再玩一次", TextAnchor.MiddleCenter,
                new Vector2(0.5f, 0.5f), new Vector2(0.5f, 0.5f), new Vector2(0.5f, 0.5f),
                Vector2.zero, new Vector2(160f, 40f), 22, Color.black);
            var btnRT = btnObj.GetComponent<RectTransform>();
            btnRT.anchoredPosition = new Vector2(0f, -110f);
            btnRT.sizeDelta = new Vector2(180f, 48f);

            var resultUI = canvasGo.AddComponent<ResultUI>();
            var resultSO = new SerializedObject(resultUI);
            resultSO.FindProperty("finalScoreText").objectReferenceValue = scoreText;
            resultSO.FindProperty("collectText").objectReferenceValue = collectText;
            resultSO.FindProperty("starsText").objectReferenceValue = starsText;
            resultSO.FindProperty("replayButton").objectReferenceValue = btn;
            resultSO.ApplyModifiedPropertiesWithoutUndo();

            EditorSceneManager.SaveScene(scene, $"{ScenesDir}/Result.unity");
            AssetDatabase.SaveAssets();
            Debug.Log("[Prototype] Result 场景已搭建并保存：Assets/Scenes/Result.unity");
        }

        // ---------- 场景元素构建 ----------

        private static void CreateBackground(Transform camT)
        {
            // 天空（钉在相机上，永远覆盖）
            AddCameraChild(camT, "Sky", PixelSprites.Bg(BgSky), new Vector3(0f, 0f, 5f), new Vector2(22f, 14f), -100);

            // 远山（视差 0.9，几乎不动；整数倍缩放保持像素对齐）
            CreateParallaxSprite("Mountain_L", new Vector3(6f, 2f, 0f),
                new Vector2(4f, 4f), -95, 0.9f, PixelSprites.Bg(BgMountain));
            CreateParallaxSprite("Mountain_R", new Vector3(37f, 1.6f, 0f),
                new Vector2(4f, 4f), -95, 0.9f, PixelSprites.Bg(BgMountain));

            // 云（视差 0.55；18px tile 放大 3 倍 = 1.5 世界单位，整数倍保持像素锐利）
            float[] cloudXs = { 6f, 13f, 21f, 29f, 36f, 42f };
            float[] cloudYs = { 6.6f, 7.3f, 6.2f, 7.6f, 6.8f, 7.1f };
            for (int i = 0; i < cloudXs.Length; i++)
            {
                CreateParallaxSprite("Cloud_" + i, new Vector3(cloudXs[i], cloudYs[i], 0f),
                    new Vector2(3f, 3f), -90, 0.55f, PixelSprites.Tile(TileCloud));
            }

            // 远景树（视差 0.75，中景剪影；18px tile 放大 4 倍 = 2 世界单位）
            float[] treeXs = { 4f, 9f, 17f, 25f, 32f, 40f };
            for (int i = 0; i < treeXs.Length; i++)
            {
                CreateParallaxSprite("Tree_" + i, new Vector3(treeXs[i], 1f, 0f),
                    new Vector2(4f, 4f), -85, 0.75f, PixelSprites.Tile(TileTree));
            }
        }

        /// <summary>主地面/平台统一使用草顶条精灵；平台厚 1 tile（0.5 单位），底部附土壤条增加厚度感，视觉与碰撞对齐。返回平台根对象。</summary>
        private static GameObject CreatePlatform(string name, Vector3 center, int[] tileSeq)
        {
            var go = new GameObject(name);
            go.transform.position = center;
            go.layer = LayerMask.NameToLayer("Ground");

            var visual = new GameObject("Visual");
            visual.transform.SetParent(go.transform, false);
            visual.transform.localPosition = new Vector3(0f, -0.25f, 0f);
            AddStripRenderer(visual, tileSeq, 1, 0);

            var soilVisual = new GameObject("SoilVisual");
            soilVisual.transform.SetParent(go.transform, false);
            soilVisual.transform.localPosition = new Vector3(0f, -0.75f, 0f);
            int[] soilCycle = { TileSoilA, TileSoilB, TileSoilC, TileSoilA };
            int[] soilSeq = new int[tileSeq.Length];
            for (int i = 0; i < soilSeq.Length; i++)
            {
                soilSeq[i] = soilCycle[i % soilCycle.Length];
            }
            AddStripRenderer(soilVisual, soilSeq, 1, -1);

            var col = go.AddComponent<BoxCollider2D>();
            float width = tileSeq.Length * 0.5f;
            col.size = new Vector2(width, 0.5f);
            col.offset = new Vector2(0f, -0.25f);
            return go;
        }

        private static void CreateWall(string name, Vector3 center)
        {
            var sr = NewSpriteObject(name, center, PixelSprites.Tile(TileSoilA), 0);
            sr.color = new Color(0.75f, 0.7f, 0.65f);
            var go = sr.gameObject;
            var col = go.AddComponent<BoxCollider2D>();
            col.size = new Vector2(0.5f, 6f);
        }

        private static void CreatePlayer(Vector3 position)
        {
            var go = new GameObject("Player");
            go.transform.position = position;
            go.tag = "Player";

            var sr = go.AddComponent<SpriteRenderer>();
            sr.sprite = PixelSprites.Char(CharHero);
            sr.sortingOrder = 20;

            AddFrozenRigidbody(go);

            var col = go.AddComponent<BoxCollider2D>();
            col.size = new Vector2(0.5f, 0.62f);
            col.offset = new Vector2(0f, 0.02f);

            var controller = go.AddComponent<PlayerController2D>();
            var ctrlSO = new SerializedObject(controller);
            ctrlSO.FindProperty("bodyRenderer").objectReferenceValue = sr;
            ctrlSO.FindProperty("idleSprite").objectReferenceValue = PixelSprites.Char(CharHero);
            ctrlSO.FindProperty("jumpSprite").objectReferenceValue = PixelSprites.Char(CharHeroJump);
            // 手感参数（显式写入，避免依赖脚本默认值快照）
            ctrlSO.FindProperty("moveSpeed").floatValue = 5.5f;
            ctrlSO.FindProperty("jumpForce").floatValue = 8.6f;
            ctrlSO.FindProperty("groundAccel").floatValue = 55f;
            ctrlSO.FindProperty("airControl").floatValue = 0.55f;
            ctrlSO.FindProperty("jumpCutMultiplier").floatValue = 0.45f;
            ctrlSO.FindProperty("riseGravityScale").floatValue = 1.15f;
            ctrlSO.FindProperty("fallGravityScale").floatValue = 2f;
            ctrlSO.FindProperty("fastFallGravity").floatValue = 3.2f;
            ctrlSO.FindProperty("maxFallSpeed").floatValue = -16f;
            ctrlSO.FindProperty("fastFallMaxSpeed").floatValue = -24f;
            ctrlSO.FindProperty("coyoteTime").floatValue = 0.14f;
            ctrlSO.FindProperty("jumpBufferTime").floatValue = 0.18f;
            ctrlSO.ApplyModifiedPropertiesWithoutUndo();

            go.AddComponent<PlayerHealth>();

            // 程序化音效源（2D 无空间衰减）
            var audioSrc = go.AddComponent<AudioSource>();
            audioSrc.playOnAwake = false;
            audioSrc.spatialBlend = 0f;
            Sfx.SetSource(audioSrc);
        }

        private static void CreateCollectible(string name, Vector3 position, Sprite sprite, int score)
        {
            var go = NewSpriteObject(name, position, sprite, 10).gameObject;

            var col = go.AddComponent<CircleCollider2D>();
            col.radius = 0.22f;
            col.isTrigger = true;

            var collectible = go.AddComponent<Collectible2D>();
            var so = new SerializedObject(collectible);
            so.FindProperty("scoreValue").intValue = score;
            so.ApplyModifiedPropertiesWithoutUndo();

            go.AddComponent<FloatAnim>();
        }

        private static void CreateSlime(string name, Vector3 position, float minX, float maxX)
        {
            var sr = NewSpriteObject(name, position, PixelSprites.Char(CharSlimeA), 12);
            var go = sr.gameObject;

            AddFrozenRigidbody(go);

            var col = go.AddComponent<BoxCollider2D>();
            col.size = new Vector2(0.55f, 0.5f);

            var enemy = go.AddComponent<PatrolEnemy>();
            var so = new SerializedObject(enemy);
            so.FindProperty("frameA").objectReferenceValue = PixelSprites.Char(CharSlimeA);
            so.FindProperty("frameB").objectReferenceValue = PixelSprites.Char(CharSlimeB);
            so.FindProperty("bodyRenderer").objectReferenceValue = sr;
            so.FindProperty("minX").floatValue = minX;
            so.FindProperty("maxX").floatValue = maxX;
            so.ApplyModifiedPropertiesWithoutUndo();
        }

        private static void CreateFire(string name, Vector3 position)
        {
            var sr = NewSpriteObject(name, position, PixelSprites.Char(CharFireA), 8);
            var go = sr.gameObject;

            var col = go.AddComponent<BoxCollider2D>();
            col.size = new Vector2(0.5f, 0.62f);
            col.isTrigger = true;

            var fire = go.AddComponent<FireHazard>();
            var so = new SerializedObject(fire);
            so.FindProperty("frameA").objectReferenceValue = PixelSprites.Char(CharFireA);
            so.FindProperty("frameB").objectReferenceValue = PixelSprites.Char(CharFireB);
            so.FindProperty("bodyRenderer").objectReferenceValue = sr;
            so.ApplyModifiedPropertiesWithoutUndo();
        }

        private static void CreateFlag(Vector3 center)
        {
            var sr = NewSpriteObject("Flag", center, PixelSprites.Tile(TileFlag), 9);
            sr.color = new Color(1f, 1f, 1f, 0.92f);
            var go = sr.gameObject;

            var col = go.AddComponent<BoxCollider2D>();
            col.size = new Vector2(1f, 2.2f);
            col.isTrigger = true;

            go.AddComponent<FlagGoal>();
        }

        // ---------- 内部工具 ----------

        /// <summary>在目标对象上加 SpriteRenderer，精灵为 tile 序列拼成的长条（宽 = tiles * 0.5 单位）。</summary>
        private static void AddStripRenderer(GameObject go, int[] tileSeq, int repeat, int order)
        {
            var sr = go.AddComponent<SpriteRenderer>();
            sr.sprite = PixelSprites.BuildStrip(tileSeq, repeat);
            sr.sortingOrder = order;
        }

        private static GameObject CreateParallaxSprite(string name, Vector3 pos, Vector2 size, int order, float factor, Sprite sprite)
        {
            var go = new GameObject(name);
            go.transform.position = pos;
            AddScaledSprite(go, size, sprite, order);
            go.AddComponent<Parallax>().factor = factor;
            return go;
        }

        private static GameObject AddCameraChild(Transform camT, string name, Sprite sprite, Vector3 localPos, Vector2 size, int order)
        {
            var go = new GameObject(name);
            go.transform.SetParent(camT, false);
            go.transform.localPosition = localPos;
            AddScaledSprite(go, size, sprite, order);
            return go;
        }

        private static void CreateHUD(int targetScore)
        {
            var canvasGo = CreateCanvas("HUD Canvas");

            var scoreText = CreateText(canvasGo.transform, "ScoreText", "分数: 0", TextAnchor.MiddleLeft,
                new Vector2(0f, 1f), new Vector2(0f, 1f), new Vector2(0f, 1f),
                new Vector2(20f, -20f), new Vector2(320f, 40f), 26, Color.white);

            var hintText = CreateText(canvasGo.transform, "HintText",
                "到达旗帜即可通关！收集越多星级越高！连续收集有连击加成！",
                TextAnchor.MiddleCenter,
                new Vector2(0.5f, 0f), new Vector2(0.5f, 0f), new Vector2(0.5f, 0f),
                new Vector2(0f, 30f), new Vector2(900f, 40f), 20, Color.white);

            // 收集进度条
            var barObj = new GameObject("ScoreBar");
            barObj.transform.SetParent(canvasGo.transform, false);
            var barImg = barObj.AddComponent<Image>();
            barImg.color = new Color(0f, 0f, 0f, 0.4f);
            var barRT = barObj.GetComponent<RectTransform>();
            barRT.anchorMin = new Vector2(0f, 1f);
            barRT.anchorMax = new Vector2(0f, 1f);
            barRT.pivot = new Vector2(0f, 1f);
            barRT.anchoredPosition = new Vector2(20f, -70f);
            barRT.sizeDelta = new Vector2(300f, 16f);

            var fillObj = new GameObject("ScoreFill");
            fillObj.transform.SetParent(barObj.transform, false);
            var fillImg = fillObj.AddComponent<Image>();
            fillImg.color = new Color(1f, 0.85f, 0.2f);
            var fillRT = fillObj.GetComponent<RectTransform>();
            fillRT.anchorMin = new Vector2(0f, 0.5f);
            fillRT.anchorMax = new Vector2(0f, 0.5f);
            fillRT.pivot = new Vector2(0f, 0.5f);
            fillRT.anchoredPosition = new Vector2(8f, 0f);
            fillRT.sizeDelta = new Vector2(0f, 12f);

            // 生命心形（3 格，放在分数右侧）
            var hearts = new Image[3];
            Sprite fullHeart = PixelSprites.Tile(TileHeart);
            for (int i = 0; i < hearts.Length; i++)
            {
                var heartObj = new GameObject("Heart_" + i);
                heartObj.transform.SetParent(canvasGo.transform, false);
                var heartImg = heartObj.AddComponent<Image>();
                heartImg.sprite = fullHeart;
                heartImg.preserveAspect = true;
                var heartRT = heartObj.GetComponent<RectTransform>();
                heartRT.anchorMin = new Vector2(0f, 1f);
                heartRT.anchorMax = new Vector2(0f, 1f);
                heartRT.pivot = new Vector2(0f, 1f);
                heartRT.anchoredPosition = new Vector2(360f + i * 34f, -24f);
                heartRT.sizeDelta = new Vector2(30f, 30f);
                hearts[i] = heartImg;
            }

            // 连击提示（分数条下方，默认隐藏）
            var comboText = CreateText(canvasGo.transform, "ComboText", "连击 x2！", TextAnchor.MiddleLeft,
                new Vector2(0f, 1f), new Vector2(0f, 1f), new Vector2(0f, 1f),
                new Vector2(20f, -56f), new Vector2(240f, 28f), 22, new Color(1f, 0.85f, 0.3f));
            comboText.gameObject.SetActive(false);

            var hud = canvasGo.AddComponent<HUD>();
            hud.scoreText = scoreText;
            hud.hintText = hintText;
            hud.comboText = comboText;
            hud.progressFill = fillImg;
            hud.hearts = hearts;
            var hudSO = new SerializedObject(hud);
            hudSO.FindProperty("targetScore").intValue = targetScore;
            hudSO.FindProperty("maxLives").intValue = GameManager.MaxLives;
            hudSO.FindProperty("fullHeart").objectReferenceValue = fullHeart;
            hudSO.FindProperty("emptyHeart").objectReferenceValue = null; // 无空心心形素材，用半透明色显示空
            hudSO.ApplyModifiedPropertiesWithoutUndo();
        }

        private static Text CreateText(Transform parent, string name, string content, TextAnchor align,
            Vector2 anchorMin, Vector2 anchorMax, Vector2 pivot, Vector2 pos, Vector2 size, int fontSize, Color color)
        {
            var obj = new GameObject(name);
            obj.transform.SetParent(parent, false);
            var text = obj.AddComponent<Text>();
            text.text = content;
            text.font = Resources.GetBuiltinResource<Font>("LegacyRuntime.ttf");
            text.fontSize = fontSize;
            text.alignment = align;
            text.color = color;
            var rt = text.rectTransform;
            rt.anchorMin = anchorMin;
            rt.anchorMax = anchorMax;
            rt.pivot = pivot;
            rt.anchoredPosition = pos;
            rt.sizeDelta = size;
            return text;
        }

        private static void EnsureScenesFolder()
        {
            if (!AssetDatabase.IsValidFolder(ScenesDir))
            {
                AssetDatabase.CreateFolder("Assets", "Scenes");
            }
        }

        /// <summary>
        /// 把 Main/Result 场景注册进 Build Settings。
        /// 运行时 SceneManager.Load("Result") / ReloadCurrent() 按名字加载场景，
        /// 未注册进 Build Settings 会报 "Scene could not be loaded" 而失败——这是此前「通不了关」的根因。
        /// </summary>
        private static void EnsureScenesInBuildSettings()
        {
            string[] paths = { $"{ScenesDir}/Main.unity", $"{ScenesDir}/Result.unity" };
            var list = new List<EditorBuildSettingsScene>();
            foreach (string path in paths)
            {
                if (AssetDatabase.LoadAssetAtPath<SceneAsset>(path) != null)
                {
                    list.Add(new EditorBuildSettingsScene(path, true));
                }
            }

            foreach (var existing in EditorBuildSettings.scenes)
            {
                if (!list.Exists(x => x.path == existing.path))
                {
                    list.Add(existing);
                }
            }

            EditorBuildSettings.scenes = list.ToArray();
        }
    }
}
#endif
