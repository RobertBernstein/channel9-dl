using McMaster.Extensions.CommandLineUtils;
using Microsoft.SyndicationFeed;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading.Tasks;

namespace channel9_dl
{
    public class Program
    {
        // sample expected arguments
        // download --rss https://s.ch9.ms/Events/Build/2017/RSS -d C:\MyVideos\Build --mp4 --mp3 --hd -n 5

        public static int Main(string[] args)
        {
            var app = new CommandLineApplication();
            app.Name = "channel9-dl";
            app.Description = "Channel 9 video downloading app powered by .NET core";
            app.HelpOption("-?|--help");            

            var rssOpt = app.Option("-r|--rss", "Required URL to the RSS Feed", CommandOptionType.SingleValue);
            rssOpt.IsRequired(false, "You must specify the RSS feed!");

            var localDirOpt = app.Option("-d|--directory", "Required full path to the download directory", CommandOptionType.SingleValue);
            localDirOpt.IsRequired(false, "You must specify the local directory!");

            var mp4Opt = app.Option("--mp4", "If set, downloads session MP4's", CommandOptionType.NoValue);
            var mp3Opt = app.Option("--mp3", "If set, downloads sessions MP3's", CommandOptionType.NoValue);

            var qualityOpt = app.Option("-h|--hd", "If set, download highest quality available, othersise, download smallest size available.", CommandOptionType.NoValue);

            var batchSizeOpt = app.Option("-n|--batch", "If set, specifies the number of groups to download concurrently", CommandOptionType.SingleValue);

            app.OnExecute( async () => 
            {
                var rssUri = new Uri(rssOpt.Value());
                var localDir = new DirectoryInfo(localDirOpt.Value());

                Console.WriteLine($"            RSS = {rssUri.AbsoluteUri}");
                Console.WriteLine($"Local Directory = {localDir.FullName}");
                Console.WriteLine();
                                
                var startTime = DateTime.Now;

                try
                {
                    var ch9Parser = new Channel9RssParser();
                    Channel9RssResult rssResult = await ch9Parser.Parse(rssUri);

                    if (batchSizeOpt.HasValue())
                    {
                        int batchSize = int.Parse(batchSizeOpt.Value());
                        await MainAsync(rssResult, localDir, mp4Opt.HasValue(), mp3Opt.HasValue(), qualityOpt.HasValue(), batchSize);
                    }
                    else
                    {
                        await MainAsync(rssResult, localDir, mp4Opt.HasValue(), mp3Opt.HasValue(), qualityOpt.HasValue());
                    }
                    
                }
                catch (AggregateException agex)
                {
                    Console.WriteLine("########################## Exception(s) ##################################");
                    foreach (var ex in agex.InnerExceptions)
                    {
                        Console.WriteLine("   " + ex.Message);
                    }
                    Console.WriteLine("########################## Exception(s) ##################################");
                }

                var endTime = DateTime.Now;

                Console.WriteLine();
                Console.WriteLine();
                Console.WriteLine("Started : " + startTime);
                Console.WriteLine("Finished: " + endTime);
                Console.WriteLine();
                Console.WriteLine("Elapsed : " + ((endTime - startTime).TotalMinutes).ToString("#0.00") + " minutes");
            });

            int result;

            try
            {
                result = app.Execute(args);
            }
            catch(Exception ex)
            {
                Console.WriteLine(ex);
                Console.WriteLine();
                result = -1;
            }

            return result;
        }


        private static async Task MainAsync(Channel9RssResult rssResult, DirectoryInfo directory, bool mp4, bool mp3, bool useHighestQuality)
        {
            Console.WriteLine($"{rssResult.SyndicationItems.Count()} items found at {rssResult.SourceUrl.ToString()}");
            Console.WriteLine();

            if (rssResult.SyndicationItems.Count() < 1)
            {
                Console.WriteLine("Exiting...");
            }
            
            foreach (var session in rssResult.Sessions)
            {
                if (mp4)
                {
                    var localVideoFile = await session.DownloadMp4SessionAsync(directory, useHighestQuality);
                }

                if (mp3)
                {
                    var localAudioFile = await session.DownloadMp3SessionAsync(directory, useHighestQuality);
                }

                Console.WriteLine();
            }
        }


        private static async Task MainAsync(Channel9RssResult rssResult, DirectoryInfo directory, bool mp4, bool mp3, bool useHighestQuality, int batchSize)
        {
            var groupedSessionList =
                rssResult.Sessions.Select((value, index) => new { Index = index, Value = value })
                  .GroupBy(x => x.Index / batchSize)
                  .Select(g => g.Select(x => x.Value).ToList())
                  .ToList();

            int total = rssResult.Sessions.Count;
            int groupCounter = 1;
            int fileCounter = 1;

            foreach (var session in groupedSessionList)
            {
                Console.WriteLine("Getting Group {0} of {1} ({2}-{3} of {4})", groupCounter++, groupedSessionList.Count, fileCounter, ((fileCounter - 1) + batchSize), total);

                var taskList = new List<Task>();

                foreach (var item in session)
                {
                    if (mp4)
                    {
                        taskList.Add(item.DownloadMp4SessionAsync(directory, useHighestQuality));
                    }

                    if (mp3)
                    {
                        taskList.Add(item.DownloadMp3SessionAsync(directory, useHighestQuality));
                    }
                }

                try
                {
                    await Task.WhenAll(taskList.ToArray());
                    fileCounter += batchSize;
                    Console.WriteLine();
                }
                catch (AggregateException ae)
                {
                    Console.WriteLine("##################################################");
                    Console.WriteLine(ae.Message);
                    foreach (var ex in ae.InnerExceptions)
                    {
                        Console.WriteLine(ex.Message);
                    }
                    Console.WriteLine("##################################################");
                }
            }


        }
    }
}
