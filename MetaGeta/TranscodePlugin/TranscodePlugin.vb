Imports System.Text
Imports System.Text.RegularExpressions
Imports System.IO
Imports MetaGeta.DataStore
Imports MetaGeta.Utilities
Imports System.Xml.Linq

'<Assembly: GlobalSetting("Transcode Plugin.Transcode Preset Name", "iPhone-ffmpeg", GlobalSettingType.ShortText)> 
'<Assembly: GlobalSetting("Transcode Plugin.mencoder Location", "%TOOLS%\mplayer\mencoder.exe", GlobalSettingType.File)> 
'<Assembly: GlobalSetting("Transcode Plugin.ffmpeg Location", "%TOOLS%\ffmpeg\ffmpeg.exe", GlobalSettingType.File)> 

Public Class TranscodePlugin
    Implements IMGFileActionPlugin, IMGPluginBase

    Private Shared ReadOnly log As log4net.ILog = log4net.LogManager.GetLogger(Reflection.MethodBase.GetCurrentMethod().DeclaringType)

    Private ReadOnly m_Presets As New List(Of Preset)
    Private m_DataStore As MGDataStore
    Private m_ID As Long

    Private m_TranscodePresetName As String
    Private m_MencoderLocation As String
    Private m_FfmpegLocation As String

    Public Sub New()

    End Sub

#Region "IMGPlugin"
    Public Sub Startup(ByVal dataStore As MetaGeta.DataStore.MGDataStore, ByVal id As Long) Implements MetaGeta.DataStore.IMGPluginBase.Startup
        m_DataStore = dataStore
        m_ID = id
        LoadPresetsFromXml()

        m_DataStore.SetPluginSetting(Me, "", "")
    End Sub

    Public Sub Shutdown() Implements MetaGeta.DataStore.IMGPluginBase.Shutdown

    End Sub

    Public Function GetFriendlyName() As String Implements MetaGeta.DataStore.IMGPluginBase.GetFriendlyName
        Return "TranscodePlugin"
    End Function

    Public Function GetUniqueName() As String Implements MetaGeta.DataStore.IMGPluginBase.GetUniqueName
        Return "TranscodePlugin"
    End Function

    Public Function GetVersion() As System.Version Implements MetaGeta.DataStore.IMGPluginBase.GetVersion
        Return New Version(1, 0, 0, 0)
    End Function

    Public ReadOnly Property PluginID() As Long Implements MetaGeta.DataStore.IMGPluginBase.PluginID
        Get
            Return m_ID
        End Get
    End Property

    Public Function GetActions() As IEnumerable(Of String) Implements IMGFileActionPlugin.GetActions
        Return New String() {ConvertActionName}
    End Function

    Public Sub DoAction(ByVal action As String, ByVal file As MetaGeta.DataStore.MGFile, ByVal progress As ProgressStatus) Implements MetaGeta.DataStore.IMGFileActionPlugin.DoAction

        Select Case action
            Case ConvertActionName
                Transcode(file, progress)
            Case Else
                Throw New Exception("Unknown action.")
        End Select
    End Sub
#End Region

    Private Sub LoadPresetsFromXml()
        m_Presets.AddRange(From p In Presets.Elements Select New Preset With { _
                           .Name = CType(p.Attribute("Name"), String), _
                           .Encoder = CType(p.Attribute("Encoder"), String), _
                           .MaxWidth = Integer.Parse(CType(p.Attribute("MaxWidth"), String)), _
                           .MaxHeight = Integer.Parse(CType(p.Attribute("MaxHeight"), String)), _
                           .CommandLine = p.Value})
    End Sub

