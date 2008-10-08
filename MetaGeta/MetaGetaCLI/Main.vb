Imports MetaGeta.DataStore

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

        For Each file In System.IO.Directory.GetFiles(path.LocalPath)
            Console.Error.WriteLine("Importing: {0}", file)
            ds.NewFile(New Uri(file))
        Next

        Dim names = From s In template.GetDimensionNames() Order By s Select s
        Console.WriteLine(names.JoinToString(","))
        For Each f In ds
            Console.WriteLine(Aggregate t In f.Tags() Order By t.Name Into JoinToCsv(t.Value))
        Next

    End Sub
End Module
