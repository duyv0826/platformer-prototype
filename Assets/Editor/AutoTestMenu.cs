#if UNITY_EDITOR
using UnityEditor;
using UnityEngine;

namespace Prototype.Editor
{
    /// <summary>
    /// 自动通关测试菜单：一键启动 Play 模式并运行自动测试。
    /// </summary>
    public static class AutoTestMenu
    {
        [MenuItem("Prototype/自动通关测试")]
        public static void StartAutoPlayTest()
        {
            if (!EditorApplication.isPlaying)
            {
                Debug.Log("[AutoTest] 进入 Play 模式并启动自动测试...");
                EditorApplication.playModeStateChanged += OnPlayModeChanged;
                EditorApplication.EnterPlaymode();
            }
            else
            {
                Debug.Log("[AutoTest] 已在 Play 模式，直接启动测试");
                TriggerAutoTest();
            }
        }

        private static void OnPlayModeChanged(PlayModeStateChange state)
        {
            if (state == PlayModeStateChange.EnteredPlayMode)
            {
                EditorApplication.playModeStateChanged -= OnPlayModeChanged;
                EditorApplication.delayCall += TriggerAutoTest;
            }
        }

        private static void TriggerAutoTest()
        {
            GameObject player = GameObject.Find("Player");
            if (player != null)
            {
                var controller = player.GetComponent<Prototype.Gameplay.PlayerController2D>();
                if (controller != null)
                {
                    var method = typeof(Prototype.Gameplay.PlayerController2D).GetMethod(
                        "StartAutoTest",
                        System.Reflection.BindingFlags.Public | System.Reflection.BindingFlags.Instance);
                    if (method != null)
                    {
                        method.Invoke(controller, null);
                        Debug.Log("[AutoTest] 自动测试已启动");
                    }
                    else
                    {
                        Debug.LogError("[AutoTest] 找不到 StartAutoTest 方法");
                    }
                }
                else
                {
                    Debug.LogError("[AutoTest] Player 上没有 PlayerController2D 组件");
                }
            }
            else
            {
                Debug.LogError("[AutoTest] 找不到 Player 对象");
            }
        }
    }
}
#endif