#Region "Transcoding"

    Public Sub Transcode(ByVal file As MetaGeta.DataStore.MGFile, ByVal progress As ProgressStatus)
        Dim presetName = m_DataStore.GetGlobalSetting("Transcode Plugin.Transcode Preset Name")
        Dim preset = m_Presets.Find(Function(pre) pre.Name = presetName)
        log.InfoFormat("Encoding using preset {0}...", presetName)

        Dim p As New Process()
        p.StartInfo = New ProcessStartInfo(GetEncoderPath(preset.Encoder))

        Dim outputPath = New Uri(file.FileName + ".iphone.mp4")
        p.StartInfo.Arguments = BuildCommandLine(preset, file, outputPath)
        log.InfoFormat("{0} {1}", p.StartInfo.FileName, p.StartInfo.Arguments)
        p.StartInfo.CreateNoWindow = True
        p.StartInfo.UseShellExecute = False

        p.StartInfo.RedirectStandardError = True
        p.StartInfo.RedirectStandardOutput = True

        Dim totalFrames = Integer.Parse(file.GetTag(TVShowDataStoreTemplate.FrameCount))
        Dim statusParser As New EncoderStatusParser(preset.Encoder, CInt(totalFrames), progress)
        AddHandler p.OutputDataReceived, AddressOf statusParser.OutputHandler
        AddHandler p.ErrorDataReceived, AddressOf statusParser.OutputHandler

        log.Debug("Starting...")
        p.Start()

        p.BeginErrorReadLine()
        p.BeginOutputReadLine()

        p.WaitForExit()
        log.Debug("Done.")
        If p.ExitCode <> 0 Then
            log.ErrorFormat("Encoding failed with error code {0}.", p.ExitCode)
        ElseIf Not IO.File.Exists(outputPath.LocalPath) Then
            log.ErrorFormat("Encoding failed- output file doesn't exist: ""{0}"".", outputPath.LocalPath)
        Else
            Dim newFile = m_DataStore.CreateFile(outputPath)
            m_DataStore.EnqueueImportFile(newFile)
            m_DataStore.DoAction(newFile, Mp4TagWriterPlugin.c_WriteTagsAction)
        End If
    End Sub

    Private Shared Function BuildCommandLine(ByVal preset As Preset, ByVal file As MGFile, ByVal outputPath As Uri) As String
        Dim cmd = Regex.Replace(preset.CommandLine, "\s+", " ")
        cmd = cmd.Replace("%input%", file.FileName)
        cmd = cmd.Replace("%output%", outputPath.LocalPath)

        cmd = cmd.Replace("%video-bitrate%", (768 * 1024).ToString())
        cmd = cmd.Replace("%max-video-bitrate%", (1500 * 1024).ToString())
        cmd = cmd.Replace("%audio-bitrate%", (128 * 1024).ToString())

        Dim width = Integer.Parse(file.GetTag(TVShowDataStoreTemplate.VideoWidthPx))
        Dim height = Integer.Parse(file.GetTag(TVShowDataStoreTemplate.VideoHeightPx))
        If width > preset.MaxWidth OrElse height > preset.MaxHeight Then
            Dim aspect = Single.Parse(file.GetTag(TVShowDataStoreTemplate.VideoDisplayAspectRatio))
            If aspect > CDbl(preset.MaxWidth) / preset.MaxHeight Then
                height = CInt(CDbl(preset.MaxWidth) / aspect)
                width = preset.MaxWidth
            Else
                height = preset.MaxHeight
                width = CInt(CDbl(preset.MaxHeight) * aspect)
            End If
        End If
        cmd = cmd.Replace("%width%", width.ToString())
        cmd = cmd.Replace("%height%", height.ToString())
        Return cmd
    End Function

