using UnityEngine;

namespace Prototype.Core
{
    /// <summary>
    /// 泛型单例 MonoBehaviour 基类。
    /// - 首次访问 Instance 时若场景中没有实例，会自动创建一个同名 GameObject 并挂载本组件。
    /// - 重复出现时销毁多余实例，保证全局唯一。
    /// - 挂载在根节点时自动 DontDestroyOnLoad，跨场景保留。
    /// </summary>
    public abstract class MonoSingleton<T> : MonoBehaviour where T : MonoSingleton<T>
    {
        private static T _instance;

        public static T Instance
        {
            get
            {
                if (_instance == null)
                {
                    _instance = Object.FindFirstObjectByType<T>();
                    if (_instance == null)
                    {
                        var go = new GameObject(typeof(T).Name);
                        _instance = go.AddComponent<T>();
                    }
                }
                return _instance;
            }
        }

        protected virtual void Awake()
        {
            if (_instance != null && _instance != this)
            {
                Destroy(gameObject);
                return;
            }

            _instance = (T)this;

            if (transform.parent == null)
            {
                DontDestroyOnLoad(gameObject);
            }

            OnAwake();
        }

        /// <summary>
        /// 子类在此做初始化（替代 Awake，避免覆盖基类的单例逻辑）。
        /// </summary>
        protected virtual void OnAwake() { }

        protected virtual void OnDestroy()
        {
            if (_instance == this)
            {
                _instance = null;
            }
        }
    }
}