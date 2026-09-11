using UnityEngine;
using UnityEngine.SceneManagement;

namespace Prototype.Gameplay
{
    public class AutoTestRunner : MonoBehaviour
    {
        public float moveSpeed = 6f;
        public float jumpForce = 9f;
        public float groundCheckDistance = 0.6f;
        public LayerMask groundMask;

        private Rigidbody2D _rb;
        private bool _running = false;
        private float _startTime = 0f;
        private int _jumpTestCount = 0;

        // 已停用：旧版自动测试会劫持玩家控制（禁用 PlayerController2D），导致试玩时玩家不受键盘控制。
        // 完整闭环验证请用编辑器菜单 Prototype/验证/M1 通关闭环（M1ClosureVerifier）。
        // [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.AfterSceneLoad)]
        // private static void OnSceneLoaded()
        // {
        //     if (SceneManager.GetActiveScene().name == "Main")
        //     {
        //         GameObject player = GameObject.Find("Player");
        //         if (player != null && player.GetComponent<AutoTestRunner>() == null)
        //         {
        //             player.AddComponent<AutoTestRunner>();
        //             Debug.Log("[AutoTest] AutoTestRunner added via RuntimeInitializeOnLoad");
        //         }
        //     }
        // }

        private void Start()
        {
            _rb = GetComponent<Rigidbody2D>();
            if (groundMask.value == 0)
            {
                groundMask = LayerMask.GetMask("Ground");
            }
            // 已停用自动启动：AutoTestRunner 仅保留手动调用入口（StartTest），
            // 防止任何情况下自动禁用玩家控制或劫持输入。
            // var pc = GetComponent<PlayerController2D>();
            // if (pc != null) pc.enabled = false;
            // Invoke("StartTest", 0.5f);
        }

        public void StartTest()
        {
            if (_running) return;
            _running = true;
            _startTime = Time.time;
            Debug.Log("[AutoTest] ===== M1 Test Start =====");
            Debug.Log("[AutoTest] Start pos: " + transform.position.ToString("F2"));
            StartCoroutine(TestRoutine());
        }

        private bool IsGrounded()
        {
            return AutoTestUtil.Grounded(transform.position, groundCheckDistance, groundMask);
        }

        private System.Collections.IEnumerator TestRoutine()
        {
            yield return new WaitUntil(() => IsGrounded());
            Debug.Log("[AutoTest] Grounded");

            Debug.Log("[AutoTest] [1] Move to x=3");
            yield return MoveToX(3f);
            yield return new WaitForSeconds(0.3f);
            Debug.Log("[AutoTest] At x=" + transform.position.x.ToString("F1"));

            Debug.Log("[AutoTest] [2] Move to x=9");
            yield return MoveToX(9f);
            yield return new WaitForSeconds(0.3f);
            Debug.Log("[AutoTest] At x=" + transform.position.x.ToString("F1") + " (score ~20)");

            Debug.Log("[AutoTest] [3] Jump test");
            _rb.velocity = new Vector2(0f, jumpForce);
            _jumpTestCount++;
            float t0 = Time.time;
            bool air = false;
            while (!air && Time.time - t0 < 0.3f)
            {
                if (!IsGrounded()) air = true;
                yield return new WaitForFixedUpdate();
            }
            float peakY = transform.position.y;
            while (!IsGrounded() && Time.time - t0 < 2f)
            {
                if (transform.position.y > peakY) peakY = transform.position.y;
                yield return new WaitForFixedUpdate();
            }
            Debug.Log("[AutoTest] Jump peak y=" + peakY.ToString("F2") + ", landed @ y=" + transform.position.y.ToString("F2"));

            Debug.Log("[AutoTest] [4] Move to x=15");
            yield return MoveToX(15f);
            yield return new WaitForSeconds(0.3f);
            Debug.Log("[AutoTest] At x=" + transform.position.x.ToString("F1") + " (score ~30)");

            Debug.Log("[AutoTest] [5] Move to x=21");
            yield return MoveToX(21f);
            yield return new WaitForSeconds(0.3f);
            Debug.Log("[AutoTest] At x=" + transform.position.x.ToString("F1") + " (score ~40)");

            Debug.Log("[AutoTest] Waiting for scene switch...");
            float ws = Time.time;
            bool sw = false;
            while (Time.time - ws < 3f)
            {
                if (SceneManager.GetActiveScene().name != "Main")
                {
                    sw = true;
                    Debug.Log("[AutoTest] Scene switched to: " + SceneManager.GetActiveScene().name);
                    break;
                }
                yield return null;
            }
            if (!sw) Debug.LogWarning("[AutoTest] No scene switch within 3s");

            float tt = Time.time - _startTime;
            Debug.Log("[AutoTest] ===== Results =====");
            Debug.Log("[AutoTest] Total time: " + tt.ToString("F1") + "s");
            Debug.Log("[AutoTest] Jump tests: " + _jumpTestCount);
            Debug.Log("[AutoTest] Scene switched: " + sw);
            Debug.Log("[AutoTest] Move speed: " + moveSpeed + " Jump force: " + jumpForce);
        }

        private System.Collections.IEnumerator MoveToX(float targetX)
        {
            float dir = targetX > transform.position.x ? 1f : -1f;
            float tol = 0.15f;
            float timeout = 6f;
            float start = Time.time;
            while (Mathf.Abs(transform.position.x - targetX) > tol && Time.time - start < timeout)
            {
                _rb.velocity = new Vector2(dir * moveSpeed, _rb.velocity.y);
                yield return new WaitForFixedUpdate();
            }
            _rb.velocity = new Vector2(0, _rb.velocity.y);
        }
    }
}
