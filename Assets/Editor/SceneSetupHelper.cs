#if UNITY_EDITOR
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;
using Prototype.Core;
using Prototype.Gameplay;
using Prototype.UI;

namespace Prototype.Editor
{
    /// <summary>
    /// 一键搭建 2D 最小可玩闭环场景。在 Unity 菜单 Prototype/搭建场景 下使用。
    /// 不需要手写 .unity，避免格式错误；点击后自动生成并保存场景。
    /// 使用纯色占位精灵，后续可替换为 Art 目录下的美术素材。
    /// </summary>
    public static class SceneSetupHelper
    {
        private const string ScenesDir = "Assets/Scenes";
        private const int TargetScore = 80;

        // ---------- 菜单入口 ----------

        [MenuItem("Prototype/搭建场景/全部（Main + Result）")]
        public static void BuildAll()
        {
            BuildMain();
            BuildResult();
            Debug.Log("[Prototype] 全部场景已搭建：Main.unity / Result.unity");
        }

        [MenuItem("Prototype/搭建场景/Main（M1 关卡）")]
        public static void BuildMain()
        {
            EnsureScenesFolder();
            Scene scene = EditorSceneManager.NewScene(NewSceneSetup.EmptyScene, NewSceneMode.Single);

            // 2D 摄像头（正交，侧视，对准关卡中部）
            var camGo = new GameObject("Main Camera");
            var cam = camGo.AddComponent<Camera>();
            cam.orthographic = true;
            cam.orthographicSize = 6f;
            cam.transform.position = new Vector3(12f, 3f, -10f);
            camGo.AddComponent<AudioListener>();

            // 主地面（24 x 1，纯白占位方块）
            var ground = CreateSpriteObject("Ground", Color.white, new Vector2(24f, 1f), new Vector3(12f, -2f, 0f));
            ground.layer = LayerMask.NameToLayer("Ground");
            ground.AddComponent<BoxCollider2D>();

            // 边界墙（左右各一道，防走出世界）
            CreateWall("LeftWall", new Vector3(0.25f, 0f, 0f));
            CreateWall("RightWall", new Vector3(23.75f, 0f, 0f));

            // 浮空平台（Ground 图层，可被地面检测识别）
            CreatePlatform("Platform_A", new Vector3(6f, 1f, 0f), new Vector2(4f, 0.5f));
            CreatePlatform("Platform_B", new Vector3(12f, 2f, 0f), new Vector2(4f, 0.5f));
            CreatePlatform("Platform_C", new Vector3(18f, 1f, 0f), new Vector2(4f, 0.5f));
            CreatePlatform("Platform_D", new Vector3(21.5f, 3f, 0f), new Vector2(3f, 0.5f));

            // 玩家（带 Rigidbody2D + 2D 控制器 + 碰撞体）
            var player = CreateSpriteObject("Player", Color.cyan, new Vector2(0.8f, 0.8f), new Vector3(2f, -1f, 0f));
            player.tag = "Player";
            var rb = player.AddComponent<Rigidbody2D>();
            rb.constraints = RigidbodyConstraints2D.FreezeRotation;
            player.AddComponent<BoxCollider2D>();
            player.AddComponent<PlayerController2D>();

            // 相机跟随
            var follow = camGo.AddComponent<CameraFollow2D>();
            var followSO = new SerializedObject(follow);
            followSO.FindProperty("target").objectReferenceValue = player.transform;
            followSO.ApplyModifiedPropertiesWithoutUndo();

            // 收集物（按 M1 关卡设计定稿分布）
            Vector3[] collectiblePositions =
            {
                new Vector3(3f, -0.5f, 0f), new Vector3(9f, -0.5f, 0f),
                new Vector3(15f, -0.5f, 0f), new Vector3(21f, -0.5f, 0f),
                new Vector3(4.5f, 1.4f, 0f), new Vector3(7.5f, 1.4f, 0f),
                new Vector3(10.5f, 2.4f, 0f), new Vector3(13.5f, 2.4f, 0f),
                new Vector3(18f, 1.4f, 0f), new Vector3(21.5f, 3.4f, 0f)
            };
            for (int i = 0; i < collectiblePositions.Length; i++)
            {
                var c = CreateSpriteObject("Collectible_" + i, new Color(1f, 0.85f, 0.2f),
                    new Vector2(0.5f, 0.5f), collectiblePositions[i]);
                var col = c.AddComponent<CircleCollider2D>();
                col.isTrigger = true;
                c.AddComponent<Collectible2D>();
            }

            // 过关条件
            var win = new GameObject("WinCondition");
            var wc = win.AddComponent<WinCondition>();
            var wcSO = new SerializedObject(wc);
            wcSO.FindProperty("targetScore").intValue = TargetScore;
            wcSO.FindProperty("nextSceneName").stringValue = "Result";
            wcSO.ApplyModifiedPropertiesWithoutUndo();

            // HUD
            CreateHUD(TargetScore);

            EditorSceneManager.SaveScene(scene, $"{ScenesDir}/Main.unity");
            AssetDatabase.SaveAssets();
            Debug.Log("[Prototype] Main 场景已搭建并保存：Assets/Scenes/Main.unity");
        }

