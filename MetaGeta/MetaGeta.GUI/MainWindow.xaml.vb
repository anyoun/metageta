Imports System.IO
Imports System.Text
Imports log4net.Config
Imports System.Reflection

Partial Public Class MainWindow
    Private Shared ReadOnly log As ILog = LogManager.GetLogger(MethodBase.GetCurrentMethod().DeclaringType)

    Private ReadOnly dsm As New DataStoreManager()
    Private ds As MGDataStore

    Private Sub Window1_Loaded(ByVal sender As Object, ByVal e As RoutedEventArgs) Handles Window1.Loaded
        dsm.Startup()
        If dsm.DataStores.Count > 0 Then
            ds = dsm.DataStores(0)
            Display()
        End If
    End Sub

    Private Sub Window1_Closing(ByVal sender As Object, ByVal e As EventArgs) Handles Window1.Closed
        dsm.Shutdown()
    End Sub

    Private Sub btnImport_Click(ByVal sender As System.Object, ByVal e As System.Windows.RoutedEventArgs) Handles btnImport.Click
        Dim t As New System.Threading.Thread(AddressOf ImportTagAndWrite)
        t.SetApartmentState(System.Threading.ApartmentState.STA)
        t.Start()
    End Sub

    Sub ImportTagAndWrite()
        log.Info("Staring Importing...")
        Dim template = New MetaGeta.DataStore.TVShowDataStoreTemplate()
        ds = dsm.NewDataStore("TV Shows", template)

        'ds.AddTaggingPlugin("MetaGeta.MediaInfoPlugin.MediaInfoPlugin, MediaInfoPlugin")
        ds.AddTaggingPlugin("MetaGeta.TVShowPlugin.EducatedGuessImporter, TVShowPlugin")
        ds.AddTaggingPlugin("MetaGeta.TVDBPlugin.TVDBPlugin, TVDBPlugin")

        ds.AddDirectory("F:\ipod\")

        For Each plugin As IMGTaggingPlugin In ds.Plugins
            Dim fp As New FileProgress(plugin.GetType().Name)
            Dim t As New System.Threading.Thread(AddressOf ShowWindow)
            t.SetApartmentState(System.Threading.ApartmentState.STA)
            t.Start(fp)
            plugin.Process(fp)
            t.Join()
        Next

        'WriteAllTags(ds)
        Dispatcher.Invoke(Windows.Threading.DispatcherPriority.Normal, New System.Threading.ThreadStart(AddressOf Display))
        log.Info("Done")
    End Sub

    Sub ShowWindow(ByVal fp As Object)
        Dim window = New ImportProgressDisplay(CType(fp, FileProgress))
        window.ShowDialog()
    End Sub

    Sub Display()
        System.Diagnostics.PresentationTraceSources.SetTraceLevel(lvItems, PresentationTraceLevel.High)
        Dim grid = CType(lvItems.View, GridView)
        For Each t In ds.Template.GetDimensionNames()
            Dim col = New GridViewColumn
            Dim b As New Binding()
            b.Converter = New MGFileConverter()
            b.ConverterParameter = t
            b.Mode = BindingMode.OneWay
            col.DisplayMemberBinding = b
            col.Header = t
            grid.Columns.Add(col)
        Next
        lvItems.ItemsSource = ds
    End Sub

    Public Sub WriteAllTags(ByVal ds As MGDataStore)
        For Each file As MGFile In ds
            WriteTags(file)
        Next
    End Sub

    Sub WriteTags(ByVal f As MGFile)
        WriteAtomicParsleyTag(f.FileName, "TVShowName", f.GetTag(TVShowDataStoreTemplate.SeriesTitle))
        WriteAtomicParsleyTag(f.FileName, "Album", f.GetTag(TVShowDataStoreTemplate.SeriesTitle))
        WriteAtomicParsleyTag(f.FileName, "TVSeasonNum", f.GetTag(TVShowDataStoreTemplate.SeasonNumber))
        WriteAtomicParsleyTag(f.FileName, "TVEpisode", f.GetTag(TVShowDataStoreTemplate.EpisodeID))
        WriteAtomicParsleyTag(f.FileName, "TVEpisodeNum", f.GetTag(TVShowDataStoreTemplate.EpisodeNumber))
        WriteAtomicParsleyTag(f.FileName, "description", f.GetTag(TVShowDataStoreTemplate.EpisodeDescription))
        If f.GetTag(TVShowDataStoreTemplate.EpisodeBanner) IsNot Nothing Then
            WriteAtomicParsleyTag(f.FileName, "artwork", f.GetTag(TVShowDataStoreTemplate.EpisodeBanner))
        End If
        WriteAtomicParsleyTag(f.FileName, "stik", "TV Show")
        WriteAtomicParsleyTag(f.FileName, "genre", "TV Shows")

        'WriteAtomicParsleyTag(f.Path, "title", f.Tags.Item(TVShowDataStoreTemplate.EpisodeTitle).Value)
        WriteAtomicParsleyTag(f.FileName, "title", String.Format( _
          "{0} - s{1}e{2} - {3}", _
          f.GetTag(TVShowDataStoreTemplate.SeriesTitle), _
          f.GetTag(TVShowDataStoreTemplate.SeasonNumber), _
          f.GetTag(TVShowDataStoreTemplate.EpisodeNumber), _
          f.GetTag(TVShowDataStoreTemplate.EpisodeTitle) _
            ))
    End Sub

    Private Sub WriteAtomicParsleyTag(ByVal path As String, ByVal apTagName As String, ByVal value As String)
        Dim p As New Process()
        p.StartInfo = New ProcessStartInfo(Environment.ExpandEnvironmentVariables("%TOOLS%\AtomicParsley\AtomicParsley.exe"))
        Dim sb As New StringBuilder()

        sb.AppendFormat(" ""{0}"" ", path)
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

<ValueConversion(GetType(MGFile), GetType(String), ParameterType:=GetType(String))> _
Public Class MGFileConverter
    Implements IValueConverter

    Public Function Convert(ByVal value As Object, ByVal targetType As System.Type, ByVal parameter As Object, ByVal culture As System.Globalization.CultureInfo) As Object Implements System.Windows.Data.IValueConverter.Convert
        Dim file As MGFile = CType(value, MGFile)
        Return file.GetTag(CType(parameter, String))
    End Function

    Public Function ConvertBack(ByVal value As Object, ByVal targetType As System.Type, ByVal parameter As Object, ByVal culture As System.Globalization.CultureInfo) As Object Implements System.Windows.Data.IValueConverter.ConvertBack
        Throw New NotImplementedException()
    End Function
End Class
