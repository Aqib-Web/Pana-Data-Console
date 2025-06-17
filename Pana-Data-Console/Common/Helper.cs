using Pana_Data_Console.Entities;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Xml.Linq;

namespace Pana_Data_Console.Common
{
    public static class Helper 
    {
        public static IRSAMediaTypeEnum GetMediaType(this XElement fileElement)
        {
            var dtype = fileElement?.Attribute("dtype")?.Value?.Trim().ToLower();
            var mediaType = IRSAMediaTypeEnum.Others;

            switch (dtype)
            {
                case "v":
                    mediaType = IRSAMediaTypeEnum.Video;
                    break;
                case "a":
                    mediaType = IRSAMediaTypeEnum.Audio;
                    break;
                case "m":
                    mediaType = IRSAMediaTypeEnum.Others;
                    break;
            }

            return mediaType;
        }
    }

}
