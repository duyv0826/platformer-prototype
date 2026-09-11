using System.Collections.Generic;
using UnityEngine;

namespace Prototype.Gameplay
{
    /// <summary>
    /// 自动测试脚本（AutoPlayTest / AutoTestManager / AutoTestRunner）的公共逻辑。
    /// 抽取三者间重复的「按名字前缀收集位置 / 按 X 排序 / 地面检测射线」代码，
    /// 每个方法与原内联实现逐语句等价，运行行为完全一致。
    /// </summary>
    public static class AutoTestUtil
    {
        /// <summary>遍历场景所有 Transform，把名字以 prefix 开头的对象位置加入 into。</summary>
        public static void GatherByNamePrefix(List<Vector3> into, string prefix)
        {
            foreach (Transform t in Object.FindObjectsOfType<Transform>())
            {
                if (t.name.StartsWith(prefix))
                {
                    into.Add(t.position);
                }
            }
        }

        /// <summary>按 x 坐标从左到右排序。</summary>
        public static void SortByX(List<Vector3> list)
        {
            list.Sort((a, b) => a.x.CompareTo(b.x));
        }

        /// <summary>从 from 处向下发射地面检测射线，命中则视为着地。</summary>
        public static bool Grounded(Vector3 from, float distance, LayerMask mask)
        {
            return Physics2D.Raycast(from, Vector2.down, distance, mask).collider != null;
        }
    }
}
