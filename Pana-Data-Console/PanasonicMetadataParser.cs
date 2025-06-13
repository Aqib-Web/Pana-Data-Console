using Pana_Data_Console.Entities;
using System;
using System.IO;
using System.Linq;
using System.Xml.Linq;

namespace Pana_Data_Console
{
    public class PanasonicMetadataParser
    {
        public AssetInfoDTO ParseMetadata(string metadataFilePath, string bookmarkFilePath = null)
        {
            // Load metadata file
            var xdoc = XDocument.Load(metadataFilePath);
           
            // Get F-Info element
            var fInfo = xdoc.Descendants("F-Info").FirstOrDefault();

            // Get FileList and File
            var fileList = fInfo.Element("FileList");
            var fileInfo = fileList.Elements("File").FirstOrDefault();


            // Create asset info with object initialization
            var assetInfo = new AssetInfo
            {
                AssetID = fileInfo.Element("F-Name")?.Value ?? "",
                FilePath = fileInfo.Element("F-Name")?.Value ?? "",
                FileSizeInBytes = long.Parse(fileInfo.Attribute("size")?.Value ?? "0"),
                DurationInMs = long.Parse(fileInfo.Element("RecDR")?.Value ?? "0"),
                RecordingStarted = DateTime.Parse(fileInfo.Element("RecST")?.Value ?? DateTime.Now.ToString()),
                RecordingEnded = DateTime.Parse(fileInfo.Element("RecET")?.Value ?? DateTime.Now.ToString()),
                CameraName = "Panasonic Body Camera",
                MediaType = IRSAMediaTypeEnum.Video,
                AssetStatus = eMediaStatus.Queued,
                AssetState = eMediaState.Normal,
                IsMaster = true
            };

            // Create DTO of assetInfo object
            var assetInfoDTO = new AssetInfoDTO
            {
                AssetID = assetInfo.AssetID,
                FilePath = assetInfo.FilePath,
                FileSizeInBytes = assetInfo.FileSizeInBytes,
                DurationInMs = assetInfo.DurationInMs,
                RecordingStarted = assetInfo.RecordingStarted,
                RecordingEnded = assetInfo.RecordingEnded,
                CameraName = assetInfo.CameraName,
                MediaType = assetInfo.MediaType,
                AssetStatus = assetInfo.AssetStatus,
                AssetState = assetInfo.AssetState,
                IsMaster = assetInfo.IsMaster
            };

            //// Load bookmark file if it exists
            //if (File.Exists(bookmarkFilePath))
            //{
            //    var bookmarkDoc = XDocument.Load(bookmarkFilePath);

            //    // Get V-ID from bookmark file
            //    var vId = bookmarkDoc.Descendants("VID").FirstOrDefault()?.Value;
            //    if (!string.IsNullOrEmpty(vId))
            //    {
            //        assetInfo.StationSysSerial = int.Parse(vId.Replace("Lobby Int Rm ", ""));
            //    }

            //    // Get bookmarks
            //    var bookmarks = bookmarkDoc.Descendants("BOOKMARK")
            //        .Select(b => $"{b.Element("TS")?.Value ?? ""},{b.Element("EVENTTYPE")?.Value ?? ""},{b.Element("DESC")?.Value ?? ""}")
            //        .ToList();

            //    if (bookmarks.Any())
            //    {
            //        assetInfo.BookmarksCsv = string.Join("|", bookmarks);
            //    }
            //}

            return assetInfoDTO;
        }
    }
} 