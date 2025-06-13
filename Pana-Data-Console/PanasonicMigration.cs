using Irsa.QuartzCrawler.Jobs;
using Irsa.QuartzCrawler.Util;
using IRSA.Host.BaseCommons.Globals;
using IRSA.Host.BLL.LINQ;
using IRSA.Host.Entities;
using IRSALogger;
using log4net;
using Newtonsoft.Json.Linq;
using Quartz;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Xml.Linq;
using IRSA.Host.BLL;
using System.Globalization;
using System.Web;
using System.Web.Script.Serialization;
using Newtonsoft.Json;

namespace Irsa.QuartzCrawler.Jobs
{
    public class PanasonicMigration : BaseMediaProcessingJob, ILoggable
    {

        private static readonly ILog log = LogManager.GetLogger(System.Reflection.MethodBase.GetCurrentMethod().DeclaringType);
        private static readonly object _lockMergePartFiles = new object();
        public log4net.ILog Log
        {
            get { return log; }
        }
        string _folderToWatch = string.Empty;
        string _errorFolder = string.Empty;
        int _NumberOfFilesToProceed = 0;
        protected override void LoadJobMapContent(Quartz.IJobExecutionContext context)
        {
            log.Debug(enterText);
            base.LoadJobMapContent(context);
            JobDataMap jobDataMap = context.JobDetail.JobDataMap;

            _folderToWatch = (string)jobDataMap.Get(JobDataMapKeys.FOLDER_TO_WATCH);
            _errorFolder = (string)jobDataMap.Get(JobDataMapKeys.PANASONIC_MIGRATION_ERROR_FOLDER_PATH);
            _ftpRootPath = (string)jobDataMap.Get(JobDataMapKeys.FTP_ROOT_PATH);
            _archivedPath = (string)jobDataMap.Get(JobDataMapKeys.ARCHIVED_PATH);
            _NumberOfFilesToProceed = Convert.ToInt32(jobDataMap.Get(JobDataMapKeys.NumberOfFilesToProceed));
            log.Debug(exitText);
        }

