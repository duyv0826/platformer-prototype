#if UNITY_EDITOR
using UnityEditor;

namespace Prototype.Editor
{
    /// <summary>
    /// 首次打开工程时，确保 Default Behavior Mode 为 2D（幂等，只处理一次）。
    /// 这样 Unity 后续新建的 GameObject 默认挂 SpriteRenderer 而非 MeshFilter，
    /// 与 2D 平台跳跃模板保持一致。
    /// 该值最终持久化在 ProjectSettings/EditorSettings.asset 的 m_DefaultBehaviorMode。
    /// </summary>
    [InitializeOnLoad]
    public static class Platformer2DSettings
    {
        private const string SessionKey = "platformer_2d_mode_applied";

        static Platformer2DSettings()
        {
            if (SessionState.GetBool(SessionKey, false)) return;
            SessionState.SetBool(SessionKey, true);

            if (EditorSettings.defaultBehaviorMode == EditorBehaviorMode.Mode2D) return;

            EditorSettings.defaultBehaviorMode = EditorBehaviorMode.Mode2D;
            AssetDatabase.SaveAssets();
            UnityEngine.Debug.Log("[Prototype] 已将 Default Behavior Mode 设为 2D。");
        }
    }
}
#endif