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

        public virtual void Draw(Vector2 position, Vector2 size, float scale, bool selected, uint color, ImDrawListPtr drawList)
        {
            size = size * scale;

            if (selected)
            {
                Vector2 borderSize = new Vector2(size.X + 4, size.Y + 4);
                Vector2 borderPos = position - borderSize / 2;
                drawList.AddRectFilled(borderPos, borderPos + borderSize, color, 4);
            }

            DrawHelper.DrawIcon(IconID, position - size / 2, size, drawList);
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
