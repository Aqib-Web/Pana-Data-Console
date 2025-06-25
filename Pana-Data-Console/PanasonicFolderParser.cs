using Pana_Data_Console.Common;
using Pana_Data_Console.Entities;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text.RegularExpressions;
using System.Xml.Linq;
using static Pana_Data_Console.Entities.EMV4AssetPayloadModels;
using File = Pana_Data_Console.Entities.EMV4AssetPayloadModels.File;

namespace Pana_Data_Console
{
    public class PanasonicFolderParser
    {
        public static EMV4Asset ParseFolder(string folderPath)
        {
            //Static Values
            string ACCESS_CODE = "XsWmuJeanfoK+Z8vZ3L1csB1RU3NqjLUjDvy+UAonueEyJKQukBEihijmVN11tQ+kAIIQfOoaYQBEKdtDal40w==";
            int OWNER_VALUE = 1;
            int STATION_ID_VALUE = 210108;
            int UNIT_ID = 1;
            string EVM4_URL = "https://g204redactiontest.blob.core.usgovcloudapi.net/tn-3/us-1/Evidence";

            // Find the *b.xml file (main event/segment list)
            var bXmlPath = Directory.EnumerateFiles(folderPath, "*b.xml").FirstOrDefault();
            if (bXmlPath == null)
                throw new FileNotFoundException("No *b.xml file found in folder.");

            var bDoc = XDocument.Load(bXmlPath);

            // Build segment list from <FileList> in b.xml
            var fileList = bDoc.Descendants("FileList")
                   .Elements("File")
                   .Where(f => (string)f.Attribute("dtype") != "m")
                   .ToList();

            // Group by split (segment number)
            var segments = fileList.GroupBy(f => (int?)f.Attribute("split") ?? 0)
                                   .OrderBy(g => g.Key)
                                   .ToList();

            // --- NEW LOGIC: Combine all files into one master asset ---
            var allFiles = new List<File>();
            DateTime? overallStart = null, overallEnd = null;
            long totalDuration = 0;
            string masterAssetName = null;
            string cameraNameMaster = null;
            IRSAMediaTypeEnum? mainMediaTypeMaster = null;

            foreach (var segment in segments)
            {
                int sequence = segment.Key;
                foreach (var fileElem in segment)
                {
                    //Parse FileName
                    var fName = fileElem.Element("F-Name")?.Value;
                    if (string.IsNullOrEmpty(fName)) continue;
                    var filePath = Path.Combine(folderPath, fName);
                    if (!System.IO.File.Exists(filePath))
                    {
                        Console.WriteLine($"Warning: File not found: {filePath}");
                        continue;
                    }
                    var ext = Path.GetExtension(fName); //Get Extension
                    var mediaType = fileElem.GetMediaType(); //Get MediaType
                    if (mainMediaTypeMaster == null)
                        mainMediaTypeMaster = mediaType;

                    // Parse times
                    var recStStr = fileElem.Element("RecST")?.Value;
                    var recEtStr = fileElem.Element("RecET")?.Value;
                    DateTime recSt, recEt;

                    if (DateTime.TryParse(recStStr, out recSt))
                    {
                        if (overallStart == null || recSt < overallStart)
                            overallStart = recSt;
                    }
                    if (DateTime.TryParse(recEtStr, out recEt))
                    {
                        if (overallEnd == null || recEt > overallEnd)
                            overallEnd = recEt;
                    }
                    
                    // Calculate Duration
                    long fileDuration = 0;
                    if (recStStr != null && recEtStr != null && DateTime.TryParse(recStStr, out recSt) && DateTime.TryParse(recEtStr, out recEt))
                        fileDuration = (long)(recEt - recSt).TotalMilliseconds;
                    if(mediaType == IRSAMediaTypeEnum.Video) 
                        totalDuration += fileDuration;

                    

                    allFiles.Add(new File
                    {
                        AccessCode = ACCESS_CODE,
                        Name = Path.GetFileNameWithoutExtension(fName),
                        Type = mediaType.ToString(),
                        Extension = ext,
                        URL = $"{EVM4_URL}{fName}",
                        Size = fileElem.Attribute("size") != null ? long.Parse(fileElem.Attribute("size").Value) : 0,
                        Duration = fileDuration,
                        Recording = new RecordingInfo
                        {
                            Started = recStStr != null && DateTime.TryParse(recStStr, out recSt) ? recSt : DateTime.MinValue,
                            Ended = recEtStr != null && DateTime.TryParse(recEtStr, out recEt) ? recEt : DateTime.MinValue
                        },
                        Sequence = sequence,
                        Version = ""
                    });
                }
            }

            //Get MasterAsset Name
            if (!string.IsNullOrEmpty(allFiles?.FirstOrDefault()?.Name))
            {
                var fullName = allFiles.First().Name;
                var match = Regex.Match(fullName, @"^(.+?_){2}");
                if (match.Success)
                {
                    masterAssetName = match.Value.TrimEnd('_');
                }
            }

            // Calculate pre-buffer duration (RecST - PreST) from the first video file in split=0
            long preBuffer = 0;

            var firstVideoFile = bDoc.Descendants("FileList")
                                     .Elements("File")
                                     .Where(f => (string)f.Attribute("split") == "0" &&
                                                 (string)f.Attribute("dtype") == "v")
                                     .FirstOrDefault();

            if (firstVideoFile != null)
            {
                var preStStr = firstVideoFile.Element("PreST")?.Value;
                var recStStr = firstVideoFile.Element("RecST")?.Value;

                if (DateTime.TryParse(preStStr, out var preSt) &&
                    DateTime.TryParse(recStStr, out var recSt) &&
                    recSt > preSt)
                {
                    preBuffer = (long)(recSt - preSt).TotalMilliseconds;
                }
            }

            // Find the *m.xml files
            var mXmlFiles = Directory.EnumerateFiles(folderPath, "*m.xml").ToList();
            var mXmlDocs = mXmlFiles.ToDictionary(f => Path.GetFileName(f), f => XDocument.Load(f));

            //Try to get camera name from *m.xml if available
            var firstMXmlDoc = mXmlDocs.Values.FirstOrDefault();

            if (firstMXmlDoc != null)
            {
                string videoChannel = fileList.First(f => f.Attribute("dtype")?.Value == "v")
                    .Attribute("ch")?.Value;

                string cameraName = firstMXmlDoc
                    .Descendants("CameraIn")
                    .FirstOrDefault(c => c.Attribute("id")?.Value == videoChannel)?
                    .Element("Name")?.Value;

                cameraNameMaster = cameraName;
            }

            var master = new Asset
            {
                DeviceTypeCategory = "BodyWorn",
                Name = masterAssetName,
                TypeOfAsset = mainMediaTypeMaster != null ? mainMediaTypeMaster.ToString() : "Unknown",
                Status = "Uploading",
                State = "Normal",
                UnitId = UNIT_ID,
                IsRestrictedView = false,
                Duration = totalDuration,
                Recording = new RecordingInfo
                {
                    Started = overallStart ?? DateTime.MinValue,
                    Ended = overallEnd ?? DateTime.MinValue
                },
                Buffering = new Buffering
                {
                    Pre = preBuffer,
                    Post = 0
                },
                Owners = new List<CMTFieldValueWrapper> { new CMTFieldValueWrapper { Value = OWNER_VALUE } },
                BookMarks = new List<BookMark>(),
                Notes = new List<Note>(),
                AudioDevice = null,
                Camera = cameraNameMaster,
                IsOverlaid = false,
                RecordedByCSV = null,
                Files = allFiles,
                Lock = null,
                Version = ""
            };

            var children = new List<Asset>();

            var emv4Asset = new EMV4Asset
            {
                Categories = new List<Category>(),
                SecurityDescriptors = new List<SecurityDescriptor>(),
                Assets = new Assets
                {
                    Master = master,
                    Children = children
                },
                StationId = new CMTFieldValueWrapper { Value = STATION_ID_VALUE },
                Tag = null,
                Version = ""
            };

            return emv4Asset;
        }
    }
} 