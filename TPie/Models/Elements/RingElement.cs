using Dalamud.Logging;
using ImGuiNET;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using System;
using System.Numerics;
using TPie.Helpers;

namespace TPie.Models.Elements
{
    [JsonConverter(typeof(RingElementConverter))]
    public abstract class RingElement
    {
        public uint IconID { get; set; }

        public virtual void Draw(Vector2 position, Vector2 size, float scale, bool selected, uint color, float alpha, ImDrawListPtr drawList)
        {
            size = size * scale;

            if (selected)
            {
                Vector2 borderSize = new Vector2(size.X + 6, size.Y + 6);
                Vector2 borderPos = position - borderSize / 2;
                drawList.AddRectFilled(borderPos, borderPos + borderSize, color, 3);
            }

            position = position - size / 2f;
            DrawHelper.DrawIcon(IconID, position, size, alpha, drawList);

            uint c = ImGui.ColorConvertFloat4ToU32(new Vector4(0, 0, 0, alpha));
            drawList.AddRect(position, position + size, c, 2, ImDrawFlags.None, 3);
        }

        public abstract void ExecuteAction();

        public abstract bool IsValid();

        public abstract string InvalidReason();

        public abstract string Description();
    }

    internal class RingElementConverter : JsonConverter
    {
        public override bool CanConvert(Type objectType)
        {
            return (objectType == typeof(RingElement));
        }

        public override object ReadJson(JsonReader reader, Type objectType, object? existingValue, JsonSerializer serializer)
        {
            try
            {
                JObject jo = JObject.Load(reader);

                string? type = jo.Value<string>("$type");
                if (type != null)
                {
                    if (type.Contains("ActionElement"))
                    {
                        uint actionId = jo.Value<uint>("ActionID");
                        return new ActionElement(actionId);
                    }

                    if (type.Contains("ItemElement"))
                    {
                        uint itemId = jo.Value<uint>("ItemID");
                        bool hq = jo.Value<bool>("HQ");
                        string name = jo.Value<string>("Name") ?? "";
                        uint iconId = jo.Value<uint>("IconID");
                        return new ItemElement(itemId, hq, name, iconId);
                    }

                    if (type.Contains("GearSetElement"))
                    {
                        uint gearSetId = jo.Value<uint>("GearSetID");
                        uint jobId = jo.Value<uint>("JobID");
                        return new GearSetElement(gearSetId, jobId);
                    }

                    if (type.Contains("Macro"))
                    {
                        string name = jo.Value<string>("Name") ?? "";
                        string command = jo.Value<string>("Command") ?? "";
                        uint iconId = jo.Value<uint>("IconID");
                        return new MacroElement(name, command, iconId);
                    }
                }
            }
            catch (Exception e)
            {
                PluginLog.Error(e.Message);
            }

            return new ActionElement(0);
        }

        public override bool CanWrite
        {
            get { return false; }
        }

        public override void WriteJson(JsonWriter writer, object? value, JsonSerializer serializer)
        {

        }
    }
}
