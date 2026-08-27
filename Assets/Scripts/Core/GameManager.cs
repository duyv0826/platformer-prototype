using UnityEngine;

namespace Prototype.Core
{
    /// <summary>
    /// 全局游戏状态管理（单例）。
    /// 负责：当前游戏状态机、分数等跨场景共享的全局数据。
    /// 状态/分数变化通过 EventBus 广播，UI 或其它系统订阅即可，无需直接引用 GameManager。
    ///
    /// 使用：GameManager.Instance.ChangeState(GameState.Playing);
    /// </summary>
    public class GameManager : MonoSingleton<GameManager>
    {
        public GameState CurrentState { get; private set; } = GameState.Boot;
        public int Score { get; private set; }

        protected override void OnAwake()
        {
            CurrentState = GameState.Boot;
            Score = 0;
        }

        public void ChangeState(GameState newState)
        {
            if (newState == CurrentState) return;

            var previous = CurrentState;
            CurrentState = newState;
            EventBus.Publish(new GameStateChangedEvent(previous, newState));
        }

        public void AddScore(int amount)
        {
            Score += amount;
            EventBus.Publish(new ScoreChangedEvent(Score, amount));
        }

        public void ResetScore()
        {
            Score = 0;
            EventBus.Publish(new ScoreChangedEvent(Score, 0));
        }
    }
}