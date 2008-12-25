Imports MetaGeta.DataStore
Imports System.IO
Imports System.Text
Imports System.Xml
Imports MetaGeta.TVShowPlugin

Module Main

    Sub Main(ByVal args As String())
        Dim dsm As New DataStoreManager()
        Dim template = New MetaGeta.TVShowPlugin.TVShowDataStoreTemplate()
        dsm.NewDataStore("cli", template)
        Dim ds = dsm.DataStores(0)

        ds.AddTaggingPlugin(New MetaGeta.MediaInfoPlugin.MediaInfoPlugin)
        ds.AddTaggingPlugin(New MetaGeta.TVShowPlugin.EducatedGuessImporter)
        ds.AddTaggingPlugin(New MetaGeta.TVDBPlugin.TVDBPlugin)

        Dim path As Uri = New Uri(Environment.CurrentDirectory)
        If args.Count > 0 Then
            path = New Uri(args(0))
        Else

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

        'Dim names = From s In template.GetDimensionNames() Order By s Select s
        'Console.WriteLine(names.JoinToString(","))
        For Each f In ds
            'Console.WriteLine(Aggregate t In f.Tags() Order By t.Name Into JoinToCsv(t.Value))
            WriteTags(f)
        Next

        ds.Close()
    End Sub

    Sub ProcessConfigFile(ByVal configFilePath As Uri)
        Dim doc As New XmlDocument()
        doc.Load(configFilePath.LocalPath)

        For Each actionNode As XmlNode In doc.SelectNodes("/MetaGetaCliConfig/*")
            Select Case actionNode.Name
                Case "ReloadLibrary"
                    Dim libraryPath = actionNode.SelectSingleNode("path").InnerText

                Case "ImportLibrary"

                Case "AddPlugin"
                    Dim typeName = actionNode.SelectSingleNode("TypeName").InnerText

                Case "AddConvertedVersion"
                    Dim outputPath = actionNode.SelectSingleNode("output").InnerText
                    Dim format = actionNode.SelectSingleNode("format").InnerText

                Case "SetTag"
                    Dim tagName = actionNode.SelectSingleNode("name").InnerText
                    Dim tagValue = actionNode.SelectSingleNode("value").InnerText

                Case "Config"

                Case Else
                    Throw New Exception("Unknown action: " + actionNode.Name)
            End Select
        Next

    End Sub

    Sub ProcessFile(ByVal ds As MGDataStore, ByVal path As Uri)
        If System.IO.Path.GetExtension(path.LocalPath) = ".mp4" Then
            Console.WriteLine("Not transcoding: {0}", path.LocalPath)
            Console.WriteLine("Importing: {0}", path.LocalPath)
            ds.NewFile(path)
        Else
            Console.WriteLine("Transcoding: {0}", path.LocalPath)
            Dim newFile = ChangeExtension(path, "mp4")

            Dim t = New TranscodePlugin.TranscodePlugin()
            t.Transcode(path, newFile, "iPhone-HQ")

            Console.WriteLine("Importing: {0}", newFile.LocalPath)
            ds.NewFile(newFile)
        End If
    End Sub

    Function ChangeExtension(ByVal path As Uri, ByVal extension As String) As Uri
        Return New Uri(IO.Path.ChangeExtension(path.LocalPath, extension))
    End Function

    Sub WriteTags(ByVal f As MGFile)
        WriteAtomicParsleyTag(f.Path, "TVShowName", f.Tags.Item(TVShowDataStoreTemplate.SeriesTitle).Value)
        WriteAtomicParsleyTag(f.Path, "TVSeasonNum", f.Tags.Item(TVShowDataStoreTemplate.SeasonNumber).Value)
        WriteAtomicParsleyTag(f.Path, "TVEpisode", f.Tags.Item(TVShowDataStoreTemplate.EpisodeID).Value)
        WriteAtomicParsleyTag(f.Path, "title", f.Tags.Item(TVShowDataStoreTemplate.EpisodeTitle).Value)
        WriteAtomicParsleyTag(f.Path, "TVEpisodeNum", f.Tags.Item(TVShowDataStoreTemplate.EpisodeNumber).Value)
        WriteAtomicParsleyTag(f.Path, "description", f.Tags.Item(TVShowDataStoreTemplate.EpisodeDescription).Value)
        WriteAtomicParsleyTag(f.Path, "artwork", f.Tags.Item(TVShowDataStoreTemplate.EpisodeBanner).Value)
        WriteAtomicParsleyTag(f.Path, "stik", "TV Show")
        WriteAtomicParsleyTag(f.Path, "genre", "TV Shows")
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

    'http://www.thetvdb.com/api/BC8024C516DFDA3B/mirrors.xml

End Module
