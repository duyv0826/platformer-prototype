#if UNITY_EDITOR
using System.Collections;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;
using Prototype.Core;

namespace Prototype.Verification
{
    /// <summary>
    /// M1 通关闭环验证器。菜单 Prototype/验证/M1 通关闭环 触发：
    /// 打开 Main → 进入 Play → 模拟收集 8 次（80 分）→ 校验切换到 Result →
    /// 校验最终分数显示 → 点击「再玩一次」→ 校验回到 Main 且分数重置 → 退出 Play。
    /// 验证结果通过 Debug.Log 输出到 Console（含 PASS/FAIL 标记）。
    /// </summary>
    public static class M1ClosureVerifier
    {
        private static bool _active;

        [MenuItem("Prototype/验证/M1 通关闭环")]
        public static void Run()
        {
            _active = true;
            EditorSceneManager.OpenScene("Assets/Scenes/Main.unity");
            EditorApplication.EnterPlaymode();
        }

        [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.AfterSceneLoad)]
        private static void OnPlayStart()
        {
            if (!_active) return;
            _active = false;
            var go = new GameObject("M1ClosureVerifier");
            go.AddComponent<ClosureDriver>();
        }
    }

    public class ClosureDriver : MonoBehaviour
    {
        private void Awake()
        {
            DontDestroyOnLoad(gameObject);
        }

        private void Start()
        {
            StartCoroutine(Run());
        }

        private IEnumerator Run()
        {
            Debug.Log("[验证] ==== M1 通关闭环开始 ====");
            yield return new WaitForSeconds(0.6f);

            var gm = GameManager.Instance;
            if (gm == null)
            {
                Debug.LogError("[验证] FAIL : GameManager 不存在");
                Finish(false);
                yield break;
            }
            Check($"初始分数为 0（实际 {gm.Score}）", gm.Score == 0);

            // 模拟收集 8 次（每次 +10 → 80 分，等价于收集 8 个收集物）
            for (int i = 0; i < 8; i++)
            {
                gm.AddScore(10);
                yield return new WaitForSeconds(0.15f);
            }
            Check($"8 次收集后分数 = 80（实际 {gm.Score}）", gm.Score == 80);

            // 等待分数达标后自动切换到 Result
            yield return WaitScene("Result", 6f);
            bool switched = UnityEngine.SceneManagement.SceneManager.GetActiveScene().name == "Result";
            Check("分数达标后切换到 Result 场景", switched);
            if (!switched)
            {
                Finish(false);
                yield break;
            }

            // Result 场景：最终分数显示
            yield return new WaitForSeconds(0.6f);
            var finalText = FindText("FinalScore");
            bool scoreShown = finalText != null && finalText.text.Contains("80");
            Check($"Result 显示最终分数 80（实际: {(finalText != null ? finalText.text : "未找到 FinalScore")}）", scoreShown);

            // 点击「再玩一次」
            var replay = FindButton("ReplayButton");
            if (replay == null)
            {
                Debug.LogError("[验证] FAIL : 未找到 ReplayButton");
                Finish(false);
                yield break;
            }
            replay.onClick.Invoke();

            // 回到 Main 且分数重置
            yield return WaitScene("Main", 6f);
            bool back = UnityEngine.SceneManagement.SceneManager.GetActiveScene().name == "Main";
            Check("再玩一次回到 Main 场景", back);
            Check($"分数已重置为 0（实际 {gm.Score}）", gm.Score == 0);

            Finish(back && scoreShown && gm.Score == 0);
        }

        private static IEnumerator WaitScene(string name, float timeout)
        {
            float t = 0f;
            while (UnityEngine.SceneManagement.SceneManager.GetActiveScene().name != name && t < timeout)
            {
                t += Time.deltaTime;
                yield return null;
            }
        }

        private static Text FindText(string name)
        {
            foreach (var t in Object.FindObjectsOfType<Text>())
            {
                if (t.name == name) return t;
            }
            return null;
        }

        private static Button FindButton(string name)
        {
            foreach (var b in Object.FindObjectsOfType<Button>())
            {
                if (b.name == name) return b;
            }
            return null;
        }

        private static void Check(string message, bool ok)
        {
            Debug.Log($"[验证] {(ok ? "PASS" : "FAIL")} : {message}");
        }

        private static void Finish(bool success)
        {
            Debug.Log(success
                ? "[验证] ==== M1 通关闭环全部通过 ===="
                : "[验证] ==== M1 通关闭环存在失败项（见上方 FAIL）====");
            EditorApplication.ExitPlaymode();
        }
    }
}
#endif
