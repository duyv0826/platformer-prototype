using UnityEngine;
using UnityEngine.UI;
using Prototype.Core;

namespace Prototype.UI
{
    /// <summary>
    /// 简易 HUD：订阅分数事件刷新「分数」文本，并在达标时更新「提示」文本。
    /// 通过 EventBus 与 GameManager 解耦。
    /// </summary>
    public class HUD : MonoBehaviour
    {
        public Text scoreText;
        public Text hintText;

        [SerializeField] private string scoreFormat = "分数: {0}";
        [SerializeField] private int targetScore = 50;
        [SerializeField] private string hintDefault;
        [SerializeField] private string hintDone = "过关！正在切换场景...";

        private void OnEnable()
        {
            EventBus.Subscribe<ScoreChangedEvent>(OnScoreChanged);
            if (string.IsNullOrEmpty(hintDefault))
            {
                hintDefault = $"收集物品达到 {targetScore} 分进入下一关";
            }
            if (hintText != null) hintText.text = hintDefault;
            if (scoreText != null) scoreText.text = string.Format(scoreFormat, 0);
        }

        private void OnDisable()
        {
            EventBus.Unsubscribe<ScoreChangedEvent>(OnScoreChanged);
        }

        private void OnScoreChanged(ScoreChangedEvent e)
        {
            if (scoreText != null)
            {
                scoreText.text = string.Format(scoreFormat, e.Total);
            }

            if (hintText != null && e.Total >= targetScore)
            {
                hintText.text = hintDone;
            }
        }
    }
}