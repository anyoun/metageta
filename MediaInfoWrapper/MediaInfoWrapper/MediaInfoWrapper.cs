using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;

namespace MediaInfoLib
{
	public class MediaInfoWrapper
	{
		private readonly MediaInfo m_MediaInfo = new MediaInfo();

		public FileMetadata ReadFile(string path)
		{
			FileMetadata file;
			if(TryReadFile(path, out file))
				return file;
			else
				throw new Exception(string.Format("Couldn't open file \"{0}\".", path));
		}

		public bool TryReadFile(string path, out FileMetadata file)
		{
			if (m_MediaInfo.Open(path) == 0)
			{
				file = null;
				return false;
			}
			else
			{
				try
				{
					file = new FileMetadata();
					file.VideoStreams = ReadVideoStreams();
					file.AudioStreams = ReadAudioStreams();
					file.TextStreams = ReadTextStreams();
					file.Chapters = ReadChapters();
					file.Images = ReadImages();

					return true;
				}
				catch(Exception inner)
				{
					throw new Exception(string.Format("Exception reading file: \"{0}\"", path), inner);
				}
				finally
				{
					m_MediaInfo.Close();
				}
			}
		}

		private IEnumerable<VideoStreamInfo> ReadVideoStreams()
		{
			StreamKind kind = StreamKind.Video;
			int numStreams = m_MediaInfo.Count_Get(kind);
			VideoStreamInfo[] streams = new VideoStreamInfo[numStreams];
			for (int i = 0; i < numStreams; i++)
			{
				streams[i] = new VideoStreamInfo();

				streams[i].PlayTime = TimeSpan.FromMilliseconds(ParseInteger(m_MediaInfo.Get(kind, i, ParameterConstants.PlayTime)));

				streams[i].BitRateBps = ParseInteger(m_MediaInfo.Get(kind, i, ParameterConstants.BitRate));

				streams[i].HeightPx = ParseInteger(m_MediaInfo.Get(kind, i, ParameterConstants.Height));
				streams[i].WidthPx = ParseInteger(m_MediaInfo.Get(kind, i, ParameterConstants.Width));

				streams[i].DisplayAspectRatio = ParseFloat(m_MediaInfo.Get(kind, i, ParameterConstants.DisplayAspectRatio));
				streams[i].PixelAspectRatio = ParseFloat(m_MediaInfo.Get(kind, i, ParameterConstants.PixelAspectRatio));

				streams[i].FrameCount = ParseInteger(m_MediaInfo.Get(kind, i, ParameterConstants.FrameCount));
				streams[i].FrameRate = ParseFloat(m_MediaInfo.Get(kind, i, ParameterConstants.FrameRate));

				streams[i].Codec = m_MediaInfo.Get(kind, i, ParameterConstants.Codec);
				streams[i].CodecProfile = m_MediaInfo.Get(kind, i, ParameterConstants.Codec_Profile);
				streams[i].CodecFamily = m_MediaInfo.Get(kind, i, ParameterConstants.CodecFamily);
				streams[i].CodecInfo = m_MediaInfo.Get(kind, i, ParameterConstants.CodecInfo);
				streams[i].CodecString = m_MediaInfo.Get(kind, i, ParameterConstants.CodecString);

				streams[i].LanguageFull = m_MediaInfo.Get(kind, i, ParameterConstants.LanguageFull);
				streams[i].StreamSize = m_MediaInfo.Get(kind, i, ParameterConstants.StreamSize);
				streams[i].Interlacement = m_MediaInfo.Get(kind, i, ParameterConstants.Interlacement);
			}
			return streams;
		}
		private IEnumerable<AudioStreamInfo> ReadAudioStreams()
		{
			StreamKind kind = StreamKind.Audio;
			int numStreams = m_MediaInfo.Count_Get(kind);
			AudioStreamInfo[] streams = new AudioStreamInfo[numStreams];
			for (int i = 0; i < numStreams; i++)
			{
				streams[i] = new AudioStreamInfo();

				streams[i].PlayTime = TimeSpan.FromMilliseconds(ParseInteger(m_MediaInfo.Get(kind, i, ParameterConstants.PlayTime)));

				streams[i].BitRateBps = ParseInteger(m_MediaInfo.Get(kind, i, ParameterConstants.BitRate));

				streams[i].Codec = m_MediaInfo.Get(kind, i, ParameterConstants.Codec);
				streams[i].CodecProfile = m_MediaInfo.Get(kind, i, ParameterConstants.Codec_Profile);
				streams[i].CodecFamily = m_MediaInfo.Get(kind, i, ParameterConstants.CodecFamily);
				streams[i].CodecInfo = m_MediaInfo.Get(kind, i, ParameterConstants.CodecInfo);
				streams[i].CodecString = m_MediaInfo.Get(kind, i, ParameterConstants.CodecString);

				streams[i].ChannelCount = ParseInteger(m_MediaInfo.Get(kind, i, ParameterConstants.ChannelCount));
				streams[i].ChannelPosition = m_MediaInfo.Get(kind, i, ParameterConstants.ChannelPositions);

				streams[i].ReplayGain_Gain = m_MediaInfo.Get(kind, i, ParameterConstants.ReplayGain_Gain);
				streams[i].ReplayGain_Peak = m_MediaInfo.Get(kind, i, ParameterConstants.ReplayGain_Peak);

				streams[i].SamplingHz = ParseInteger(m_MediaInfo.Get(kind, i, ParameterConstants.SamplingRate));
				streams[i].Resolution = ParseInteger(m_MediaInfo.Get(kind, i, ParameterConstants.Resolution));

				streams[i].StreamSize = m_MediaInfo.Get(kind, i, ParameterConstants.StreamSize);
				streams[i].LanguageFull = m_MediaInfo.Get(kind, i, ParameterConstants.LanguageFull);
				streams[i].Title = m_MediaInfo.Get(kind, i, ParameterConstants.Title);
			}
			return streams;
		}
		private IEnumerable<TextStreamInfo> ReadTextStreams()
		{
			StreamKind kind = StreamKind.Text;
			int numStreams = m_MediaInfo.Count_Get(kind);
			TextStreamInfo[] streams = new TextStreamInfo[numStreams];
			for (int i = 0; i < numStreams; i++)
			{
				streams[i] = new TextStreamInfo();

				streams[i].Codec = m_MediaInfo.Get(kind, i, ParameterConstants.Codec);
				streams[i].CodecProfile = m_MediaInfo.Get(kind, i, ParameterConstants.Codec_Profile);
				streams[i].CodecFamily = m_MediaInfo.Get(kind, i, ParameterConstants.CodecFamily);
				streams[i].CodecInfo = m_MediaInfo.Get(kind, i, ParameterConstants.CodecInfo);
				streams[i].CodecString = m_MediaInfo.Get(kind, i, ParameterConstants.CodecString);

				streams[i].LanguageFull = m_MediaInfo.Get(kind, i, ParameterConstants.LanguageFull);
			}
			return streams;
		}
		private IEnumerable<ChaptersInfo> ReadChapters()
		{
			string chapterInformText = m_MediaInfo.Get(StreamKind.Chapters, 0, "Inform");
			List<ChaptersInfo> chapters = new List<ChaptersInfo>();
			foreach (Match m in Regex.Matches(chapterInformText, @"(\d+)\s*: (\d\d:\d\d:\d\d\.\d\d\d) (.*)"))
			{
				chapters.Add(new ChaptersInfo(
					ParseInteger(m.Groups[1].Value),
					TimeSpan.Parse(m.Groups[2].Value),
					m.Groups[3].Value.TrimEnd('\r', '\n')
				));
			}
			return chapters;
		}
		private IEnumerable<ImageInfo> ReadImages()
		{
			StreamKind kind = StreamKind.Image;
			int numStreams = m_MediaInfo.Count_Get(kind);
			ImageInfo[] streams = new ImageInfo[numStreams];
			for (int i = 0; i < numStreams; i++)
			{
				streams[i] = new ImageInfo();
				streams[i].SummaryText = m_MediaInfo.Get(kind, i, "Inform");
			}
			return streams;
		}

