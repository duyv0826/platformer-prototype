using System;
using System.Collections.Generic;

namespace Prototype.Core
{
    /// <summary>
    /// 轻量静态事件总线（类型安全的发布/订阅）。
    /// 用于模块间松耦合通信：发送方只管 Publish，接收方只管 Subscribe，互不知晓对方存在。
    ///
    /// 注意：
    /// - 静态事件总线不会随场景卸载自动清理订阅，订阅者必须在其生命周期结束时调用 Unsubscribe，
    ///   否则可能引用已销毁对象导致漏调用或内存泄漏。
    /// - 如希望自动清理，可在 MonoBehaviour 的 OnEnable/OnDisable 中配对 Subscribe/Unsubscribe。
    /// </summary>
    public static class EventBus
    {
        private static readonly Dictionary<Type, Delegate> Handlers = new Dictionary<Type, Delegate>();

        public static void Subscribe<T>(Action<T> handler)
        {
            if (handler == null) return;

            if (Handlers.TryGetValue(typeof(T), out var existing))
            {
                Handlers[typeof(T)] = (Action<T>)existing + handler;
            }
            else
            {
                Handlers[typeof(T)] = handler;
            }
        }

        public static void Unsubscribe<T>(Action<T> handler)
        {
            if (handler == null) return;

            if (Handlers.TryGetValue(typeof(T), out var existing))
            {
                var combined = (Action<T>)existing - handler;
                if (combined == null)
                {
                    Handlers.Remove(typeof(T));
                }
                else
                {
                    Handlers[typeof(T)] = combined;
                }
            }
        }

        public static void Publish<T>(T eventData)
        {
            if (Handlers.TryGetValue(typeof(T), out var existing))
            {
                ((Action<T>)existing)?.Invoke(eventData);
            }
        }

        /// <summary>
        /// 清空所有订阅。通常在测试或整局重置时调用。
        /// </summary>
        public static void Clear()
        {
            Handlers.Clear();
        }
    }
}