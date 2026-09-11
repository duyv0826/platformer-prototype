using UnityEngine;

namespace Prototype.Gameplay
{
    /// <summary>
    /// 2D 侧视相机跟随。放在 Main Camera 上，平滑跟随目标（保持 z 为偏移值以便正交相机取景）。
    /// lockY 不为 -999 时 Y 固定在给定值（横向关卡更稳），否则平滑跟随玩家高度。
    /// </summary>
    public class CameraFollow2D : MonoBehaviour
    {
        [SerializeField] private Transform target;
        [SerializeField] private Vector3 offset = new Vector3(0f, 0f, -10f);
        [SerializeField] private float smooth = 5f;
        [SerializeField] private float lockY = -999f;

        private void LateUpdate()
        {
            if (target == null) return;

            float desiredY = lockY > -998f ? lockY : target.position.y;
            Vector3 desired = new Vector3(target.position.x, desiredY, offset.z);
            transform.position = Vector3.Lerp(transform.position, desired, smooth * Time.deltaTime);
        }
    }
}