		#region Parsing Helpers

		private static int ParseInteger(string s)
		{
			int result;
			if (int.TryParse(s, out result))
				return result;
			else
				return -1;
		}

		private static float ParseFloat(string s)
		{
			float result;
			if (float.TryParse(s, out result))
				return result;
			else
				return -1;
		}

		#endregion
	}

	public class FileMetadata
	{
		internal FileMetadata() { }

		public IEnumerable<VideoStreamInfo> VideoStreams { get; internal set; }
		public IEnumerable<AudioStreamInfo> AudioStreams { get; internal set; }
		public IEnumerable<TextStreamInfo> TextStreams { get; internal set; }
		public IEnumerable<ChaptersInfo> Chapters { get; internal set; }
		public IEnumerable<ImageInfo> Images { get; internal set; }


		//General..
	}

	public class VideoStreamInfo
	{
		internal VideoStreamInfo() { }

		/// <summary> Play time of the stream </summary>
		public TimeSpan PlayTime { get; internal set; }

		/// <summary> Bit rate in bps (bits per second) </summary>
		public int BitRateBps { get; internal set; }

		/// <summary> Width in pixels</summary>
		public int WidthPx { get; internal set; }
		/// <summary> Height in pixels</summary>
		public int HeightPx { get; internal set; }

