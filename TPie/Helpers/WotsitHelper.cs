using Dalamud.Plugin.Ipc;
using FFXIVClientStructs.FFXIV.Component.GUI;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using TPie.Models;

namespace TPie.Helpers
{
    internal class WotsitHelper
    {
        private readonly ICallGateSubscriber<string, string, string, uint, string> _registerWithSearch;
        private readonly ICallGateSubscriber<string, bool> _invoke;
        private readonly ICallGateSubscriber<string, bool> _unregisterAll;

        private Dictionary<string, Ring> _map = new Dictionary<string, Ring>();

        #region Singleton
        private WotsitHelper()
        {
            _registerWithSearch = Plugin.PluginInterface.GetIpcSubscriber<string, string, string, uint, string>("FA.RegisterWithSearch");
            _unregisterAll = Plugin.PluginInterface.GetIpcSubscriber<string, bool>("FA.UnregisterAll");

            _invoke = Plugin.PluginInterface.GetIpcSubscriber<string, bool>("FA.Invoke");
            _invoke.Subscribe(Invoke);
        }

        public static void Initialize() { Instance = new WotsitHelper(); }

        public static WotsitHelper Instance { get; private set; } = null!;

        ~WotsitHelper()
        {
            Dispose(false);
        }

        public void Dispose()
        {
            Dispose(true);
            GC.SuppressFinalize(this);

            UnregisterAll();
        }

        protected void Dispose(bool disposing)
        {
            if (!disposing)
            {
                return;
            }

            Instance = null!;
        }
        #endregion

        public void Update()
        {
            _map.Clear();
            if (!UnregisterAll())
            {
                return; 
            }

            List<Ring> rings = Plugin.Settings.Rings;

            foreach (Ring ring in rings)
            {
                if (ring.Name.Length <= 0) { continue; }

                string guid = _registerWithSearch.InvokeFunc(
                    Plugin.PluginInterface.InternalName,
                    "TPie Ring Settings: " + ring.Name,
                    "tpie " + ring.Name,
                    66472
                );

                _map.Add(guid, ring);
            }
        }

        public void Invoke(string guid)
        {
            if (_map.TryGetValue(guid, out Ring? ring) && ring != null)
            {
                Plugin.ShowRingSettingsWindowInCursor(ring);
            }
        }

        public bool UnregisterAll()
        {
            try
            {
                _unregisterAll.InvokeFunc(Plugin.PluginInterface.InternalName);
                return true;
            } 
            catch
            {
                return false;
            }
        }
    }
}
