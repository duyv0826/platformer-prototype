using UnityEngine;
using Prototype.Core;
using Prototype.Effects;
using Prototype.Audio;

namespace Prototype.Gameplay
{
    /// <summary>
    /// 可收集物（2D）。玩家（带 "Player" 标签）进入触发区时：
    /// 1) 向 GameManager 登记收集（自动计算连击倍率，分值 = 基础分 × 连击）；
    /// 2) 生成飘分文字与彩色粒子反馈，播放收集音效；3) 自我销毁。
    /// 分值由场景配置（红心 5 分 / 蓝宝石 15 分）。
    /// 需要：本对象 Collider2D 设为 Is Trigger；玩家对象带 "Player" 标签且有碰撞体。
    /// </summary>
    public class Collectible2D : MonoBehaviour
    {
        [SerializeField] private int scoreValue = 5;

        private bool _collected;

        private void OnTriggerEnter2D(Collider2D other)
        {
            if (_collected) return;
            if (!other.CompareTag("Player")) return;

            _collected = true;
            int combo = GameManager.Instance.RegisterCollect(scoreValue);
            int gained = scoreValue * combo;

            FloatingText.Spawn(transform.position, $"+{gained}",
                combo >= 3 ? new Color(1f, 0.85f, 0.2f) : Color.white);
            CollectBurst.Spawn(transform.position,
                combo >= 3 ? new Color(1f, 0.8f, 0.25f) : new Color(1f, 0.5f, 0.7f));
            Sfx.Collect();

            Destroy(gameObject);
        }
    }
}
