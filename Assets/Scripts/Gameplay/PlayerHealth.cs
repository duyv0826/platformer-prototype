using System.Collections;
using UnityEngine;
using Prototype.Core;
using Prototype.Audio;

namespace Prototype.Gameplay
{
    /// <summary>
    /// 玩家生命与受伤反馈。敌人/陷阱调用 Damage() 扣血；扣血后有 1.5 秒无敌时间（闪烁提示）。
    /// 生命归零时重置本局（分数清零、回到 Main 场景起点）。
    /// </summary>
    public class PlayerHealth : MonoBehaviour
    {
        [SerializeField] private float invincibleTime = 1.5f;
        [SerializeField] private float blinkInterval = 0.08f;
        [SerializeField] private bool resetLevelOnDeath = true;

        private SpriteRenderer[] _renderers;
        private bool _invincible;

        private void Awake()
        {
            _renderers = GetComponentsInChildren<SpriteRenderer>();
        }

        private void Start()
        {
            EventBus.Publish(new LivesChangedEvent(GameManager.Instance.Lives));
        }

        public void Damage()
        {
            if (_invincible) return;

            Sfx.Hurt();
            if (!GameManager.Instance.LoseLife())
            {
                Die();
                return;
            }

            StartCoroutine(BlinkAndRecover());
        }

        private void Die()
        {
            GameManager.Instance.ResetScore();
            SceneManager.Instance.ReloadCurrent();
        }

        private IEnumerator BlinkAndRecover()
        {
            _invincible = true;
            float t = 0f;
            while (t < invincibleTime)
            {
                bool visible = Mathf.FloorToInt(t / blinkInterval) % 2 == 0;
                SetVisible(visible);
                t += Time.deltaTime;
                yield return null;
            }
            SetVisible(true);
            _invincible = false;
        }

        private void SetVisible(bool visible)
        {
            if (_renderers == null) return;
            foreach (var sr in _renderers)
            {
                if (sr != null) sr.enabled = visible;
            }
        }
    }
}
