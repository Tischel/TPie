using Dalamud.Plugin.Ipc;
using ImGuiScene;
using Lumina.Excel;
using System;
using System.Collections.Generic;
using System.IO;

namespace TPie.Helpers
{
    public class TexturesCache : IDisposable
    {
        private Dictionary<uint, TextureWrap> _cache = new();

        public TextureWrap? GetTexture<T>(uint rowId, bool highQuality = false, uint stackCount = 0) where T : ExcelRow
        {
            var sheet = Plugin.DataManager.GetExcelSheet<T>();

            return sheet == null ? null : GetTexture<T>(sheet.GetRow(rowId), highQuality, stackCount);
        }

        public TextureWrap? GetTexture<T>(dynamic? row, bool highQuality = false, uint stackCount = 0) where T : ExcelRow
        {
            if (row == null)
            {
                return null;
            }

            var iconId = row.Icon;
            return GetTextureFromIconId(iconId, highQuality, stackCount);
        }

        public TextureWrap? GetTextureFromIconId(uint iconId, bool highQuality = false, uint stackCount = 0)
        {
            if (_cache.TryGetValue(iconId + stackCount, out var texture))
            {
                return texture;
            }

            var newTexture = LoadTexture(iconId + stackCount, highQuality);
            if (newTexture == null)
            {
                return null;
            }

            _cache.Add(iconId + stackCount, newTexture);

            return newTexture;
        }

        private unsafe TextureWrap? LoadTexture(uint id, bool highQuality)
        {
            var hqText = highQuality ? "hq/" : "";
            var path = $"ui/icon/{id / 1000 * 1000:000000}/{hqText}{id:000000}_hr1.tex";

            try
            {
                var resolvedPath = _penumbraPathResolver.InvokeFunc(path);

                if (resolvedPath != null && resolvedPath != path)
                {
                    return TextureLoader.LoadTexture(resolvedPath, true);
                }
            }
            catch { }

            return TextureLoader.LoadTexture(path, false);
        }

        private void RemoveTexture<T>(uint rowId) where T : ExcelRow
        {
            var sheet = Plugin.DataManager.GetExcelSheet<T>();

            if (sheet == null)
            {
                return;
            }

            RemoveTexture<T>(sheet.GetRow(rowId));
        }

        public void RemoveTexture<T>(dynamic? row) where T : ExcelRow
        {
            if (row == null || row?.Icon == null)
            {
                return;
            }

            var iconId = row!.Icon;
            RemoveTexture(iconId);
        }

        public void RemoveTexture(uint iconId)
        {
            if (_cache.ContainsKey(iconId))
            {
                _cache.Remove(iconId);
            }
        }

        public void Clear() => _cache.Clear();

        #region plugin textures
        public TextureWrap? RingBackground;

        public void LoadPluginTextures()
        {
            try
            {
                string ringBgPath = Path.Combine(Path.GetDirectoryName(Plugin.AssemblyLocation) ?? "", "Media", "ring_bg.png");
                if (File.Exists(ringBgPath))
                {
                    RingBackground = Plugin.UiBuilder.LoadImage(ringBgPath);
                }
            }
            catch { }
        }
        #endregion

        private ICallGateSubscriber<string, string> _penumbraPathResolver;

        public TexturesCache()
        {
            _penumbraPathResolver = Plugin.PluginInterface.GetIpcSubscriber<string, string>("Penumbra.ResolveInterfacePath");
        }

        ~TexturesCache()
        {
            Dispose(false);
        }

        public void Dispose()
        {
            Dispose(true);
            GC.SuppressFinalize(this);
        }

        protected void Dispose(bool disposing)
        {
            if (!disposing)
            {
                return;
            }

            foreach (var key in _cache.Keys)
            {
                var tex = _cache[key];
                tex?.Dispose();
            }

            _cache.Clear();
        }
    }
}
