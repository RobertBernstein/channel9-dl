using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Net;
using System.Text;
using System.Threading.Tasks;

namespace channel9_dl
{
    public class SessionInfo
    {
        public SessionInfo()
        {
            this.VideoRecordings = new List<VideoRecording>();
        }


        public string SessionID { get; set; }
        public string Title { get; set; }
        public Uri SessionSite { get; set; }
        public List<VideoRecording> VideoRecordings { get; set; }
        public DateTime PublishDate { get; set; }

        public bool HasLocalFile { get; set; }
        
        
       
        public string Presenter { get; set; }


        public override string ToString()
        {
            return $"{Title} [{this.VideoRecordings.FirstOrDefault()?.Name}]";
        }



        public async Task<FileInfo> DownloadMp4SessionAsync(DirectoryInfo directory, bool highestQuality, bool overwrite = false)
        {
            IOrderedEnumerable<VideoRecording> videos;

            if (highestQuality)
            {
                videos = this.VideoRecordings.Where(v => v.MediaType == "video/mp4").OrderByDescending(v => v.Length);
            }
            else
            {
                videos = this.VideoRecordings.Where(v => v.MediaType == "video/mp4").OrderBy(v => v.Length);
            }

            var videoToDownload = videos.FirstOrDefault();

            if (videoToDownload != null)
            {
                Console.WriteLine($"Downloading... [{videoToDownload.GetLocalFileName()}]");

                var video = await videoToDownload.DownloadTo(directory, overwrite);
                if (video.Exists)
                {
                    this.HasLocalFile = true;
                }
                else
                {
                    this.HasLocalFile = false;
                }

                return video;
            }
            else
            {
                Console.WriteLine($" *** No MP4 Video URLs ***");
                this.HasLocalFile = false;
                return null;
            }
        }

        public async Task<FileInfo> DownloadMp3SessionAsync(DirectoryInfo directory, bool highestQuality, bool overwrite = false)
        {
            IOrderedEnumerable<VideoRecording> audioTracks;

            if (highestQuality)
            {
                audioTracks = this.VideoRecordings.Where(v => v.MediaType == "audio/mp3").OrderByDescending(v => v.Length);
            }
            else
            {
                audioTracks = this.VideoRecordings.Where(v => v.MediaType == "audio/mp4").OrderBy(v => v.Length);
            }

            var audioToDownload = audioTracks.FirstOrDefault();

            if (audioToDownload != null)
            {
                Console.WriteLine($"Downloading... [{audioToDownload.GetLocalFileName()}]");
                return await audioToDownload.DownloadTo(directory, overwrite);
            }
            else
            {
                Console.WriteLine($" *** No MP3 Audio URLs ***");
                return null;
            }
        }
    }
}