using System;
using System.Collections.Generic;
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
                string folderPath = @"D:\101209715";

                var parser = new PanasonicMetadataParser();
                var newParser = new PanasonicFolderParser();
                var em4Asset = newParser.ParseFolder(folderPath);

                var assetInfo = parser.ParseMetadata(metadataPath, bookmarkPath);
                var bookmarksInfo = parser.ParseBookmark(bookmarkPath);

                // Convert to JSON with pretty printing
                var jsonSettings = new JsonSerializerSettings
                {
                    Formatting = Formatting.Indented,
                    //NullValueHandling = NullValueHandling.Ignore
                };

            #region Old Payload
            /*
            var payload = new EMV4Asset
            {
                Categories = new List<Category>(),  // <------EMPTY FOR NOW
                Assets = new Assets
                {
                    Master = new Asset
                    {
                        Id = "65011",
                        DeviceTypeCategory = "BodyWorn",
                        Name = assetInfo.AssetID + "_003",  // <= <= <=
                        TypeOfAsset = assetInfo.MediaType.ToString(),  // <= <= <=
                        Status = "Uploading",
                        State = "Normal",
                        UnitId = 1,
                        IsRestrictedView = false,
                        Duration = assetInfo.DurationInMs,  // <= <= <=

                        Recording = new RecordingInfo
                        {
                            Started = assetInfo.RecordingStarted,  // <= <= <=
                            Ended = assetInfo.RecordingEnded  // <= <= <=
                        },

                        Buffering = new Buffering
                        {
                            Pre = 0,
                            Post = 0
                        },

                        Owners = new List<CMTFieldValueWrapper>
                        {
                            new CMTFieldValueWrapper { Value = 1 }
                        },

                        BookMarks = new List<BookMark> (), // <------EMPTY FOR NOW
                        Notes = new List<Note> (), // <------EMPTY FOR NOW

                        AudioDevice = (string)null,
                        Camera = (string)null,
                        IsOverlaid = false,
                        RecordedByCSV = "admin@getac.com",

                        Files = new List<File>
                        {
                            new File
                            {
                                //Id = 0,
                                //AssetId = 0,
                                FilesId = 65011,
                                AccessCode = "XsWmuJeanfoK+Z8vZ3L1csB1RU3NqjLUjDvy+UAonueEyJKQukBEihijmVN11tQ+kAIIQfOoaYQBEKdtDal40w==",
                                Name = assetInfo.AssetID + "_003",  // <= <= <=
                                Type = assetInfo.MediaType.ToString(), // <= <= <=
                                Extension = ".mp4",
                                URL = $"https://g204redactiontest.blob.core.usgovcloudapi.net/tn-3/us-1/Evidence/{assetInfo.AssetID}_003.mp4",  // <= <= <=
                                Size = assetInfo.FileSizeInBytes, // <= <= <=
                                Duration = assetInfo.DurationInMs, // <= <= <=
                                Recording = new RecordingInfo
                                {
                                    Started = assetInfo.RecordingStarted,  // <= <= <=
                                    Ended = assetInfo.RecordingEnded  // <= <= <=
                                },
                                Sequence = 1,
                                Checksum = null,
                                Version = "",
                            }
                        },
                        Lock = (string)null
                    },
                    Children = new List<Asset>(), // <------EMPTY FOR NOW
                },

                StationId = new CMTFieldValueWrapper
                {
                    Value = 205011
                },
                Tag = (string)null,
                Version = ""
            };
            */
            #endregion


            //var json = JsonConvert.SerializeObject(payload);

            var json2 = JsonConvert.SerializeObject(em4Asset);
            SendPost(json2);

            //string payloadJsonFormatted = JsonConvert.SerializeObject(payload, jsonSettings);

            string payloadJsonFormatted2 = JsonConvert.SerializeObject(em4Asset, jsonSettings);
            //Console.WriteLine(payloadJsonFormatted);
            Console.WriteLine(payloadJsonFormatted2);
            Console.ReadKey();
        }

        static void SendPost(string jsonPayload)
        {
            var url = "https://dev-evm4-m.irsavideo.com/api/v1/Evidences";
            var bearerToken = "eyJhbGciOiJIUzI1NiIsInR5cCI6IkpXVCJ9.eyJUZW5hbnRJZCI6IjE5IiwiVXNlcklkIjoiMSIsIkFzc2lnbmVkR3JvdXBzIjoiW3tcIklkXCI6MSxcIlBlcm1pc3Npb25cIjoxfV0iLCJBc3NpZ25lZE1vZHVsZXMiOiIxLDIsMyw0LDUsNiw3LDgsOSwxMCwxMSwxMiwxMywxNCwxNSwxNywxOCwxOSwyMCwyMSwyMiwyMywyNCwyNSwyNiwyNywyOCwyOSwzMCwzMSwzMiwzMywzNCwzNSwzNiwzNywzOCwzOSw0MCw0MSw0Miw0Myw0NCw0NSw0Niw0Nyw0OCw0OSw1MCw1MSw1Miw1Myw1NCw1NSw1Niw1Nyw2MCw2MSw2Miw2Myw2NCw3MCw3MSw3Miw3Myw3NCw3NSw3Nyw3OCw3OSw4MCw4MSw4Miw4Myw4NCw4NSw4Niw4Nyw4OCw4OSw5MCw5MSw5Miw5Myw5NCw5NSw5Niw5Nyw5OCw5OSwxMDEsMTAyLDEwMywxMDQsMTA1LDEwNiwxMDcsMTA5LDExMCwxMTIsMTEzLDExNCwxMTUsMTE2LDExNywxMTgsMTE5LDEyMCwxMjEsMTIyIiwiTG9naW5JZCI6ImFkbWluQGdldGFjLmNvbSIsIkZOYW1lIjoiU3VwZXIiLCJMTmFtZSI6IjEyMyIsIldvcmtzcGFjZUlkIjoiIiwiU3F1YWRJbmZvcyI6IltdIiwibmJmIjoxNzUwNzc2MzQ2LCJleHAiOjE3NTA3Nzk5NDYsImlhdCI6MTc1MDc3NjM0Nn0.3-t6wUI6tVPzgN0vzRQyKqaWaZSbE1fhlK9Idr072N4";
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