        [MenuItem("Prototype/搭建场景/Result（结算画面）")]
        public static void BuildResult()
        {
            EnsureScenesFolder();
            Scene scene = EditorSceneManager.NewScene(NewSceneSetup.EmptyScene, NewSceneMode.Single);

            var canvasGo = new GameObject("Result Canvas");
            var canvas = canvasGo.AddComponent<Canvas>();
            canvas.renderMode = RenderMode.ScreenSpaceOverlay;
            canvasGo.AddComponent<CanvasScaler>();
            canvasGo.AddComponent<GraphicRaycaster>();

            CreateText(canvasGo.transform, "Title", "已通关！", TextAnchor.MiddleCenter,
                new Vector2(0.5f, 1f), new Vector2(0.5f, 1f), new Vector2(0.5f, 1f),
                new Vector2(0f, -80f), new Vector2(500f, 60f), 40, Color.white);

            var scoreText = CreateText(canvasGo.transform, "FinalScore", "最终分数: 0", TextAnchor.MiddleCenter,
                new Vector2(0.5f, 0.5f), new Vector2(0.5f, 0.5f), new Vector2(0.5f, 0.5f),
                new Vector2(0f, 40f), new Vector2(500f, 50f), 28, Color.white);

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
            btnRT.anchoredPosition = new Vector2(0f, -40f);
            btnRT.sizeDelta = new Vector2(160f, 44f);

            var resultUI = canvasGo.AddComponent<ResultUI>();
            var resultSO = new SerializedObject(resultUI);
            resultSO.FindProperty("finalScoreText").objectReferenceValue = scoreText;
            resultSO.FindProperty("replayButton").objectReferenceValue = btn;
            resultSO.ApplyModifiedPropertiesWithoutUndo();

            EditorSceneManager.SaveScene(scene, $"{ScenesDir}/Result.unity");
            AssetDatabase.SaveAssets();
            Debug.Log("[Prototype] Result 场景已搭建并保存：Assets/Scenes/Result.unity");
        }

        // ---------- 内部工具 ----------

        /// <summary>生成带碰撞体的边界墙（默认图层，不参与地面检测）。</summary>
        private static GameObject CreateWall(string name, Vector3 center)
        {
            var wall = CreateSpriteObject(name, Color.gray, new Vector2(0.5f, 8f), center);
            wall.AddComponent<BoxCollider2D>();
            return wall;
        }

        /// <summary>生成 Ground 图层浮空平台（可被玩家地面检测射线识别）。</summary>
        private static GameObject CreatePlatform(string name, Vector3 center, Vector2 size)
        {
            var platform = CreateSpriteObject(name, Color.white, size, center);
            platform.layer = LayerMask.NameToLayer("Ground");
            platform.AddComponent<BoxCollider2D>();
            return platform;
        }

