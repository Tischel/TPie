using Dalamud.Logging;
using ImGuiNET;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using System;
using System.Numerics;
using TPie.Helpers;

namespace TPie.Models.Items
{
    [JsonConverter(typeof(RingItemConverter))]
    public abstract class RingItem
    {
        public uint IconID { get; protected set; }

        public virtual void Draw(Vector2 position, Vector2 size, bool selected, uint color, ImDrawListPtr drawList)
        {
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

    public class RingItemConverter : JsonConverter
    {
        public override bool CanConvert(Type objectType)
        {
            return (objectType == typeof(RingItem));
        }

        public override object ReadJson(JsonReader reader, Type objectType, object? existingValue, JsonSerializer serializer)
        {
            try
            {
                JObject jo = JObject.Load(reader);

                string? type = jo.Value<string>("$type");
                if (type != null)
                {
                    if (type.Contains("ActionItem"))
                    {
                        uint actionId = jo.Value<uint>("ActionID");
                        return new ActionItem(actionId);
                    }
                }
            }
            catch (Exception e)
            {
                PluginLog.Error(e.Message);
            }

            return new ActionItem(0);
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
