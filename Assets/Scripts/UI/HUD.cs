using UnityEngine;
using UnityEngine.UI;
using Prototype.Core;

namespace Prototype.UI
{
    /// <summary>
    /// HUD：订阅分数/生命事件刷新「分数」「目标」「生命心形」与收集进度条。
    /// 通过 EventBus 与 GameManager 解耦。
    /// </summary>
    public class HUD : MonoBehaviour
    {
        public Text scoreText;
        public Text hintText;
        public Text comboText;
        public Image progressFill;
        public Image[] hearts;

        [SerializeField] private string scoreFormat = "分数: {0}";
        [SerializeField] private int targetScore = 60;
        [SerializeField] private int maxLives = 3;
        [SerializeField] private Sprite fullHeart;
        [SerializeField] private Sprite emptyHeart;
        [SerializeField] private string hintDefault;
        [SerializeField] private string hintDone = "分数越高星级越高，直接冲向旗帜吧！";

        private void OnEnable()
        {
            EventBus.Subscribe<ScoreChangedEvent>(OnScoreChanged);
            EventBus.Subscribe<LivesChangedEvent>(OnLivesChanged);
            EventBus.Subscribe<ComboChangedEvent>(OnComboChanged);

            if (string.IsNullOrEmpty(hintDefault))
            {
                hintDefault = "到达旗帜即可通关！收集越多星级越高！连续收集有连击加成！";
            }
            if (hintText != null) hintText.text = hintDefault;
            if (scoreText != null) scoreText.text = string.Format(scoreFormat, 0);
            SetProgress(0);
            UpdateHearts(GameManager.Instance != null ? GameManager.Instance.Lives : maxLives);
            UpdateCombo(1);
        }

        private void OnDisable()
        {
            EventBus.Unsubscribe<ScoreChangedEvent>(OnScoreChanged);
            EventBus.Unsubscribe<LivesChangedEvent>(OnLivesChanged);
            EventBus.Unsubscribe<ComboChangedEvent>(OnComboChanged);
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

            SetProgress(e.Total);
        }

        private void OnLivesChanged(LivesChangedEvent e)
        {
            UpdateHearts(e.Lives);
        }

        private void OnComboChanged(ComboChangedEvent e)
        {
            UpdateCombo(e.Combo);
        }

        private void UpdateCombo(int combo)
        {
            if (comboText == null) return;
            if (combo >= 2)
            {
                comboText.text = $"连击 x{combo}！";
                comboText.gameObject.SetActive(true);
            }
            else
            {
                comboText.gameObject.SetActive(false);
            }
        }

        private void UpdateHearts(int lives)
        {
            if (hearts == null) return;
            bool useSprites = fullHeart != null && emptyHeart != null;
            for (int i = 0; i < hearts.Length; i++)
            {
                if (hearts[i] == null) continue;
                bool full = i < lives;
                if (useSprites)
                {
                    hearts[i].sprite = full ? fullHeart : emptyHeart;
                    hearts[i].color = Color.white;
                }
                else
                {
                    hearts[i].color = full ? Color.white : new Color(0.2f, 0.2f, 0.2f, 0.5f);
                }
            }
        }

        private void SetProgress(int score)
        {
            if (progressFill == null) return;
            float ratio = Mathf.Clamp01(score / (float)targetScore);
            progressFill.rectTransform.sizeDelta = new Vector2(280f * ratio, 12f);
        }
    }
}
