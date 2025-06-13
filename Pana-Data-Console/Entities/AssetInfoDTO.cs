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
        public string CameraName { get; set; }
        public IRSAMediaTypeEnum MediaType { get; set; }
        public eMediaStatus AssetStatus { get; set; }
        public eMediaState AssetState { get; set; }
        public bool IsMaster { get; set; }
    }

}
