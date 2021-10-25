using Lumina.Excel;
using Newtonsoft.Json;
using TPie.Helpers;
using LuminaAction = Lumina.Excel.GeneratedSheets.Action;

namespace TPie.Models.Items
{
    public class ActionItem : RingItem
    {
        public readonly uint ActionID;

        [JsonIgnore] private readonly LuminaAction? _data;

        public ActionItem(uint actionID)
        {
            ActionID = actionID;

            ExcelSheet<LuminaAction>? sheet = Plugin.DataManager.GetExcelSheet<LuminaAction>();
            _data = sheet?.GetRow(actionID);

            IconID = _data?.Icon ?? 0;
        }

        public override void ExecuteAction()
        {
            if (_data != null)
            {
                ChatHelper.SendChatMessage($"/ac \"{_data.Name}\"");
            }
        }

        public override bool IsValid()
        {
            return ActionID > 0 && _data != null;
        }
    }
}
