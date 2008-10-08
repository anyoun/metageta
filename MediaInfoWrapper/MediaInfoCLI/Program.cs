using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using MediaInfoLib;

namespace MediaInfoCLI
{
	class Program
	{
		static void Main(string[] args)
		{
			MediaInfoWrapper mi = new MediaInfoWrapper();

			if (args.Length == 1)
			{
				TestFile(mi, args[0]);
			}
			else
			{
				foreach (string file in System.IO.Directory.GetFiles("."))
				{
					TestFile(mi, file);
				}
			}
		}

		private static void TestFile(MediaInfoWrapper mediaInfo, string path)
		{
			FileMetadata f = mediaInfo.ReadFile(path);

			foreach (VideoStreamInfo i in f.VideoStreams)
				Console.WriteLine(i.ToString());

			foreach (AudioStreamInfo i in f.AudioStreams)
				Console.WriteLine(i.ToString());

			foreach (TextStreamInfo i in f.TextStreams)
				Console.WriteLine(i.ToString());

			foreach (ChaptersInfo i in f.Chapters)
				Console.WriteLine(i.ToString());

			foreach (ImageInfo i in f.Images)
				Console.WriteLine(i.ToString());
		}
	}
}
