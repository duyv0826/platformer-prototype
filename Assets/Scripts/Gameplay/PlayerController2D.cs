using UnityEngine;

namespace Prototype.Gameplay
{
    /// <summary>
    /// 2D 平台跳跃玩家控制。
    /// 需要对象上挂有 Rigidbody2D 与碰撞体（BoxCollider2D / CapsuleCollider2D）。
    /// - 左右移动：A/D 或方向键（Legacy Input 的 Horizontal 轴）。
    /// - 跳跃：空格（Legacy Input 的 Jump 轴，默认已映射），仅在地面时可跳。
    /// - 朝向：移动时左右翻转精灵。
    /// </summary>
    [RequireComponent(typeof(Rigidbody2D))]
    public class PlayerController2D : MonoBehaviour
    {
        [SerializeField] private float moveSpeed = 6f;
        [SerializeField] private float jumpForce = 9f;
        [SerializeField] private float groundCheckDistance = 0.6f;
        [Tooltip("留 0 时在 Awake 中自动解析为 Ground 图层")]
        [SerializeField] private LayerMask groundMask;
        [Tooltip("留空则使用自身位置作为地面检测起点")]
        [SerializeField] private Transform groundCheck;

        private Rigidbody2D _rb;
        private bool _facingRight = true;

        private void Awake()
        {
            _rb = GetComponent<Rigidbody2D>();
            if (groundCheck == null)
            {
                groundCheck = transform;
            }
            if (groundMask == 0)
            {
                groundMask = LayerMask.GetMask("Ground");
            }
        }

        private void Update()
        {
            if (Input.GetButtonDown("Jump") && IsGrounded())
            {
                _rb.velocity = new Vector2(_rb.velocity.x, jumpForce);
            }
        }

        private void FixedUpdate()
        {
            float h = Input.GetAxisRaw("Horizontal");
            _rb.velocity = new Vector2(h * moveSpeed, _rb.velocity.y);

            if (h > 0.05f && !_facingRight) Flip();
            else if (h < -0.05f && _facingRight) Flip();
        }

        private bool IsGrounded()
        {
            return Physics2D.Raycast(groundCheck.position, Vector2.down, groundCheckDistance, groundMask).collider != null;
        }

        private void Flip()
        {
            _facingRight = !_facingRight;
            Vector3 scale = transform.localScale;
            scale.x *= -1f;
            transform.localScale = scale;
        }
    }
}