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

        public string GetParametersCsv()
        {
            return m_MediaInfo.Option("Info_Parameters_CSV");
        }

        public FileMetadata ReadFile(string path)
        {
            FileMetadata file;
            if (TryReadFile(path, out file))
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
                    file = new FileMetadata
                               {
                                   VideoStreams = ReadVideoStreams(),
                                   AudioStreams = ReadAudioStreams(),
                                   TextStreams = ReadTextStreams(),
                                   Chapters = ReadChapters(),
                                   Images = ReadImages()
                               };

                    return true;
                }
                catch (Exception inner)
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
            const StreamKind kind = StreamKind.Video;
            int numStreams = m_MediaInfo.Count_Get(kind);
            var streams = new VideoStreamInfo[numStreams];
            for (int i = 0; i < numStreams; i++)
            {
                streams[i] = new VideoStreamInfo
                                 {
                                     PlayTime =
                                         TimeSpan.FromMilliseconds(
                                             ParseInteger(m_MediaInfo.Get(kind, i, ParameterConstants.PlayTime))),
                                     BitRateBps = ParseInteger(m_MediaInfo.Get(kind, i, ParameterConstants.BitRate)),
                                     HeightPx = ParseInteger(m_MediaInfo.Get(kind, i, ParameterConstants.Height)),
                                     WidthPx = ParseInteger(m_MediaInfo.Get(kind, i, ParameterConstants.Width)),
                                     DisplayAspectRatio =
                                         ParseFloat(m_MediaInfo.Get(kind, i, ParameterConstants.DisplayAspectRatio)),
                                     PixelAspectRatio = ParseFloat(m_MediaInfo.Get(kind, i, ParameterConstants.PixelAspectRatio)),
                                     FrameCount = ParseInteger(m_MediaInfo.Get(kind, i, ParameterConstants.FrameCount)),
                                     FrameRate = ParseFloat(m_MediaInfo.Get(kind, i, ParameterConstants.FrameRate)),
                                     Codec = m_MediaInfo.Get(kind, i, ParameterConstants.Codec),
                                     CodecProfile = m_MediaInfo.Get(kind, i, ParameterConstants.Codec_Profile),
                                     CodecFamily = m_MediaInfo.Get(kind, i, ParameterConstants.CodecFamily),
                                     CodecInfo = m_MediaInfo.Get(kind, i, ParameterConstants.CodecInfo),
                                     CodecString = m_MediaInfo.Get(kind, i, ParameterConstants.CodecString),
                                     CodecSettings = m_MediaInfo.Get(kind, i, "Codec_Settings"),
                                     LanguageFull = m_MediaInfo.Get(kind, i, ParameterConstants.LanguageFull),
                                     StreamSize = m_MediaInfo.Get(kind, i, ParameterConstants.StreamSize),
                                     Interlacement = m_MediaInfo.Get(kind, i, ParameterConstants.Interlacement),

                                     VideoFormatInfo = new VideoFormatInfo()
                                                            {
                                                                Format_Settings = m_MediaInfo.Get(kind, i, ParameterConstants.Format_Settings),
                                                                Format_Settings_BVOP = m_MediaInfo.Get(kind, i, ParameterConstants.Format_Settings_BVOP),
                                                                Format_Settings_BVOP_String = m_MediaInfo.Get(kind, i, ParameterConstants.Format_Settings_BVOP_String),
                                                                Format_Settings_QPel = m_MediaInfo.Get(kind, i, ParameterConstants.Format_Settings_QPel),
                                                                Format_Settings_QPel_String = m_MediaInfo.Get(kind, i, ParameterConstants.Format_Settings_QPel_String),
                                                                Format_Settings_GMC = m_MediaInfo.Get(kind, i, ParameterConstants.Format_Settings_GMC),
                                                                Format_Settings_GMC_String = m_MediaInfo.Get(kind, i, ParameterConstants.Format_Settings_GMC_String),
                                                                Format_Settings_Matrix = m_MediaInfo.Get(kind, i, ParameterConstants.Format_Settings_Matrix),
                                                                Format_Settings_Matrix_String = m_MediaInfo.Get(kind, i, ParameterConstants.Format_Settings_Matrix_String),
                                                                Format_Settings_Matrix_Data = m_MediaInfo.Get(kind, i, ParameterConstants.Format_Settings_Matrix_Data),
                                                                Format_Settings_CABAC = m_MediaInfo.Get(kind, i, ParameterConstants.Format_Settings_CABAC),
                                                                Format_Settings_CABAC_String = m_MediaInfo.Get(kind, i, ParameterConstants.Format_Settings_CABAC_String),
                                                                Format_Settings_RefFrames = m_MediaInfo.Get(kind, i, ParameterConstants.Format_Settings_RefFrames),
                                                                Format_Settings_RefFrames_String = m_MediaInfo.Get(kind, i, ParameterConstants.Format_Settings_RefFrames_String),
                                                                Format_Settings_Pulldown = m_MediaInfo.Get(kind, i, ParameterConstants.Format_Settings_Pulldown),
                                                            }
                                 };
            }
            return streams;
        }
        private IEnumerable<AudioStreamInfo> ReadAudioStreams()
        {
            const StreamKind kind = StreamKind.Audio;
            int numStreams = m_MediaInfo.Count_Get(kind);
            var streams = new AudioStreamInfo[numStreams];
            for (int i = 0; i < numStreams; i++)
            {
                streams[i] = new AudioStreamInfo
                                 {
                                     PlayTime =
                                         TimeSpan.FromMilliseconds(
                                         ParseInteger(m_MediaInfo.Get(kind, i, ParameterConstants.PlayTime))),
                                     BitRateBps = ParseInteger(m_MediaInfo.Get(kind, i, ParameterConstants.BitRate)),
                                     Codec = m_MediaInfo.Get(kind, i, ParameterConstants.Codec),
                                     CodecProfile = m_MediaInfo.Get(kind, i, ParameterConstants.Codec_Profile),
                                     CodecFamily = m_MediaInfo.Get(kind, i, ParameterConstants.CodecFamily),
                                     CodecInfo = m_MediaInfo.Get(kind, i, ParameterConstants.CodecInfo),
                                     CodecString = m_MediaInfo.Get(kind, i, ParameterConstants.CodecString),
                                     ChannelCount = ParseInteger(m_MediaInfo.Get(kind, i, ParameterConstants.ChannelCount)),
                                     ChannelPosition = m_MediaInfo.Get(kind, i, ParameterConstants.ChannelPositions),
                                     ReplayGain_Gain = m_MediaInfo.Get(kind, i, ParameterConstants.ReplayGain_Gain),
                                     ReplayGain_Peak = m_MediaInfo.Get(kind, i, ParameterConstants.ReplayGain_Peak),
                                     SamplingHz = ParseInteger(m_MediaInfo.Get(kind, i, ParameterConstants.SamplingRate)),
                                     Resolution = ParseInteger(m_MediaInfo.Get(kind, i, ParameterConstants.Resolution)),
                                     StreamSize = m_MediaInfo.Get(kind, i, ParameterConstants.StreamSize),
                                     LanguageFull = m_MediaInfo.Get(kind, i, ParameterConstants.LanguageFull),
                                     Title = m_MediaInfo.Get(kind, i, ParameterConstants.Title)
                                 };
            }
            return streams;
        }
        private IEnumerable<TextStreamInfo> ReadTextStreams()
        {
            const StreamKind kind = StreamKind.Text;
            int numStreams = m_MediaInfo.Count_Get(kind);
            var streams = new TextStreamInfo[numStreams];
            for (int i = 0; i < numStreams; i++)
            {
                streams[i] = new TextStreamInfo
                                 {
                                     Codec = m_MediaInfo.Get(kind, i, ParameterConstants.Codec),
                                     CodecProfile = m_MediaInfo.Get(kind, i, ParameterConstants.Codec_Profile),
                                     CodecFamily = m_MediaInfo.Get(kind, i, ParameterConstants.CodecFamily),
                                     CodecInfo = m_MediaInfo.Get(kind, i, ParameterConstants.CodecInfo),
                                     CodecString = m_MediaInfo.Get(kind, i, ParameterConstants.CodecString),
                                     LanguageFull = m_MediaInfo.Get(kind, i, ParameterConstants.LanguageFull)
                                 };
            }
            return streams;
        }
        private IEnumerable<ChaptersInfo> ReadChapters()
        {
            string chapterInformText = m_MediaInfo.Get(StreamKind.Chapters, 0, "Inform");
            var chapters = new List<ChaptersInfo>();
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
            const StreamKind kind = StreamKind.Image;
            int numStreams = m_MediaInfo.Count_Get(kind);
            var streams = new ImageInfo[numStreams];
            for (int i = 0; i < numStreams; i++)
            {
                streams[i] = new ImageInfo
                                 {
                                     SummaryText = m_MediaInfo.Get(kind, i, "Inform")
                                 };
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
        /// <summary> Info encoding settings </summary>
        public string CodecSettings { get; internal set; }

        /// <summary> ?? Interlaced ?? </summary>
        public string Interlacement { get; internal set; }
        /// <summary> Stream size in bytes </summary>
        public string StreamSize { get; internal set; }
        /// <summary> Language String (full) </summary>
        public string LanguageFull { get; internal set; }

        public VideoFormatInfo VideoFormatInfo { get; internal set; }

        public override string ToString()
        {
            var sb = new StringBuilder();

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
            sb.AppendFormat("CodecSettings = {0}\r\n", this.CodecSettings);
            sb.AppendFormat("Interlacement = {0}\r\n", this.Interlacement);
            sb.AppendFormat("StreamSize = {0}\r\n", this.StreamSize);
            sb.AppendFormat("LanguageFull = {0}\r\n", this.LanguageFull);

            sb.Append(this.VideoFormatInfo);

            return sb.ToString();
        }
    }

    public class VideoFormatInfo
    {
        internal VideoFormatInfo() { }

        /// <summary> Settings needed for decoder used, summary </summary>
        public string Format_Settings { get; internal set; }
        /// <summary> Settings needed for decoder used, detailled </summary>
        public string Format_Settings_BVOP { get; internal set; }
        /// <summary> Settings needed for decoder used, detailled </summary>
        public string Format_Settings_BVOP_String { get; internal set; }
        /// <summary> Settings needed for decoder used, detailled </summary>
        public string Format_Settings_QPel { get; internal set; }
        /// <summary> Settings needed for decoder used, detailled </summary>
        public string Format_Settings_QPel_String { get; internal set; }
        /// <summary> Settings needed for decoder used, detailled </summary>
        public string Format_Settings_GMC { get; internal set; }
        /// <summary> Settings needed for decoder used, detailled </summary>
        public string Format_Settings_GMC_String { get; internal set; }
        /// <summary> Settings needed for decoder used, detailled </summary>
        public string Format_Settings_Matrix { get; internal set; }
        /// <summary> Settings needed for decoder used, detailled </summary>
        public string Format_Settings_Matrix_String { get; internal set; }
        /// <summary> Matrix, in binary format encoded BASE64. Order = intra, non-intra, gray intra, gray non-intra </summary>
        public string Format_Settings_Matrix_Data { get; internal set; }
        /// <summary> Settings needed for decoder used, detailled </summary>
        public string Format_Settings_CABAC { get; internal set; }
        /// <summary> Settings needed for decoder used, detailled </summary>
        public string Format_Settings_CABAC_String { get; internal set; }
        /// <summary> Settings needed for decoder used, detailled </summary>
        public string Format_Settings_RefFrames { get; internal set; }
        /// <summary> Settings needed for decoder used, detailled </summary>
        public string Format_Settings_RefFrames_String { get; internal set; }
        /// <summary> Settings needed for decoder used, detailled </summary>
        public string Format_Settings_Pulldown { get; internal set; }

        public override string ToString()
        {
            return string.Format("Format_Settings: {0}\nFormat_Settings_BVOP: {1}\nFormat_Settings_BVOP_String: {2}\nFormat_Settings_QPel: {3}\nFormat_Settings_QPel_String: {4}\nFormat_Settings_GMC: {5}\nFormat_Settings_GMC_String: {6}\nFormat_Settings_Matrix: {7}\nFormat_Settings_Matrix_String: {8}\nFormat_Settings_Matrix_Data: {9}\nFormat_Settings_CABAC: {10}\nFormat_Settings_CABAC_String: {11}\nFormat_Settings_RefFrames: {12}\nFormat_Settings_RefFrames_String: {13}\nFormat_Settings_Pulldown: {14}\n", Format_Settings, Format_Settings_BVOP, Format_Settings_BVOP_String, Format_Settings_QPel, Format_Settings_QPel_String, Format_Settings_GMC, Format_Settings_GMC_String, Format_Settings_Matrix, Format_Settings_Matrix_String, Format_Settings_Matrix_Data, Format_Settings_CABAC, Format_Settings_CABAC_String, Format_Settings_RefFrames, Format_Settings_RefFrames_String, Format_Settings_Pulldown);
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
            var sb = new StringBuilder();

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
            var sb = new StringBuilder();

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
            var sb = new StringBuilder();

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

        public static readonly string[] AllVideoParams = new[]{
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

        public static readonly string[] AllAudioParams = new[]{
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

        #region Codec

        /// <summary> Settings needed for decoder used, summary </summary>
        public const string Format_Settings = "Format_Settings";
        /// <summary> Settings needed for decoder used, detailled </summary>
        public const string Format_Settings_BVOP = "Format_Settings_BVOP";
        /// <summary> Settings needed for decoder used, detailled </summary>
        public const string Format_Settings_BVOP_String = "Format_Settings_BVOP/String";
        /// <summary> Settings needed for decoder used, detailled </summary>
        public const string Format_Settings_QPel = "Format_Settings_QPel";
        /// <summary> Settings needed for decoder used, detailled </summary>
        public const string Format_Settings_QPel_String = "Format_Settings_QPel/String";
        /// <summary> Settings needed for decoder used, detailled </summary>
        public const string Format_Settings_GMC = "Format_Settings_GMC";
        /// <summary> Settings needed for decoder used, detailled </summary>
        public const string Format_Settings_GMC_String = "Format_Settings_GMC/String";
        /// <summary> Settings needed for decoder used, detailled </summary>
        public const string Format_Settings_Matrix = "Format_Settings_Matrix";
        /// <summary> Settings needed for decoder used, detailled </summary>
        public const string Format_Settings_Matrix_String = "Format_Settings_Matrix/String";
        /// <summary> Matrix, in binary format encoded BASE64. Order = intra, non-intra, gray intra, gray non-intra </summary>
        public const string Format_Settings_Matrix_Data = "Format_Settings_Matrix_Data";
        /// <summary> Settings needed for decoder used, detailled </summary>
        public const string Format_Settings_CABAC = "Format_Settings_CABAC";
        /// <summary> Settings needed for decoder used, detailled </summary>
        public const string Format_Settings_CABAC_String = "Format_Settings_CABAC/String";
        /// <summary> Settings needed for decoder used, detailled </summary>
        public const string Format_Settings_RefFrames = "Format_Settings_RefFrames";
        /// <summary> Settings needed for decoder used, detailled </summary>
        public const string Format_Settings_RefFrames_String = "Format_Settings_RefFrames/String";
        /// <summary> Settings needed for decoder used, detailled </summary>
        public const string Format_Settings_Pulldown = "Format_Settings_Pulldown";

        public static readonly string[] AllCodecParams = new[]{
            Format_Settings,
            Format_Settings_BVOP,
            Format_Settings_BVOP_String,
            Format_Settings_QPel,
            Format_Settings_QPel_String,
            Format_Settings_GMC,
            Format_Settings_GMC_String,
            Format_Settings_Matrix,
            Format_Settings_Matrix_String,
            Format_Settings_Matrix_Data,
            Format_Settings_CABAC,
            Format_Settings_CABAC_String,
            Format_Settings_RefFrames,
            Format_Settings_RefFrames_String,
            Format_Settings_Pulldown,
        };

        #endregion
    }

    /*
     * 
     * {[^;]*};{.+}?
     * /// <summary> \2 </summary>\npublic const string \1 = "\1";
     *
     */
}
