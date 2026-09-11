using UnityEngine;

namespace Prototype.Effects
{
    /// <summary>
    /// 收集时的方块粒子爆发：若干彩色小方块随机飞出、受重力下坠、淡出销毁。
    /// 纯 SpriteRenderer + 白色像素纹理，无外部资源依赖。
    /// </summary>
    public class CollectBurst : MonoBehaviour
    {
        private static Sprite _white;

        public static void Spawn(Vector3 pos, Color color, int count = 8)
        {
            for (int i = 0; i < count; i++)
            {
                var go = new GameObject("Burst");
                go.transform.position = pos + (Vector3)(Random.insideUnitCircle * 0.12f);
                float size = Random.Range(0.06f, 0.13f);
                go.transform.localScale = new Vector3(size, size, 1f);

                var sr = go.AddComponent<SpriteRenderer>();
                sr.sprite = White();
                sr.color = color;
                sr.sortingOrder = 30;

                var p = go.AddComponent<CollectBurst>();
                p._vel = new Vector2(Random.Range(-2.4f, 2.4f), Random.Range(1.4f, 3.4f));
                p._life = Random.Range(0.35f, 0.55f);
            }
        }

        private static Sprite White()
        {
            if (_white == null)
            {
                _white = Sprite.Create(Texture2D.whiteTexture,
                    new Rect(0f, 0f, 1f, 1f), new Vector2(0.5f, 0.5f), 1f);
            }
            return _white;
        }

        private Vector2 _vel;
        private float _life;

        private void Update()
        {
            _life -= Time.deltaTime;
            if (_life <= 0f)
            {
                Destroy(gameObject);
                return;
            }

            _vel.y -= 7f * Time.deltaTime;
            transform.position += (Vector3)(_vel * Time.deltaTime);

            var sr = GetComponent<SpriteRenderer>();
            if (sr != null)
            {
                Color c = sr.color;
                c.a = Mathf.Clamp01(_life * 2.5f);
                sr.color = c;
            }
        }
    }
}
