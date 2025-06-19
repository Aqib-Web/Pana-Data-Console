using System;
using System.IO;
using System.Linq;
using System.Net.Http;
using System.Text;
using System.Xml.Linq;
using Newtonsoft.Json;
using static Pana_Data_Console.Entities.EMV4AssetPayloadModels;

namespace Pana_Data_Console
{
    class Program
    {
        static void Main(string[] args)
        {
            //try
            //{
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

            //string assetInfoJson = JsonConvert.SerializeObject(assetInfo, jsonSettings);
            //string bookmarksInfoJson = JsonConvert.SerializeObject(bookmarksInfo, jsonSettings);
            //Console.WriteLine($"Successfully parsed Panasonic metadata:  (Filename: {Path.GetFileName(metadataPath)})");
            //Console.WriteLine(assetInfoJson);
            //Console.WriteLine("\n----------------------------------------------------------------------------------------");
            //Console.WriteLine($"\nSuccessfully parsed Bookmark metadata: (Filename: {Path.GetFileName(bookmarkPath)})");
            //Console.WriteLine(bookmarksInfoJson);
            //Console.WriteLine(assetInfo.MediaType.ToString());
            //}
            //catch (Exception ex)
            //{
            //    Console.WriteLine($"Error: {ex.Message}");
            //    if (ex.InnerException != null)
            //    {
            //        Console.WriteLine($"Inner Error: {ex.InnerException.Message}");
            //    }
            //}

            var payload = new
            {
                categories = new string[] { },
                assets = new
                {
                    master = new
                    {
                        id = "65011",
                        name = assetInfo.AssetID,
                        DeviceTypeCategory = "BodyWorn",
                        typeOfAsset = assetInfo.MediaType.ToString(),
                        status = "Uploading",
                        state = "Normal",
                        unitId = 1,
                        isRestrictedView = false,
                        duration = assetInfo.DurationInMs,
                        //recording = new
                        //{
                        //    started = assetInfo.RecordingStarted,
                        //    ended = assetInfo.RecordingEnded
                        //},
                        recording = new RecordingInfo
                        {
                            Started = assetInfo.RecordingStarted,
                            Ended = assetInfo.RecordingEnded
                        },
                        buffering = new { pre = 0, post = 0 },
                        owners = new[] { new { CMTFieldValue = "1" } },
                        bookMarks = new object[] { },
                        notes = new object[] { },
                        audioDevice = (string)null,
                        camera = (string)null,
                        isOverlaid = false,
                        recordedByCSV = "admin@getac.com",
                        files = new[]
                        {
                        new {
                            id = 0,
                            assetId = 0,
                            filesId = "65011",
                            accessCode = "XsWmuJeanfoK+Z8vZ3L1csB1RU3NqjLUjDvy+UAonueEyJKQukBEihijmVN11tQ+kAIIQfOoaYQBEKdtDal40w==",
                            name = "006_test_file_name",
                            type = "Video",
                            extension = ".mp4",
                            url = "https://g204redactiontest.blob.core.usgovcloudapi.net/tn-3/us-1/Evidence/006_test_file_name.mp4",
                            size = assetInfo.FileSizeInBytes,
                            duration = assetInfo.DurationInMs,
                            recording = new {
                                started = assetInfo.RecordingStarted,
                                ended = assetInfo.RecordingEnded
                            },
                            sequence = 1,
                            checksum = (string)null,
                            version = ""
                            }
                        },
                        @lock = (string)null
                    },
                    children = new object[] { }
                },
                stationId = new { CMTFieldValue = 205007 },
                tag = (string)null,
                version = ""
            };
            
            var json = JsonConvert.SerializeObject(payload);
            //SendPost(json);

            string payloadJsonFormatted = JsonConvert.SerializeObject(payload, jsonSettings);
            Console.WriteLine(payloadJsonFormatted);

            Console.ReadKey();
        }

        static void SendPost(string jsonPayload)
        {
            var url = "https://dev-evm4-m.irsavideo.com/api/v1/Evidences"; // Replace with actual URL
            var bearerToken = "eyJhbGciOiJIUzI1NiIsInR5cCI6IkpXVCJ9.eyJUZW5hbnRJZCI6IjE5IiwiVXNlcklkIjoiMSIsIkFzc2lnbmVkR3JvdXBzIjoiW3tcIklkXCI6MSxcIlBlcm1pc3Npb25cIjoxfV0iLCJBc3NpZ25lZE1vZHVsZXMiOiIxLDIsMyw0LDUsNiw3LDgsOSwxMCwxMSwxMiwxMywxNCwxNSwxNywxOCwxOSwyMCwyMSwyMiwyMywyNCwyNSwyNiwyNywyOCwyOSwzMCwzMSwzMiwzMywzNCwzNSwzNiwzNywzOCwzOSw0MCw0MSw0Miw0Myw0NCw0NSw0Niw0Nyw0OCw0OSw1MCw1MSw1Miw1Myw1NCw1NSw1Niw1Nyw2MCw2MSw2Miw2Myw2NCw3MCw3MSw3Miw3Myw3NCw3NSw3Nyw3OCw3OSw4MCw4MSw4Miw4Myw4NCw4NSw4Niw4Nyw4OCw4OSw5MCw5MSw5Miw5Myw5NCw5NSw5Niw5Nyw5OCw5OSwxMDEsMTAyLDEwMywxMDQsMTA1LDEwNiwxMDcsMTA5LDExMCwxMTIsMTEzLDExNCwxMTUsMTE2LDExNywxMTgsMTE5LDEyMCwxMjEsMTIyIiwiTG9naW5JZCI6ImFkbWluQGdldGFjLmNvbSIsIkZOYW1lIjoiU3VwZXIiLCJMTmFtZSI6IjEyMyIsIldvcmtzcGFjZUlkIjoiIiwiU3F1YWRJbmZvcyI6IltdIiwibmJmIjoxNzUwMzI4MTA1LCJleHAiOjE3NTAzMzE3MDUsImlhdCI6MTc1MDMyODEwNX0.Yd1Lq80HIwZ2bv7E3IRdwDJBswXVoK_sg1JQm4zwVyo"; // Copy from your browser cookie

            using (var client = new HttpClient())
            {
                client.DefaultRequestHeaders.Clear();
                client.DefaultRequestHeaders.Add("Authorization", $"Bearer {bearerToken}");

                var content = new StringContent(jsonPayload, Encoding.UTF8, "application/json");
                var response = client.PostAsync(url, content).Result;

                Console.WriteLine("Status: " + response.StatusCode);
                Console.WriteLine("Response: " + response.Content.ReadAsStringAsync().Result);
            }
        }
    }
}