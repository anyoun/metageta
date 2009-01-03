Imports System.IO
Imports System.Text

Public Class ScriptInterface

    Public Sub AddTaggingPluginByName(ByVal ds As MGDataStore, ByVal assemblyQualifiedTypeName As String)
        Dim t = Type.GetType(assemblyQualifiedTypeName, True)
        Dim plugin = CType(Activator.CreateInstance(t), IMGTaggingPlugin)
        ds.AddTaggingPlugin(plugin)
    End Sub

    Public Sub AddDirectory(ByVal ds As MGDataStore, ByVal dir As String)
        If File.Exists(dir) Then
            ds.NewFile(New Uri(dir))
        ElseIf Directory.Exists(dir) Then
            For Each file In Directory.GetFiles(dir)
                ds.NewFile(New Uri(file))
            Next
        Else
            Throw New Exception(String.Format("Can't find path: {0}", dir))
        End If
    End Sub

    Public Sub RunTaggingPlugins(ByVal ds As MGDataStore)
        ds.RunTaggingPlugins()
    End Sub

    Public Function ToCsv(ByVal ds As MGDataStore) As String
        Dim sb As New StringBuilder()
        sb.AppendLine(Aggregate s In ds.Template.GetDimensionNames() Order By s Into JoinToCsv(s))
        For Each f In ds
            sb.AppendLine(Aggregate t In f.Tags() Order By t.Name Into JoinToCsv(t.Value))
        Next
        Return sb.ToString()
    End Function

    Public Sub WriteAllTags(ByVal ds As MGDataStore)
        For Each file As MGFile In ds
            WriteTags(file)
        Next
    End Sub

    Sub WriteTags(ByVal f As MGFile)
        WriteAtomicParsleyTag(f.Path, "TVShowName", f.Tags.Item(TVShowDataStoreTemplate.SeriesTitle).Value)
        WriteAtomicParsleyTag(f.Path, "Album", f.Tags.Item(TVShowDataStoreTemplate.SeriesTitle).Value)
        WriteAtomicParsleyTag(f.Path, "TVSeasonNum", f.Tags.Item(TVShowDataStoreTemplate.SeasonNumber).Value)
        WriteAtomicParsleyTag(f.Path, "TVEpisode", f.Tags.Item(TVShowDataStoreTemplate.EpisodeID).Value)
        WriteAtomicParsleyTag(f.Path, "TVEpisodeNum", f.Tags.Item(TVShowDataStoreTemplate.EpisodeNumber).Value)
        WriteAtomicParsleyTag(f.Path, "description", f.Tags.Item(TVShowDataStoreTemplate.EpisodeDescription).Value)
        If f.Tags.Item(TVShowDataStoreTemplate.EpisodeBanner).IsSet Then
            WriteAtomicParsleyTag(f.Path, "artwork", f.Tags.Item(TVShowDataStoreTemplate.EpisodeBanner).Value)
        End If
        WriteAtomicParsleyTag(f.Path, "stik", "TV Show")
        WriteAtomicParsleyTag(f.Path, "genre", "TV Shows")

        'WriteAtomicParsleyTag(f.Path, "title", f.Tags.Item(TVShowDataStoreTemplate.EpisodeTitle).Value)
        WriteAtomicParsleyTag(f.Path, "title", String.Format( _
          "{0} - s{1}e{2} - {3}", _
          f.Tags.Item(TVShowDataStoreTemplate.SeriesTitle).Value, _
          f.Tags.Item(TVShowDataStoreTemplate.SeasonNumber).Value, _
          f.Tags.Item(TVShowDataStoreTemplate.EpisodeNumber).Value, _
          f.Tags.Item(TVShowDataStoreTemplate.EpisodeTitle).Value _
            ))
    End Sub

    Private Sub WriteAtomicParsleyTag(ByVal path As Uri, ByVal apTagName As String, ByVal value As String)
        Dim p As New Process()
        p.StartInfo = New ProcessStartInfo(Environment.ExpandEnvironmentVariables("%TOOLS%\AtomicParsley\AtomicParsley.exe"))
        Dim sb As New StringBuilder()

        sb.AppendFormat(" ""{0}"" ", path.LocalPath)
        sb.AppendFormat(" --{0} ""{1}"" ", apTagName, value.Replace("""", """"""))
        sb.Append(" --overWrite ")
        p.StartInfo.Arguments = sb.ToString()

        p.Start()
        p.WaitForExit()
    End Sub

    Private Function CapLength(ByVal s As String) As String
        Return s.Substring(0, Math.Min(255, s.Length))
    End Function


End Class
