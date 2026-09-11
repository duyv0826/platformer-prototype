#if UNITY_EDITOR
using System.Collections.Generic;
using System.IO;
using UnityEditor;
using UnityEngine;

namespace Prototype.Editor
{
    /// <summary>
    /// Kenney Pixel Platformer 素材加载器（CC0 授权，来源 https://kenney.nl/assets/pixel-platformer ）。
    /// - 统一把 PNG 设为 Sprite、Point 过滤、关闭压缩与 mipmap，保持像素锐利。
    /// - Tiles 18x18 用 PPU=36（1 格 = 0.5 世界单位），角色 24x24 用 PPU=36（身高约 0.67 单位）。
    /// - 提供拼合长条纹理（BuildStrip），把多个地砖拼成一条连续地形，减少场景对象数量。
    /// </summary>
    public static class PixelSprites
    {
        public const string TilesDir = "Assets/Art/PixelPlatformer/Tiles/Tiles";
        public const string CharsDir = "Assets/Art/PixelPlatformer/Tiles/Characters";
        public const string BgDir = "Assets/Art/PixelPlatformer/Tiles/Backgrounds";
        public const string GenDir = "Assets/Art/Generated";

        public const int TilesPPU = 36;   // 18px tile -> 0.5 世界单位
        public const int CharsPPU = 36;   // 24px 角色 -> 0.67 世界单位
        public const int BgPPU = 24;      // 24px 背景 -> 1 世界单位

        /// <summary>加载地砖（18x18）。</summary>
        public static Sprite Tile(int index)
        {
            return Load(TilesDir, index, TilesPPU);
        }

        /// <summary>加载角色帧（24x24）。</summary>
        public static Sprite Char(int index)
        {
            return Load(CharsDir, index, CharsPPU);
        }

        /// <summary>加载背景块（24x24）。</summary>
        public static Sprite Bg(int index)
        {
            return Load(BgDir, index, BgPPU);
        }

        /// <summary>批量确保导入设置（首次调用最慢，之后直接读缓存）。</summary>
        public static void EnsureImportSettings()
        {
            EnsureDirImports(TilesDir, TilesPPU);
            EnsureDirImports(CharsDir, CharsPPU);
            EnsureDirImports(BgDir, BgPPU);
            AssetDatabase.SaveAssets();
        }

        private static void EnsureDirImports(string dir, int ppu)
        {
            if (!AssetDatabase.IsValidFolder(dir)) return;
            string[] guids = AssetDatabase.FindAssets("t:Texture2D", new[] { dir });
            foreach (string guid in guids)
            {
                string path = AssetDatabase.GUIDToAssetPath(guid);
                ApplyImporter(path, ppu);
            }
        }

        private static Sprite Load(string dir, int index, int ppu)
        {
            string path = $"{dir}/tile_{index:D4}.png";
            Sprite sprite = AssetDatabase.LoadAssetAtPath<Sprite>(path);
            if (sprite == null)
            {
                AssetDatabase.ImportAsset(path, ImportAssetOptions.ForceSynchronousImport);
                ApplyImporter(path, ppu);
                sprite = AssetDatabase.LoadAssetAtPath<Sprite>(path);
            }
            return sprite;
        }

        private static void ApplyImporter(string path, int ppu)
        {
            var imp = AssetImporter.GetAtPath(path) as TextureImporter;
            if (imp == null) return;

            bool dirty = false;
            if (!imp.isReadable) { imp.isReadable = true; dirty = true; }
            if (imp.textureType != TextureImporterType.Sprite) { imp.textureType = TextureImporterType.Sprite; dirty = true; }
            if (imp.spriteImportMode != SpriteImportMode.Single) { imp.spriteImportMode = SpriteImportMode.Single; dirty = true; }
            if (imp.spritePixelsPerUnit != ppu) { imp.spritePixelsPerUnit = ppu; dirty = true; }
            if (imp.filterMode != FilterMode.Point) { imp.filterMode = FilterMode.Point; dirty = true; }
            if (imp.mipmapEnabled) { imp.mipmapEnabled = false; dirty = true; }
            if (imp.textureCompression != TextureImporterCompression.Uncompressed)
            {
                imp.textureCompression = TextureImporterCompression.Uncompressed;
                dirty = true;
            }
            if (imp.wrapMode != TextureWrapMode.Repeat) { imp.wrapMode = TextureWrapMode.Repeat; dirty = true; }

            if (dirty) imp.SaveAndReimport();
        }

        /// <summary>
        /// 把一组 tile 横向循环拼接成一条长纹理并保存为 Sprite 资产。
        /// 例：BuildStrip(new[]{0,1,2,3}, 22) -> 4 个 tile 循环 22 次 = 88 tiles 宽。
        /// </summary>
        public static Sprite BuildStrip(int[] tileIndexes, int repeat)
        {
            EnsureFolder(GenDir);
            int tilePx = 18;
            int count = tileIndexes.Length * repeat;
            int width = count * tilePx;
            int height = tilePx;

            var tex = new Texture2D(width, height, TextureFormat.RGBA32, false);
            tex.filterMode = FilterMode.Point;

            for (int i = 0; i < count; i++)
            {
                Sprite tile = Tile(tileIndexes[i % tileIndexes.Length]);
                Texture2D src = tile.texture;
                Rect rect = tile.rect;
                var srcPixels = src.GetPixels((int)rect.x, (int)rect.y, (int)rect.width, (int)rect.height);
                tex.SetPixels(i * tilePx, 0, tilePx, tilePx, srcPixels);
            }
            tex.Apply();

            string name = "strip_" + string.Join("_", tileIndexes) + "_x" + repeat;
            string path = $"{GenDir}/{name}.png";
            string absPath = Path.Combine(Path.GetDirectoryName(Application.dataPath), path);
            File.WriteAllBytes(absPath, tex.EncodeToPNG());
            Object.DestroyImmediate(tex);

            AssetDatabase.ImportAsset(path, ImportAssetOptions.ForceSynchronousImport);
            var imp = AssetImporter.GetAtPath(path) as TextureImporter;
            imp.textureType = TextureImporterType.Sprite;
            imp.spriteImportMode = SpriteImportMode.Single;
            imp.spritePixelsPerUnit = TilesPPU;
            imp.filterMode = FilterMode.Point;
            imp.mipmapEnabled = false;
            imp.textureCompression = TextureImporterCompression.Uncompressed;
            imp.SaveAndReimport();

            return AssetDatabase.LoadAssetAtPath<Sprite>(path);
        }

        private static void EnsureFolder(string path)
        {
            string[] parts = path.Split('/');
            string current = parts[0];
            for (int i = 1; i < parts.Length; i++)
            {
                string next = current + "/" + parts[i];
                if (!AssetDatabase.IsValidFolder(next))
                {
                    AssetDatabase.CreateFolder(current, parts[i]);
                }
                current = next;
            }
        }
    }
}
#endif
