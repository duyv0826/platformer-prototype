using UnityEngine;
using UnityEngine.UI;
using Prototype.Core;

namespace Prototype.UI
{
    /// <summary>
    /// 结算画面逻辑：显示最终分数（取自持久化的 GameManager），点击「再玩一次」重置分数并回到 Main。
    /// </summary>
    public class ResultUI : MonoBehaviour
    {
        [SerializeField] private Text finalScoreText;
        [SerializeField] private Button replayButton;

        private void Start()
        {
            if (finalScoreText != null)
            {
                finalScoreText.text = $"最终分数: {GameManager.Instance.Score}";
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