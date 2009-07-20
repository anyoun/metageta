Imports System.Text
Imports System.Text.RegularExpressions
Imports System.IO
Imports System.ComponentModel
Imports MetaGeta.DataStore
Imports MetaGeta.Utilities
Imports System.Xml.Linq

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

    Public ReadOnly Property FriendlyName() As String Implements IMGPluginBase.FriendlyName
        Get
            Return "TranscodePlugin"
        End Get
    End Property
    Public ReadOnly Property UniqueName() As String Implements IMGPluginBase.UniqueName
        Get
            Return "TranscodePlugin"
        End Get
    End Property
    Public ReadOnly Property Version() As Version Implements IMGPluginBase.Version
        Get
            Return New Version(1, 0, 0, 0)
        End Get
    End Property

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
        Dim preset = m_Presets.Find(Function(pre) pre.Name = TranscodePresetName)
        log.InfoFormat("Encoding using preset {0}...", TranscodePresetName)

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
            Dim newFile = m_DataStore.AddNewFile(outputPath) 'Will also enqueue an import
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
        Dim aspect = Double.Parse(file.GetTag(TVShowDataStoreTemplate.VideoDisplayAspectRatio))

        Dim s = CalculateDimensions(New Size(width, height), preset, aspect)
        cmd = cmd.Replace("%width%", s.Width.ToString())
        cmd = cmd.Replace("%height%", s.Height.ToString())

        cmd = cmd.Replace("%fps%", CalculateFrameRate(Double.Parse(file.GetTag(TVShowDataStoreTemplate.FrameRate))).ToString())

        Return cmd
    End Function

    Private Shared Function CalculateDimensions(ByRef s As Size, ByVal preset As Preset, ByVal aspectRatio As Double) As Size
        If aspectRatio < 1.34 And aspectRatio >= 1.33 Then
            aspectRatio = 4.0 / 3.0 '....
        ElseIf aspectRatio < 1.78 And aspectRatio >= 1.77 Then
            aspectRatio = 16.0 / 9.0 '....
        ElseIf aspectRatio <= 2.4 And aspectRatio >= 2.35 Then
            aspectRatio = 2.35 '....
        End If

        If s.Width > preset.MaxWidth OrElse s.Height > preset.MaxHeight Then
            If aspectRatio > CDbl(preset.MaxWidth) / preset.MaxHeight Then
                s.Height = CInt(CDbl(preset.MaxWidth) / aspectRatio)
                s.Width = preset.MaxWidth
            Else
                s.Height = preset.MaxHeight
                s.Width = CInt(CDbl(preset.MaxHeight) * aspectRatio)
            End If
        End If

        s.Width += s.Width Mod 2
        s.Height += s.Height Mod 2

        Return s
    End Function

    Private Structure Size
        Public Width As Integer
        Public Height As Integer

        Public Sub New(ByVal width As Integer, ByVal height As Integer)
            Me.Width = width
            Me.Height = height
        End Sub
    End Structure

    Private Shared Function CalculateFrameRate(ByVal rate As Double) As FrameRate
        Select Case rate
            Case 119.88
                Return New FrameRate(120000, 1001)
            Case 60
                Return New FrameRate(60)
            Case 59.94
                Return New FrameRate(60000, 1001)
            Case 29.97, 29
                Return New FrameRate(30000, 1001)
            Case 25
                Return New FrameRate(25)
            Case 23.976
                Return New FrameRate(24000, 1001)
            Case 7.992
                Return New FrameRate(8000, 1001)
            Case Else
                Throw New Exception(String.Format("Unknown frame rate: {0}.", rate))
        End Select
    End Function

    Private Structure FrameRate
        Public Numerator As Decimal
        Public Denominator As Decimal

        Public Sub New(ByVal numerator As Integer, ByVal denominator As Integer)
            Me.Numerator = numerator
            Me.Denominator = denominator
        End Sub
        Public Sub New(ByVal numerator As Double)
            Me.Numerator = CDec(numerator)
            Me.Denominator = 1
        End Sub

        Public Overrides Function ToString() As String
            If Denominator = 1 Then
                Return Numerator.ToString()
            Else
                Return String.Format("{0}/{1}", Numerator, Denominator)
            End If
        End Function
    End Structure


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
            -i "%input%" -r %fps% -vcodec libx264 -b %video-bitrate% -s %width%x%height%
            -coder 0 -bf 0 -refs 1 -flags2 -wpred-dct8x8 -level 30 -threads 4
            -maxrate %max-video-bitrate% -bufsize 3000000 -ab %audio-bitrate% -acodec libfaac -ac 2 "%output%"
        </Preset>
        <Preset Name="iPhone-ffmpeg-30s" Encoder="ffmpeg" MaxWidth="480" MaxHeight="320">
            -t 30 -ss 0 
            -i "%input%" -r %fps% -vcodec libx264 -b %video-bitrate% -s %width%x%height%
            -coder 0 -bf 0 -refs 1 -flags2 -wpred-dct8x8 -level 30 -threads 4
            -maxrate %max-video-bitrate% -bufsize 3000000 -ab %audio-bitrate% -acodec libfaac -ac 2 "%output%"
        </Preset>
    </TranscodingPresets>

    Public Const ConvertActionName As String = "Convert to iPod format"
#End Region

    Private Function GetEncoderPath(ByVal encoder As String) As String
        If encoder = "mencoder" Then
            Return Environment.ExpandEnvironmentVariables(MencoderLocation)
        Else
            Return Environment.ExpandEnvironmentVariables(FfmpegLocation)
        End If
    End Function

#Region "Settings"
    <Settings("Transcode Preset Name", "iPhone-ffmpeg", SettingType.ShortText, "Preset")> _
    Public Property TranscodePresetName() As String
        Get
            Return m_TranscodePresetName
        End Get
        Set(ByVal value As String)
            If value <> m_TranscodePresetName Then
                m_TranscodePresetName = value
                RaiseEvent SettingChanged(Me, New PropertyChangedEventArgs("TranscodePresetName"))
            End If
        End Set
    End Property

    <Settings("mencoder Location", "%TOOLS%\mplayer\mencoder.exe", SettingType.File, "Programs")> _
    Public Property MencoderLocation() As String
        Get
            Return m_MencoderLocation
        End Get
        Set(ByVal value As String)
            If value <> m_MencoderLocation Then
                m_MencoderLocation = value
                RaiseEvent SettingChanged(Me, New PropertyChangedEventArgs("MencoderLocation"))
            End If
        End Set
    End Property

    <Settings("FFmpeg Location", "%TOOLS%\ffmpeg\ffmpeg.exe", SettingType.File, "Programs")> _
    Public Property FfmpegLocation() As String
        Get
            Return m_FfmpegLocation
        End Get
        Set(ByVal value As String)
            If value <> m_FfmpegLocation Then
                m_FfmpegLocation = value
                RaiseEvent SettingChanged(Me, New PropertyChangedEventArgs("FfmpegLocation"))
            End If
        End Set
    End Property
#End Region

    Public Event SettingChanged(ByVal sender As Object, ByVal e As System.ComponentModel.PropertyChangedEventArgs) Implements MetaGeta.DataStore.IMGPluginBase.SettingChanged
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
        log.Debug(outLine.Data)
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
        'log.InfoFormat("Status: {0,7:##0.00%} {1,6:#0.00}fps {2:HH:mm:ss} remaining ~{3:###,##0}kbps", PercentDone, EncodingFps, TimeRemaining, EstimatedBitrate)
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