Imports System.Text
Imports System.Text.RegularExpressions
Imports System.Xml
Imports System.Xml.XPath
Imports System.IO
Imports MetaGeta.Utilities

Public Class TranscodePlugin

    Private ReadOnly m_Presets As New Dictionary(Of String, Preset)

    Public Sub New()
        Dim d As New XPathDocument(Path.Combine(My.Application.Info.DirectoryPath, "TranscodingPresets.xml"))
        Dim nav = d.CreateNavigator()
        For Each node As XPathNavigator In nav.Select("/TranscodingPresets/Preset")
            Dim p As New Preset
            p.Name = node.GetAttribute("Name", "")
            p.Encoder = node.GetAttribute("Encoder", "")
            p.CommandLine = node.Value

            m_Presets.Add(p.Name, p)
        Next
    End Sub

    Public Sub Transcode(ByVal source As Uri, ByVal destination As Uri, ByVal presetName As String)
        Dim p As New Process()
        p.StartInfo = New ProcessStartInfo(Environment.ExpandEnvironmentVariables("%TOOLS%\mplayer\mencoder.exe"))
        Dim sb As New StringBuilder()

        sb.AppendFormat(" ""{0}"" ", source.LocalPath)

        If False Then
            'Stops after 30 seconds
            sb.Append(" -endpos 30 ")
        End If

        Dim preset = m_Presets(presetName)
        sb.Append(Regex.Replace(preset.CommandLine, "\s+", " "))

        sb.AppendFormat(" -o ""{0}"" ", destination.LocalPath)

        Console.WriteLine("Encoding using mencoder preset {0}...", presetName)

        p.StartInfo.Arguments = sb.ToString()
        p.StartInfo.CreateNoWindow = True
        p.StartInfo.UseShellExecute = False

        p.StartInfo.RedirectStandardError = True
        p.StartInfo.RedirectStandardOutput = True

        AddHandler p.OutputDataReceived, AddressOf OutputHandler

        Console.WriteLine()
        Console.WriteLine()

        p.Start()

        p.BeginErrorReadLine()
        p.BeginOutputReadLine()

        p.WaitForExit()

        If p.ExitCode <> 0 Then
            Throw New Exception("mencoder failed!")
        End If

        Console.WriteLine()
    End Sub

    Private Shared Sub OutputHandler(ByVal sendingProcess As Object, ByVal outLine As DataReceivedEventArgs)

        If outLine.Data Is Nothing Then
            Return
        End If
        Dim p = CType(sendingProcess, Process)
        Dim s As New MencoderStatusLine(outLine.Data)

        If s.PositionPercent <> -1 Then
            StringHelpers.DrawProgressBar(s.PositionPercent, String.Format("{0,6:#0.00}fps {1:HH:mm:ss} remaining", s.EncodingSpeedFps, s.EstimatedTimeRemaining))
        End If
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

                If PositionPercent <> 0 AndAlso EncodingSpeedFps <> 0 Then
                    EstimatedTimeRemaining = New TimeSpan(0, 0, (PositionFrames / PositionPercent - PositionFrames) / EncodingSpeedFps)
                End If
            Else
                PositionPercent = -1
            End If
        End Sub

        Public ReadOnly PositionTime As TimeSpan
        Public ReadOnly PositionFrames As Long
        Public ReadOnly PositionPercent As Double
        Public ReadOnly EncodingSpeedFps As Double
        Public ReadOnly EstimatedVideoBitrate As Double
        Public ReadOnly EstimatedAudioBitrate As Double
        Public ReadOnly EstimatedFileSizeMB As Integer

        Public ReadOnly EstimatedTimeRemaining As TimeSpan
    End Structure

    Private Structure Preset
        Public Name As String
        Public Encoder As String
        Public CommandLine As String
    End Structure

End Class
