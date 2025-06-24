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
        public EMV4Asset ParseFolder(string folderPath)
        {
            // Find the .b.xml file (main event/segment list)
            var bXmlPath = Directory.EnumerateFiles(folderPath, "*b.xml").FirstOrDefault();
            if (bXmlPath == null)
                throw new FileNotFoundException("No .b.xml file found in folder.");

            var bDoc = XDocument.Load(bXmlPath);

            // Find all .m.xml files (segment metadata)
            //var mXmlFiles = Directory.EnumerateFiles(folderPath, "*m.xml").ToList();
            //var mXmlDocs = mXmlFiles.ToDictionary(f => Path.GetFileName(f), f => XDocument.Load(f));


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
            string assetIdMaster = null;
            string masterAssetName = null;
            string cameraNameMaster = null;
            IRSAMediaTypeEnum? mainMediaTypeMaster = null;

            foreach (var segment in segments)
            {
                int sequence = segment.Key;
                foreach (var fileElem in segment)
                {
                    var fName = fileElem.Element("F-Name")?.Value;
                    if (string.IsNullOrEmpty(fName)) continue;
                    var filePath = Path.Combine(folderPath, fName);
                    if (!System.IO.File.Exists(filePath))
                    {
                        Console.WriteLine($"Warning: File not found: {filePath}");
                        continue;
                    }
                    var ext = Path.GetExtension(fName);
                    var mediaType = fileElem.GetMediaType();
                    if (mainMediaTypeMaster == null)
                        mainMediaTypeMaster = mediaType;
                    //if (assetIdMaster == null)
                    //    assetIdMaster = Path.GetFileNameWithoutExtension(fName);

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
                    long fileDuration = 0;
                    if (recStStr != null && recEtStr != null && DateTime.TryParse(recStStr, out recSt) && DateTime.TryParse(recEtStr, out recEt))
                        fileDuration = (long)(recEt - recSt).TotalMilliseconds;
                    totalDuration += fileDuration;

                    allFiles.Add(new File
                    {
                        //FilesId = 0,
                        AccessCode = "XsWmuJeanfoK+Z8vZ3L1csB1RU3NqjLUjDvy+UAonueEyJKQukBEihijmVN11tQ+kAIIQfOoaYQBEKdtDal40w==",
                        Name = Path.GetFileNameWithoutExtension(fName),
                        Type = mediaType.ToString(),
                        Extension = ext,
                        URL = $"https://g204redactiontest.blob.core.usgovcloudapi.net/tn-3/us-1/Evidence/{fName}",
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

            // Try to get camera name from .m.xml if available
            //if (mXmlDocs.Count > 0)
            //{
            //    var mDoc = mXmlDocs.Values.FirstOrDefault();
            //    var vInfo = mDoc?.Descendants("V-Info").FirstOrDefault();
            //    cameraNameMaster = vInfo?.Element("Recorder")?.Attribute("model")?.Value;
            //}


            if (!string.IsNullOrEmpty(allFiles?.FirstOrDefault()?.Name))
            {
                var fullName = allFiles.First().Name;
                var match = Regex.Match(fullName, @"^(.+?_){2}");
                if (match.Success)
                {
                    masterAssetName = match.Value.TrimEnd('_');
                }
            }

            var master = new Asset
            {
                //Id = assetIdMaster,
                DeviceTypeCategory = "BodyWorn",
                Name = masterAssetName,
                TypeOfAsset = mainMediaTypeMaster != null ? mainMediaTypeMaster.ToString() : "Unknown",
                Status = "Uploading",
                State = "Normal",
                UnitId = 1,
                IsRestrictedView = false,
                Duration = totalDuration,
                Recording = new RecordingInfo
                {
                    Started = overallStart ?? DateTime.MinValue,
                    Ended = overallEnd ?? DateTime.MinValue
                },
                Buffering = new Buffering { Pre = 0, Post = 0 },
                Owners = new List<CMTFieldValueWrapper> { new CMTFieldValueWrapper { Value = 1 } },
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
                //Id = master?.Id,
                Categories = new List<Category>(),
                SecurityDescriptors = new List<SecurityDescriptor>(),
                Assets = new Assets
                {
                    Master = master,
                    Children = children
                },
                StationId = new CMTFieldValueWrapper { Value = 210104 },
                Tag = null,
                Version = ""
            };

            return emv4Asset;
        }
    }
} 