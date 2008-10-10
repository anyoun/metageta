Imports MetaGeta.DataStore
Imports System.IO
Imports System.Text

Module Main

    Sub Main(ByVal args As String())

        Dim dsm As New DataStoreManager()
        Dim template = New MetaGeta.TVShowPlugin.TVShowDataStoreTemplate()
        dsm.NewDataStore("cli", template)
        Dim ds = dsm.DataStores(0)

        ds.AddTaggingPlugin(New MetaGeta.MediaInfoPlugin.MediaInfoPlugin)
        ds.AddTaggingPlugin(New MetaGeta.TVShowPlugin.EducatedGuessImporter)

        Dim path As Uri = New Uri(Environment.CurrentDirectory)
        If args.Count > 0 Then
            path = New Uri(args(0))
        End If

        If File.Exists(path.LocalPath) Then
            ProcessFile(ds, path)
        ElseIf Directory.Exists(path.LocalPath) Then
            For Each file In Directory.GetFiles(path.LocalPath)
                ProcessFile(ds, New Uri(file))
            Next
        Else
            Console.WriteLine("Can't find path: {0}", path.LocalPath)
        End If

        Dim names = From s In template.GetDimensionNames() Order By s Select s
        Console.WriteLine(names.JoinToString(","))
        For Each f In ds
            Console.WriteLine(Aggregate t In f.Tags() Order By t.Name Into JoinToCsv(t.Value))
            WriteTags(f)
        Next
    End Sub

    Sub ProcessFile(ByVal ds As MGDataStore, ByVal path As Uri)
        Console.WriteLine("Transcoding: {0}", path.LocalPath)
        Dim newFile = ChangeExtension(path, "mp4")
        TranscodePlugin.TranscodePlugin.TranscodeMencoder(path, newFile)
        Console.WriteLine("Importing: {0}", newFile.LocalPath)
        ds.NewFile(newFile)
    End Sub

    Function ChangeExtension(ByVal path As Uri, ByVal extension As String) As Uri
        Return New Uri(IO.Path.ChangeExtension(path.LocalPath, extension))
    End Function

    Sub WriteTags(ByVal f As MGFile)
        Dim p As New Process()
        p.StartInfo = New ProcessStartInfo("F:\src\MetaGeta\tools\AtomicParsley\AtomicParsley.exe")
        Dim sb As New StringBuilder()

        sb.AppendFormat(" ""{0}"" ", f.Path.LocalPath)

        sb.AppendFormat(" --TVShowName ""{0}"" ", f.Tags.Item(MetaGeta.TVShowPlugin.TVShowDataStoreTemplate.SeriesTitle).Value)
        sb.AppendFormat(" --TVSeasonNum ""{0}"" ", f.Tags.Item(MetaGeta.TVShowPlugin.TVShowDataStoreTemplate.SeasonNumber).Value)
        'sb.AppendFormat(" --TVEpisode ""{0}"" ", f.Tags.Item(MetaGeta.TVShowPlugin.TVShowDataStoreTemplate.EpisodeTitle).Value)
        sb.AppendFormat(" --title ""{0}"" ", f.Tags.Item(MetaGeta.TVShowPlugin.TVShowDataStoreTemplate.EpisodeTitle).Value)
        sb.AppendFormat(" --TVEpisodeNum ""{0}"" ", f.Tags.Item(MetaGeta.TVShowPlugin.TVShowDataStoreTemplate.EpisodeNumber).Value)

        sb.Append(" --stik ""TV Show"" --genre ""TV Shows"" ")
        sb.Append(" --overWrite ")

        Console.WriteLine(sb.ToString())
        p.StartInfo.Arguments = sb.ToString()

        p.Start()
        p.WaitForExit()
    End Sub

    'http://www.thetvdb.com/api/BC8024C516DFDA3B/mirrors.xml

End Module
