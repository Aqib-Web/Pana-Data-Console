using Newtonsoft.Json;
using System;
using System.Collections.Generic;


namespace Pana_Data_Console.Entities
{
    class EMV4AssetPayloadModels
    {

        public class RecordingInfo
        {
            public DateTime Started { get; set; }
            public DateTime Ended { get; set; }
            public int TimeOffset { get; set; }
        }

        public class Buffering
        {
            public int Pre { get; set; }
            public int Post { get; set; }
        }

        public class Checksum
        {
            [JsonProperty("Checksum")]
            public string ChecksumValue { get; set; }
            public bool Status { get; set; }
            public string Algorithm { get; set; }
        }

        public class CMTFieldValueWrapper
        {
            public int CMTFieldValue { get; set; }
        }

        public class FileInfo
        {
            public string Id { get; set; }
            public string AssetId { get; set; }
            public int FilesId { get; set; }
            public string Name { get; set; }
            public string Type { get; set; }
            public string Extension { get; set; }
            public string URL { get; set; }
            public int Size { get; set; }
            public int Duration { get; set; }
            public RecordingInfo Recording { get; set; }
            public int Sequence { get; set; }
            public Checksum Checksum { get; set; }
            public string Version { get; set; }
            public DateTime CreatedOn { get; set; }
            public DateTime ModifiedOn { get; set; }
        }

        public class Bookmark
        {
            public string Id { get; set; }
            public string AssetId { get; set; }
            public DateTime BookmarkTime { get; set; }
            public int Position { get; set; }
            public string Description { get; set; }
            public string MadeBy { get; set; }
            public DateTime CreatedOn { get; set; }
            public string Version { get; set; }
            public CMTFieldValueWrapper User { get; set; }
            public CMTFieldValueWrapper UserInfo { get; set; }
        }

        // Mid-level models
        public class Owner
        {
            public int CMTFieldValue { get; set; }
        }

        public class Field
        {
            public string Id { get; set; }
            public string Key { get; set; }
            public string Value { get; set; }
            public string DataType { get; set; }
            public string Version { get; set; }
            public DateTime CreatedOn { get; set; }
        }

        public class FormData
        {
            public string FormId { get; set; }
            public CMTFieldValueWrapper Record { get; set; }
            public List<Field> Fields { get; set; }
        }

        public class Category
        {
            public string Id { get; set; }
            public List<FormData> FormData { get; set; }
            public DateTime AssignedOn { get; set; }
            public CMTFieldValueWrapper Record { get; set; }
            public CMTFieldValueWrapper DataRetentionPolicy { get; set; }
            public string Name { get; set; }
        }

        public class SecurityDescriptor
        {
            public int GroupId { get; set; }
            public int Permission { get; set; }
        }

        public class Asset
        {
            public string Id { get; set; }
            public string Name { get; set; }
            public string TypeOfAsset { get; set; }
            public string Status { get; set; }
            public string State { get; set; }
            public int UnitId { get; set; }
            public bool IsRestrictedView { get; set; }
            public int Duration { get; set; }
            public RecordingInfo Recording { get; set; }
            public Buffering Buffering { get; set; }
            public string Camera { get; set; }
            public bool IsOverlaid { get; set; }
            public string RecordedByCSV { get; set; }
            public List<Bookmark> BookMarks { get; set; }
            public List<FileInfo> Files { get; set; }
            public List<Owner> Owners { get; set; }
            public string Version { get; set; }
            public DateTime CreatedOn { get; set; }

        }

        public class Assets
        {
            public Asset Master { get; set; }
            public List<Asset> Children { get; set; }
        }

        // Root model
        public class EMV4Asset
        {
            public string Id { get; set; }
            public List<Category> Categories { get; set; }
            public List<SecurityDescriptor> SecurityDescriptors { get; set; }
            public Assets Assets { get; set; }
            public DateTime ExpireOn { get; set; }
            public CMTFieldValueWrapper StationId { get; set; }
            public string Tag { get; set; }
            public string Version { get; set; }
            public DateTime CreatedOn { get; set; }
            public DateTime ModifiedOn { get; set; }
        }
    }
}
