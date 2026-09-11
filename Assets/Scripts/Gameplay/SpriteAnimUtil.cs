using UnityEngine;

namespace Prototype.Gameplay
{
    /// <summary>
    /// 2 帧精灵动画的公共逻辑（供 FireHazard / PatrolEnemy 等复用，消除重复代码）。
    /// 纯静态工具：调用方保留自己的计时器与序列化字段，行为与内联版本逐 token 等价。
    /// </summary>
    public static class SpriteAnimUtil
    {
        /// <summary>
        /// 按固定间隔在 frameA / frameB 之间翻转精灵。
        /// </summary>
        /// <param name="timer">调用方持有的累计计时器（ref，达到间隔会被重置）。</param>
        /// <param name="interval">翻帧间隔（秒）。</param>
        public static void TickTwoFrame(ref float timer, float interval, SpriteRenderer sr, Sprite frameA, Sprite frameB)
        {
            if (sr == null || frameA == null || frameB == null) return;

            timer += Time.deltaTime;
            if (timer >= interval)
            {
                timer = 0f;
                sr.sprite = sr.sprite == frameA ? frameB : frameA;
            }
        }
    }
}
