using UnityEngine;
using Prototype.Core;

namespace Prototype.Gameplay
{
    /// <summary>
    /// 过关触发条件：订阅分数变化事件，达到目标分数后切换到下一场景（只触发一次）。
    /// 通过 EventBus 解耦，不直接引用任何 UI 或玩家对象。
    /// </summary>
    public class WinCondition : MonoBehaviour
    {
        [SerializeField] private int targetScore = 50;
        [SerializeField] private string nextSceneName = "Result";
        [SerializeField] private bool loadAsync = false;

        private bool _triggered;

        private void OnEnable()
        {
            EventBus.Subscribe<ScoreChangedEvent>(OnScoreChanged);
        }

        private void OnDisable()
        {
            EventBus.Unsubscribe<ScoreChangedEvent>(OnScoreChanged);
        }

        private void OnScoreChanged(ScoreChangedEvent e)
        {
            if (_triggered) return;
            if (e.Total < targetScore) return;

            _triggered = true;
            if (loadAsync)
            {
                SceneManager.Instance.LoadAsync(nextSceneName);
            }
            else
            {
                SceneManager.Instance.Load(nextSceneName);
            }
        }
    }
}