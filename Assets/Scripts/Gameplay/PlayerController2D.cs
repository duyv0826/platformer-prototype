using UnityEngine;
using Prototype.Audio;

namespace Prototype.Gameplay
{
    /// <summary>
    /// 2D 平台跳跃玩家控制（像素角色版）。
    /// - 移动：A/D 或方向键，地面线性加速、空中弱控制。
    /// - 跳跃：空格。支持「土狼时间」（离开平台边缘短时间内仍可跳）与「跳跃缓冲」（落地前按键会被记住），
    ///   提前松键触发跳砍（短按跳得低），按住下方向/S 快速下落。
    /// - 重力：非对称（上升轻、下降重），落地干脆利落；带最大下落速度上限。
    /// - 朝向：移动时左右翻转。
    /// </summary>
    [RequireComponent(typeof(Rigidbody2D))]
    public class PlayerController2D : MonoBehaviour
    {
        [SerializeField] private float moveSpeed = 5.5f;
        [SerializeField] private float jumpForce = 8.6f;
        [SerializeField] private float groundAccel = 55f;
        [SerializeField] private float airControl = 0.55f;
        [SerializeField] private float jumpCutMultiplier = 0.45f;

        [Header("非对称重力")]
        [SerializeField] private float riseGravityScale = 1.15f;
        [SerializeField] private float fallGravityScale = 2f;
        [SerializeField] private float fastFallGravity = 3.2f;
        [SerializeField] private float maxFallSpeed = -16f;
        [SerializeField] private float fastFallMaxSpeed = -24f;

        [Header("容错")]
        [SerializeField] private float groundCheckDistance = 0.45f;
        [SerializeField] private float coyoteTime = 0.14f;
        [SerializeField] private float jumpBufferTime = 0.18f;
        [Tooltip("留 0 时在 Awake 中自动解析为 Ground 图层")]
        [SerializeField] private LayerMask groundMask;
        [Tooltip("留空则使用自身位置作为地面检测起点")]
        [SerializeField] private Transform groundCheck;

        [Header("动画")]
        [SerializeField] private Sprite idleSprite;
        [SerializeField] private Sprite jumpSprite;
        [SerializeField] private SpriteRenderer bodyRenderer;

        private Rigidbody2D _rb;
        private SpriteRenderer _sr;
        private bool _facingRight = true;

        private float _lastGroundedTime = -10f;
        private float _jumpBufferCounter = -10f;

        private void Awake()
        {
            _rb = GetComponent<Rigidbody2D>();
            if (groundCheck == null) groundCheck = transform;
            if (groundMask == 0) groundMask = LayerMask.GetMask("Ground");
            if (bodyRenderer != null) _sr = bodyRenderer;
        }

        private void Update()
        {
            // 跳跃缓冲：按键后短时间内在落地瞬间自动起跳
            if (Input.GetButtonDown("Jump"))
            {
                _jumpBufferCounter = jumpBufferTime;
            }
            else
            {
                _jumpBufferCounter -= Time.deltaTime;
            }

            if (IsGrounded())
            {
                _lastGroundedTime = Time.time;
            }

            // 土狼时间：刚离开平台时仍允许跳跃
            if (_jumpBufferCounter > 0f && Time.time - _lastGroundedTime <= coyoteTime)
            {
                _rb.velocity = new Vector2(_rb.velocity.x, jumpForce);
                _jumpBufferCounter = -1f;
                Sfx.Jump();
            }
            else if (Input.GetButtonUp("Jump") && _rb.velocity.y > 0f)
            {
                // 跳砍：提前松键让上升速度骤减，实现"按多久跳多高"
                _rb.velocity = new Vector2(_rb.velocity.x, _rb.velocity.y * jumpCutMultiplier);
            }

            UpdateSprite();
        }

        private void FixedUpdate()
        {
            float h = Input.GetAxisRaw("Horizontal");
            float targetVx = h * moveSpeed;
            float accel = IsGrounded() ? groundAccel : groundAccel * airControl;
            _rb.velocity = new Vector2(
                Mathf.MoveTowards(_rb.velocity.x, targetVx, accel * Time.fixedDeltaTime),
                _rb.velocity.y);

            ApplyGravity();

            if (h > 0.05f && !_facingRight) Flip();
            else if (h < -0.05f && _facingRight) Flip();
        }

        /// <summary>非对称重力：上升轻（跳得高）、下降重（落地干脆）；快速下落与最大下落速度上限。</summary>
        private void ApplyGravity()
        {
            float vy = _rb.velocity.y;
            float g;
            bool fastFalling = Input.GetAxisRaw("Vertical") < 0f && vy < 0f;

            if (fastFalling)
            {
                g = fastFallGravity;
            }
            else if (vy > 0.5f)
            {
                g = riseGravityScale;
            }
            else
            {
                g = fallGravityScale;
            }
            _rb.gravityScale = g;

            // 下落速度上限：正常下落较轻，快速下落允许更快
            float maxFall = fastFalling ? fastFallMaxSpeed : maxFallSpeed;
            if (vy < maxFall)
            {
                _rb.velocity = new Vector2(_rb.velocity.x, maxFall);
            }
        }

        private bool IsGrounded()
        {
            return Physics2D.Raycast(groundCheck.position, Vector2.down, groundCheckDistance, groundMask).collider != null;
        }

        private void UpdateSprite()
        {
            if (_sr == null) return;

            bool onGround = IsGrounded();
            float vy = _rb.velocity.y;

            if (!onGround && vy > 0.5f && jumpSprite != null)
            {
                _sr.sprite = jumpSprite;
            }
            else if (!onGround && vy < -0.5f && jumpSprite != null)
            {
                _sr.sprite = jumpSprite;
            }
            else if (idleSprite != null)
            {
                _sr.sprite = idleSprite;
            }
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