		/// <summary> Display Aspect ratio </summary>
		public float DisplayAspectRatio { get; internal set; }
		/// <summary> Pixel Aspect ratio </summary>
		public float PixelAspectRatio { get; internal set; }

		/// <summary> Frame rate </summary>
		public float FrameRate { get; internal set; }
		/// <summary> Frame count </summary>
		public int FrameCount { get; internal set; }

		/// <summary> Codec used (text) </summary>
		public string Codec { get; internal set; }
		/// <summary> Codec used (test) </summary>
		public string CodecString { get; internal set; }
		/// <summary> Profile of the codec </summary>
		public string CodecProfile { get; internal set; }
		/// <summary> Codec family </summary>
		public string CodecFamily { get; internal set; }
		/// <summary> Info about codec </summary>
		public string CodecInfo { get; internal set; }

		/// <summary> ?? Interlaced ?? </summary>
		public string Interlacement { get; internal set; }
		/// <summary> Stream size in bytes </summary>
		public string StreamSize { get; internal set; }
		/// <summary> Language String (full) </summary>
		public string LanguageFull { get; internal set; }

		public override string ToString()
		{
			StringBuilder sb = new StringBuilder();

			sb.AppendLine("VideoStreamInfo:");

			sb.AppendFormat("PlayTime = {0}\r\n", this.PlayTime);
			sb.AppendFormat("BitRateBps = {0}\r\n", this.BitRateBps);
			sb.AppendFormat("WidthPx = {0}\r\n", this.WidthPx);
			sb.AppendFormat("HeightPx = {0}\r\n", this.HeightPx);
			sb.AppendFormat("DisplayAspectRatio = {0}\r\n", this.DisplayAspectRatio);
			sb.AppendFormat("PixelAspectRatio = {0}\r\n", this.PixelAspectRatio);
			sb.AppendFormat("FrameRate = {0}\r\n", this.FrameRate);
			sb.AppendFormat("FrameCount = {0}\r\n", this.FrameCount);
			sb.AppendFormat("Codec = {0}\r\n", this.Codec);
			sb.AppendFormat("CodecString = {0}\r\n", this.CodecString);
			sb.AppendFormat("CodecProfile = {0}\r\n", this.CodecProfile);
			sb.AppendFormat("CodecFamily = {0}\r\n", this.CodecFamily);
			sb.AppendFormat("CodecInfo = {0}\r\n", this.CodecInfo);
			sb.AppendFormat("Interlacement = {0}\r\n", this.Interlacement);
			sb.AppendFormat("StreamSize = {0}\r\n", this.StreamSize);
			sb.AppendFormat("LanguageFull = {0}\r\n", this.LanguageFull);

			return sb.ToString();
		}
	}
	public class AudioStreamInfo
	{
		internal AudioStreamInfo() { }

		/// <summary> Play time of the stream </summary>
		public TimeSpan PlayTime { get; internal set; }

		/// <summary> Bit rate in bps (bits per second) </summary>
		public int BitRateBps { get; internal set; }
		/// <summary> Bit rate mode (VBR, CBR) </summary>
		public string BitRateMode { get; internal set; }

		/// <summary> Codec used (text) </summary>
		public string Codec { get; internal set; }
		/// <summary> Codec used (test) </summary>
		public string CodecString { get; internal set; }
		/// <summary> Profile of the codec </summary>
		public string CodecProfile { get; internal set; }
		/// <summary> Codec family </summary>
		public string CodecFamily { get; internal set; }
		/// <summary> Info about codec </summary>
		public string CodecInfo { get; internal set; }

		/// <summary> Number of channels </summary>
		public int ChannelCount { get; internal set; }
		/// <summary> Position of channels </summary>
		public string ChannelPosition { get; internal set; }

		/// <summary> Sampling rate in Hertz </summary>
		public int SamplingHz { get; internal set; }
		/// <summary> Resolution in bits (8, 16, 20, 24) </summary>
		public int Resolution { get; internal set; }

