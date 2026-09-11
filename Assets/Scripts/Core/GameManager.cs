using UnityEngine;

namespace Prototype.Core
{
    /// <summary>
    /// 全局游戏状态管理（单例）。
    /// 负责：当前游戏状态机、分数、生命值、收集统计等跨场景共享的全局数据。
    /// 状态/数据变化通过 EventBus 广播，UI 或其它系统订阅即可，无需直接引用 GameManager。
    ///
    /// 使用：GameManager.Instance.ChangeState(GameState.Playing);
    /// </summary>
    public class GameManager : MonoSingleton<GameManager>
    {
        public const int MaxLives = 3;
        public const float ComboWindow = 4f;
        public const int ComboCap = 5;

        public GameState CurrentState { get; private set; } = GameState.Boot;
        public int Score { get; private set; }
        public int Lives { get; private set; } = MaxLives;
        public int Collected { get; private set; }
        public int TotalCollectibles { get; set; }
        public bool Won { get; private set; }
        public int ComboCount { get; private set; } = 1;

        private float _lastCollectTime = -999f;

        protected override void OnAwake()
        {
            CurrentState = GameState.Boot;
            Score = 0;
            Lives = MaxLives;
            Collected = 0;
            TotalCollectibles = 0;
            Won = false;
            ComboCount = 1;
            _lastCollectTime = -999f;
        }

        private void Update()
        {
            // 连击超时自动归零
            if (ComboCount > 1 && Time.time - _lastCollectTime > ComboWindow)
            {
                ComboCount = 1;
                EventBus.Publish(new ComboChangedEvent(1));
            }
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

        /// <summary>记录收集：连击窗口内连续收集则连击 +1（最高 x5），按 分值×连击 计分。返回本次连击倍率。</summary>
        public int RegisterCollect(int delta)
        {
            Collected++;

            if (Time.time - _lastCollectTime <= ComboWindow)
            {
                ComboCount = Mathf.Min(ComboCap, ComboCount + 1);
            }
            else
            {
                ComboCount = 1;
            }
            _lastCollectTime = Time.time;

            AddScore(delta * ComboCount);
            EventBus.Publish(new ComboChangedEvent(ComboCount));
            return ComboCount;
        }

        /// <summary>扣除 1 生命。返回是否存活。</summary>
        public bool LoseLife()
        {
            Lives = Mathf.Max(0, Lives - 1);
            EventBus.Publish(new LivesChangedEvent(Lives));
            return Lives > 0;
        }

        public void ResetScore()
        {
            Score = 0;
            Lives = MaxLives;
            Collected = 0;
            Won = false;
            ComboCount = 1;
            _lastCollectTime = -999f;
            EventBus.Publish(new ScoreChangedEvent(Score, 0));
            EventBus.Publish(new LivesChangedEvent(Lives));
            EventBus.Publish(new ComboChangedEvent(1));
        }

        public void MarkWon()
        {
            Won = true;
        }
    }
}
