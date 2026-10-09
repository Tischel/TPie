using FFXIVClientStructs.FFXIV.Client.Game;
using System;

namespace TPie.Helpers
{
    internal static unsafe class CooldownHelper
    {
        public static uint GetSpellActionId(uint actionId)
        {
            var am = ActionManager.Instance();
            return am != null ? am->GetAdjustedActionId(actionId) : actionId;
        }

        public static ushort GetMaxCharges(uint actionId) => Plugin.ObjectTable.LocalPlayer == null ? (ushort)1 : Math.Max((ushort)1, ActionManager.GetMaxCharges(actionId, Plugin.ObjectTable.LocalPlayer.Level));

        public static int GetCharges(uint actionId)
        {
            var am = ActionManager.Instance();
            if (am == null) return 1;

            float elapsed = am->GetRecastTimeElapsed(ActionType.Action, GetSpellActionId(actionId));
            ushort maxCharges = GetMaxCharges(actionId);
            if (maxCharges <= 1)
            {
                return elapsed == 0 ? 1 : 0;
            }

            float recastTime = GetRecastTime(ActionType.Action, actionId);
            return recastTime == 0 ? maxCharges : (int)(elapsed / recastTime);
        }

        public static float GetRecastTimeElapsed(ActionType type, uint actionId)
        {
            var am = ActionManager.Instance();
            if (am == null) return 0f;

            float total = GetRecastTime(type, actionId);
            float elapsed = am->GetRecastTimeElapsed(type, GetSpellActionId(actionId));

            if (type == ActionType.Action)
            {
                ushort maxCharges = GetMaxCharges(actionId);

                if (maxCharges > 1 && elapsed > total)
                {
                    elapsed -= total;
                }
            }

            return elapsed;
        }

        public static float GetRecastTime(ActionType type, uint actionId)
        {
            var am = ActionManager.Instance();
            if (am == null) return 0f;

            float recast = am->GetRecastTime(type, GetSpellActionId(actionId));

            if (type == ActionType.Action)
            {
                ushort maxCharges = GetMaxCharges(actionId);
                if (maxCharges > 0)
                {
                    recast /= maxCharges;
                }
            }

            return recast;
        }
    }
}
