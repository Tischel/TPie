using System;
using Dalamud.Plugin.Ipc;

namespace TPie.Ipc
{
    internal sealed class TPieIpcProvider : IDisposable
    {
        private const string IsRingActiveName = "TPie.IsRingActive";

        private readonly ICallGateProvider<bool> _isRingActiveProvider;

        public TPieIpcProvider()
        {
            _isRingActiveProvider = Plugin.PluginInterface.GetIpcProvider<bool>(IsRingActiveName);
            _isRingActiveProvider.RegisterFunc(IsRingActive);
        }

        public void Dispose()
        {
            _isRingActiveProvider.UnregisterFunc();
        }

        private static bool IsRingActive() => Plugin.RingsManager is { IsRingActive: true };
    }
}
