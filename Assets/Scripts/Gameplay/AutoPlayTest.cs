using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;

namespace Prototype.Gameplay
{
    /// <summary>
    /// 自动通关测试脚本：自动控制玩家收集所有收集物，记录测试数据。
    /// 按 T 键开始自动测试。
    /// </summary>
    public class AutoPlayTest : MonoBehaviour
    {
        public float moveSpeed = 6f;
        public float jumpForce = 9f;
        public float groundCheckDistance = 0.6f;
        public LayerMask groundMask;

        private Rigidbody2D _rb;
        private Transform _groundCheck;
        private bool _isRunning = false;
        private int _currentTargetIndex = 0;
        private List<Vector3> _collectiblePositions = new List<Vector3>();
        private float _startTime = 0f;
        private int _lastScore = 0;
        private int _platformAJumpAttempts = 0;
        private int _platformBJumpAttempts = 0;
        private int _platformCJumpAttempts = 0;
        private bool _onPlatformA = false;
        private bool _onPlatformB = false;
        private bool _onPlatformC = false;

        private void Awake()
        {
            _rb = GetComponent<Rigidbody2D>();
            if (groundMask == 0) groundMask = LayerMask.GetMask("Ground");
        }

        private void Start()
        {
            // 收集所有 Collectible 的位置（按 x 排序）
            GameObject[] collectibles = GameObject.FindGameObjectsWithTag("Collectible");
            if (collectibles.Length == 0)
            {
                // 如果没 tag，按名字找
                AutoTestUtil.GatherByNamePrefix(_collectiblePositions, "Collectible_");
            }
            else
            {
                foreach (GameObject go in collectibles)
                {
                    _collectiblePositions.Add(go.transform.position);
                }
            }

            // 按 x 坐标排序（从左到右）
            AutoTestUtil.SortByX(_collectiblePositions);

            Debug.Log("[AutoPlayTest] 找到 " + _collectiblePositions.Count + " 个收集物");
            for (int i = 0; i < _collectiblePositions.Count; i++)
            {
                Debug.Log("  收集物 " + i + ": (" + _collectiblePositions[i].x.ToString("F1") + ", " + _collectiblePositions[i].y.ToString("F1") + ")");
            }

            Debug.Log("[AutoPlayTest] 按 T 键开始自动通关测试");
            Debug.Log("[AutoPlayTest] 玩家初始位置: " + transform.position.ToString("F1"));
        }

        private void Update()
        {
            if (Input.GetKeyDown(KeyCode.T) && !_isRunning)
            {
                StartTest();
            }
        }

        public void StartTest()
        {
            _isRunning = true;
            _startTime = Time.time;
            _currentTargetIndex = 0;
            Debug.Log("[AutoPlayTest] ===== 自动通关测试开始 =====");
            StartCoroutine(AutoPlayRoutine());
        }

        private bool IsGrounded()
        {
            return AutoTestUtil.Grounded(transform.position, groundCheckDistance, groundMask);
        }

        private IEnumerator AutoPlayRoutine()
        {
            // 等待玩家落地
            yield return new WaitUntil(() => IsGrounded());
            Debug.Log("[AutoPlayTest] 玩家已落地，开始移动");

            // 逐个收集收集物
            while (_currentTargetIndex < _collectiblePositions.Count)
            {
                Vector3 target = _collectiblePositions[_currentTargetIndex];
                Debug.Log("[AutoPlayTest] 目标收集物 " + _currentTargetIndex + ": (" + target.x.ToString("F1") + ", " + target.y.ToString("F1") + ")");

                // 移动到目标 x 位置
                yield return MoveToX(target.x);

                // 如果目标在高处，跳上去
                if (target.y > transform.position.y + 0.5f)
                {
                    Debug.Log("[AutoPlayTest] 目标在高处，尝试跳跃到达");
                    yield return JumpToHeight(target.y + 0.5f);
                }

                // 等待收集（短暂停留）
                yield return new WaitForSeconds(0.2f);

                _currentTargetIndex++;
            }

            // 等待场景切换
            Debug.Log("[AutoPlayTest] 所有收集物已处理，等待场景切换...");
            float waitStartTime = Time.time;
            while (Time.time - waitStartTime < 3f)
            {
                if (SceneManager.GetActiveScene().name != "Main")
                {
                    Debug.Log("[AutoPlayTest] 场景已切换到: " + SceneManager.GetActiveScene().name);
                    break;
                }
                yield return null;
            }

            float totalTime = Time.time - _startTime;
            Debug.Log("[AutoPlayTest] ===== 测试完成，总用时: " + totalTime.ToString("F1") + " 秒 =====");
        }

        private IEnumerator MoveToX(float targetX)
        {
            float direction = targetX > transform.position.x ? 1f : -1f;
            float tolerance = 0.3f;

            while (Mathf.Abs(transform.position.x - targetX) > tolerance)
            {
                // 应用水平速度
                _rb.velocity = new Vector2(direction * moveSpeed, _rb.velocity.y);

                // 如果在路上遇到更高的平台，跳上去
                RaycastHit2D hit = Physics2D.Raycast(transform.position, new Vector2(direction, 0), 1f, groundMask);
                if (hit.collider != null && hit.point.y > transform.position.y + 0.3f && IsGrounded())
                {
                    Debug.Log("[AutoPlayTest] 前方有高台，跳跃");
                    _rb.velocity = new Vector2(_rb.velocity.x, jumpForce);
                    yield return new WaitForSeconds(0.3f);
                }

                yield return new WaitForFixedUpdate();
            }

            // 停止水平移动
            _rb.velocity = new Vector2(0, _rb.velocity.y);
            Debug.Log("[AutoPlayTest] 到达 x=" + transform.position.x.ToString("F1") + " (目标 x=" + targetX.ToString("F1") + ")");
        }

        private IEnumerator JumpToHeight(float targetHeight)
        {
            int attempts = 0;
            int maxAttempts = 8;

            while (transform.position.y < targetHeight - 0.2f && attempts < maxAttempts)
            {
                if (IsGrounded())
                {
                    attempts++;
                    Debug.Log("[AutoPlayTest] 跳跃尝试 " + attempts + "/" + maxAttempts + "，当前 y=" + transform.position.y.ToString("F1"));
                    _rb.velocity = new Vector2(_rb.velocity.x, jumpForce);
                    yield return new WaitForSeconds(0.5f);
                }
                else
                {
                    yield return new WaitForFixedUpdate();
                }
            }

            if (attempts >= maxAttempts)
            {
                Debug.LogWarning("[AutoPlayTest] 跳跃失败，达到最大尝试次数 " + maxAttempts);
            }
            else
            {
                Debug.Log("[AutoPlayTest] 成功到达高度 y=" + transform.position.y.ToString("F1") + "，用时 " + attempts + " 次跳跃");
            }
        }
    }
}
