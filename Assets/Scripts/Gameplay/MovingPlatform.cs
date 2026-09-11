using UnityEngine;

namespace Prototype.Gameplay
{
    /// <summary>
    /// 上下浮动的移动平台。玩家站在上面时自动成为平台子对象，随平台一起移动；
    /// 离开后恢复独立，不影响跳跃等物理行为。
    /// </summary>
    [RequireComponent(typeof(BoxCollider2D))]
    public class MovingPlatform : SineBobber
    {
        [SerializeField] private float amplitude = 0.8f;
        [SerializeField] private float speed = 1.1f;

        protected override float Amplitude => amplitude;
        protected override float Speed => speed;

        private void FixedUpdate()
        {
            ApplyBob();
        }

        private void OnCollisionStay2D(Collision2D collision)
        {
            if (!collision.gameObject.CompareTag("Player")) return;

            // 只有玩家在平台上方时携带；侧碰/顶撞不携带
            if (collision.transform.position.y > transform.position.y + 0.05f)
            {
                collision.transform.SetParent(transform, true);
            }
        }

        private void OnCollisionExit2D(Collision2D collision)
        {
            if (collision.gameObject.CompareTag("Player"))
            {
                collision.transform.SetParent(null, true);
            }
        }
    }
}
