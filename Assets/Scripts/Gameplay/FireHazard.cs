using UnityEngine;

namespace Prototype.Gameplay
{
    /// <summary>
    /// 火焰陷阱：静态 hazards，2 帧动画。玩家触碰受伤（无敌时间内不重复扣血由 PlayerHealth 处理）。
    /// </summary>
    [RequireComponent(typeof(Collider2D))]
    public class FireHazard : MonoBehaviour
    {
        [SerializeField] private float animInterval = 0.2f;
        [SerializeField] private Sprite frameA;
        [SerializeField] private Sprite frameB;
        [SerializeField] private SpriteRenderer bodyRenderer;

        private SpriteRenderer _sr;
        private float _animTimer;

        private void Awake()
        {
            if (bodyRenderer != null) _sr = bodyRenderer;
        }

        private void Update()
        {
            SpriteAnimUtil.TickTwoFrame(ref _animTimer, animInterval, _sr, frameA, frameB);
        }

        private void OnTriggerEnter2D(Collider2D other)
        {
            if (!other.CompareTag("Player")) return;

            var health = other.GetComponent<PlayerHealth>();
            if (health != null) health.Damage();
        }

        private void OnCollisionEnter2D(Collision2D collision)
        {
            if (!collision.gameObject.CompareTag("Player")) return;

            var health = collision.gameObject.GetComponent<PlayerHealth>();
            if (health != null) health.Damage();
        }
    }
}