        protected override void Run(Quartz.IJobExecutionContext context)
        {
            log.Debug(enterText);
            List<string> filesToProcess = GetFileListByLastModifiedAsc(_folderToWatch);
           
            if (filesToProcess.Count > _NumberOfFilesToProceed)
                filesToProcess.RemoveRange(19, (filesToProcess.Count - _NumberOfFilesToProceed));
            string file;
            for (int i = 0; i < filesToProcess.Count; i++)
            {
                Log.Info(string.Format("Started processing of {0} ", filesToProcess[i]));
                file = filesToProcess[i];
                string fileName = "";
                string fileNameFull = "";
                bool isSuccess = false;
                try
                {
                    if (!file.EndsWith("av.lock"))
                    {
                        file = Path.Combine(_folderToWatch, Path.GetFileNameWithoutExtension(file) + ".xml");
                        if (File.Exists(file))
                        {
                            XDocument xdoc = XDocument.Load(file);
                            if (ValidateFileData(xdoc, file))
                            {
                                IRSA.Host.LINQEntities.ModelsEF.VideoUploadQueue videoUploadQueue = new IRSA.Host.LINQEntities.ModelsEF.VideoUploadQueue()
                                {
                                    FileSize = 0,
                                    BytesUploaded = 0,
                                    CompletedOn = DateTime.Now,
                                    CreatedOn = DateTime.Now,
                                    UpdatedOn = DateTime.Now,
                                    RetryProcessingCount = null
                                };


                                if (!String.IsNullOrWhiteSpace(file))
                                { 
                                    fileNameFull = (from s in xdoc.Descendants("F-Info")
                                                    select s.Element("F-Name").Value).FirstOrDefault().ToString();
                                    fileNameFull = fileNameFull.Replace("/", "_");
                                    int index = fileNameFull.LastIndexOf("_") + 1;
                                    fileName = fileNameFull.Substring(index);
                                }
                                string mediaFilePath = Path.Combine(_folderToWatch, fileName + ".xml");
                                if (File.Exists(mediaFilePath))
                                {

                                    var assetId = fileName;
                                    var existongVideoID = IRSALINQMedia.EvidenceExists(assetId);

                                    if (existongVideoID > 0)
                                    {
                                        string updatedFileName = mediaFilePath.Replace(Path.GetFileNameWithoutExtension(mediaFilePath), assetId + "_meta");
                                        System.IO.File.Move(mediaFilePath, updatedFileName);
                                        MoveAxonMediaItems(Path.GetFileNameWithoutExtension(updatedFileName), Path.GetFileNameWithoutExtension(updatedFileName), _folderToWatch, _ftpRootPath, ".xml");

                                        mediaFilePath = Path.Combine(_folderToWatch, fileName + ".av3");
                                        string updatedFileNameAV = mediaFilePath.Replace(Path.GetFileNameWithoutExtension(mediaFilePath), assetId);
                                        System.IO.File.Move(mediaFilePath, updatedFileNameAV);
                                        MoveAxonMediaItems(Path.GetFileNameWithoutExtension(updatedFileNameAV), Path.GetFileNameWithoutExtension(updatedFileNameAV), _folderToWatch, _ftpRootPath, ".av3");

                                        mediaFilePath = Path.Combine(_folderToWatch, fileName + "b.xml");
                                        string updatedFileNameB = mediaFilePath.Replace(Path.GetFileNameWithoutExtension(mediaFilePath), assetId + "_bookmark");
                                        System.IO.File.Move(mediaFilePath, updatedFileNameB);
                                        MoveAxonMediaItems(Path.GetFileNameWithoutExtension(updatedFileNameB), Path.GetFileNameWithoutExtension(updatedFileNameB), _folderToWatch, _ftpRootPath, ".xml");

                                        mediaFilePath = Path.Combine(_folderToWatch, fileName + ".jpg");
                                        string updatedFileNameThumb = mediaFilePath.Replace(Path.GetFileNameWithoutExtension(mediaFilePath), assetId + "_thumb");
                                        System.IO.File.Move(mediaFilePath, updatedFileNameThumb);
                                        MoveAxonMediaItems(Path.GetFileNameWithoutExtension(updatedFileNameThumb), Path.GetFileNameWithoutExtension(updatedFileNameThumb), _folderToWatch, _ftpRootPath, ".jpg");

                                        continue;
                                    }
                                    TimeZoneInfo timeZone = TimeZoneInfo.Local;
                                    try
                                    {
                                        var timeZoneFile = (from s in xdoc.Descendants("F-Info")
                                                            select s.Element("TZoneInfo").Attribute("name1").Value).FirstOrDefault();
                                        
                                        if (timeZoneFile != "")
                                        {
                                            if (timeZoneFile.Contains("Central"))
                                            {
                                                timeZone = TimeZoneInfo.FindSystemTimeZoneById("Central Standard Time");
                                            }
                                            else
                                            {
                                                timeZone = TimeZoneInfo.FindSystemTimeZoneById(timeZoneFile);
                                            }
                                        }
                                        else
                                        {
                                            timeZone = TimeZoneInfo.FindSystemTimeZoneById("Central Standard Time");
                                        }
                                    }
                                    catch (Exception e) {
                                        log.Error(string.Format("Exception while reading TimeZone information from {0}, setting the default time zone Central Standard Time ", file), e);
                                        timeZone = TimeZoneInfo.FindSystemTimeZoneById("Central Standard Time");
                                    }                            

                                    var video = (from s in xdoc.Descendants("F-Info")
                                                 select new Videos()
                                                 {
                                                     VideoID = Convert.ToString(s.Element("F-Name").Value).Substring(s.Element("F-Name").Value.IndexOf('/')+1),
                                                     RecordingStarted = TimeZoneInfo.ConvertTimeToUtc(DateTime.ParseExact(s.Element("RecST").Value.ToString(), "yyyy-MM-dd,HH:mm:ss", CultureInfo.InvariantCulture),timeZone),
                                                     RecordingEnded = TimeZoneInfo.ConvertTimeToUtc(DateTime.ParseExact(s.Element("RecET").Value.ToString(), "yyyy-MM-dd,HH:mm:ss", CultureInfo.InvariantCulture), timeZone),
                                                     Duration = Convert.ToInt64(s.Element("RecDR").Value)
                                                 }).FirstOrDefault();

                                    try
                                    {
                                        var unitID = (from s in xdoc.Descendants("F-Info")
                                                      select s.Element("V-ID").Value).FirstOrDefault();

                                        if (unitID != "")
                                        {
                                            var clientSys = IRSAClients.GetClientID(unitID);
                                            if (clientSys != 0)
                                            {
                                                video.IRSAClientID = new IRSAClient() { SysSerial = (int)clientSys };
                                            }
                                        }
                                    }catch(Exception ex)
                                    {
                                        log.Error(string.Format("Exception while reading V-ID / ClientID information from {0}", file), ex);
                                    }

                                    string stationName ="";
                                    try
                                    {
                                        stationName = (from s in xdoc.Descendants("F-Info")
                                                           select s.Element("Agency").Value).FirstOrDefault();
                                    }catch(Exception ex)
                                    {
                                        log.Error(string.Format("Exception while reading Agency/ stationName information from {0} ", file), ex);
                                    }

                                    List<IRSAUser> officers  = null;
                                    try
                                    {
                                        officers = (from s in xdoc.Descendants("Officers").Elements()
                                                        select new IRSAUser()
                                                        {
                                                            OfficerID = Convert.ToString(s.Element("O-ID").Value)
                                                        }).ToList();
                                    }catch(Exception ex)
                                    {
                                        log.Error(string.Format("Exception while reading O-ID / Officers information from {0} ", file), ex);
                                    }
                                    

                                    
                                    List<IRSAUser> users = new List<IRSAUser>();
                                    long userID = IRSALINQUsers.GetUserByOfficerIDs(officers);

                                    if (userID == 0)
                                    {
                                        int stationID = 0;
                                        var stationObj = IRSALINQStations.GetIRSAStationByName(stationName);
                                        if (stationObj != null)
                                            stationID = stationObj.sysSerial;

                                        if (stationID != 0)
                                        {
                                            IRSAUser user = new IRSAUser();
                                            var agencyUser = IRSALINQUsers.GetGuestUserByStationId((int)stationID);
                                            user.Station = new IRSAStation();
                                            user.Station.SysSerial = Convert.ToInt64(agencyUser.StationID);
                                            user.SysSerial = agencyUser.sysSerial;
                                            user.LoginID = agencyUser.LoginID;
                                            user.IsAdmin = agencyUser.IsAdmin;
                                            user.UsersRoleID = agencyUser.RoleID;
                                            users.Add(user);
                                        }
                                        else
                                        {
                                            var adminUser = IRSALINQUsers.GetUser(1); //Getting SuperUser to get its station
                                            var stationId = adminUser.StationID.HasValue ? adminUser.StationID.Value : 1;
                                            IRSAUser user = new IRSAUser();
                                            var guestUser = IRSALINQUsers.GetGuestUserByStationId(stationId);
                                            user.Station = new IRSAStation();
                                            user.Station.SysSerial = Convert.ToInt64(guestUser.StationID);
                                            user.SysSerial = guestUser.sysSerial;
                                            user.LoginID = guestUser.LoginID;
                                            user.IsAdmin = guestUser.IsAdmin;
                                            user.UsersRoleID = guestUser.RoleID;
                                            users.Add(user);
                                        }                                        
                                    }
                                    else
                                    {
                                        users.Add(IRSAUsers.GetUsers(new IRSAUser() { SysSerial = userID }).FirstOrDefault());
                                    }
                                    int setStationID = Convert.ToInt32(users.FirstOrDefault().Station.SysSerial);
                                    var incidents = GetIncidentFromMapping(file, setStationID);

                                    if (incidents.Count == 0)
                                    {
                                        int incId = IRSALINQIncidents.GetIncidentByName("Uncategorized");
                                        if (incId > 0)
                                            incidents.Add(incId);
                                    }


                                    int retentionPolicyId = 0;
                                    var incidentWithMaxRentionHour = IRSALINQIncidents.GetIncidentWithMaxRetentionHours(incidents);

                                    if (incidentWithMaxRentionHour != null)
                                    {
                                        retentionPolicyId = incidentWithMaxRentionHour.RetentionPolicyId;
                                    }
                                    else
                                    {
                                        retentionPolicyId = IRSALINQStations.GetUnclassifiedPolicyID(users[0].Station.SysSerial);
                                    }

                                    List<AssetInfo> assetList = new List<AssetInfo>();
                                    AssetInfo assetInfo = new AssetInfo
                                    {
                                        BLOBUri = null,
                                        FileExtension = "xml",
                                        MediaType = IRSAMediaTypeEnum.Others,
                                        ImageWidth = 0,
                                        ImageHeight = 0,
                                        AssetID = video.VideoID + "_meta",
                                        IsMaster = false,
                                        IsTruncated = false,
                                        AssetNotes = null,
                                        RetentionPolicyId = retentionPolicyId,
                                        DurationInMs = video.Duration,
                                        FileSizeInBytes = new FileInfo(Path.Combine(_folderToWatch, fileName + ".xml")).Length,
                                        RecordingStarted = video.RecordingStarted,
                                        RecordingEnded = video.RecordingEnded,
                                        IsOverlayOnVideo = true,
                                        IsRestrictedView = false,
                                        AssetStatus = eMediaStatus.Queued,
                                        AssetState = eMediaState.Normal,
                                        ClientStatus = IRSAFileStatusEnum.Uploaded,
                                        StorageType = IRSA.Host.Common.Helper.GetCurrentStorageType(),
                                        TotalSegments = 1,
                                        CameraName = "Panasonic Arbitrator",
                                        CommulativeSize = 0,
                                        CommulativeDuration = 0,
                                        UploadDate = DateTime.UtcNow,
                                        StationSysSerial = (int)users[0].Station.SysSerial,
                                        OwnerIds = new List<long> { users[0].SysSerial },
                                        IRSAClientId = (video.IRSAClientID != null ? (int?)(video.IRSAClientID.SysSerial) : null)
                                    };
                                    assetList.Add(assetInfo);

                                    var assetInfoCopy = new JavaScriptSerializer().Serialize(assetInfo);

                                    AssetInfo assetAV = new JavaScriptSerializer().Deserialize<AssetInfo>(assetInfoCopy);
                                    assetAV.IsMaster = true;
                                    assetAV.AssetID = video.VideoID;
                                    assetAV.FileExtension = "av";
                                    assetAV.MediaType = IRSAMediaTypeEnum.Others;
                                    assetAV.FileSizeInBytes = new FileInfo(Path.Combine(_folderToWatch, fileName + ".av3")).Length;
                                    assetList.Add(assetAV);

                                    AssetInfo assetbookmark = null;
                                    string mediaFilePathBookmarkExist = Path.Combine(_folderToWatch, Path.GetFileNameWithoutExtension(file) + "b.xml");
                                    if (File.Exists(mediaFilePathBookmarkExist))
                                    {
                                        assetbookmark = new JavaScriptSerializer().Deserialize<AssetInfo>(assetInfoCopy);
                                        assetbookmark.IsMaster = false;
                                        assetbookmark.AssetID = video.VideoID + "_bookmark";
                                        assetbookmark.FileExtension = "xml";
                                        assetbookmark.MediaType = IRSAMediaTypeEnum.Others;
                                        assetbookmark.FileSizeInBytes = new FileInfo(Path.Combine(_folderToWatch, fileName + "b.xml")).Length;
                                        assetList.Add(assetbookmark);
                                    }

                                    AssetInfo assetThumb = null;
                                    string mediaFilePathThumbnailExist = Path.Combine(_folderToWatch, Path.GetFileNameWithoutExtension(file) + ".jpg");
                                    if (File.Exists(mediaFilePathThumbnailExist))
                                    {
                                        assetThumb = new JavaScriptSerializer().Deserialize<AssetInfo>(assetInfoCopy);
                                        assetThumb.IsMaster = false;
                                        assetThumb.AssetID = video.VideoID + "_thumb";
                                        assetThumb.FileExtension = "jpg";
                                        assetThumb.MediaType = IRSAMediaTypeEnum.Image;
                                        assetThumb.FileSizeInBytes = new FileInfo(Path.Combine(_folderToWatch, fileName + ".jpg")).Length;
                                        assetList.Add(assetThumb);
                                    }

                                    long evidencrGroupIdGenerated = 0;
                                    try
                                    {
                                        //IRSAStations.StationExistOrDefault(stationID)
                                        isSuccess = IRSAHostAssetUpload.SaveEvidenceGroup(users, ref assetList, ref evidencrGroupIdGenerated,
                                            incidents, 0, false, 0, users[0].Station.SysSerial, true);

                                        IRSALINQMedia.SaveEvidenceGroupMetaData(evidencrGroupIdGenerated, users[0].Station.SysSerial);
                                        video.SysSerial = assetInfo.VideoSysSerial;
                                        Log.Debug(string.Format("Insert metadata of file {0}.XML with Id {1}", file, video.SysSerial));

                                    }
                                    catch (Exception ex)
                                    {
                                        if (video.SysSerial != 0)
                                        {
                                            videoUploadQueue.VideoId = video.SysSerial;
                                            videoUploadQueue.Details = ex.Message.ToString();
                                            IRSALINQVideoUploadQueue.UpsertVideoUploadQueue(videoUploadQueue, false);
                                            log.Error(string.Format(ex.Message, file));
                                        }
                                        continue;
                                    }
                                    if (isSuccess)
                                    {
                                        try
                                        {
                                            var GPSS = (from s in xdoc.Descendants("GPIO").Elements()
                                                        select new IRSA.Host.LINQEntities.ModelsEF.VideoGp()
                                                        {
                                                            VideoID = Convert.ToInt64(video.SysSerial),
                                                            EventID = 0,
                                                            Position = System.Data.Entity.Spatial.DbGeography.FromText("POINT(0 0)"),
                                                            Altitude = 0,
                                                            Speed = 0,
                                                            LogTime = DateTime.Now,
                                                            UpdatedOn = DateTime.Now
                                                        }).ToList();

                                            IRSALINQVideoGps.Insert(GPSS);
                                            Log.Debug(string.Format("GPS Updated with evidence id {0}", video.SysSerial));

                                            string updatedFileName = mediaFilePath.Replace(Path.GetFileNameWithoutExtension(mediaFilePath), assetInfo.AssetID);
                                            System.IO.File.Move(mediaFilePath, updatedFileName);
                                            MoveAxonMediaItems(Path.GetFileNameWithoutExtension(updatedFileName), Path.GetFileNameWithoutExtension(updatedFileName), _folderToWatch, _ftpRootPath, ".xml");
                                        }
                                        catch (Exception ex)
                                        {
                                            videoUploadQueue.VideoId = video.SysSerial;
                                            videoUploadQueue.Details = "GPS not updated ";
                                            IRSALINQVideoUploadQueue.UpsertVideoUploadQueue(videoUploadQueue, false);
                                            log.Error(string.Format("GPS not updated ", file));

                                        }

                                        string mediaFilePathBookmark = Path.Combine(_folderToWatch, Path.GetFileNameWithoutExtension(file) + "b.xml");
                                        if (File.Exists(mediaFilePathBookmark) && !File.Exists(file + ".lock"))
                                        {
                                            try
                                            {
                                                xdoc = XDocument.Load(mediaFilePathBookmark);
                                                if (ValidateBookMarkData(xdoc))
                                                {                                                   
                                                    try
                                                    {
                                                        List<VideoBookmark> videoBookmark = new List<VideoBookmark>();
                                                        var bookmark = (from s in xdoc.Descendants("GENERAL").Elements()
                                                                        where s.Name.LocalName == "BOOKMARK"
                                                                        select new VideoBookmark()
                                                                        {
                                                                            BookmarkTime = DateTime.ParseExact(s.Element("TS").Value.ToString(), "MM-dd-yyyy HH:mm:ss", CultureInfo.InvariantCulture),
                                                                            VideoID = new Videos() { SysSerial = video.SysSerial },
                                                                            Description = s.Element("DESC").Value,
                                                                            VideoPosition = 0,
                                                                            IsUserBookmark = CheckUserBookmarked(file),
                                                                            Severity = 1,
                                                                        });

                                                        foreach (VideoBookmark vbm in bookmark)
                                                        {
                                                            IRSAVideoBookmarks.Insert(vbm);
                                                            Log.Debug(string.Format("Bookmarks Updated with evidence id {0}", video.SysSerial));
                                                        }
                                                    }
                                                    catch (Exception ex)
                                                    {
                                                        videoUploadQueue.VideoId = video.SysSerial;
                                                        videoUploadQueue.Details = "Bookmark not updated ";
                                                        IRSALINQVideoUploadQueue.UpsertVideoUploadQueue(videoUploadQueue, false);
                                                        log.Error(string.Format("Bookmark not updated ", file));
                                                        log.Error(string.Format("Bookmark update error ", ex.Message));
                                                    }

                                                    string updatedFileName = mediaFilePathBookmark.Replace(Path.GetFileNameWithoutExtension(mediaFilePath) + "b", assetbookmark.AssetID);
                                                    System.IO.File.Move(mediaFilePathBookmark, updatedFileName);
                                                    MoveAxonMediaItems(Path.GetFileNameWithoutExtension(updatedFileName), Path.GetFileNameWithoutExtension(updatedFileName), _folderToWatch, _ftpRootPath, ".xml");
                                                }
                                                else
                                                {
                                                    videoUploadQueue.VideoId = video.SysSerial;
                                                    videoUploadQueue.Details = "Bookmark not updated";
                                                    IRSALINQVideoUploadQueue.UpsertVideoUploadQueue(videoUploadQueue, false);
                                                    log.Error(string.Format("Exception while proccessing file {0} ", file + "b.xml"));
                                                }

                                            }
                                            catch (Exception ex)
                                            {
                                                videoUploadQueue.VideoId = video.SysSerial;
                                                videoUploadQueue.Details = "Bookmark not updated";
                                                IRSALINQVideoUploadQueue.UpsertVideoUploadQueue(videoUploadQueue, false);
                                                log.Error(string.Format("Exception while proccessing file {0} ", file + "b.xml"), ex);
                                            }
                                        }
                                        else
                                        {
                                            videoUploadQueue.VideoId = video.SysSerial;
                                            videoUploadQueue.Details = "Bookmark File not found";
                                            IRSALINQVideoUploadQueue.UpsertVideoUploadQueue(videoUploadQueue, false);
                                            log.Error(string.Format("Bookmark File not found ", file));
                                        }

                                        string mediaFilePathVideo = Path.Combine(_folderToWatch, Path.GetFileNameWithoutExtension(file) + ".av3");
                                        if (File.Exists(mediaFilePathVideo) && !File.Exists(file + ".lock"))
                                        {
                                            try
                                            {
                                                // UploadVideo(mediaFilePathVideo, IRSAUsers.GetUsers(new IRSAUser() { SysSerial = userID }).FirstOrDefault(), video);
                                                string updatedFileName = mediaFilePathVideo.Replace(Path.GetFileNameWithoutExtension(mediaFilePath), assetAV.AssetID);
                                                System.IO.File.Move(mediaFilePathVideo, updatedFileName);
                                                MoveAxonMediaItems(Path.GetFileNameWithoutExtension(updatedFileName), Path.GetFileNameWithoutExtension(updatedFileName), _folderToWatch, _ftpRootPath, ".av3");
                                            }
                                            catch (Exception ex)
                                            {
                                                videoUploadQueue.VideoId = video.SysSerial;
                                                videoUploadQueue.Details = "Failed to move video file to ftp root";
                                                IRSALINQVideoUploadQueue.UpsertVideoUploadQueue(videoUploadQueue, false);
                                                log.Error(string.Format("Failed to move video file to ftp root ", mediaFilePathVideo));
                                            }
                                        }
                                        else
                                        {
                                            videoUploadQueue.VideoId = video.SysSerial;
                                            videoUploadQueue.Details = "Video File not found";
                                            IRSALINQVideoUploadQueue.UpsertVideoUploadQueue(videoUploadQueue, false);
                                            log.Error(string.Format("Video File not found ", mediaFilePathVideo));
                                        }


                                        string mediaFilePathThumbnail = Path.Combine(_folderToWatch, Path.GetFileNameWithoutExtension(file) + ".jpg");
                                        if (File.Exists(mediaFilePathThumbnail) && !File.Exists(file + ".lock"))
                                        {
                                            try
                                            {
                                                string updatedFileName = mediaFilePathThumbnail.Replace(Path.GetFileNameWithoutExtension(mediaFilePath), assetThumb.AssetID);
                                                System.IO.File.Move(mediaFilePathThumbnail, updatedFileName);
                                                MoveAxonMediaItems(Path.GetFileNameWithoutExtension(updatedFileName), Path.GetFileNameWithoutExtension(updatedFileName), _folderToWatch, _ftpRootPath, ".jpg");
                                            }
                                            catch (Exception ex)
                                            {
                                                videoUploadQueue.VideoId = video.SysSerial;
                                                videoUploadQueue.Details = "Failed to move Content Thumbnail file to ftp root";
                                                IRSALINQVideoUploadQueue.UpsertVideoUploadQueue(videoUploadQueue, false);
                                                log.Error(string.Format("Failed to move Content Thumbnail file to ftp root ", mediaFilePathThumbnail));
                                            }
                                        }
                                        else
                                        {
                                            videoUploadQueue.VideoId = video.SysSerial;
                                            videoUploadQueue.Details = "Content Thumbnail File not found";
                                            IRSALINQVideoUploadQueue.UpsertVideoUploadQueue(videoUploadQueue, false);
                                            log.Error(string.Format("Content Thumbnail File not found ", mediaFilePathThumbnail));
                                        }
                                    }

                                }
                                else
                                {
                                    log.Error(string.Format("{0} Due to conflict in filename and <F-Name> Tag inside the metadata file, moving the files to Error Folder ", file));
                                    MoveFilesToErrorFolder(file);
                                }
                            }
                            else
                            {
                                MoveFilesToErrorFolder(file);
                            }
                        }
                        else
                        {
                            log.Error(string.Format("Metadata File {0} not found ", file));
                            MoveFilesToErrorFolder(file);
                        }
                    }
                }
                catch (Exception ex)
                {
                    log.Error(string.Format("Exception while proccessing file {0} ", file), ex);
                    MoveFilesToErrorFolder(file);
                }
            }
            log.Debug(exitText);
        }
        private void MoveFilesToErrorFolder(string filename)
        {
            try
            {
                string fileNameWithoutExtension = Path.GetFileNameWithoutExtension(filename);
                if (File.Exists(Path.Combine(_folderToWatch, fileNameWithoutExtension + ".av3")))
                {
                    MoveAxonMediaItems(fileNameWithoutExtension, fileNameWithoutExtension, _folderToWatch, _errorFolder, ".av3");
                    log.Error(string.Format("{0} Moved to error Folder due to exception while processing the file", fileNameWithoutExtension+".av3"));
                }
                if (File.Exists(Path.Combine(_folderToWatch, fileNameWithoutExtension + ".xml")))
                {
                    MoveAxonMediaItems(fileNameWithoutExtension, fileNameWithoutExtension, _folderToWatch, _errorFolder, ".xml");
                    log.Error(string.Format("{0} Moved to error Folder due to exception while processing the file", fileNameWithoutExtension + ".xml"));
                }
                if (File.Exists(Path.Combine(_folderToWatch, fileNameWithoutExtension + "b.xml")))
                {
                    var bookmarkFileNameWithoutExtension = Path.GetFileNameWithoutExtension(Path.Combine(_folderToWatch, fileNameWithoutExtension + "b.xml"));
                    MoveAxonMediaItems(bookmarkFileNameWithoutExtension, bookmarkFileNameWithoutExtension, _folderToWatch, _errorFolder, ".xml");
                    log.Error(string.Format("{0} Moved to error Folder due to exception while processing the file", fileNameWithoutExtension + "b.xml"));
                }
                if (File.Exists(Path.Combine(_folderToWatch, fileNameWithoutExtension + ".jpg")))
                {
                    MoveAxonMediaItems(fileNameWithoutExtension, fileNameWithoutExtension, _folderToWatch, _errorFolder, ".jpg");
                    log.Error(string.Format("{0} Moved to error Folder due to exception while processing the file", fileNameWithoutExtension + ".jpg"));
                }
                
            }
            catch(Exception ex)
            {
                log.Error(string.Format("Error While moving files(with exception) to error folder ", filename));
            }

        }
        private List<int> GetIncidentFromMapping(string file,int stationID)
        {
            List<int> incidents = new List<int>();
            try
            {                
                var incidentNames = GetIncidentsFromBookmarkFile(file).Distinct().ToList();

                if (incidentNames.Count == 0)
                {
                    int incId = IRSALINQIncidents.GetIncidentByName("Uncategorized");
                    if (incId > 0)
                        incidents.Add(incId);
                }
                var fileName = Path.Combine(_folderToWatch, "ClassificationMapping.json");              
                var fileContent =  System.IO.File.ReadAllText(fileName, Encoding.GetEncoding("iso-8859-1"));
                if (!string.IsNullOrEmpty(fileContent) )
                {
                    JObject fileJson = null;
                    try
                    {
                        fileJson = JObject.Parse(fileContent);
                    }catch(Exception ex)
                    {
                        log.Error(String.Format("Json Error while parsing ClassificationMapping.json", file));
                    }
                    if(fileJson != null)
                    {
                        foreach (KeyValuePair<string, JToken> property in fileJson)
                        {
                            foreach (var name in incidentNames)
                            {
                                if (property.Key == name)
                                {
                                    try
                                    {
                                        var incident = IRSALINQIncidents.GetIncidentByNameAndStation(Convert.ToString(property.Value), stationID);
                                        if (incident != 0 && !incidents.Contains(incident))
                                            incidents.Add(incident);
                                    }
                                    catch (Exception ex)
                                    {
                                        log.Error(String.Format("Incidnet ID not found in {0}", file));
                                    }
                                }
                            }

                        }
                    }
                }
                else
                {
                    log.Error(String.Format("No classification/category mapping file found"));
                }
            }catch(Exception ex)
            {
                log.Error(String.Format("Error while reading incident from {0}", file));
            }
            
            return incidents;
        }
        private bool CheckUserBookmarked(string file)
        {
            string mediaFilePathBookmark = Path.Combine(_folderToWatch, Path.GetFileNameWithoutExtension(file) + "b.xml");
            bool result = false;
            if (File.Exists(mediaFilePathBookmark) && !File.Exists(file + ".lock"))
            {
                XDocument xdoc;
                try
                {
                    xdoc = XDocument.Load(mediaFilePathBookmark);
                    List<VideoBookmark> videoBookmark = new List<VideoBookmark>();
                    var userCreated = (from s in xdoc.Descendants("GENERAL").Elements()
                                         where s.Name.LocalName == "USERINFO"
                                        select s.Element("INFO_ID").Value.ToString()).Count();
                    if (userCreated > 0)
                        result = true;
                    else
                        result = false;
                }
                catch(Exception ex)
                {
                    log.Error(String.Format("Error while fetching USERINFO from Bookmark file {0}", file + "b.xml"));
                }
            }
            return result;            
        }
        private List<string> GetIncidentsFromBookmarkFile(string file)
        {
            string mediaFilePathBookmark = Path.Combine(_folderToWatch, Path.GetFileNameWithoutExtension(file) + "b.xml");
            List<string> incidentNames = new List<string>();
            if (File.Exists(mediaFilePathBookmark) && !File.Exists(file + ".lock"))
            {
                XDocument xdoc;
                try
                {
                    xdoc = XDocument.Load(mediaFilePathBookmark);
                    List<VideoBookmark> videoBookmark = new List<VideoBookmark>();
                    incidentNames = (from s in xdoc.Descendants("GENERAL").Elements()
                                    where s.Name.LocalName == "BOOKMARK"
                                    select s.Element("EVENTTYPE").Value.ToString()).ToList();

                }
                catch (Exception ex)
                {
                    log.Error(string.Format("Exception while getting incident id's from bookmark file file {0} ", mediaFilePathBookmark), ex);
                }
            }
            else
            {
                log.Error(string.Format("Bookmark File {0} not found for Incident ID's ", mediaFilePathBookmark));
            }
            return incidentNames;
        }
        private bool ValidateBookMarkData(XDocument file)
        {
            bool isValid = true;
            string error = "";
            foreach (XElement element in file.Descendants("GENERAL"))
            {
                foreach(XElement bookmark in element.Descendants("BOOKMARK"))
                {
                    if(bookmark.Element("TS") != null)
                    {
                        if (bookmark.Element("TS").Value == "")
                        {
                            error += "TS/Bookmarktime value doesnot exist in bookmark file \n";
                            isValid = false;
                        }
                        else
                        {
                            try
                            {
                                DateTime.ParseExact(bookmark.Element("TS").Value.ToString(), "MM-dd-yyyy HH:mm:ss", CultureInfo.InvariantCulture);
                            }
                            catch (Exception ex)
                            {
                                error += "TS/Bookmarktime date format is not valid. Datetime format should be MM-dd-yyyy HH:mm:ss \n";
                                isValid = false;
                            }
                        }
                    }
                    else
                    {
                        error += "TS/Bookmarktime doesnot exist in bookmark file \n";
                        isValid = false;
                    }
                }
                
            }
            if (!isValid)
                log.Error(error);
            return isValid;
        }

