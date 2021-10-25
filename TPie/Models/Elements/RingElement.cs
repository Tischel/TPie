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
        public uint IconID { get; protected set; }

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
            drawList.AddRect(position, position + size, 0xFF000000, 2, ImDrawFlags.None, 3);
        }

        public abstract void ExecuteAction();

        public abstract bool IsValid();
    }

    public class RingElementConverter : JsonConverter
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
                        uint iconId = jo.Value<uint>("IconID");
                        return new ItemElement(itemId, hq, iconId);
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