#End Region
#Region "Constants"
    Private Shared ReadOnly Presets As XElement = _
    <TranscodingPresets>
        <Preset Name="iPod-mencoder" Encoder="mencoder" MaxWidth="480" MaxHeight="320">
            "%input%"
            -sws 9 -of lavf -lavfopts format=mp4 -vf scale=%width%:%height%,dsize=%width%:%height%,harddup -endpos %duration-seconds%
            -ovc x264 -x264encopts bitrate=%video-bitrate%:vbv_maxrate=%max-video-bitrate%:vbv_bufsize=2000:nocabac:me=umh:subq=6:frameref=6:trellis=1:level_idc=30:global_header:threads=4
            -oac faac -faacopts mpeg=4:object=2:br=%audio-bitrate%:raw -channels 2 -srate 48000 -o "%output%"
        </Preset>
        <Preset Name="iPhone-ffmpeg" Encoder="ffmpeg" MaxWidth="480" MaxHeight="320">
            -i "%input%" -vcodec libx264 -b %video-bitrate% -s %width%x%height%
            -coder 0 -bf 0 -refs 1 -flags2 -wpred-dct8x8 -level 30 -threads 4
            -maxrate %max-video-bitrate% -bufsize 3000000 -ab %audio-bitrate% -acodec libfaac -ac 2 "%output%"
        </Preset>
        <Preset Name="iPhone-ffmpeg-30s" Encoder="ffmpeg" MaxWidth="480" MaxHeight="320">
            -t 30 -ss 0 
            -i "%input%" -vcodec libx264 -b %video-bitrate% -s %width%x%height%
            -coder 0 -bf 0 -refs 1 -flags2 -wpred-dct8x8 -level 30 -threads 4
            -maxrate %max-video-bitrate% -bufsize 3000000 -ab %audio-bitrate% -acodec libfaac -ac 2 "%output%"
        </Preset>
    </TranscodingPresets>

    Public Const ConvertActionName As String = "Convert to iPod format"
#End Region

    Private Function GetEncoderPath(ByVal encoder As String) As String
        If encoder = "mencoder" Then
            Return Environment.ExpandEnvironmentVariables(m_DataStore.GetGlobalSetting("Transcode Plugin.mencoder Location"))
        Else
            Return Environment.ExpandEnvironmentVariables(m_DataStore.GetGlobalSetting("Transcode Plugin.ffmpeg Location"))
        End If
    End Function

#Region "Settings"
    <Settings("Transcode Preset Name", "iPhone-ffmpeg", SettingType.ShortText, "Preset")> _
    Public Property TranscodePresetName() As String
        Get
            Return m_TranscodePresetName
        End Get
        Set(ByVal value As String)
            m_TranscodePresetName = value
        End Set
    End Property

    <Settings("mencoder Location", "%TOOLS%\mplayer\mencoder.exe", SettingType.File, "Programs")> _
    Public Property MencoderLocation() As String
        Get
            Return m_MencoderLocation
        End Get
        Set(ByVal value As String)
            m_MencoderLocation = value
        End Set
    End Property

    <Settings("FFmpeg Location", "%TOOLS%\ffmpeg\ffmpeg.exe", SettingType.File, "Programs")> _
    Public Property FfmpegLocation() As String
        Get
            Return m_FfmpegLocation
        End Get
        Set(ByVal value As String)
            m_FfmpegLocation = value
        End Set
    End Property
#End Region

End Class

