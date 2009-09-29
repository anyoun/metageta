// Copyright 2009 Will Thomas
// 
// This file is part of MetaGeta.
// 
// MetaGeta is free software: you can redistribute it and/or modify
// it under the terms of the GNU General Public License as published by
// the Free Software Foundation, either version 3 of the License, or
// (at your option) any later version.
// 
// MetaGeta is distributed in the hope that it will be useful,
// but WITHOUT ANY WARRANTY; without even the implied warranty of
// MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE. See the
// GNU General Public License for more details.
// 
// You should have received a copy of the GNU General Public License
// along with MetaGeta. If not, see <http://www.gnu.org/licenses/>.

#region

using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Text.RegularExpressions;
using System.Xml.Linq;
using log4net;
using MetaGeta.DataStore;

#endregion

namespace TranscodePlugin {
    public class TranscodePlugin : IMGFileActionPlugin, IMGPluginBase {
        public const string ConvertActionName = "Convert to iPod format";
        private static readonly ILog log = LogManager.GetLogger(MethodBase.GetCurrentMethod().DeclaringType);
        private readonly List<Preset> m_Presets = new List<Preset>();
        private MGDataStore m_DataStore;
        private string m_FfmpegLocation;

        private long m_ID;
        private string m_MencoderLocation;
        private string m_TranscodePresetName;

        public TranscodePlugin() {}

        #region "IMGPlugin"

        #region IMGFileActionPlugin Members

        public IEnumerable<string> GetActions() {
            return new string[] {ConvertActionName};
        }


        public void DoAction(string action, MGFile file, ProgressStatus progress) {
            switch (action) {
                case ConvertActionName:
                    Transcode(file, progress);
                    break;
                default:
                    throw new Exception("Unknown action.");
            }
        }

        #endregion

        #region IMGPluginBase Members

        public void Startup(MGDataStore dataStore, long id) {
            m_DataStore = dataStore;
            m_ID = id;
            LoadPresetsFromXml();
        }


        public void Shutdown() {}

        public string FriendlyName {
            get { return "TranscodePlugin"; }
        }

        public string UniqueName {
            get { return "TranscodePlugin"; }
        }

        public Version Version {
            get { return new Version(1, 0, 0, 0); }
        }

        public long PluginID {
            get { return m_ID; }
        }

        #endregion

        #endregion

        #region IMGPluginBase Members

        public event PropertyChangedEventHandler SettingChanged;

        #endregion

        private void LoadPresetsFromXml() {
            m_Presets.AddRange(from p in Presets.Elements()
                               select new Preset() {
                                                       Name = (string) p.Attribute("Name"),
                                                       Encoder = (string) p.Attribute("Encoder"),
                                                       MaxWidth = int.Parse((string) p.Attribute("MaxWidth")),
                                                       MaxHeight = int.Parse((string) p.Attribute("MaxHeight")),
                                                       CommandLine = p.Value
                                                   });
        }

        private string GetEncoderPath(string encoder) {
            if (encoder == "mencoder")
                return Environment.ExpandEnvironmentVariables(MencoderLocation);
            else
                return Environment.ExpandEnvironmentVariables(FfmpegLocation);
        }

        #region "Settings"

        [Settings("Transcode Preset Name", "iPhone-ffmpeg", SettingType.ShortText, "Preset")]
        public string TranscodePresetName {
            get { return m_TranscodePresetName; }
            set {
                if (value != m_TranscodePresetName) {
                    m_TranscodePresetName = value;
                    if (SettingChanged != null)
                        SettingChanged(this, new PropertyChangedEventArgs("TranscodePresetName"));
                }
            }
        }

        [Settings("mencoder Location", "%TOOLS%\\mplayer\\mencoder.exe", SettingType.File, "Programs")]
        public string MencoderLocation {
            get { return m_MencoderLocation; }
            set {
                if (value != m_MencoderLocation) {
                    m_MencoderLocation = value;
                    if (SettingChanged != null)
                        SettingChanged(this, new PropertyChangedEventArgs("MencoderLocation"));
                }
            }
        }

        [Settings("FFmpeg Location", "%TOOLS%\\ffmpeg\\ffmpeg.exe", SettingType.File, "Programs")]
        public string FfmpegLocation {
            get { return m_FfmpegLocation; }
            set {
                if (value != m_FfmpegLocation) {
                    m_FfmpegLocation = value;
                    if (SettingChanged != null)
                        SettingChanged(this, new PropertyChangedEventArgs("FfmpegLocation"));
                }
            }
        }