		/// <summary> The gain to apply to reach 89dB SPL on playback </summary>
		public string ReplayGain_Gain { get; internal set; }
		/// <summary> The maximum absolute peak value of the item </summary>
		public string ReplayGain_Peak { get; internal set; }

		/// <summary> Name of the track </summary>
		public string Title { get; internal set; }
		/// <summary> Stream size in bytes </summary>
		public string StreamSize { get; internal set; }
		/// <summary> Language String (full) </summary>
		public string LanguageFull { get; internal set; }

		public override string ToString()
		{
			StringBuilder sb = new StringBuilder();

			sb.AppendLine("AudioStreamInfo:");

			sb.AppendFormat("PlayTime = {0}\r\n", this.PlayTime);
			sb.AppendFormat("BitRateBps = {0}\r\n", this.BitRateBps);
			sb.AppendFormat("BitRateMode = {0}\r\n", this.BitRateMode);
			sb.AppendFormat("Codec = {0}\r\n", this.Codec);
			sb.AppendFormat("CodecString = {0}\r\n", this.CodecString);
			sb.AppendFormat("CodecProfile = {0}\r\n", this.CodecProfile);
			sb.AppendFormat("CodecFamily = {0}\r\n", this.CodecFamily);
			sb.AppendFormat("CodecInfo = {0}\r\n", this.CodecInfo);
			sb.AppendFormat("ChannelCount = {0}\r\n", this.ChannelCount);
			sb.AppendFormat("ChannelPosition = {0}\r\n", this.ChannelPosition);
			sb.AppendFormat("SamplingHz = {0}\r\n", this.SamplingHz);
			sb.AppendFormat("Resolution = {0}\r\n", this.Resolution);
			sb.AppendFormat("ReplayGain_Gain = {0}\r\n", this.ReplayGain_Gain);
			sb.AppendFormat("ReplayGain_Peak = {0}\r\n", this.ReplayGain_Peak);
			sb.AppendFormat("Title = {0}\r\n", this.Title);
			sb.AppendFormat("StreamSize = {0}\r\n", this.StreamSize);
			sb.AppendFormat("LanguageFull = {0}\r\n", this.LanguageFull);

			return sb.ToString();
		}
	}
	public class TextStreamInfo
	{
		internal TextStreamInfo() { }

		/// <summary> Language String (full) </summary>
		public string LanguageFull { get; internal set; }

		/// <summary> Codec used (text) </summary>
		public string Codec { get; internal set; }
		/// <summary> Codec used (test) </summary>
		public string CodecString { get; internal set; }
		/// <summary> Profile of the codec </summary>
		public string CodecProfile { get; internal set; }
		/// <summary> Codec family </summary>
		public string CodecFamily { get; internal set; }
		/// <summary> Info about codec </summary>
		public string CodecInfo { get; internal set; }

		public override string ToString()
		{
			StringBuilder sb = new StringBuilder();

			sb.AppendLine("TextStreamInfo:");

			sb.AppendFormat("Codec = {0}\r\n", this.Codec);
			sb.AppendFormat("CodecString = {0}\r\n", this.CodecString);
			sb.AppendFormat("CodecProfile = {0}\r\n", this.CodecProfile);
			sb.AppendFormat("CodecFamily = {0}\r\n", this.CodecFamily);
			sb.AppendFormat("CodecInfo = {0}\r\n", this.CodecInfo);
			sb.AppendFormat("LanguageFull = {0}\r\n", this.LanguageFull);

			return sb.ToString();
		}

	}
	public class ChaptersInfo
	{
		internal ChaptersInfo(int index, TimeSpan startTime, string title) 
		{
			Index = index;
			StartTime = startTime;
			Title = title;
		}

		public int Index { get; internal set; }
		public TimeSpan StartTime { get; internal set; }
		public string Title { get; internal set; }

		public override string ToString()
		{
			StringBuilder sb = new StringBuilder();

			sb.AppendLine("ChaptersInfo:");

			sb.AppendFormat("Index = {0}\r\n", this.Index);
			sb.AppendFormat("StartTime = {0}\r\n", this.StartTime);
			sb.AppendFormat("Title = {0}\r\n", this.Title);

			return sb.ToString();
		}
	}
	public class ImageInfo
	{
		internal ImageInfo() { }
		public string SummaryText { get; internal set; }

		public override string ToString()
		{
			StringBuilder sb = new StringBuilder();

			sb.AppendLine("ImageInfo:");

			sb.AppendFormat("SummaryText = {0}\r\n", this.SummaryText);

			return sb.ToString();
		}
	}