Friend Class EncoderStatusParser
    Private Shared ReadOnly log As log4net.ILog = log4net.LogManager.GetLogger(Reflection.MethodBase.GetCurrentMethod().DeclaringType)
    Private Shared c_MencoderParseRegex As New Regex("\s*Pos: \s* (\d*.?\d*)s \s* (\d*)f \s* \(\s*(\d+)%\) \s* (\d*.?\d*)fps \s* Trem: \s* (\d*.?\d*)min \s* (\d*)mb \s* A-V:(-?\d*.?\d*) \s* \[(\d*):(\d*)\] \s*", RegexOptions.IgnorePatternWhitespace)
    Private Shared c_FfmpegParseRegex As New Regex("\s*frame= \s* (\d+) \s* fps= \s* (\d+) \s*  q= \s* ([\d.]+) \s* size= \s* (\d+) \s* kB \s* time= \s* ([\d.]+) \s* bitrate= \s* ([\d.]+) \s*kbits/s\s*", RegexOptions.IgnorePatternWhitespace)

    Private m_EncoderName As String
    Private m_TotalFrames As Integer
    Private ReadOnly m_Progress As ProgressStatus

    Private m_PositionFrames As Long
    Private m_StartTime As DateTime
    Private m_EstimatedBitrate As Double
    Private m_EstimatedFileSizeMB As Double

    Public Sub New(ByVal encoderName As String, ByVal totalFrames As Integer, ByVal progress As ProgressStatus)
        m_EncoderName = encoderName
        m_TotalFrames = totalFrames
        m_StartTime = DateTime.Now
        m_Progress = progress
    End Sub

    Public Sub OutputHandler(ByVal sendingProcess As Object, ByVal outLine As DataReceivedEventArgs)
        If outLine.Data Is Nothing Then Return
        Dim p = CType(sendingProcess, Process)
        log.DebugFormat("{0}: {1}", p.ProcessName, outLine.Data)
        Parse(outLine.Data)

        m_Progress.ProgressPct = PercentDone
        m_Progress.StatusMessage = String.Format("{0,6:#0.00}fps {1:###,##0}kbps", EncodingFps, EstimatedBitrate)
    End Sub

    Private Sub Parse(ByVal line As String)
        'Mencoder:
        'Pos:   6.5s    198f (11%) 27.84fps Trem:   0min   5mb  A-V:-0.004 [773:64]
        'ffmpeg:
        'frame=  920 fps=101 q=31.0 size=    1011kB time=14.91 bitrate= 555.2kbits/s
        If m_EncoderName = "mencoder" Then
            Dim m = c_MencoderParseRegex.Match(line)
            If m.Success Then
                'PositionTime = New TimeSpan(0, 0, Integer.Parse(m.Groups(1).Value))
                m_PositionFrames = Long.Parse(m.Groups(2).Value)
                'PositionPercent = Double.Parse(m.Groups(3).Value) / 100
                'EncodingSpeedFps = Double.Parse(m.Groups(4).Value)
                '5 = time remaining?
                m_EstimatedFileSizeMB = Double.Parse(m.Groups(6).Value)
                '7 = a/v delay?
                m_EstimatedBitrate = Double.Parse(m.Groups(8).Value) + Double.Parse(m.Groups(9).Value)
            Else
                'm_PositionFrames = -1
            End If
        Else
            Dim m = c_FfmpegParseRegex.Match(line)
            If m.Success Then
                m_PositionFrames = Long.Parse(m.Groups(1).Value)
                'fps = Long.Parse(m.Groups(2).Value)
                'quantizer = Long.Parse(m.Groups(3).Value)
                m_EstimatedFileSizeMB = Double.Parse(m.Groups(4).Value) / 1024.0
                'time = Long.Parse(m.Groups(5).Value)
                m_EstimatedBitrate = Double.Parse(m.Groups(6).Value)
            Else
                'm_PositionFrames = -1
            End If
        End If
        log.InfoFormat("Status: {0,7:##0.00%} {1,6:#0.00}fps {2:HH:mm:ss} remaining ~{3:###,##0}kbps", PercentDone, EncodingFps, TimeRemaining, EstimatedBitrate)
    End Sub

    Public ReadOnly Property PositionFrames() As Long
        Get
            Return m_PositionFrames
        End Get
    End Property

    Public ReadOnly Property PercentDone() As Double
        Get
            Return m_PositionFrames / m_TotalFrames
        End Get
    End Property

    Public ReadOnly Property EstimatedBitrate() As Double
        Get
            Return m_EstimatedBitrate
        End Get
    End Property

    Public ReadOnly Property EstimatedFileSizeMB() As Double
        Get
            Return m_EstimatedFileSizeMB
        End Get
    End Property

    Public ReadOnly Property TimeRemaining() As TimeSpan
        Get
            Return If(EncodingFps = 0, TimeSpan.MaxValue, New TimeSpan(0, 0, CInt((m_TotalFrames - m_PositionFrames) / EncodingFps)))
        End Get
    End Property

    Public ReadOnly Property EncodingFps() As Double
        Get
            Return m_PositionFrames / (DateTime.Now - m_StartTime).TotalSeconds
        End Get
    End Property
End Class

Friend Structure Preset
    Public Name As String
    Public Encoder As String
    Public CommandLine As String
    Public MaxWidth As Integer
    Public MaxHeight As Integer
End Structure