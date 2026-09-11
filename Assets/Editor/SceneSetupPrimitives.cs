#if UNITY_EDITOR
using UnityEngine;
using UnityEngine.UI;

namespace Prototype.Editor
{
    /// <summary>
    /// 场景搭建的通用图元工厂（相机 / 画布 / 精灵对象 / 刚体 / 缩放精灵）。
    /// 从 SceneSetupHelper 抽取，消除其中大量重复的 GameObject 组装样板；
    /// 每个方法与原内联代码逐语句等价，生成结果完全一致。
    /// </summary>
    public static class SceneSetupPrimitives
    {
        /// <summary>创建正交主相机（纯色背景）。返回相机所在 GameObject。</summary>
        public static GameObject CreateCamera(float orthoSize, Color bg)
        {
            var camGo = new GameObject("Main Camera");
            var cam = camGo.AddComponent<Camera>();
            cam.orthographic = true;
            cam.orthographicSize = orthoSize;
            cam.clearFlags = CameraClearFlags.SolidColor;
            cam.backgroundColor = bg;
            return camGo;
        }

        /// <summary>创建 ScreenSpaceOverlay 画布（含 CanvasScaler 与 GraphicRaycaster）。</summary>
        public static GameObject CreateCanvas(string name)
        {
            var canvasGo = new GameObject(name);
            var canvas = canvasGo.AddComponent<Canvas>();
            canvas.renderMode = RenderMode.ScreenSpaceOverlay;
            canvasGo.AddComponent<CanvasScaler>();
            canvasGo.AddComponent<GraphicRaycaster>();
            return canvasGo;
        }

        /// <summary>创建带 SpriteRenderer 的世界空间对象；返回该 SpriteRenderer（用 .gameObject 取根对象）。</summary>
        public static SpriteRenderer NewSpriteObject(string name, Vector3 worldPos, Sprite sprite, int order)
        {
            var go = new GameObject(name);
            go.transform.position = worldPos;
            var sr = go.AddComponent<SpriteRenderer>();
            sr.sprite = sprite;
            sr.sortingOrder = order;
            return sr;
        }

        /// <summary>为对象添加冻结旋转的 Rigidbody2D（2D 平台跳跃常用）。</summary>
        public static Rigidbody2D AddFrozenRigidbody(GameObject go)
        {
            var rb = go.AddComponent<Rigidbody2D>();
            rb.constraints = RigidbodyConstraints2D.FreezeRotation;
            return rb;
        }

        /// <summary>为对象添加按 size 缩放的 SpriteRenderer。</summary>
        public static void AddScaledSprite(GameObject go, Vector2 size, Sprite sprite, int order)
        {
            go.transform.localScale = new Vector3(size.x, size.y, 1f);
            var sr = go.AddComponent<SpriteRenderer>();
            sr.sprite = sprite;
            sr.sortingOrder = order;
        }
    }
}
#endif