        #endregion

        #region "Transcoding"

        public void Transcode(MGFile file, ProgressStatus progress) {
            Preset preset = m_Presets.Find(pre => pre.Name == TranscodePresetName);
            log.InfoFormat("Encoding using preset {0}...", TranscodePresetName);

            var p = new Process();
            p.StartInfo = new ProcessStartInfo(GetEncoderPath(preset.Encoder));

            var outputPath = new Uri(file.FileName + ".iphone.mp4");
            p.StartInfo.Arguments = BuildCommandLine(preset, file, outputPath);
            log.InfoFormat("{0} {1}", p.StartInfo.FileName, p.StartInfo.Arguments);
            p.StartInfo.CreateNoWindow = true;
            p.StartInfo.UseShellExecute = false;

            p.StartInfo.RedirectStandardError = true;
            p.StartInfo.RedirectStandardOutput = true;

            int totalFrames = file.Tags.GetInt(TVShowDataStoreTemplate.FrameCount).Value;
            var statusParser = new EncoderStatusParser(preset.Encoder, totalFrames, progress);
            p.OutputDataReceived += statusParser.OutputHandler;
            p.ErrorDataReceived += statusParser.OutputHandler;

            log.Debug("Starting...");
            p.Start();

            p.BeginErrorReadLine();
            p.BeginOutputReadLine();

            p.WaitForExit();
            log.Debug("Done.");
            if (p.ExitCode != 0)
                log.ErrorFormat("Encoding failed with error code {0}.", p.ExitCode);
            else if (!File.Exists(outputPath.LocalPath))
                log.ErrorFormat("Encoding failed- output file doesn't exist: \"{0}\".", outputPath.LocalPath);
            else {
                MGFile newFile = m_DataStore.AddNewFile(outputPath);
                //Will also enqueue an import
                m_DataStore.DoAction(newFile, Mp4TagWriterPlugin.c_WriteTagsAction);
            }
        }

        private static string BuildCommandLine(Preset preset, MGFile file, Uri outputPath) {
            string cmd = Regex.Replace(preset.CommandLine, "\\s+", " ");
            cmd = cmd.Replace("%input%", file.FileName);
            cmd = cmd.Replace("%output%", outputPath.LocalPath);

            cmd = cmd.Replace("%video-bitrate%", (768 * 1024).ToString());
            cmd = cmd.Replace("%max-video-bitrate%", (1500 * 1024).ToString());
            cmd = cmd.Replace("%audio-bitrate%", (128 * 1024).ToString());

            int width = file.Tags.GetInt(TVShowDataStoreTemplate.VideoWidthPx).Value;
            int height = file.Tags.GetInt(TVShowDataStoreTemplate.VideoHeightPx).Value;
            double aspect = file.Tags.GetDouble(TVShowDataStoreTemplate.VideoDisplayAspectRatio).Value;

            var size = new Size(width, height);
            Size s = CalculateDimensions(ref size, preset, aspect);
            cmd = cmd.Replace("%width%", s.Width.ToString());
            cmd = cmd.Replace("%height%", s.Height.ToString());

            cmd = cmd.Replace("%fps%", CalculateFrameRate(file.Tags.GetDouble(TVShowDataStoreTemplate.FrameRate).Value).ToString());

            return cmd;
        }

        private static Size CalculateDimensions(ref Size s, Preset preset, double aspectRatio) {
            if (aspectRatio < 1.34 & aspectRatio >= 1.33) {
                aspectRatio = 4.0 / 3.0;
                //....
            } else if (aspectRatio < 1.78 & aspectRatio >= 1.77) {
                aspectRatio = 16.0 / 9.0;
                //....
            } else if (aspectRatio <= 2.4 & aspectRatio >= 2.35) {
                aspectRatio = 2.35;
                //....
            }

            if (s.Width > preset.MaxWidth || s.Height > preset.MaxHeight) {
                if (aspectRatio > (double) preset.MaxWidth / preset.MaxHeight) {
                    s.Height = (int) ((double) preset.MaxWidth / aspectRatio);
                    s.Width = preset.MaxWidth;
                } else {
                    s.Height = preset.MaxHeight;
                    s.Width = (int) ((double) preset.MaxHeight * aspectRatio);
                }
            }

            s.Width += s.Width % 2;
            s.Height += s.Height % 2;

            return s;
        }

