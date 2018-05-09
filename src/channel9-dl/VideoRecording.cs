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
    public class VideoRecording
    {
        private static char[] invalidChars = { '\\', '/', ':', '*', '?', '"', '<', '>', '|', '\"', '"', '.' };



        public string Name
        {
            get
            {
                return this.Url.ToString().Substring(this.Url.ToString().LastIndexOf("/", StringComparison.CurrentCultureIgnoreCase) + 1);
            }
        }

        public SessionInfo SessionInfo { get; set; }

        public string MediaType { get; set; }

        public Uri Url { get; set; }

        public long Length { get; set; }

        public Stopwatch DownloadTimer { get; set; }

        public FileInfo LocalFile { get; set; }

        public Exception DownloadException { get; set; }



        public override string ToString()
        {
            return $"{Name} ({MediaType}) [{Length}]";
        }



        public string GetRemoteFileName()
        {
            var path = this.Url.ToString();

            int pos1 = path.LastIndexOf('/') + 1;

            return path.Substring(pos1);
        }

        public string GetRemoteFileExtension()
        {
            var path = this.GetRemoteFileName();

            int pos1 = path.LastIndexOf('/') + 1;
            int pos2 = path.LastIndexOf('.');

            return path.Substring(pos2 - pos1);
        }

        public string GetLocalFileName()
        {
            // Build the filename from the title of the session and the list of presenters for the session.
            var scrubbedString =
                string.IsNullOrWhiteSpace(this.SessionInfo.Presenter) ? this.SessionInfo.Title : $"{this.SessionInfo.Title} ({this.SessionInfo.Presenter})";

            VideoRecording.invalidChars.ToList().ForEach(x =>
            {
                scrubbedString = scrubbedString.Replace(x, '-');
            });

            int maxLength = 200;
            if (scrubbedString.Length > maxLength)
            {
                scrubbedString = scrubbedString.Substring(0, maxLength) + "... ";
            }

            return string.Format("{0}{1}", scrubbedString, this.GetRemoteFileExtension());
        }



        public async Task<FileInfo> DownloadTo(DirectoryInfo directory, bool overwrite = false)
        {
            if (!directory.Exists)
            {
                directory.Create();
                directory.Refresh();
            }

            string destinationFileName = Path.Combine(directory.FullName, this.GetLocalFileName());

            if (File.Exists(destinationFileName))
            {
                if (overwrite)
                {
                    File.Delete(destinationFileName);
                }
                else
                {
                    this.LocalFile = new FileInfo(destinationFileName);
                    this.DisplayComplete();
                    return this.LocalFile;
                }
            }

            try
            {
                using (WebClient client = new WebClient())
                {
                    this.DownloadTimer = Stopwatch.StartNew();
                    await client.DownloadFileTaskAsync(this.Url, destinationFileName);
                    this.DownloadTimer.Stop();

                    this.LocalFile = new FileInfo(destinationFileName);
                    this.DisplayComplete();
                }

                return this.LocalFile;
            }
            catch (Exception ex)
            {
                if (this.DownloadTimer != null)
                {
                    this.DownloadTimer.Stop();
                }

                this.DownloadException = ex;

                return null;
            }
        }

        private void DisplayComplete()
        {
            if (this.DownloadTimer != null)
            {
                Console.WriteLine("   {0} => {1}", this.LocalFile.Name, this.DownloadTimer.Elapsed);
            }
            else
            {
                Console.WriteLine("   {0} Already in Collection", this.LocalFile.Name);
            }
        }
    }
}
