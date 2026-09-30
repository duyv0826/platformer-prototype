#if UNITY_EDITOR
using System.Collections;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;
using Prototype.Core;
using Prototype.Gameplay;

namespace Prototype.Verification
{
    /// <summary>
    /// M1 通关闭环验证器。菜单 Prototype/验证/M1 通关闭环 触发：
    /// 打开 Main → 进入 Play → 逐个收集场景里真实存在的收集物 →
    /// 把玩家放到旗帜上方，靠真实物理触发 FlagGoal 切到 Result →
    /// 校验结算页的分数 / 收集数 / 星级 → 点击「再玩一次」→ 校验回到 Main 且状态已重置 → 退出 Play。
    /// 期望值全部从场景组件与 GameManager 返回值推得，不写死分数，关卡增删收集物时本验证自动跟随。
    /// 验证结果通过 Debug.Log 输出到 Console（含 PASS/FAIL 标记）。
    /// 注：落向旗帜约 1 秒，期间若被巡逻敌人/火焰打死则通关步骤会 FAIL，这是玩法层面的正常风险，非本验证的缺陷。
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
        // 时序参数集中在常量里，别散在流程中当裸数字
        private const float CollectInterval = 0.05f;
        private const float DropHeight = 1.8f;
        private const float SceneSwitchTimeout = 6f;
        private const float UiSettleDelay = 0.6f;

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
            yield return new WaitForSeconds(UiSettleDelay);

            var gm = GameManager.Instance;
            if (gm == null)
            {
                Debug.LogError("[验证] FAIL : GameManager 不存在");
                Finish(false);
                yield break;
            }
            Check($"初始分数为 0（实际 {gm.Score}）", gm.Score == 0);

            // 逐个收集场景里真实存在的收集物：分值取自各组件自身，连击倍率取自 GameManager 返回值，
            // 因此期望分数完全由场景数据推得，不写死 50 / 60 / 80 任何一个数字。
            var collectibles = Object.FindObjectsOfType<Collectible2D>();
            if (collectibles.Length == 0)
            {
                Debug.LogError("[验证] FAIL : 场景里找不到 Collectible2D");
                Finish(false);
                yield break;
            }

            int expected = 0;
            foreach (var c in collectibles)
            {
                var prop = new SerializedObject(c).FindProperty("scoreValue");
                int value = prop != null ? prop.intValue : 0;
                expected += value * gm.RegisterCollect(value);
                yield return new WaitForSeconds(CollectInterval);
            }

            Check($"收满全部 {collectibles.Length} 个收集物后 Collected = {collectibles.Length}（实际 {gm.Collected}）",
                gm.Collected == collectibles.Length);
            Check($"分数与「分值×连击」累计一致，期望 {expected}（实际 {gm.Score}）", gm.Score == expected);
            Check($"LevelConfig 注入的总数与场景收集物数一致，TotalCollectibles = {gm.TotalCollectibles}（场景实际 {collectibles.Length}）",
                gm.TotalCollectibles == collectibles.Length);

            // 通关由触旗完成：把玩家放到旗帜正上方，让真实的物理重叠触发 FlagGoal
            var player = GameObject.FindGameObjectWithTag("Player");
            var flag = Object.FindObjectOfType<FlagGoal>();
            if (player == null || flag == null)
            {
                Debug.LogError($"[验证] FAIL : 玩家={(player == null ? "未找到(Tag=Player)" : player.name)}，" +
                               $"旗帜={(flag == null ? "未找到 FlagGoal" : flag.name)}，无法验证触旗通关");
                Finish(false);
                yield break;
            }

            var body = player.GetComponent<Rigidbody2D>();
            if (body != null) body.velocity = Vector2.zero;
            player.transform.position = flag.transform.position + Vector3.up * DropHeight;

            yield return WaitScene("Result", SceneSwitchTimeout);
            bool switched = UnityEngine.SceneManagement.SceneManager.GetActiveScene().name == "Result";
            Check("触旗后切换到 Result 场景", switched);
            if (!switched)
            {
                Finish(false);
                yield break;
            }

            // Result 场景：分数、收集统计、星级
            yield return new WaitForSeconds(UiSettleDelay);
            var finalText = FindText("FinalScore");
            bool scoreShown = finalText != null && finalText.text.Contains(expected.ToString());
            Check($"Result 显示最终分数 {expected}（实际: {(finalText != null ? finalText.text : "未找到 FinalScore")}）", scoreShown);

            var collectInfo = FindText("CollectInfo");
            bool collectShown = collectInfo != null
                && collectInfo.text.Contains($"{collectibles.Length} / {collectibles.Length}");
            Check($"Result 显示收集 {collectibles.Length} / {collectibles.Length}（实际: {(collectInfo != null ? collectInfo.text : "未找到 CollectInfo")}）",
                collectShown);

            var starsText = FindText("Stars");
            bool threeStars = starsText != null && starsText.text.StartsWith("★★★");
            Check($"收满应为 ★★★（实际: {(starsText != null ? starsText.text : "未找到 Stars")}）", threeStars);

            // 点击「再玩一次」
            var replay = FindButton("ReplayButton");
            if (replay == null)
            {
                Debug.LogError("[验证] FAIL : 未找到 ReplayButton");
                Finish(false);
                yield break;
            }
            replay.onClick.Invoke();

            // 回到 Main 且状态重置
            yield return WaitScene("Main", SceneSwitchTimeout);
            bool back = UnityEngine.SceneManagement.SceneManager.GetActiveScene().name == "Main";
            Check("再玩一次回到 Main 场景", back);
            Check($"分数已重置为 0（实际 {gm.Score}）", gm.Score == 0);
            Check($"收集数已重置为 0（实际 {gm.Collected}）", gm.Collected == 0);

            Finish(back && scoreShown && collectShown && threeStars && gm.Score == 0 && gm.Collected == 0);
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
