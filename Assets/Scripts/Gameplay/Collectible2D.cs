using UnityEngine;
using Prototype.Core;

namespace Prototype.Gameplay
{
    /// <summary>
    /// 可收集物（2D）。玩家（带 "Player" 标签）进入触发区时，向 GameManager 累加分数并自我销毁。
    /// 计分通过事件总线广播，UI 自动刷新，本组件不依赖任何 UI 引用。
    /// 需要：本对象 Collider2D 设为 Is Trigger；玩家对象带 "Player" 标签且有碰撞体。
    /// </summary>
    public class Collectible2D : MonoBehaviour
    {
        [SerializeField] private int scoreValue = 10;

        private bool _collected;

        private void OnTriggerEnter2D(Collider2D other)
        {
            if (_collected) return;
            if (!other.CompareTag("Player")) return;

            _collected = true;
            GameManager.Instance.AddScore(scoreValue);
            Destroy(gameObject);
        }
    }
}