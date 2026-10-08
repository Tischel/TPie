using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using System;
using System.Collections.Generic;
using System.IO;
using System.IO.Compression;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using TPie.Models;
using TPie.Models.Elements;

namespace TPie.Helpers
{
    internal static class ImportExportHelper
    {
        public static string CompressAndBase64Encode(string jsonString)
        {
            using MemoryStream output = new();

            using (DeflateStream gzip = new(output, CompressionLevel.Optimal))
            {
                using StreamWriter writer = new(gzip, Encoding.UTF8);
                writer.Write(jsonString);
            }

            return Convert.ToBase64String(output.ToArray());
        }

        // A real ring is a few KB of JSON; anything near this is not a ring (or is a zip bomb).
        private const int MaxDecompressedChars = 1_000_000;

        public static string Base64DecodeAndDecompress(string base64String)
        {
            var base64EncodedBytes = Convert.FromBase64String(base64String);

            using MemoryStream inputStream = new(base64EncodedBytes);
            using DeflateStream gzip = new(inputStream, CompressionMode.Decompress);
            using StreamReader reader = new(gzip, Encoding.UTF8);

            StringBuilder result = new();
            char[] buffer = new char[8192];
            int read;
            while ((read = reader.Read(buffer, 0, buffer.Length)) > 0)
            {
                result.Append(buffer, 0, read);
                if (result.Length > MaxDecompressedChars)
                    throw new InvalidDataException("Import string is too large to be a ring.");
            }

            return result.ToString();
        }

        // Exports carry $type names for older TPie versions, but imports never honor them: letting pasted text
        // pick .NET types to instantiate is a known code-execution risk. Ring elements are built by
        // RingElementConverter from the "$type" text instead, the same way the settings file is loaded.
        private static readonly JsonSerializerSettings SerializerSettings = new JsonSerializerSettings
        {
            TypeNameAssemblyFormatHandling = TypeNameAssemblyFormatHandling.Simple,
            TypeNameHandling = TypeNameHandling.Objects
        };

        private static readonly JsonSerializerSettings ImportSettings = new JsonSerializerSettings
        {
            TypeNameHandling = TypeNameHandling.None
        };

        /// <summary>Chat commands the given rings would run, as "ring: /command" lines, for review before import.</summary>
        public static List<string> CommandsIn(IEnumerable<Ring> rings)
        {
            List<string> result = new();
            foreach (Ring ring in rings)
            {
                foreach (RingElement element in ring.Items)
                {
                    if (element is CommandElement command && !string.IsNullOrWhiteSpace(command.Command))
                        result.Add($"{ring.Name}: {command.Command}");
                }
            }
            return result;
        }

        public static string GenerateExportString(Ring ring)
        {
            return GenerateExportString(new Ring[] { ring });
        }
        public static string GenerateExportString(ICollection<Ring> rings)
        {
            string result = "";

            foreach (Ring ring in rings)
            {
                string jsonString = JsonConvert.SerializeObject(ring, Formatting.Indented, SerializerSettings);
                result += "|" + CompressAndBase64Encode(jsonString);
            }

            return result;
        }

        public static List<Ring> ImportRings(string importString)
        {
            List<Ring> result = new List<Ring>();

            string[] importStrings = importString.Trim().Split(new string[] { "|" }, StringSplitOptions.RemoveEmptyEntries);
            if (importStrings.Length == 0)
            {
                return result;
            }

            foreach (string str in importStrings)
            {
                try
                {
                    string jsonString = Base64DecodeAndDecompress(str);

                    var typeString = (string?)JObject.Parse(jsonString)["$type"];
                    if (typeString == null) continue;

                    if (!typeString.StartsWith("TPie.Models.Ring")) continue;

                    Ring? ring = JsonConvert.DeserializeObject<Ring>(jsonString, ImportSettings);
                    if (ring == null) continue;

                    result.Add(ring);
                }
                catch (Exception e)
                {
                    Plugin.Logger.Warning("Skipped an import entry: " + e.Message);
                }
            }

            return result;
        }
    }
}
