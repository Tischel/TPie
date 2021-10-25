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

        public static string Base64DecodeAndDecompress(string base64String)
        {
            var base64EncodedBytes = Convert.FromBase64String(base64String);

            using MemoryStream inputStream = new(base64EncodedBytes);
            using DeflateStream gzip = new(inputStream, CompressionMode.Decompress);
            using StreamReader reader = new(gzip, Encoding.UTF8);
            var decodedString = reader.ReadToEnd();

            return decodedString;
        }

        public static string GenerateExportString(Ring[] rings)
        {
            JsonSerializerSettings settings = new JsonSerializerSettings
            {
                TypeNameAssemblyFormatHandling = TypeNameAssemblyFormatHandling.Simple,
                TypeNameHandling = TypeNameHandling.Objects
            };

            string result = "";

            foreach (Ring ring in rings)
            {
                string jsonString = JsonConvert.SerializeObject(ring, Formatting.Indented, settings);
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
                string jsonString = Base64DecodeAndDecompress(str);

                var typeString = (string?)JObject.Parse(jsonString)["$type"];
                if (typeString == null) continue;

                Type? type = Type.GetType(typeString);
                if (type == null || type != typeof(Ring)) continue;

                Ring? ring = JsonConvert.DeserializeObject<Ring>(jsonString);
                if (ring == null) continue;

                result.Add(ring);
            }

            return result;
        }
    }
}