        private bool ValidateFileData(XDocument file,string filename)
        {
            bool isValid = true;
            var rootElement = file.Descendants("F-Info");
            if (rootElement.Count() >0)
            {
                foreach (XElement element in file.Descendants("F-Info"))
                {
                    if (element.Element("F-Name") != null)
                    {
                        if (element.Element("F-Name").Value == "")
                        {
                            log.Error("F-Name value doesnot exist in file ");
                            isValid = false;
                        }
                    }
                    else
                    {
                        log.Error("F-Name doesnot exist in file \n");
                        isValid = false;
                    }
                    if (element.Element("RecST") != null)
                    {
                        if (element.Element("RecST").Value == "")
                        {
                            log.Error("RecST value doesnot exist in file ");
                            isValid = false;
                        }
                        else
                        {
                            try
                            {
                                DateTime.ParseExact(element.Element("RecST").Value.ToString(), "yyyy-MM-dd,HH:mm:ss", CultureInfo.InvariantCulture);
                            }
                            catch (Exception ex)
                            {
                                log.Error("RecST date format is not valid. Datetime format should be yyyy-MM-dd,HH:mm:ss ");
                                isValid = false;
                            }
                        }
                    }
                    else
                    {
                        log.Error("RecST doesnot exist in file ");
                        isValid = false;
                    }
                    if (element.Element("RecET") != null)
                    {
                        if (element.Element("RecET").Value == "")
                        {
                            log.Error("RecET value doesnot exist in file ");
                            isValid = false;
                        }
                        else
                        {
                            try
                            {
                                DateTime.ParseExact(element.Element("RecET").Value.ToString(), "yyyy-MM-dd,HH:mm:ss", CultureInfo.InvariantCulture);
                            }
                            catch (Exception ex)
                            {
                                log.Error("RecET date format is not valid. Datetime format should be yyyy-MM-dd,HH:mm:ss ");
                                isValid = false;
                            }
                        }
                    }
                    else
                    {
                        log.Error("RecET doesnot exist in file ");
                        isValid = false;
                    }
                    if (element.Element("RecDR") != null)
                    {
                        if (element.Element("RecDR").Value == "")
                        {
                            log.Error("Duration doesnot exist in file ");
                            isValid = false;
                        }
                    }
                    else
                    {
                        log.Error("RecDR doesnot exist in file ");
                        isValid = false;
                    }
                    if (element.Element("V-ID") != null)
                    {
                        if (element.Element("V-ID").Value == "")
                        {
                            log.Error("StationID doesnot exist in file ");
                            isValid = false;
                        }
                    }
                    else
                    {
                        log.Error("V-ID doesnot exist in file ");
                        isValid = false;
                    }
                }
            }
            else
            {
                log.Error(string.Format("F-Info doesnot exist in file or Corrupt File {0} ",filename));
                isValid = false;
            }
                
            return isValid;

        }

