using UnityEngine;
using Prototype.Core;
using Prototype.Effects;
using Prototype.Audio;

namespace Prototype.Gameplay
{
    /// <summary>
    /// 地面巡逻敌人（Kenney slime 精灵，2 帧动画）。
    /// - 在 [minX, maxX] 区间左右来回移动。
    /// - 玩家从上方踩到（接触法线朝上）→ 敌人被消灭并 +10 分。
    /// - 玩家从侧面/下方碰到 → 玩家受伤（PlayerHealth.Damage）。
    /// </summary>
    [RequireComponent(typeof(Rigidbody2D))]
    public class PatrolEnemy : MonoBehaviour
    {
        [SerializeField] private float speed = 1.2f;
        [SerializeField] private float minX = 0f;
        [SerializeField] private float maxX = 4f;
        [SerializeField] private float animInterval = 0.3f;
        [SerializeField] private Sprite frameA;
        [SerializeField] private Sprite frameB;
        [SerializeField] private SpriteRenderer bodyRenderer;
        [SerializeField] private int killScore = 10;

        private Rigidbody2D _rb;
        private SpriteRenderer _sr;
        private int _dir = -1;
        private float _animTimer;

        private void Awake()
        {
            _rb = GetComponent<Rigidbody2D>();
            if (bodyRenderer != null) _sr = bodyRenderer;
        }

        private void Start()
        {
            minX = Mathf.Min(minX, transform.position.x - 0.01f);
            maxX = Mathf.Max(maxX, transform.position.x + 0.01f);
        }

        private void Update()
        {
            SpriteAnimUtil.TickTwoFrame(ref _animTimer, animInterval, _sr, frameA, frameB);
        }

        private void FixedUpdate()
        {
            Vector3 pos = transform.position;
            if (pos.x <= minX) _dir = 1;
            else if (pos.x >= maxX) _dir = -1;

            _rb.velocity = new Vector2(_dir * speed, _rb.velocity.y);

            if (_sr != null)
            {
                Vector3 s = transform.localScale;
                s.x = Mathf.Abs(s.x) * (_dir < 0 ? 1f : -1f);
                transform.localScale = s;
            }
        }

        private void OnCollisionEnter2D(Collision2D collision)
        {
            if (!collision.gameObject.CompareTag("Player")) return;

            Vector2 normal = collision.GetContact(0).normal;
            if (normal.y > 0.5f)
            {
                // 玩家踩头 → 消灭敌人
                GameManager.Instance.AddScore(killScore);
                FloatingText.Spawn(transform.position, $"+{killScore}", new Color(0.7f, 1f, 0.7f));
                CollectBurst.Spawn(transform.position, new Color(0.55f, 0.9f, 0.6f), 6);
                Sfx.Stomp();
                Destroy(gameObject);
            }
            else
            {
                var health = collision.gameObject.GetComponent<PlayerHealth>();
                if (health != null) health.Damage();
            }
        }
    }
}
