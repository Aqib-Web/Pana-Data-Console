namespace Pana_Data_Console.Entities
{
    public enum IRSAMediaTypeEnum
    {
        Video = 1,
        Audio = 2,
        Image = 3,
        WMV_Video = 5,
        WordDoc = 6,
        PDFDoc = 7,
        Text = 8,
        Zip = 9,
        ExcelDoc = 10,
        PowerPointDoc = 11,
        AvenueSource = 12,
        // Only for Auto Update Case
        // ===================================
        DLL = 13,
        Exe = 14,
        Msi = 15,
        bin = 16,
        // ===================================
        Others = 17,
        BBvideo = 18,
        BW2Certificate = 19
    }

    public enum eMediaStatus
    {
        Queued = 1,
        Uploading = 2,
        Processing = 3,
        VerifiedHash = 4,
        Failed = 5,
        Available = 6,
        MetadataOnly = 7,
        RequestUpload = 8,
        ManuallyUploaded = 10,
        UnitReset = 11,


    }

    public enum eMediaState
    {
        Normal = 1,
        Removing = 2,
        Trash = 3,
        Deleted = 4,
        Hold = 5
    }

    public enum IRSAFileStatusEnum
    {
        NoChange = 0,
        QueuedForUploading = 1,
        Uploading = 2,
        Uploaded = 3,
        Transcoding = 4,
        Transcoded = 5,
        Dispatched = 6,
        Error = 7,
        Retry = 8,
        Delete = 9
    }


}
