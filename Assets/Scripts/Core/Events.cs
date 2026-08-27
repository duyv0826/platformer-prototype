namespace Prototype.Core
{
    /// <summary>
    /// 示例事件结构。通过 EventBus 发布/订阅，实现模块间解耦。
    /// 新增事件时，在此文件（或独立文件）定义 readonly struct 即可。
    /// 使用 readonly struct 保证事件数据不可变、零 GC 分配（配合 EventBus 的泛型约束）。
    /// </summary>

    public enum GameState
    {
        Boot,
        MainMenu,
        Playing,
        Paused,
        GameOver
    }

    public readonly struct GameStateChangedEvent
    {
        public readonly GameState Previous;
        public readonly GameState Current;

        public GameStateChangedEvent(GameState previous, GameState current)
        {
            Previous = previous;
            Current = current;
        }
    }

    public readonly struct ScoreChangedEvent
    {
        public readonly int Total;
        public readonly int Delta;

        public ScoreChangedEvent(int total, int delta)
        {
            Total = total;
            Delta = delta;
        }
    }

    public readonly struct SceneLoadedEvent
    {
        public readonly string SceneName;

        public SceneLoadedEvent(string sceneName)
        {
            SceneName = sceneName;
        }
    }
}