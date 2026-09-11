using UnityEngine;

namespace Prototype.Gameplay
{
    /// <summary>
    /// 正弦上下浮动的公共基类（供 FloatAnim / MovingPlatform 复用，消除重复代码）。
    /// 子类保留各自的 amplitude / speed 序列化字段与默认值，通过抽象属性暴露给基类，
    /// 因此不会改变任何已有序列化数据或默认手感。
    /// </summary>
    public abstract class SineBobber : MonoBehaviour
    {
        protected Vector3 _base;
        protected float _phase;

        protected abstract float Amplitude { get; }
        protected abstract float Speed { get; }

        protected virtual void Awake()
        {
            _base = transform.position;
            _phase = Random.value * Mathf.PI * 2f;
        }

        /// <summary>按正弦把对象移动到 base 位置的上下偏移处（与原内联实现逐 token 等价）。</summary>
        protected void ApplyBob()
        {
            float t = Mathf.Sin(Time.time * Speed + _phase) * Amplitude;
            transform.position = _base + Vector3.up * t;
        }
    }
}
