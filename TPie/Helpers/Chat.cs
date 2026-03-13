using FFXIVClientStructs.FFXIV.Client.System.String;
using FFXIVClientStructs.FFXIV.Client.UI;
using FFXIVClientStructs.FFXIV.Client.UI.Shell;

namespace TPie.Helpers;

public static class Chat
{
    public static unsafe void ExecuteCommand(string command)
    {
        if (!command.StartsWith('/'))
            return;

        using var cmd = new Utf8String(command);

        // Technically not needed since we don't use payloads but provides a better example.
        cmd.SanitizeString(
            AllowedEntities.Unknown9     |
            AllowedEntities.Payloads          |
            AllowedEntities.OtherCharacters   |
            AllowedEntities.SpecialCharacters |
            AllowedEntities.Numbers           |
            AllowedEntities.LowercaseLetters  |
            AllowedEntities.UppercaseLetters  );

        if (cmd.Length > 500)
            return;

        RaptureShellModule.Instance()->ExecuteCommandInner(&cmd, UIModule.Instance());
    }

    public static unsafe bool IsInputTextActive => RaptureAtkModule.Instance()->IsTextInputActive();
}