        private static string GetFullFileName(string filename, string fileType)
        {
            log.Debug(enterText);
            string newFilename = String.Empty;
            if (fileType == ".pdf")
                newFilename = string.Format("AT_{0}", filename);
            else if (fileType == ".json")
                newFilename = string.Format("MD_{0}", filename);
            else if (fileType == ".mp4")
                newFilename = string.Format("{0}", filename);
            else
                newFilename = string.Format("{0}", filename);
            log.Debug(exitText);
            return newFilename;
        }

        private static bool MoveAxonMediaItems(string mediaId, string fileName, string sourcePath, string destinationPath, string mediaExtension)
        {
            log.Debug(enterText);
            bool success = false;
            log.Info(String.Format("Moving files related to media Id : {0} from {1} to {2}", fileName, sourcePath, destinationPath));

            string searchExtenssion = "";
            if (string.IsNullOrEmpty(mediaExtension))
                throw new Exception("Passed File extension is NULL or Empty");
            else
                searchExtenssion = string.Format("{0}", mediaExtension);

            string[] files = Directory.GetFiles(sourcePath, fileName + searchExtenssion, SearchOption.TopDirectoryOnly); // replace with extension based search

            var newFilename = GetFullFileName(mediaId, mediaExtension);
            if (files != null && files.Length > 0)
            {
                BaseMediaProcessingJob.CreateLockFile(String.Format("{0}\\{1}", destinationPath, newFilename)); // logic to restest
            }

            try
            {
                var formats = IRSA.Host.Common.Helper.SupportedUploadedFileFormats;

                foreach (string file in files)
                {
                    if (!File.Exists(file))
                        continue;
                    FileInfo fileInfo = new FileInfo(file);
                    log.Info(String.Format("Moving file {0} to {1} ", fileInfo.Name, destinationPath));

                    string extension = fileInfo.Extension.Trim().ToLower();

                    string dest = Path.Combine(destinationPath, newFilename + extension); // c:\\folder\\newfilename.type
                    if (File.Exists(dest))
                        File.Delete(dest);

                    string ext = Path.GetExtension(dest);
                    File.Move(fileInfo.FullName, dest);

                    fileInfo.Delete();
                }
            }
            catch (Exception ex)
            {
                log.Error(string.Format("Failed to move file {0} from {1} to {2}", mediaId, sourcePath, destinationPath), ex);
            }
            finally
            {
                BaseMediaProcessingJob.DeleteLockFile(String.Format("{0}\\{1}", destinationPath, newFilename));

            }

            log.Debug(exitText);
            return success;
        }
        private enum AssetStatus
        {
            active,
            deleted
        }

        protected override void SetFileListToJobMap(IJobExecutionContext context)
        {
            log.Debug(enterText);
            throw new NotImplementedException();
        }

        public static List<String> GetFileListByLastModifiedAsc(string folderToWatch)
        {
            log.Debug(enterText);
            List<String> listFiles = new List<String>();
            var lockedFiles = Directory.GetFiles(folderToWatch, "*.lock", SearchOption.TopDirectoryOnly).ToArray();
            DirectoryInfo dirInfo = new DirectoryInfo(folderToWatch);
            FileSystemInfo[] fileSysInfo = dirInfo.GetFiles("*.av3", SearchOption.TopDirectoryOnly).OrderBy(x => x.LastWriteTime).Where(y=> !lockedFiles.Contains(y.FullName+".lock")).ToArray<FileSystemInfo>();
          
            string[] fileNameList = fileSysInfo.Select(x => x.FullName).ToArray<string>();

            listFiles.AddRange(fileNameList);
            log.Debug(exitText);
            return listFiles;
        }
    }
}
