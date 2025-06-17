using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Pana_Data_Console.Entities
{
    public class AssetInfoDTO
    {
        public string AssetID { get; set; }
        public string FilePath { get; set; }
        public long FileSizeInBytes { get; set; }
        public long DurationInMs { get; set; }
        public DateTime RecordingStarted { get; set; }
        public DateTime RecordingEnded { get; set; }
        public int TotalSegments { get; set; }
        public string CameraName { get; set; }
        public IRSAMediaTypeEnum MediaType { get; set; }
        public bool IsMaster { get; set; }

        public string UnitID { get; set; }
        public string VehicleID { get; set; }

        public string BookmarksCsv { get; set; }
    }

}
