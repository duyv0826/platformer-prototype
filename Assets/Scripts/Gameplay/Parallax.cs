using UnityEngine;

namespace Prototype.Gameplay
{
    /// <summary>
    /// 2D 背景视差：对象只做水平视差移动。
    /// factor = 1 完全固定（纯远景），factor = 0 完全跟随相机（贴屏）。
    /// 记录初始世界位置与初始相机位置，之后按 (相机位移 × (1 - factor)) 平移。
    /// </summary>
    public class Parallax : MonoBehaviour
    {
        [SerializeField] public float factor = 0.8f;

        private Transform _cam;
        private Vector3 _base;
        private Vector3 _camBase;

        private void Start()
        {
            _cam = Camera.main.transform;
            _base = transform.position;
            _camBase = _cam.position;
        }

        private void LateUpdate()
        {
            if (_cam == null) return;
            Vector3 offset = _cam.position - _camBase;
            transform.position = _base + new Vector3(offset.x * (1f - factor), 0f, 0f);
        }
    }
}
