using System;
using System.IO;
using Newtonsoft.Json;

namespace Pana_Data_Console
{
    class Program
    {
        static void Main(string[] args)
        {
            try
            {
                string metadataPath = @"C:\Users\Muhammad.aqib\source\repos\Pana-Data-Console\Pana-Data-Console\MetaFiles\000m.xml";
                string bookmarkPath = @"C:\Users\Muhammad.aqib\source\repos\Pana-Data-Console\Pana-Data-Console\MetaFiles\000b.xml";

                var parser = new PanasonicMetadataParser();
                var assetInfo = parser.ParseMetadata(metadataPath, bookmarkPath);

                // Convert to JSON with pretty printing
                var jsonSettings = new JsonSerializerSettings
                {
                    Formatting = Formatting.Indented,
                    //NullValueHandling = NullValueHandling.Ignore
                };

                string json = JsonConvert.SerializeObject(assetInfo, jsonSettings);
                Console.WriteLine("Successfully parsed Panasonic metadata:");
                Console.WriteLine(json);
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error: {ex.Message}");
                if (ex.InnerException != null)
                {
                    Console.WriteLine($"Inner Error: {ex.InnerException.Message}");
                }
            }

            Console.ReadKey();
        }
    }
}