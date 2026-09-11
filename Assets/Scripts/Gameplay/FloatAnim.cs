using UnityEngine;

namespace Prototype.Gameplay
{
    /// <summary>
    /// 让收集物做上下轻微浮动，增加视觉动感。幅度/频率可调，相位随机避免整齐划一。
    /// </summary>
    public class FloatAnim : SineBobber
    {
        [SerializeField] private float amplitude = 0.1f;
        [SerializeField] private float speed = 2f;

        protected override float Amplitude => amplitude;
        protected override float Speed => speed;

        private void Update()
        {
            ApplyBob();
        }
    }
}
