using UnityEngine;

namespace Prototype.Gameplay
{
    /// <summary>
    /// 2D 侧视相机跟随。放在 Main Camera 上，平滑跟随目标（保持 z 为偏移值以便正交相机取景）。
    /// </summary>
    public class CameraFollow2D : MonoBehaviour
    {
        [SerializeField] private Transform target;
        [SerializeField] private Vector3 offset = new Vector3(0f, 0f, -10f);
        [SerializeField] private float smooth = 5f;

        private void LateUpdate()
        {
            if (target == null) return;

            Vector3 desired = new Vector3(target.position.x, target.position.y, offset.z);
            transform.position = Vector3.Lerp(transform.position, desired, smooth * Time.deltaTime);
        }
    }
}