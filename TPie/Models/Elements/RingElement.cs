using Dalamud.Logging;
using ImGuiNET;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using System;
using System.Numerics;
using System.Text.RegularExpressions;
using TPie.Helpers;

namespace TPie.Models.Elements
{
    [JsonConverter(typeof(RingElementConverter))]
    public abstract class RingElement
    {
        public uint IconID { get; set; }
        public ItemBorder Border { get; set; } = ItemBorder.GlobalBorderSettingsCopy();

        public virtual void Draw(Vector2 position, Vector2 size, float scale, bool selected, uint color, float alpha, bool tooltip, ImDrawListPtr drawList)
        {
            size = size * scale;

            if (selected)
            {
                float offset = (Border.Thickness + 4) * scale;
                Vector2 borderSize = new Vector2(size.X + offset, size.Y + offset);
                Vector2 borderPos = position - borderSize / 2;
                drawList.AddRectFilled(borderPos, borderPos + borderSize, color, Border.Radius * scale);
            }

            position = position - size / 2f;
            DrawHelper.DrawIcon(IconID, position, size, alpha, drawList);

            if (Border.Thickness > 0)
            {
                uint c = ImGui.ColorConvertFloat4ToU32(new Vector4(Border.Color.X, Border.Color.Y, Border.Color.Z, alpha));
                drawList.AddRect(position, position + size, c, Border.Radius * scale, ImDrawFlags.None, Border.Thickness * scale);
            }

            if (tooltip && ImGui.IsMouseHoveringRect(position, position + size))
            {
                ImGui.SetTooltip(UserFriendlyName() + ": " + Description());
            }
        }

        public abstract void ExecuteAction();

        public abstract bool IsValid();

        public abstract string InvalidReason();

        public abstract string Description();

        public string UserFriendlyName()
        {
            string str = GetType().Name.Replace("Element", "");

            Regex? regex = new(@"
                    (?<=[A-Z])(?=[A-Z][a-z]) |
                    (?<=[^A-Z])(?=[A-Z]) |
                    (?<=[A-Za-z])(?=[^A-Za-z])",
                RegexOptions.IgnorePatternWhitespace);

            return regex.Replace(str, " ");
        }
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
                    ItemBorder border = jo.GetValue("Border")?.ToObject<ItemBorder>() ?? ItemBorder.Default();

                    if (type.Contains("ActionElement"))
                    {
                        uint actionId = jo.Value<uint>("ActionID");

                        ActionElement element = new ActionElement(actionId);
                        element.Border = border;
                        return element;
                    }

                    if (type.Contains("ItemElement"))
                    {
                        uint itemId = jo.Value<uint>("ItemID");
                        bool hq = jo.Value<bool>("HQ");
                        string name = jo.Value<string>("Name") ?? "";
                        uint iconId = jo.Value<uint>("IconID");

                        ItemElement element = new ItemElement(itemId, hq, name, iconId);
                        element.Border = border;
                        return element;
                    }

                    if (type.Contains("GearSetElement"))
                    {
                        uint gearSetId = jo.Value<uint>("GearSetID");
                        bool useId = jo.GetValue("UseID") != null ? jo.Value<bool>("UseID") : true;
                        string name = jo.Value<string>("GearSetName") ?? "";
                        bool drawText = jo.GetValue("DrawText") != null ? jo.Value<bool>("DrawText") : true;
                        uint jobId = jo.Value<uint>("JobID");

                        GearSetElement element = new GearSetElement(gearSetId, useId, name, drawText, jobId);
                        element.Border = border;
                        return element;
                    }

                    if (type.Contains("GameMacro"))
                    {
                        string name = jo.Value<string>("Name") ?? "";
                        int id = jo.Value<int>("Identifier");
                        bool shared = jo.GetValue("IsShared") != null ? jo.Value<bool>("IsShared") : false;
                        uint iconId = jo.Value<uint>("IconID");

                        GameMacroElement element = new GameMacroElement(name, id, shared, iconId);
                        element.Border = border;
                        return element;
                    }

                    if (type.Contains("Macro") || type.Contains("Command"))
                    {
                        string name = jo.Value<string>("Name") ?? "";
                        string command = jo.Value<string>("Command") ?? "";
                        uint iconId = jo.Value<uint>("IconID");

                        CommandElement element = new CommandElement(name, command, iconId);
                        element.Border = border;
                        return element;
                    }

                    if (type.Contains("NestedRing"))
                    {
                        string ringName = jo.Value<string>("RingName") ?? "";
                        float activationTime = jo.Value<float>("ActivationTime");
                        activationTime = activationTime == 0 ? 1 : activationTime;
                        uint iconId = jo.Value<uint>("IconID");

                        NestedRingElement element = new NestedRingElement(ringName, activationTime, iconId);
                        element.Border = border;
                        return element;
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