	internal static class ParameterConstants
	{
		#region Video

		public const string Codec = "Codec";
		/// <summary> Codec used (text) </summary>
		public const string CodecFamily = "Codec/Family";
		/// <summary> Info about codec </summary>
		public const string CodecInfo = "Codec/Info";
		/// <summary> Codec used (test) </summary>
		public const string CodecString = "Codec/String";
		/// <summary> Link </summary>
		public const string PlayTime = "PlayTime";
		/// <summary> Bit rate in bps </summary>
		public const string BitRate = "BitRate";
		/// <summary> Width </summary>
		public const string Width = "Width";
		/// <summary> Height </summary>
		public const string Height = "Height";
		/// <summary> Pixel Aspect ratio </summary>
		public const string PixelAspectRatio = "PixelAspectRatio";
		/// <summary> Display Aspect ratio </summary>
		public const string DisplayAspectRatio = "DisplayAspectRatio";
		/// <summary> Frame rate </summary>
		public const string FrameRate = "FrameRate";
		/// <summary> Frame count </summary>
		public const string FrameCount = "FrameCount";
		/// <summary> Interlaced?? </summary>
		public const string Interlacement = "Interlacement";
		/// <summary> bits/(Pixel*Frame) (like Gordian Knot) </summary>
		public const string StreamSize = "StreamSize";
		/// <summary> Language (full) </summary>
		public const string LanguageFull = "Language/String";

		public static readonly string[] AllVideoParams = new string[]{
			"Codec",
			"Codec/Family",
			"Codec/Info",
			"PlayTime",
			"BitRate",
			"Width",
			"Height",
			"PixelAspectRatio",
			"DisplayAspectRatio",
			"FrameRate",
			"FrameCount",
			"Interlacement",
			"StreamSize",
			"Language/String",
		};

		#endregion

		#region Audio

		/// <summary> Profile of the codec </summary>
		public const string Codec_Profile = "Codec_Profile";
		/// <summary> Bit rate mode (VBR, CBR) </summary>
		public const string BitRate_Mode = "BitRate_Mode";
		/// <summary> Number of channels </summary>
		public const string ChannelCount = "Channel(s)";
		/// <summary> Position of channels </summary>
		public const string ChannelPositions = "ChannelPositions";
		/// <summary> Sampling rate (Hz?) </summary>
		public const string SamplingRate = "SamplingRate";
		/// <summary> Frame count </summary>
		public const string SamplingCount = "SamplingCount";
		/// <summary> Resolution in bits (8, 16, 20, 24) </summary>
		public const string Resolution = "Resolution";
		/// <summary> The gain to apply to reach 89dB SPL on playback </summary>
		public const string ReplayGain_Gain = "ReplayGain_Gain";
		/// <summary> The maximum absolute peak value of the item </summary>
		public const string ReplayGain_Peak = "ReplayGain_Peak";
		/// <summary> Name of the track </summary>
		public const string Title = "Title";

		public static readonly string[] AllAudioParams = new string[]{
			"Codec", //Codec used
			"Codec/Family", //Codec family
			"Codec/Info", //Info about codec
			"Codec_Description", //Manual description
			"Codec_Profile", //Profile of the codec
			"PlayTime", //Play time of the stream
			"BitRate_Mode", //Bit rate mode (VBR, CBR)
			"BitRate", //Bit rate in bps
			"BitRate_Minimum", //Minimum Bit rate in bps
			"BitRate_Nominal", //Nominal Bit rate in bps
			"BitRate_Maximum", //Maximum Bit rate in bps
			"Channel(s)", //Number of channels
			"ChannelPositions", //Position of channels
			"SamplingRate", //Sampling rate
			"SamplingRate/String", //in KHz
			"SamplingCount", //Frame count
			"Resolution", //Resolution in bits (8, 16, 20, 24)
			"ReplayGain_Gain", //The gain to apply to reach 89dB SPL on playback
			"ReplayGain_Peak", //The maximum absolute peak value of the item
			"StreamSize", //Stream size in bytes
			"Title", //Name of the track
			"Language/String", //Language (full)
		};

		#endregion

		#region Text

		#endregion

		#region Chapters

		#endregion

		#region Image

		#endregion
	}

	/*
	 * 
	 * ([^;]*)(;(.+))?
	 * /// <summary> \3 </summary>\npublic const string \1 = "\1"
	 * /// <summary> \3 </summary>\npublic const string <\1> = "\1"
	 *
	 */
}