        private static FrameRate CalculateFrameRate(double rate) {
            if (rate == 119.88)
                return new FrameRate(120000, 1001);
            else if (rate == 60)
                return new FrameRate(60);
            else if (rate == 59.94)
                return new FrameRate(60000, 1001);
            else if (rate == 29.97 || rate == 29)
                return new FrameRate(30000, 1001);
            else if (rate == 25)
                return new FrameRate(25);
            else if (rate == 23.976)
                return new FrameRate(24000, 1001);
            else if (rate == 7.992)
                return new FrameRate(8000, 1001);
            else
                throw new Exception(string.Format("Unknown frame rate: {0}.", rate));
        }

        #region Nested type: FrameRate

        private struct FrameRate {
            public decimal Denominator;
            public decimal Numerator;

            public FrameRate(int numerator, int denominator) {
                Numerator = numerator;
                Denominator = denominator;
            }

            public FrameRate(double numerator) {
                Numerator = (decimal) numerator;
                Denominator = 1;
            }

            public override string ToString() {
                if (Denominator == 1)
                    return Numerator.ToString();
                else
                    return string.Format("{0}/{1}", Numerator, Denominator);
            }
        }

        #endregion

        #region Nested type: Size

        private struct Size {
            public int Height;
            public int Width;

            public Size(int width, int height) {
                Width = width;
                Height = height;
            }
        }

        #endregion

        #endregion

        #region "Constants"

        private static readonly XElement Presets =
            XElement.Parse(
                @"
    <TranscodingPresets>
        <Preset Name=""iPod-mencoder"" Encoder=""mencoder"" MaxWidth=""480"" MaxHeight=""320"">
            ""%input%""
            -sws 9 -of lavf -lavfopts format=mp4 -vf scale=%width%:%height%,dsize=%width%:%height%,harddup -endpos %duration-seconds%
            -ovc x264 -x264encopts bitrate=%video-bitrate%:vbv_maxrate=%max-video-bitrate%:vbv_bufsize=2000:nocabac:me=umh:subq=6:frameref=6:trellis=1:level_idc=30:global_header:threads=4
            -oac faac -faacopts mpeg=4:object=2:br=%audio-bitrate%:raw -channels 2 -srate 48000 -o ""%output%""
        </Preset>
        <Preset Name=""iPhone-ffmpeg"" Encoder=""ffmpeg"" MaxWidth=""480"" MaxHeight=""320"">
            -i ""%input%"" -r %fps% -vcodec libx264 -b %video-bitrate% -s %width%x%height%
            -coder 0 -bf 0 -refs 1 -flags2 -wpred-dct8x8 -level 30 -threads 4
            -maxrate %max-video-bitrate% -bufsize 3000000 -ab %audio-bitrate% -acodec libfaac -ac 2 ""%output%""
        </Preset>
        <Preset Name=""iPhone-ffmpeg-30s"" Encoder=""ffmpeg"" MaxWidth=""480"" MaxHeight=""320"">
            -t 30 -ss 0 
            -i ""%input%"" -r %fps% -vcodec libx264 -b %video-bitrate% -s %width%x%height%
            -coder 0 -bf 0 -refs 1 -flags2 -wpred-dct8x8 -level 30 -threads 4
            -maxrate %max-video-bitrate% -bufsize 3000000 -ab %audio-bitrate% -acodec libfaac -ac 2 ""%output%""
        </Preset>
    </TranscodingPresets>
");

        #endregion
    }

    internal class EncoderStatusParser {
        private static readonly ILog log = LogManager.GetLogger(MethodBase.GetCurrentMethod().DeclaringType);

        private static Regex c_FfmpegParseRegex =
            new Regex(
                "\\s*frame= \\s* (\\d+) \\s* fps= \\s* (\\d+) \\s*  q= \\s* ([\\d.]+) \\s* size= \\s* (\\d+) \\s* kB \\s* time= \\s* ([\\d.]+) \\s* bitrate= \\s* ([\\d.]+) \\s*kbits/s\\s*",
                RegexOptions.IgnorePatternWhitespace);

