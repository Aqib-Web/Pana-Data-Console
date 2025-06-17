using System;
using System.IO;
using System.Linq;
using System.Xml.Linq;
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
                var bookmarksInfo = parser.ParseBookmark(bookmarkPath);

                // Convert to JSON with pretty printing
                var jsonSettings = new JsonSerializerSettings
                {
                    Formatting = Formatting.Indented,
                    //NullValueHandling = NullValueHandling.Ignore
                };

                string assetInfoJson = JsonConvert.SerializeObject(assetInfo, jsonSettings);
                string bookmarksInfoJson = JsonConvert.SerializeObject(bookmarksInfo, jsonSettings);
                Console.WriteLine($"Successfully parsed Panasonic metadata:  (Filename: {Path.GetFileName(metadataPath)})");
                Console.WriteLine(assetInfoJson);
                Console.WriteLine("\n----------------------------------------------------------------------------------------");
                Console.WriteLine($"\nSuccessfully parsed Bookmark metadata: (Filename: {Path.GetFileName(bookmarkPath)})");
                Console.WriteLine(bookmarksInfoJson);
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