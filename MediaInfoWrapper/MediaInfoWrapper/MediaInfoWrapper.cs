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
            if (m_MediaInfo.Open(path) == 0)
            {
                throw new Exception(string.Format("Couldn't open file \"{0}\".", path));
            }
            try
            {
                return new FileMetadata
                           {
                               Format = m_MediaInfo.Get(StreamKind.General, 0, ParameterConstants.Format),
                               FormatInfo = m_MediaInfo.Get(StreamKind.General, 0, ParameterConstants.FormatInfo),
                               FormatVersion = m_MediaInfo.Get(StreamKind.General, 0, ParameterConstants.FormatVersion),
                               FormatProfile = m_MediaInfo.Get(StreamKind.General, 0, ParameterConstants.FormatProfile),
                               FormatSettings = m_MediaInfo.Get(StreamKind.General, 0, ParameterConstants.FormatSettings),

                               VideoStreams = ReadVideoStreams(),
                               AudioStreams = ReadAudioStreams(),
                               TextStreams = ReadTextStreams(),
                               Chapters = ReadChapters(),
                               Images = ReadImages()
                           };
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
                                             ParsingHelper.ParseInteger(m_MediaInfo.Get(kind, i, ParameterConstants.PlayTime))),
                                     BitRateBps = ParsingHelper.ParseInteger(m_MediaInfo.Get(kind, i, ParameterConstants.BitRate)),
                                     MaximumBitRateBps = ParsingHelper.ParseInteger(m_MediaInfo.Get(kind, i, ParameterConstants.MaximumBitRate)),
                                     HeightPx = ParsingHelper.ParseInteger(m_MediaInfo.Get(kind, i, ParameterConstants.Height)),
                                     WidthPx = ParsingHelper.ParseInteger(m_MediaInfo.Get(kind, i, ParameterConstants.Width)),
                                     DisplayAspectRatio =
                                         ParsingHelper.ParseFloat(m_MediaInfo.Get(kind, i, ParameterConstants.DisplayAspectRatio)),
                                     PixelAspectRatio = ParsingHelper.ParseFloat(m_MediaInfo.Get(kind, i, ParameterConstants.PixelAspectRatio)),
                                     FrameCount = ParsingHelper.ParseInteger(m_MediaInfo.Get(kind, i, ParameterConstants.FrameCount)),
                                     FrameRate = ParsingHelper.ParseFloat(m_MediaInfo.Get(kind, i, ParameterConstants.FrameRate)),
                                     Codec = m_MediaInfo.Get(kind, i, ParameterConstants.Codec),
                                     CodecProfile = m_MediaInfo.Get(kind, i, ParameterConstants.Codec_Profile),
                                     CodecFamily = m_MediaInfo.Get(kind, i, ParameterConstants.CodecFamily),
                                     CodecInfo = m_MediaInfo.Get(kind, i, ParameterConstants.CodecInfo),
                                     CodecString = m_MediaInfo.Get(kind, i, ParameterConstants.CodecString),
                                     FormatSettings = m_MediaInfo.Get(kind, i, ParameterConstants.Format_Settings),
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
                                                                Format_Settings_CABAC = ParsingHelper.ParseYesNo(m_MediaInfo.Get(kind, i, ParameterConstants.Format_Settings_CABAC)),
                                                                Format_Settings_CABAC_String = m_MediaInfo.Get(kind, i, ParameterConstants.Format_Settings_CABAC_String),
                                                                Format_Settings_RefFrames = ParsingHelper.ParseInteger(m_MediaInfo.Get(kind, i, ParameterConstants.Format_Settings_RefFrames)),
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
                                         ParsingHelper.ParseInteger(m_MediaInfo.Get(kind, i, ParameterConstants.PlayTime))),
                                     BitRateBps = ParsingHelper.ParseInteger(m_MediaInfo.Get(kind, i, ParameterConstants.BitRate)),
                                     Codec = m_MediaInfo.Get(kind, i, ParameterConstants.Codec),
                                     CodecProfile = m_MediaInfo.Get(kind, i, ParameterConstants.Codec_Profile),
                                     CodecFamily = m_MediaInfo.Get(kind, i, ParameterConstants.CodecFamily),
                                     CodecInfo = m_MediaInfo.Get(kind, i, ParameterConstants.CodecInfo),
                                     CodecString = m_MediaInfo.Get(kind, i, ParameterConstants.CodecString),
                                     ChannelCount = ParsingHelper.ParseInteger(m_MediaInfo.Get(kind, i, ParameterConstants.ChannelCount)),
                                     ChannelPosition = m_MediaInfo.Get(kind, i, ParameterConstants.ChannelPositions),
                                     ReplayGain_Gain = m_MediaInfo.Get(kind, i, ParameterConstants.ReplayGain_Gain),
                                     ReplayGain_Peak = m_MediaInfo.Get(kind, i, ParameterConstants.ReplayGain_Peak),
                                     SamplingHz = ParsingHelper.ParseInteger(m_MediaInfo.Get(kind, i, ParameterConstants.SamplingRate)),
                                     Resolution = ParsingHelper.ParseInteger(m_MediaInfo.Get(kind, i, ParameterConstants.Resolution)),
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
                    ParsingHelper.ParseInteger(m.Groups[1].Value),
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
    }

    public class FileMetadata
    {
        internal FileMetadata() { }

        public IEnumerable<VideoStreamInfo> VideoStreams { get; internal set; }
        public IEnumerable<AudioStreamInfo> AudioStreams { get; internal set; }
        public IEnumerable<TextStreamInfo> TextStreams { get; internal set; }
        public IEnumerable<ChaptersInfo> Chapters { get; internal set; }
        public IEnumerable<ImageInfo> Images { get; internal set; }

        /// <summary> Format used </summary>
        public string Format { get; internal set; }
        /// <summary> Info about this Format </summary>
        public string FormatInfo { get; internal set; }
        /// <summary> Version of this format </summary>
        public string FormatVersion { get; internal set; }
        /// <summary> Profile of the Format </summary>
        public string FormatProfile { get; internal set; }
        /// <summary> Settings needed for decoder used </summary>
        public string FormatSettings { get; internal set; }

        public bool IsCompatible(DeviceType device)
        {
            return Format == "MPEG-4"
                   && VideoStreams.Count() == 1 && AudioStreams.Count() == 1
                   && VideoStreams.Single().IsCompatible(device)
                   && AudioStreams.Single().IsCompatible(device);
        }

        public override string ToString()
        {
            var sb = new StringBuilder();

            sb.AppendLine("General:");

            sb.AppendFormat("Format = {0}\n", this.Format);
            sb.AppendFormat("FormatInfo = {0}\n", this.FormatInfo);
            sb.AppendFormat("FormatVersion = {0}\n", this.FormatVersion);
            sb.AppendFormat("FormatProfile = {0}\n", this.FormatProfile);
            sb.AppendFormat("FormatSettings = {0}\n", this.FormatSettings);

            foreach (var i in VideoStreams) sb.AppendLine(i.ToString());
            foreach (var i in AudioStreams) sb.AppendLine(i.ToString());
            foreach (var i in TextStreams) sb.AppendLine(i.ToString());
            foreach (var i in Chapters) sb.AppendLine(i.ToString());
            foreach (var i in Images) sb.AppendLine(i.ToString());

            return sb.ToString();
        }
    }

    public class VideoStreamInfo
    {
        internal VideoStreamInfo() { }

        /// <summary> Play time of the stream </summary>
        public TimeSpan PlayTime { get; internal set; }

        /// <summary> Bit rate in bps (bits per second) </summary>
        public int BitRateBps { get; internal set; }
        /// <summary> Bit rate in bps (kilobits per second) </summary>
        public float BitRateKbps { get { return BitRateBps / 1024.0f; } }
        /// <summary> MaximumBit rate in bps (bits per second) </summary>
        public int MaximumBitRateBps { get; internal set; }

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
        public string FormatSettings { get; internal set; }

        /// <summary> ?? Interlaced ?? </summary>
        public string Interlacement { get; internal set; }
        /// <summary> Stream size in bytes </summary>
        public string StreamSize { get; internal set; }
        /// <summary> Language String (full) </summary>
        public string LanguageFull { get; internal set; }

        public VideoFormatInfo VideoFormatInfo { get; internal set; }

        private bool TryParseAvcProfile(out AvcProfile profile, out float level)
        {
            profile = AvcProfile.Unknown;
            level = -1;

            var match = Regex.Match(CodecProfile, "^(.*)@L([0-9.]+)$");
            if (!match.Success)
                return false;

            var nameString = match.Groups[1].Value;
            try
            {
                profile = (AvcProfile)Enum.Parse(typeof(AvcProfile), nameString);
            }
            catch (ArgumentException)
            {
                return false;
            }
            if (profile == AvcProfile.Baseline && VideoFormatInfo.Format_Settings_RefFrames == 1)
                profile = AvcProfile.ConstrainedBaseline;

            var levelString = match.Groups[2].Value;
            return float.TryParse(levelString, out level);
        }

        public bool IsAvc { get { return Codec == "AVC"; } }
        public float? AvcProfileLevel
        {
            get
            {
                AvcProfile ignored; float level;
                return TryParseAvcProfile(out ignored, out level) ? (float?)level : null;
            }
        }
        public AvcProfile AvcProfileName
        {
            get
            {
                AvcProfile profile; float ignored;
                return TryParseAvcProfile(out profile, out ignored) ? profile : AvcProfile.Unknown;
            }
        }

        public bool IsCompatible(DeviceType device)
        {
            AvcProfile profile; float level;
            if (!IsAvc || !TryParseAvcProfile(out profile, out level))
                return false;

            switch (device)
            {
                case DeviceType.iPod5G:
                    if (WidthPx <= 640 && HeightPx <= 480 && profile <= AvcProfile.ConstrainedBaseline && BitRateKbps <= 1500)
                        return true;
                    if (WidthPx <= 320 && HeightPx <= 240 && profile <= AvcProfile.Baseline && level <= 1.3 && BitRateKbps <= 768)
                        return true;
                    return false;
                case DeviceType.iPodClassic:
                case DeviceType.iPhone:
                    if (WidthPx <= 640 && HeightPx <= 480 && profile <= AvcProfile.ConstrainedBaseline && BitRateKbps <= 1500)
                        return true;
                    if (WidthPx <= 640 && HeightPx <= 480 && profile <= AvcProfile.Baseline && level <= 3 && BitRateKbps <= 2500)
                        return true;
                    return false;
                default:
                    throw new ArgumentOutOfRangeException("device");
            }
        }

        public override string ToString()
        {
            var sb = new StringBuilder();

            sb.AppendLine("VideoStreamInfo:");

            sb.AppendFormat("PlayTime = {0}\n", this.PlayTime);
            sb.AppendFormat("BitRateBps = {0}\n", this.BitRateBps);
            sb.AppendFormat("BitRateKbps = {0}\n", this.BitRateKbps);
            sb.AppendFormat("MaximumBitRateBps = {0}\n", this.MaximumBitRateBps);
            sb.AppendFormat("WidthPx = {0}\n", this.WidthPx);
            sb.AppendFormat("HeightPx = {0}\n", this.HeightPx);
            sb.AppendFormat("DisplayAspectRatio = {0}\n", this.DisplayAspectRatio);
            sb.AppendFormat("PixelAspectRatio = {0}\n", this.PixelAspectRatio);
            sb.AppendFormat("FrameRate = {0}\n", this.FrameRate);
            sb.AppendFormat("FrameCount = {0}\n", this.FrameCount);
            sb.AppendFormat("Codec = {0}\n", this.Codec);
            sb.AppendFormat("CodecString = {0}\n", this.CodecString);
            sb.AppendFormat("CodecProfile = {0}\n", this.CodecProfile);
            sb.AppendFormat("CodecFamily = {0}\n", this.CodecFamily);
            sb.AppendFormat("CodecInfo = {0}\n", this.CodecInfo);
            sb.AppendFormat("FormatSettings = {0}\n", this.FormatSettings);
            sb.AppendFormat("Interlacement = {0}\n", this.Interlacement);
            sb.AppendFormat("StreamSize = {0}\n", this.StreamSize);
            sb.AppendFormat("LanguageFull = {0}\n", this.LanguageFull);

            sb.AppendFormat("AvcProfileLevel = {0}\n", this.AvcProfileLevel);
            sb.AppendFormat("AvcProfileName = {0}\n", this.AvcProfileName);

            sb.AppendFormat("iPod5G Compatible = {0}\n", this.IsCompatible(DeviceType.iPod5G) ? "Yes" : "No");
            sb.AppendFormat("iPodClassic Compatible = {0}\n", this.IsCompatible(DeviceType.iPodClassic) ? "Yes" : "No");
            sb.AppendFormat("iPhone Compatible = {0}\n", this.IsCompatible(DeviceType.iPhone) ? "Yes" : "No");

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
        public bool Format_Settings_CABAC { get; internal set; }
        /// <summary> Settings needed for decoder used, detailled </summary>
        public string Format_Settings_CABAC_String { get; internal set; }
        /// <summary> Settings needed for decoder used, detailled </summary>
        public int Format_Settings_RefFrames { get; internal set; }
        /// <summary> Settings needed for decoder used, detailled </summary>
        public string Format_Settings_RefFrames_String { get; internal set; }
        /// <summary> Settings needed for decoder used, detailled </summary>
        public string Format_Settings_Pulldown { get; internal set; }

        public override string ToString()
        {
            return string.Format("Format_Settings: {0}\nFormat_Settings_BVOP: {1}\nFormat_Settings_BVOP_String: {2}\nFormat_Settings_QPel: {3}\nFormat_Settings_QPel_String: {4}\nFormat_Settings_GMC: {5}\nFormat_Settings_GMC_String: {6}\nFormat_Settings_Matrix: {7}\nFormat_Settings_Matrix_String: {8}\nFormat_Settings_Matrix_Data: {9}\nFormat_Settings_CABAC: {10}\nFormat_Settings_CABAC_String: {11}\nFormat_Settings_RefFrames: {12}\nFormat_Settings_RefFrames_String: {13}\nFormat_Settings_Pulldown: {14}\n", Format_Settings, Format_Settings_BVOP, Format_Settings_BVOP_String, Format_Settings_QPel, Format_Settings_QPel_String, Format_Settings_GMC, Format_Settings_GMC_String, Format_Settings_Matrix, Format_Settings_Matrix_String, Format_Settings_Matrix_Data, Format_Settings_CABAC, Format_Settings_CABAC_String, Format_Settings_RefFrames, Format_Settings_RefFrames_String, Format_Settings_Pulldown);
        }
    }

    public enum AvcProfile
    {
        Unknown = 0,
        ConstrainedBaseline = 1,
        Baseline = 2,
        Main = 3,
        Extended = 4,
        High = 5
    }

    public enum DeviceType
    {
        iPod5G = 1,
        iPodClassic = 2,
        iPhone = 3
    }

    public class AudioStreamInfo
    {
        internal AudioStreamInfo() { }

        /// <summary> Play time of the stream </summary>
        public TimeSpan PlayTime { get; internal set; }

        /// <summary> Bit rate in bps (bits per second) </summary>
        public int BitRateBps { get; internal set; }
        /// <summary> Bit rate in bps (kilobits per second) </summary>
        public float BitRateKbps { get { return BitRateBps / 1024.0f; } }
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

        public bool IsCompatible(DeviceType device)
        {
            switch (device)
            {
                case DeviceType.iPod5G:
                case DeviceType.iPodClassic:
                case DeviceType.iPhone:
                    return CodecInfo == "AAC Low Complexity" && BitRateKbps <= 160 && ChannelCount <= 2;
                default:
                    throw new ArgumentOutOfRangeException("device");
            }
        }

        public override string ToString()
        {
            var sb = new StringBuilder();

            sb.AppendLine("AudioStreamInfo:");

            sb.AppendFormat("PlayTime = {0}\n", this.PlayTime);
            sb.AppendFormat("BitRateBps = {0}\n", this.BitRateBps);
            sb.AppendFormat("BitRateMode = {0}\n", this.BitRateMode);
            sb.AppendFormat("Codec = {0}\n", this.Codec);
            sb.AppendFormat("CodecString = {0}\n", this.CodecString);
            sb.AppendFormat("CodecProfile = {0}\n", this.CodecProfile);
            sb.AppendFormat("CodecFamily = {0}\n", this.CodecFamily);
            sb.AppendFormat("CodecInfo = {0}\n", this.CodecInfo);
            sb.AppendFormat("ChannelCount = {0}\n", this.ChannelCount);
            sb.AppendFormat("ChannelPosition = {0}\n", this.ChannelPosition);
            sb.AppendFormat("SamplingHz = {0}\n", this.SamplingHz);
            sb.AppendFormat("Resolution = {0}\n", this.Resolution);
            sb.AppendFormat("ReplayGain_Gain = {0}\n", this.ReplayGain_Gain);
            sb.AppendFormat("ReplayGain_Peak = {0}\n", this.ReplayGain_Peak);
            sb.AppendFormat("Title = {0}\n", this.Title);
            sb.AppendFormat("StreamSize = {0}\n", this.StreamSize);
            sb.AppendFormat("LanguageFull = {0}\n", this.LanguageFull);

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

            sb.AppendFormat("Codec = {0}\n", this.Codec);
            sb.AppendFormat("CodecString = {0}\n", this.CodecString);
            sb.AppendFormat("CodecProfile = {0}\n", this.CodecProfile);
            sb.AppendFormat("CodecFamily = {0}\n", this.CodecFamily);
            sb.AppendFormat("CodecInfo = {0}\n", this.CodecInfo);
            sb.AppendFormat("LanguageFull = {0}\n", this.LanguageFull);

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

            sb.AppendFormat("Index = {0}\n", this.Index);
            sb.AppendFormat("StartTime = {0}\n", this.StartTime);
            sb.AppendFormat("Title = {0}\n", this.Title);

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

            sb.AppendFormat("SummaryText = {0}\n", this.SummaryText);

            return sb.ToString();
        }
    }

    internal static class ParameterConstants
    {
        #region General

        /// <summary> Format used </summary>
        public const string Format = "Format";
        /// <summary> Info about this Format </summary>
        public const string FormatInfo = "Format/Info";
        /// <summary> Version of this format </summary>
        public const string FormatVersion = "Format_Version";
        /// <summary> Profile of the Format </summary>
        public const string FormatProfile = "Format_Profile";
        /// <summary> Settings needed for decoder used </summary>
        public const string FormatSettings = "Format_Settings";

        #endregion

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
        /// <summary> Maximum Bit rate in bps </summary>
        public const string MaximumBitRate = "BitRate_Maximum";
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

    internal static class ParsingHelper
    {
        public static int ParseInteger(string s)
        {
            int result;
            if (int.TryParse(s, out result))
                return result;
            else
                return -1;
        }
        public static float ParseFloat(string s)
        {
            float result;
            if (float.TryParse(s, out result))
                return result;
            else
                return -1;
        }
        public static bool ParseYesNo(string s)
        {
            return StringComparer.InvariantCultureIgnoreCase.Equals(s, "Yes");
        }
    }

    /*
     * 
     * {[^;]*};{.+}?
     * /// <summary> \2 </summary>\npublic const string \1 = "\1";
     *
     */
}