        private static Regex c_MencoderParseRegex =
            new Regex(
                "\\s*Pos: \\s* (\\d*.?\\d*)s \\s* (\\d*)f \\s* \\(\\s*(\\d+)%\\) \\s* (\\d*.?\\d*)fps \\s* Trem: \\s* (\\d*.?\\d*)min \\s* (\\d*)mb \\s* A-V:(-?\\d*.?\\d*) \\s* \\[(\\d*):(\\d*)\\] \\s*",
                RegexOptions.IgnorePatternWhitespace);

        private readonly ProgressStatus m_Progress;
        private string m_EncoderName;
        private double m_EstimatedBitrate;

        private double m_EstimatedFileSizeMB;
        private long m_PositionFrames;
        private DateTime m_StartTime;
        private int m_TotalFrames;

        public EncoderStatusParser(string encoderName, int totalFrames, ProgressStatus progress) {
            m_EncoderName = encoderName;
            m_TotalFrames = totalFrames;
            m_StartTime = DateTime.Now;
            m_Progress = progress;
        }

        public long PositionFrames {
            get { return m_PositionFrames; }
        }

        public double PercentDone {
            get { return m_PositionFrames / m_TotalFrames; }
        }

        public double EstimatedBitrate {
            get { return m_EstimatedBitrate; }
        }

        public double EstimatedFileSizeMB {
            get { return m_EstimatedFileSizeMB; }
        }

        public TimeSpan TimeRemaining {
            get { return EncodingFps == 0 ? TimeSpan.MaxValue : new TimeSpan(0, 0, (int) ((m_TotalFrames - m_PositionFrames) / EncodingFps)); }
        }

        public double EncodingFps {
            get { return m_PositionFrames / (DateTime.Now - m_StartTime).TotalSeconds; }
        }

        public void OutputHandler(object sendingProcess, DataReceivedEventArgs outLine) {
            if (outLine.Data == null)
                return;
            log.Debug(outLine.Data);
            Parse(outLine.Data);

            m_Progress.ProgressPct = PercentDone;
            m_Progress.StatusMessage = string.Format("{0,6:#0.00}fps {1:###,##0}kbps", EncodingFps, EstimatedBitrate);
        }

        private void Parse(string line) {
            //Mencoder:
            //Pos:   6.5s    198f (11%) 27.84fps Trem:   0min   5mb  A-V:-0.004 [773:64]
            //ffmpeg:
            //frame=  920 fps=101 q=31.0 size=    1011kB time=14.91 bitrate= 555.2kbits/s
            if (m_EncoderName == "mencoder") {
                Match m = c_MencoderParseRegex.Match(line);
                if (m.Success) {
                    //PositionTime = New TimeSpan(0, 0, Integer.Parse(m.Groups(1).Value))
                    m_PositionFrames = long.Parse(m.Groups[2].Value);
                    //PositionPercent = Double.Parse(m.Groups(3).Value) / 100
                    //EncodingSpeedFps = Double.Parse(m.Groups(4).Value)
                    //5 = time remaining?
                    m_EstimatedFileSizeMB = double.Parse(m.Groups[6].Value);
                    //7 = a/v delay?
                    m_EstimatedBitrate = double.Parse(m.Groups[8].Value) + double.Parse(m.Groups[9].Value);
                } else {
                    //m_PositionFrames = -1
                }
            } else {
                Match m = c_FfmpegParseRegex.Match(line);
                if (m.Success) {
                    m_PositionFrames = long.Parse(m.Groups[1].Value);
                    //fps = Long.Parse(m.Groups(2).Value)
                    //quantizer = Long.Parse(m.Groups(3).Value)
                    m_EstimatedFileSizeMB = double.Parse(m.Groups[4].Value) / 1024.0;
                    //time = Long.Parse(m.Groups(5).Value)
                    m_EstimatedBitrate = double.Parse(m.Groups[6].Value);
                } else {
                    //m_PositionFrames = -1
                }
            }
            //log.InfoFormat("Status: {0,7:##0.00%} {1,6:#0.00}fps {2:HH:mm:ss} remaining ~{3:###,##0}kbps", PercentDone, EncodingFps, TimeRemaining, EstimatedBitrate)
        }
    }

    internal struct Preset {
        public string CommandLine;
        public string Encoder;
        public int MaxHeight;
        public int MaxWidth;
        public string Name;
    }
}