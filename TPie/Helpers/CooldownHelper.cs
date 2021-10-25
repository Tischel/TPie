using FFXIVClientStructs.FFXIV.Client.Game;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace TPie.Helpers
{
    internal static unsafe class CooldownHelper
    {
        public static uint GetSpellActionId(uint actionId) => ActionManager.Instance()->GetAdjustedActionId(actionId);

        public static float GetRecastTimeElapsed(ActionType type, uint actionId) => ActionManager.Instance()->GetRecastTimeElapsed(type, GetSpellActionId(actionId));

        public static float GetRecastTime(ActionType type, uint actionId) => ActionManager.Instance()->GetRecastTime(type, GetSpellActionId(actionId));

        public static float GetCooldown(ActionType type, uint actionId) => Math.Abs(GetRecastTime(type, GetSpellActionId(actionId)) - GetRecastTimeElapsed(type, GetSpellActionId(actionId)));
    }
}
