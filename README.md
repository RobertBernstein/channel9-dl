# channel9-dl
.NET core powered tool to help download MSDN Channel 9 videos for offline viewing
Better documentation to come but the intent of this tool is to facilitate downloading of channel 9 training videos for offline use.

Sample expected arguments
dotnet channel9-dl --rss https://s.ch9.ms/Events/Build/2017/RSS -d C:\MyVideos\Build --mp4 --mp3 --hd -n 5

where 
  --rss is required - specify the RSS feed from the channel 9 event containing the recordings
  -d is required - specify the local directory to where you wish to download the recordings
  --mp4 is optional - specify to include mp4 videos
  --mp3 is optional - specify to include mp3 audio tracks
  (BTW - no validation yet but if you do not specify at least one, nothing will be downloaded)
  --hd is optional - specify to indicate you wish to download the highest quality available.  If not specified, the smallest filesize will be downloaded.
  -n is optional and determines the number of concurrent downloads
