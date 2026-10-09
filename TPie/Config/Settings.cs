using Dalamud.Logging;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Numerics;
using TPie.Helpers;
using TPie.Models;
using TPie.Models.Elements;

namespace TPie.Config
{
    public class Settings
    {
        public List<Ring> Rings = new List<Ring>();

        public bool AppearAtCursor = true;
        public Vector2 CenterPositionOffset = Vector2.Zero;
        public bool AutoCenterCursor = false;

        public bool UseCustomFont = true;
        public int FontSize = 20;

        public bool DrawRingBackground = true;

        public RingAnimationType AnimationType = RingAnimationType.Spiral;
        public float AnimationDuration = 0.2f;

        public bool AnimateIconSizes = true;

        public bool ShowCooldowns = true;
        public bool ShowRemainingItemCount = true;

        public bool KeybindPassthrough = false;
        public bool EnableQuickSettings = true;
        public bool EnableEscapeKeybind = true;

        public ItemBorder GlobalBorderSettings = ItemBorder.Default();

        public void AddRing(Ring ring)
        {
            if (Rings.Any(o => o.KeyBind.Equals(ring.KeyBind)))
            {
                ring.KeyBind.Reset();
            }

            Rings.Add(ring);

            WotsitHelper.Instance?.Update();
        }

        public void ValidateKeyBind(Ring prioritizedRing)
        {
            foreach (Ring ring in Rings)
            {
                if (ring == prioritizedRing) continue;

                if (ring.KeyBind.Equals(prioritizedRing.KeyBind))
                {
                    if (prioritizedRing.KeyBind.IsGlobal || ring.KeyBind.IsGlobal)
                    {
                        ring.KeyBind.Reset();
                    }
                    else
                    {
                        HashSet<uint> tmp = new HashSet<uint>(prioritizedRing.KeyBind.Jobs);
                        tmp.IntersectWith(ring.KeyBind.Jobs);

                        if (tmp.Count > 0)
                        {
                            ring.KeyBind.Reset();
                        }
                    }
                }
            }
        }

        #region load / save
        private static string JsonPath = Path.Combine(Plugin.PluginInterface.GetPluginConfigDirectory(), "Settings.json");
        /// <summary>Set when Settings.json couldn't be read; the path of the copy kept aside.</summary>
        public static string? LoadFailedCopy { get; private set; }

        public static Settings Load()
        {
            string path = JsonPath;
            Settings? settings = null;

            try
            {
                if (File.Exists(path))
                {
                    string jsonString = File.ReadAllText(path);
                    settings = JsonConvert.DeserializeObject<Settings>(jsonString);
                }
            }
            catch (Exception e)
            {
                Plugin.Logger.Error("Error reading settings file: " + e.Message);

                // Keep the unreadable file: otherwise the next save writes defaults over every ring.
                try
                {
                    string kept = path + $".unreadable-{DateTime.Now:yyyyMMdd-HHmmss}";
                    File.Copy(path, kept, true);
                    Plugin.Logger.Error($"Kept a copy of the unreadable settings at {kept}");
                    LoadFailedCopy = kept;
                }
                catch (Exception copyError)
                {
                    Plugin.Logger.Error("Could not keep a copy of the unreadable settings: " + copyError.Message);
                }
            }

            if (settings == null)
            {
                settings = new Settings();
                if (!File.Exists(path))
                {
                    Save(settings);
                }
            }

            return settings;
        }

        public static void Save(Settings settings)
        {
            try
            {
                JsonSerializerSettings serializerSettings = new JsonSerializerSettings
                {
                    TypeNameAssemblyFormatHandling = TypeNameAssemblyFormatHandling.Simple,
                    TypeNameHandling = TypeNameHandling.Objects
                };
                string jsonString = JsonConvert.SerializeObject(settings, Formatting.Indented, serializerSettings);

                if (File.Exists(JsonPath))
                {
                    // Keep the last three saves: .bak (newest), .bak2, .bak3.
                    try
                    {
                        if (File.Exists(JsonPath + ".bak2")) File.Copy(JsonPath + ".bak2", JsonPath + ".bak3", true);
                        if (File.Exists(JsonPath + ".bak")) File.Copy(JsonPath + ".bak", JsonPath + ".bak2", true);
                        File.Copy(JsonPath, JsonPath + ".bak", true);
                    }
                    catch (Exception e)
                    {
                        Plugin.Logger.Warning("Could not back up settings before saving: " + e.Message);
                    }
                }

                File.WriteAllText(JsonPath, jsonString);
            }
            catch (Exception e)
            {
                Plugin.Logger.Error("Error saving settings file: " + e.Message);
            }
        }
        #endregion
    }

    public enum RingAnimationType
    {
        None = 0,
        Spiral = 1,
        Sequential = 2,
        Fade = 3
    }
}
