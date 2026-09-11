using UnityEngine;
using Prototype.Core;
using Prototype.Audio;

namespace Prototype.Gameplay
{
    /// <summary>
    /// 终点旗帜：玩家触碰后通关（标记胜利、播放音效、加载 Result 场景）。
    /// 触旗即可通关，分数/收集率决定结算星级。
    /// </summary>
    public class FlagGoal : MonoBehaviour
    {
        [SerializeField] private string nextSceneName = "Result";
        [SerializeField] private float settleDelay = 0.6f;

        private bool _triggered;

        private void OnTriggerEnter2D(Collider2D other)
        {
            if (_triggered) return;
            if (!other.CompareTag("Player")) return;

            _triggered = true;
            GameManager.Instance.MarkWon();
            Sfx.Win();
            Invoke(nameof(GoToResult), settleDelay);
        }

        private void GoToResult()
        {
            SceneManager.Instance.Load(nextSceneName);
        }
    }
}
