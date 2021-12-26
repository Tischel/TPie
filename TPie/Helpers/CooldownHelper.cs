using FFXIVClientStructs.FFXIV.Client.Game;
using System;

namespace TPie.Helpers
{
    internal static unsafe class CooldownHelper
    {
        public static uint GetSpellActionId(uint actionId) => ActionManager.Instance()->GetAdjustedActionId(actionId);

        public static ushort GetMaxCharges(uint actionId) => Plugin.ClientState.LocalPlayer == null ? (ushort)1 : Math.Max((ushort)1, ActionManager.GetMaxCharges(actionId, Plugin.ClientState.LocalPlayer.Level));

        public static int GetCharges(uint actionId)
        {
            float elapsed = ActionManager.Instance()->GetRecastTimeElapsed(ActionType.Spell, GetSpellActionId(actionId));
            ushort maxCharges = GetMaxCharges(actionId);
            if (maxCharges <= 1)
            {
                return elapsed == 0 ? 1 : 0;
            }

            float recastTime = GetRecastTime(ActionType.Spell, actionId);
            return recastTime == 0 ? maxCharges : (int)(elapsed / recastTime);
        }

        public static float GetRecastTimeElapsed(ActionType type, uint actionId)
        {
            float total = GetRecastTime(type, actionId);
            float elapsed = ActionManager.Instance()->GetRecastTimeElapsed(type, GetSpellActionId(actionId));

            if (type == ActionType.Spell)
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
            float recast = ActionManager.Instance()->GetRecastTime(type, GetSpellActionId(actionId));

            if (type == ActionType.Spell)
            {
                recast /= GetMaxCharges(actionId);
            }

            return recast;
        }
    }
}
