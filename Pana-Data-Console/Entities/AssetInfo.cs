using Pana_Data_Console.Entities;
using System;
using System.Collections.Generic;

namespace Pana_Data_Console
{
    public class AssetInfo
    {
        /// <summary>
        /// VideoSysSerial. No need to supply this when inserting new Video records.
        /// </summary>
        public long VideoSysSerial { get; set; }
        public string FilePath { get; set; }
        public string BLOBUri { get; set; }

        /// <summary>
        /// File Extension. Specify without dot e.g mp3, png, jpg etc.
        /// </summary>
        public string FileExtension { get; set; }

        public IRSAMediaTypeEnum MediaType { get; set; }

        public int ImageWidth { get; set; }
        public int ImageHeight { get; set; }

        /// <summary>
        /// GUID of Media. If null then it will be generated automatically
        /// </summary>
        public Guid? RowGuid { get; set; }
        public string AssetID { get; set; }

        public int? IRSAClientId { get; set; }
        public bool IsMaster { get; set; }
        public bool IsTruncated { get; set; }
        public string AssetNotes { get; set; }

        /// <summary>
        /// This can include Devices, SourceType etc. Must be in following XML format:
        /// e.g <VideoTags>
        ///        <row><SysSerial></SysSerial><TagGroup>SourceType</TagGroup><TagName>FieldContact</TagName><StationId>1</StationId><Value>John Doe/111-222-333</Value></row>
        ///        <row><SysSerial></SysSerial><TagGroup>UD</TagGroup><TagName>SourceDeviceID</TagName><StationId>1</StationId><Value>IPhone 6</Value></row>
        ///        <row><SysSerial></SysSerial><TagGroup>UD</TagGroup><TagName>SourceDeviceID</TagName><StationId>1</StationId><Value>HTC Phone</Value></row>
        ///     </VideoTags>
        /// </summary>
        public string AssetTags { get; set; }

        /// <summary>
        /// Duration of Media (Seconds)
        /// </summary>
        public long DurationInMs { get; set; }

        public long FileSizeInBytes { get; set; }
        public DateTime RecordingStarted { get; set; }
        public DateTime RecordingEnded { get; set; }
        public string MetaData { get; set; }
        public string WMVPath { get; set; }

        public int? PreBufferTime { get; set; }
        public int? PostBufferTime { get; set; }

        public string ParentID { get; set; }
        public string SiblingID { get; set; }
        /// <summary>
        /// For InCar case only
        /// </summary>
        public int RetentionPolicyId { get; set; }
        public bool IsOverlayOnVideo { get; set; }
        public bool IsRestrictedView { get; set; }
        public byte[] VideoCheckSum { get; set; }
        public bool? isCheckSumValid { get; set; }
        public DateTime? ClientExpiryDate { get; set; }
        /// <summary>
        /// Used as container In InCar services
        /// </summary>
        public string BookmarksCsv { get; set; }
        public string MediaGPS { get; set; }

        /// <summary>
        /// UserIds of Media owners
        /// </summary>
        public List<long> OwnerIds { get; set; }

        public eMediaStatus AssetStatus { get; set; }
        public eMediaState AssetState { get; set; }
        public IRSAFileStatusEnum ClientStatus { get; set; }

        public string CameraName { get; set; }

        public string MicName { get; set; }
        public DateTime? ImportDate { get; set; }

        public DateTime? UploadDate { get; set; }
        public int Imported { get; set; }
        public int StorageType { get; set; }

        /// <summary>
        /// For Continous Videos only. Indicates total segments (parts) of a Continous Video
        /// </summary>
        public int TotalSegments { get; set; }

        /// <summary>
        /// For Continous Videos only. Indicates commulative duration (in millseconds) of all segments of a continous Video.
        /// Note: There can be a possible little difference between actual duration of whole video and this field's value.
        /// </summary>
        public long CommulativeDuration { get; set; }

        /// <summary>
        /// For Continous Videos only. Indicates commulative size (in bytes) of all segments of a continous Video.
        /// </summary>
        public long CommulativeSize { get; set; }

        public bool IsMetadata { get; set; }

        public int TempSerial { get; set; }
        public string ChecksumAlgo { get; set; }

        public int StationSysSerial { get; set; }
        public int UploadPriority { get; set; }
        public string ActualFileName { get; set; }
    }
}
