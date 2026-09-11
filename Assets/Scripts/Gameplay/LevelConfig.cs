using UnityEngine;
using Prototype.Core;

namespace Prototype.Gameplay
{
    /// <summary>
    /// 关卡配置：进入场景时把本关收集物总数注入 GameManager（结算页统计收集率用）。
    /// </summary>
    public class LevelConfig : MonoBehaviour
    {
        [SerializeField] private int totalCollectibles;

        private void Start()
        {
            GameManager.Instance.TotalCollectibles = Mathf.Max(1, totalCollectibles);
        }
    }
}
