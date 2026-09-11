using UnityEngine;
using UnityEngine.UI;
using Prototype.Core;

namespace Prototype.UI
{
    /// <summary>
    /// 结算画面：显示最终分数、收集统计与星级评价（按收集率 60%/80%/100% 给 1/2/3 星），
    /// 点击「再玩一次」重置本局并回到 Main。
    /// </summary>
    public class ResultUI : MonoBehaviour
    {
        [SerializeField] private Text finalScoreText;
        [SerializeField] private Text collectText;
        [SerializeField] private Text starsText;
        [SerializeField] private Button replayButton;

        private void Start()
        {
            var gm = GameManager.Instance;

            if (finalScoreText != null)
            {
                finalScoreText.text = $"最终分数: {gm.Score}";
            }

            int total = Mathf.Max(1, gm.TotalCollectibles);
            if (collectText != null)
            {
                collectText.text = $"收集: {gm.Collected} / {gm.TotalCollectibles}";
            }

            if (starsText != null)
            {
                float rate = gm.Collected / (float)total;
                int stars = rate >= 1f ? 3 : (rate >= 0.8f ? 2 : (rate >= 0.6f ? 1 : 0));
                starsText.text = new string('★', stars) + new string('☆', 3 - stars);
            }

            if (replayButton != null)
            {
                replayButton.onClick.AddListener(OnReplay);
            }
        }

        private void OnReplay()
        {
            GameManager.Instance.ResetScore();
            SceneManager.Instance.Load("Main");
        }
    }
}
