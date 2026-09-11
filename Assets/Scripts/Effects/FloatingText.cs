using UnityEngine;

namespace Prototype.Effects
{
    /// <summary>
    /// 世界空间飘分文字：生成后缓慢上飘、淡出、自毁。
    /// 用 TextMesh 避免 Canvas 依赖，适合 2D 正交相机场景。
    /// </summary>
    public class FloatingText : MonoBehaviour
    {
        public static void Spawn(Vector3 pos, string text, Color color)
        {
            var go = new GameObject("FlyText");
            go.transform.position = pos + new Vector3(Random.Range(-0.1f, 0.1f), 0.35f, 0f);
            go.transform.localScale = new Vector3(0.08f, 0.08f, 1f);

            var tm = go.AddComponent<TextMesh>();
            tm.text = text;
            tm.fontSize = 56;
            tm.font = Resources.GetBuiltinResource<Font>("LegacyRuntime.ttf");
            tm.color = color;
            tm.anchor = TextAnchor.MiddleCenter;
            tm.alignment = TextAlignment.Center;

            go.AddComponent<FloatingText>();
        }

        private float _life = 0.9f;
        private Vector3 _vel;
        private TextMesh _tm;

        private void Start()
        {
            _vel = new Vector3(Random.Range(-0.15f, 0.15f), 1.4f, 0f);
            _tm = GetComponent<TextMesh>();
        }

        private void Update()
        {
            _life -= Time.deltaTime;
            if (_life <= 0f)
            {
                Destroy(gameObject);
                return;
            }

            transform.position += _vel * Time.deltaTime;

            if (_tm != null)
            {
                Color c = _tm.color;
                c.a = Mathf.Clamp01(_life / 0.45f);
                _tm.color = c;
            }
        }
    }
}
