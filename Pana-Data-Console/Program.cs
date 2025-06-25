using Newtonsoft.Json;
using System;

namespace Pana_Data_Console
{
    class Program
    {
        static void Main(string[] args)
        {
            string folderPath = @"D:\101209715";
            
            var em4Asset = PanasonicFolderParser.ParseFolder(folderPath);
            var json = JsonConvert.SerializeObject(em4Asset);
 
            // JSON pretty printing setting
            var jsonSettings = new JsonSerializerSettings
            {
                Formatting = Formatting.Indented,
                NullValueHandling = NullValueHandling.Ignore
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

            string payloadJsonFormatted = JsonConvert.SerializeObject(em4Asset, jsonSettings);
            Console.WriteLine(payloadJsonFormatted);

            //ApiClient.PostEvidenceToEVM4(json);


            Console.ReadKey();
        }


    }
}