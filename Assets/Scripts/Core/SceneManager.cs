using UnityEngine;

namespace Prototype.Core
{
    /// <summary>
    /// 场景加载与切换（单例）。
    /// 注意：本类名与 UnityEngine.SceneManagement.SceneManager 同名，
    /// 因此内部统一使用完全限定名 UnityEngine.SceneManagement.SceneManager 调用引擎 API，避免递归。
    ///
    /// 使用：
    ///   SceneManager.Instance.Load("Main");
    ///   var handle = SceneManager.Instance.LoadAsync("Main");
    /// </summary>
    public class SceneManager : MonoSingleton<SceneManager>
    {
        public void Load(string sceneName, UnityEngine.SceneManagement.LoadSceneMode mode = UnityEngine.SceneManagement.LoadSceneMode.Single)
        {
            UnityEngine.SceneManagement.SceneManager.LoadScene(sceneName, mode);
            EventBus.Publish(new SceneLoadedEvent(sceneName));
        }

        public void Load(int sceneIndex, UnityEngine.SceneManagement.LoadSceneMode mode = UnityEngine.SceneManagement.LoadSceneMode.Single)
        {
            UnityEngine.SceneManagement.SceneManager.LoadScene(sceneIndex, mode);
        }

        public AsyncOperation LoadAsync(string sceneName)
        {
            var handle = UnityEngine.SceneManagement.SceneManager.LoadSceneAsync(sceneName);
            EventBus.Publish(new SceneLoadedEvent(sceneName));
            return handle;
        }

        public void ReloadCurrent()
        {
            var active = UnityEngine.SceneManagement.SceneManager.GetActiveScene();
            UnityEngine.SceneManagement.SceneManager.LoadScene(active.name);
            EventBus.Publish(new SceneLoadedEvent(active.name));
        }
    }
}