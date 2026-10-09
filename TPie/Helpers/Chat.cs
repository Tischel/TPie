using FFXIVClientStructs.FFXIV.Client.System.String;
using FFXIVClientStructs.FFXIV.Client.UI;
using FFXIVClientStructs.FFXIV.Client.UI.Shell;

namespace TPie.Helpers;

public static class Chat
{
    public static unsafe void ExecuteCommand(string command)
    {
        if (string.IsNullOrEmpty(command) || !command.StartsWith('/'))
            return;

        Plugin.Logger.Info($"[TPie] Chat.ExecuteCommand: {command}");

        void Run()
        {
            var uiModule = UIModule.Instance();
            if (uiModule == null)
                return;

            using var cmd = new Utf8String(command);
            if (cmd.Length > 500)
                return;

            uiModule->ProcessChatBoxEntry(&cmd);
        }

        if (Plugin.Framework.IsInFrameworkUpdateThread)
        {
            Run();
        }
        else
        {
            Plugin.Framework.RunOnFrameworkThread(Run);
        }
    }

    public static unsafe bool IsInputTextActive
    {
        get
        {
            var atkModule = RaptureAtkModule.Instance();
            return atkModule != null && atkModule->IsTextInputActive();
        }
    }
}
