using UnityEngine;

namespace Prototype.Audio
{
    /// <summary>
    /// 程序化音效：运行时用正弦波合成短音效（跳跃/收集/受伤/踩敌/通关），不依赖外部音频文件。
    /// 场景搭建时把玩家身上的 AudioSource 注入（Sfx.SetSource），之后各处直接调用静态方法即可。
    /// </summary>
    public static class Sfx
    {
        private static AudioSource _src;
        private static AudioClip _jump, _collect, _hurt, _stomp, _win;

        public static void SetSource(AudioSource src) => _src = src;

        public static void Jump() => Play(ref _jump, 520f, 780f, 0.12f, 0.35f);
        public static void Collect() => Play(ref _collect, 880f, 1320f, 0.13f, 0.3f);
        public static void Hurt() => Play(ref _hurt, 240f, 110f, 0.28f, 0.45f);
        public static void Stomp() => Play(ref _stomp, 320f, 140f, 0.18f, 0.4f);
        public static void Win() => Play(ref _win, 660f, 1040f, 0.55f, 0.5f);

        private static void Play(ref AudioClip clip, float f0, float f1, float dur, float vol)
        {
            if (_src == null) return;
            if (clip == null) clip = MakeTone(f0, f1, dur, vol);
            _src.PlayOneShot(clip);
        }

        private static AudioClip MakeTone(float f0, float f1, float dur, float vol)
        {
            int sr = 22050;
            int n = Mathf.CeilToInt(sr * dur);
            var data = new float[n];

            for (int i = 0; i < n; i++)
            {
                float t = i / (float)sr;
                float f = Mathf.Lerp(f0, f1, t / dur);
                // 起音快速、指数衰减的包络，模拟短促电子音
                float env = Mathf.Min(1f, t / 0.01f) * Mathf.Exp(-4.5f * t / dur);
                data[i] = Mathf.Sin(2f * Mathf.PI * f * t) * env * vol;
            }

            var clip = AudioClip.Create("sfx", n, 1, sr, false);
            clip.SetData(data, 0);
            return clip;
        }
    }
}
