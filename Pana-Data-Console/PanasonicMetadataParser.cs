using Pana_Data_Console.Common;
using Pana_Data_Console.Entities;
using System;
using System.Collections.Generic;
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
            var fileInfo = xdoc.Descendants("F-Info")
                   .FirstOrDefault()?
                   .Element("FileList")?
                   .Elements("File")
                   .FirstOrDefault();

            // Get V-Info element
            var vInfo = xdoc.Descendants("V-Info").FirstOrDefault();



            //Data Extraction

            var unitID = vInfo?.Element("Recorder")?.Attribute("serial")?.Value;
            var CameraName = vInfo?.Element("Recorder")?.Attribute("model")?.Value;
            var vehicleId = xdoc.Descendants("Login")
                    .Elements("Field")
                    .FirstOrDefault(f => f.Attribute("id")?.Value == "5")
                    ?.Value;

            var fileName = fileInfo.Element("F-Name")?.Value ?? "";

            // Create asset info with object initialization
            var assetInfo = new AssetInfo
            {
                AssetID = Path.GetFileNameWithoutExtension(fileName),
                FilePath = metadataFilePath,
                FileExtension = Path.GetExtension(fileName)?.TrimStart('.'),
                FileSizeInBytes = long.Parse(fileInfo.Attribute("size")?.Value ?? "0"),
                RecordingStarted = DateTime.Parse(fileInfo.Element("RecST")?.Value ?? DateTime.Now.ToString()),
                RecordingEnded = DateTime.Parse(fileInfo.Element("RecET")?.Value ?? DateTime.Now.ToString()),
                CameraName = $"Panasonic {CameraName}",
                MediaType = fileInfo.GetMediaType(),
                IsMaster = true
            };

            // Load bookmark file if it exists
            if (File.Exists(bookmarkFilePath))
            {
                var bookmarkDoc = XDocument.Load(bookmarkFilePath);

                // Get V-ID from bookmark file
                var vId = bookmarkDoc.Descendants("VID").FirstOrDefault()?.Value;
                if (!string.IsNullOrEmpty(vId))
                {
                    assetInfo.StationSysSerial = int.Parse(vId.Replace("Lobby Int Rm ", ""));
                }

                // Get bookmarks
                var bookmarks = bookmarkDoc.Descendants("BOOKMARK")
                    .Select(b => $"{b.Element("TS")?.Value ?? ""},{b.Element("EVENTTYPE")?.Value ?? ""},{b.Element("DESC")?.Value ?? ""}")
                    .ToList();

                if (bookmarks.Any())
                {
                    assetInfo.BookmarksCsv = string.Join("|", bookmarks);
                }

                //Get Total Segments Count
                var totalSegments = bookmarkDoc.Descendants("FileList").LastOrDefault()?
                    .Elements("File")
                    .Select(f => (int?)f.Attribute("split"))
                    .Max() + 1 ?? 0;
                assetInfo.TotalSegments = totalSegments;
            }

            // Create DTO of assetInfo object
            var assetInfoDTO = new AssetInfoDTO
            {
                AssetID = assetInfo.AssetID,
                FilePath = assetInfo.FilePath,
                FileSizeInBytes = assetInfo.FileSizeInBytes,
                DurationInMs = (long)(assetInfo.RecordingEnded - assetInfo.RecordingStarted).TotalMilliseconds,
                RecordingStarted = assetInfo.RecordingStarted,
                RecordingEnded = assetInfo.RecordingEnded,
                TotalSegments = assetInfo.TotalSegments,
                CameraName = assetInfo.CameraName,
                MediaType = assetInfo.MediaType,
                IsMaster = assetInfo.IsMaster,
                UnitID = unitID,
                VehicleID = vehicleId,
                BookmarksCsv = assetInfo.BookmarksCsv
            };

            return assetInfoDTO;
        }

        public List<Bookmark> ParseBookmark(string bookmarkFilePath)
        {
            var bookmarks = new List<Bookmark>();

            if (!File.Exists(bookmarkFilePath))
                return bookmarks;

            var doc = XDocument.Load(bookmarkFilePath);

            bookmarks = doc.Descendants("BOOKMARK")
                           .Select(b => new Bookmark
                           {
                               VehicleID = b.Element("VID")?.Value ?? "",
                               Area = b.Element("AREA")?.Value ?? "",
                               Timestamp = b.Element("TS")?.Value ?? "",
                               Channel = b.Element("CH")?.Value ?? "",
                               EventType = b.Element("EVENTTYPE")?.Value ?? "",
                               Description = b.Element("DESC")?.Value ?? ""
                           })
                           .ToList();

            return bookmarks;
        }
    }
} 