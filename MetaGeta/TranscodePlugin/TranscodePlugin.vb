Imports System.Text
Imports System.Text.RegularExpressions

Public Class TranscodePlugin

    Public Shared Sub Transcodex264(ByVal source As Uri, ByVal destination As Uri, ByVal profileFile As Uri)

        'Dim xs As New Xml.Serialization.XmlSerializer(GetType(GenericProfileOfx264Settings))
        'Dim profile As GenericProfileOfx264Settings

        'Using Str As New IO.StreamReader(profileFile.LocalPath)
        '   profile = xs.Deserialize(Str)
        'End Using

        Dim p As New Process()
        p.StartInfo = New ProcessStartInfo("f:\src\MetaGeta\tools\x264\x264.exe")
        Dim sb As New StringBuilder()

        sb.Append(" --bitrate 750 ")
        sb.Append(" --level 3 --no-cabac --partitions p8x8,b8x8,i4x4 --vbv-bufsize 10000 --vbv-maxrate 10000 ")
        sb.Append(" --threads auto --thread-input --progress --no-psnr --no-ssim ")
        sb.AppendFormat(" --output ""{0}"" ""{1}"" ", destination.LocalPath, source.LocalPath)

        Console.WriteLine(sb.ToString())
        p.StartInfo.Arguments = sb.ToString()
        p.StartInfo.CreateNoWindow = True
        p.StartInfo.UseShellExecute = False

        p.StartInfo.RedirectStandardError = True
        p.StartInfo.RedirectStandardOutput = True

        AddHandler p.OutputDataReceived, AddressOf OutputHandler

        p.Start()

        p.BeginErrorReadLine()
        p.BeginOutputReadLine()

        'Console.WriteLine(s)

        p.WaitForExit()
    End Sub

    Public Shared Sub TranscodeMencoder(ByVal source As Uri, ByVal destination As Uri)
        Dim p As New Process()
        p.StartInfo = New ProcessStartInfo("F:\src\MetaGeta\tools\mplayer\mencoder.exe")
        Dim sb As New StringBuilder()

        Dim videoBitrate = 250
        Dim audioBitrate = 64
        Dim maxWidth = 320

        sb.AppendFormat(" ""{0}"" ", source.LocalPath)
        sb.Append(" -endpos 30 ")
        'sb.AppendFormat(" -sws 9 -of lavf -lavfopts format=mp4 -vf dsize={0}:-2,harddup ", maxWidth)
        sb.AppendFormat(" -sws 9 -of lavf -lavfopts format=mp4 -vf scale=320:-2,dsize=320:-2,harddup ")
        sb.AppendFormat("-ovc x264 -x264encopts bitrate={0}:vbv_maxrate=1500:vbv_bufsize=2000:nocabac:me=umh:subq=6:frameref=6:trellis=1:level_idc=30:global_header:threads=2", videoBitrate)
        sb.AppendFormat(" -oac faac -faacopts mpeg=4:object=2:br={0}:raw -channels 2 -srate 48000 ", audioBitrate)
        sb.AppendFormat(" -o ""{0}"" ", destination.LocalPath)

        Console.WriteLine(sb.ToString())
        p.StartInfo.Arguments = sb.ToString()
        p.StartInfo.CreateNoWindow = True
        p.StartInfo.UseShellExecute = False

        p.StartInfo.RedirectStandardError = True
        p.StartInfo.RedirectStandardOutput = True

        AddHandler p.OutputDataReceived, AddressOf OutputHandler

        p.Start()

        p.BeginErrorReadLine()
        p.BeginOutputReadLine()

        p.WaitForExit()
    End Sub

    Private Shared Sub OutputHandler(ByVal sendingProcess As Object, ByVal outLine As DataReceivedEventArgs)

        If outLine.Data Is Nothing Then
            Return
        End If
        Dim p = CType(sendingProcess, Process)
        Dim s As New MencoderStatusLine(outLine.Data)

        Console.WriteLine("{0}: {1} {2:00%}", p.ProcessName, outLine.Data, s.PositionPercent)
    End Sub

    Private Structure MencoderStatusLine
        Private Shared c_ParseRegex As New Regex("\s*Pos: \s* (\d*.?\d*)s \s* (\d*)f \s* \(\s*(\d+)%\) \s* (\d*.?\d*)fps \s* Trem: \s* (\d*.?\d*)min \s* (\d*)mb \s* A-V:(-?\d*.?\d*) \s* \[(\d*):(\d*)\] \s*", RegexOptions.IgnorePatternWhitespace)

        Public Sub New(ByVal line As String)
            'Pos:   6.5s    198f (11%) 27.84fps Trem:   0min   5mb  A-V:-0.004 [773:64]
            Dim m = c_ParseRegex.Match(line)
            If m.Success Then
                PositionTime = New TimeSpan(0, 0, Double.Parse(m.Groups(1).Value))
                PositionFrames = Long.Parse(m.Groups(2).Value)
                PositionPercent = Double.Parse(m.Groups(3).Value) / 100
                EncodingSpeedFps = Double.Parse(m.Groups(4).Value)
                '5 = time remaining?
                EstimatedFileSizeMB = Double.Parse(m.Groups(6).Value)
                '7 = a/v delay?
                EstimatedVideoBitrate = Double.Parse(m.Groups(8).Value)
                EstimatedAudioBitrate = Double.Parse(m.Groups(9).Value)
            End If
        End Sub

        Public ReadOnly PositionTime As TimeSpan
        Public ReadOnly PositionFrames As Long
        Public ReadOnly PositionPercent As Double
        Public ReadOnly EncodingSpeedFps As Double
        Public ReadOnly EstimatedVideoBitrate As Double
        Public ReadOnly EstimatedAudioBitrate As Double
        Public ReadOnly EstimatedFileSizeMB As Integer
    End Structure

End Class
