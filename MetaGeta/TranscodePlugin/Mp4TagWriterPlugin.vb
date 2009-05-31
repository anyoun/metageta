Imports MetaGeta.DataStore
Imports System.Text
Imports MetaGeta.Utilities

Public Class Mp4TagWriterPlugin
    Implements IMGPluginBase, IMGFileActionPlugin

    Private Shared ReadOnly log As log4net.ILog = log4net.LogManager.GetLogger(Reflection.MethodBase.GetCurrentMethod().DeclaringType)

    Private m_DataStore As MGDataStore
    Private m_Id As Long

#Region "IMGPluginBase"
    Public Function GetFriendlyName() As String Implements MetaGeta.DataStore.IMGPluginBase.GetFriendlyName
        Return "MPEG-4 Tag Writer Plugin"
    End Function
    Public Function GetUniqueName() As String Implements MetaGeta.DataStore.IMGPluginBase.GetUniqueName
        Return "Mp4TagWriterPlugin"
    End Function
    Public Function GetVersion() As System.Version Implements MetaGeta.DataStore.IMGPluginBase.GetVersion
        Return New Version(1, 0, 0, 0)
    End Function
    Public ReadOnly Property PluginID() As Long Implements MetaGeta.DataStore.IMGPluginBase.PluginID
        Get
            Return m_Id
        End Get
    End Property

    Public Sub Startup(ByVal dataStore As MetaGeta.DataStore.MGDataStore, ByVal id As Long) Implements MetaGeta.DataStore.IMGPluginBase.Startup
        m_DataStore = dataStore
        m_Id = id
    End Sub

    Public Sub Shutdown() Implements MetaGeta.DataStore.IMGPluginBase.Shutdown

    End Sub
#End Region

    Public Function GetActions() As System.Collections.Generic.IEnumerable(Of String) Implements MetaGeta.DataStore.IMGFileActionPlugin.GetActions
        Return c_WriteTagsAction.SingleToEnumerable()
    End Function

    Public Sub DoAction(ByVal action As String, ByVal file As MetaGeta.DataStore.MGFile, ByVal progress As MetaGeta.DataStore.ProgressStatus) Implements MetaGeta.DataStore.IMGFileActionPlugin.DoAction
        If action <> c_WriteTagsAction Then Throw New ArgumentException()

        If file.GetTag(TVShowDataStoreTemplate.Format) <> "MPEG-4" Then
            progress.StatusMessage = "Not MPEG-4"
            Return
        End If

        WriteAtomicParsleyTag(file.FileName, "TVShowName", file.GetTag(TVShowDataStoreTemplate.SeriesTitle))
        progress.ProgressPct = 0.1
        WriteAtomicParsleyTag(file.FileName, "album", file.GetTag(TVShowDataStoreTemplate.SeriesTitle))
        progress.ProgressPct += 0.1
        WriteAtomicParsleyTag(file.FileName, "year", file.GetTag(TVShowDataStoreTemplate.EpisodeFirstAired))
        progress.ProgressPct += 0.1
        WriteAtomicParsleyTag(file.FileName, "TVSeasonNum", file.GetTag(TVShowDataStoreTemplate.SeasonNumber))
        progress.ProgressPct += 0.1
        WriteAtomicParsleyTag(file.FileName, "TVEpisode", file.GetTag(TVShowDataStoreTemplate.EpisodeID))
        progress.ProgressPct += 0.1
        WriteAtomicParsleyTag(file.FileName, "TVEpisodeNum", file.GetTag(TVShowDataStoreTemplate.EpisodeNumber))
        progress.ProgressPct += 0.1
        WriteAtomicParsleyTag(file.FileName, "description", file.GetTag(TVShowDataStoreTemplate.EpisodeDescription))
        If file.GetTag(TVShowDataStoreTemplate.EpisodeBanner) IsNot Nothing Then
            WriteAtomicParsleyTag(file.FileName, "artwork", file.GetTag(TVShowDataStoreTemplate.EpisodeBanner))
        End If
        progress.ProgressPct += 0.1
        WriteAtomicParsleyTag(file.FileName, "stik", "TV Show")
        progress.ProgressPct += 0.1
        WriteAtomicParsleyTag(file.FileName, "genre", "TV Shows")
        progress.ProgressPct += 0.1

        WriteAtomicParsleyTag(file.FileName, "title", file.Tags.Item(TVShowDataStoreTemplate.EpisodeTitle).Value)
        'WriteAtomicParsleyTag(file.FileName, "title", String.Format( _
        '  "{0} - s{1}e{2} - {3}", _
        '  file.GetTag(TVShowDataStoreTemplate.SeriesTitle), _
        '  file.GetTag(TVShowDataStoreTemplate.SeasonNumber), _
        '  file.GetTag(TVShowDataStoreTemplate.EpisodeNumber), _
        '  file.GetTag(TVShowDataStoreTemplate.EpisodeTitle) _
        '    ))
        progress.ProgressPct += 0.1
    End Sub

    Private Sub WriteAtomicParsleyTag(ByVal path As String, ByVal apTagName As String, ByVal value As String)
        If value Is Nothing Then
            log.WarnFormat("Can't write tag ""{0}"" for file ""{1}"" since the value is null.", apTagName, path)
            Return
        End If

        Dim p As New Process()
        p.StartInfo = New ProcessStartInfo(Environment.ExpandEnvironmentVariables("%TOOLS%\AtomicParsley\AtomicParsley.exe"))
        Dim sb As New StringBuilder()

        sb.AppendFormat(" ""{0}"" ", path)
        sb.AppendFormat(" --{0} ""{1}"" ", apTagName, CapLength(value).Replace("""", """"""))
        sb.Append(" --overWrite ")
        p.StartInfo.Arguments = sb.ToString()
        p.StartInfo.RedirectStandardError = True
        p.StartInfo.RedirectStandardOutput = True
        p.StartInfo.UseShellExecute = False
        p.StartInfo.CreateNoWindow = False
        p.StartInfo.ErrorDialog = False
        p.StartInfo.WindowStyle = ProcessWindowStyle.Hidden
        AddHandler p.OutputDataReceived, AddressOf OutputHandler
        AddHandler p.ErrorDataReceived, AddressOf OutputHandler
        p.Start()
        p.BeginErrorReadLine()
        p.BeginOutputReadLine()
        p.WaitForExit()
        If p.ExitCode <> 0 Then
            Throw New Exception(String.Format("AtomicParsley failed with exit code: {0}", p.ExitCode))
        End If
    End Sub

    Private Function CapLength(ByVal s As String) As String
        Return s.Substring(0, Math.Min(255, s.Length))
    End Function

    Public Const c_WriteTagsAction As String = "WriteTags"

    Private Sub OutputHandler(ByVal sender As Object, ByVal e As DataReceivedEventArgs)
        log.Debug(e.Data)
    End Sub
End Class
