using UnityEditor;
using UnityEngine;

namespace Prototype.Editor
{
    [InitializeOnLoad]
    public static class AutoTestLauncher
    {
        private static bool _waitingForPlayer = false;

        static AutoTestLauncher()
        {
            EditorApplication.playModeStateChanged += OnPlayModeChanged;
            EditorApplication.update += OnEditorUpdate;
            Debug.Log("[AutoTestLauncher] Initialized");
        }

        private static void OnPlayModeChanged(PlayModeStateChange state)
        {
            Debug.Log("[AutoTestLauncher] Play mode state: " + state);
            if (state == PlayModeStateChange.EnteredPlayMode)
            {
                _waitingForPlayer = true;
            }
            else if (state == PlayModeStateChange.ExitingPlayMode)
            {
                _waitingForPlayer = false;
            }
        }

        private static void OnEditorUpdate()
        {
            // 已停用：旧版自动测试会在进入 Play 后给玩家挂 AutoTestRunner 并禁用手动控制，
            // 导致试玩时玩家不受键盘控制。完整闭环验证请用菜单 Prototype/验证/M1 通关闭环。
            // if (_waitingForPlayer && EditorApplication.isPlaying)
            // {
            //     ... 挂载逻辑已移除 ...
            // }
        }
    }
}
