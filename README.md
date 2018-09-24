# channel9-dl

.NET core powered tool to help download MSDN Channel 9 videos for offline viewing.

PRE-CONDITION: .NET Core SDK or Visual Studio 2017 installed.

Use `git clone --recurse-submodules <repository_url>` to clone this repository.

Build with Visual Studio 2017
  - Open \src\channel9-dl.sln
  - Build (i.e. CTRL+SHIFT+B)

Build with .NET Core SDK 
  - cd to \src\ directory.
  - Use `dotnet restore channel9-dl.sln` to restore all NuGetPackages.
  - Use `dotnet build channel9-dl.sln` to build the solution.


USAGE:

NOTE: To download channel 9 session content for offline use:
  - Navigate to Channel 9's event/show site. For example, https://channel9.msdn.com/Events/Build/2018
  -  Click the "RSS" icon and navigate to the feed.  For example, https://s.ch9.ms/Events/Build/2018/RSS.
  - Cut/paste this feed and use as the `--rss` input parameter below. 

cd to ..\src\channel9-dl\bin\Debug\netcoreapp2.1\

Run :

`dotnet channel9-dl.dll --rss https://s.ch9.ms/Events/Build/2018/RSS -d C:\MyVideos\Build2018 --mp4 --mp3 --hd -n 5`

or 

`dotnet channel9-dl.dll --rss https://s.ch9.ms/Events/Ignite/2018/RSS -d C:\MyVideos\Ignite2018 --mp4 --mp3 --hd -n 5`


where
  --rss is required - specify the RSS feed from the channel 9 event containing the recordings
  -d is required - specify the local directory to where you wish to download the recordings
  --mp4 is optional - specify to include mp4 videos
  --mp3 is optional - specify to include mp3 audio tracks
  (BTW - no validation yet but if you do not specify at least one, nothing will be downloaded)
  --hd is optional - specify to indicate you wish to download the highest quality available.  If not specified, the smallest filesize will be downloaded.
  -n is optional and determines the number of concurrent downloads