        private static void CreateHUD(int targetScore)
        {
            var canvasGo = new GameObject("HUD Canvas");
            var canvas = canvasGo.AddComponent<Canvas>();
            canvas.renderMode = RenderMode.ScreenSpaceOverlay;
            canvasGo.AddComponent<CanvasScaler>();
            canvasGo.AddComponent<GraphicRaycaster>();

            var scoreText = CreateText(canvasGo.transform, "ScoreText", "分数: 0", TextAnchor.MiddleLeft,
                new Vector2(0f, 1f), new Vector2(0f, 1f), new Vector2(0f, 1f),
                new Vector2(20f, -20f), new Vector2(320f, 40f), 28, Color.white);

            var hintText = CreateText(canvasGo.transform, "HintText",
                $"用 A/D 或方向键移动，空格跳跃，收集物品达到 {targetScore} 分进入下一关",
                TextAnchor.MiddleCenter,
                new Vector2(0.5f, 0f), new Vector2(0.5f, 0f), new Vector2(0.5f, 0f),
                new Vector2(0f, 30f), new Vector2(640f, 40f), 20, Color.white);

            var hud = canvasGo.AddComponent<HUD>();
            hud.scoreText = scoreText;
            hud.hintText = hintText;
            var hudSO = new SerializedObject(hud);
            hudSO.FindProperty("targetScore").intValue = targetScore;
            hudSO.ApplyModifiedPropertiesWithoutUndo();
        }

        /// <summary>生成带纯白精灵与 SpriteRenderer 的 2D 对象（尺寸即为世界单位）。</summary>
        private static GameObject CreateSpriteObject(string name, Color color, Vector2 size, Vector3 position)
        {
            var go = new GameObject(name);
            go.transform.position = position;
            go.transform.localScale = new Vector3(size.x, size.y, 1f);
            var sr = go.AddComponent<SpriteRenderer>();
            sr.sprite = GetPlaceholderSprite();
            sr.color = color;
            return go;
        }

        /// <summary>
        /// 获取(必要时生成)储物于 Assets/Art 的 1x1 白色精灵资源。
        /// 精灵持久化为工程资产，保证场景保存/重开后引用不丢失。
        /// </summary>
        private static Sprite GetPlaceholderSprite()
        {
            const string path = "Assets/Art/Placeholder_White.png";
            EnsureFolder("Assets/Art");

            var importer = AssetImporter.GetAtPath(path) as TextureImporter;
            if (importer == null)
            {
                var tex = new Texture2D(100, 100);
                var px = new Color[100 * 100];
                for (int i = 0; i < px.Length; i++) px[i] = Color.white;
                tex.SetPixels(px);
                tex.Apply();
                System.IO.File.WriteAllBytes(System.IO.Path.Combine(
                    System.IO.Path.GetDirectoryName(Application.dataPath), "Assets/Art/Placeholder_White.png"), tex.EncodeToPNG());
                Object.DestroyImmediate(tex);
                AssetDatabase.ImportAsset(path, ImportAssetOptions.ForceUpdate);
            }

            importer = AssetImporter.GetAtPath(path) as TextureImporter;
            if (importer.textureType != TextureImporterType.Sprite)
            {
                importer.textureType = TextureImporterType.Sprite;
                importer.spritePixelsPerUnit = 100f;
                AssetDatabase.ImportAsset(path, ImportAssetOptions.ForceUpdate);
            }

            return AssetDatabase.LoadAssetAtPath<Sprite>(path);
        }

        private static void EnsureFolder(string path)
        {
            string[] parts = path.Split('/');
            string current = parts[0];
            for (int i = 1; i < parts.Length; i++)
            {
                string next = current + "/" + parts[i];
                if (!AssetDatabase.IsValidFolder(next))
                {
                    AssetDatabase.CreateFolder(current, parts[i]);
                }
                current = next;
            }
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
    }
}
#endif