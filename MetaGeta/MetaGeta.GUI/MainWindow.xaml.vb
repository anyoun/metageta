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

        System.Diagnostics.PresentationTraceSources.SetTraceLevel(lvItems, PresentationTraceLevel.High)
        Me.DataContext = dsm
    End Sub

    Private Sub Window1_Closing(ByVal sender As Object, ByVal e As EventArgs) Handles Window1.Closed
        dsm.Shutdown()
    End Sub

    Private Sub lvItems_DataContextChanged(ByVal sender As System.Object, ByVal e As System.Windows.DependencyPropertyChangedEventArgs) Handles lvItems.DataContextChanged
        Dim grid = CType(lvItems.View, GridView)
        grid.Columns.Clear()
        Dim ds = CType(lvItems.DataContext, MGDataStore)
        If ds Is Nothing Then Return

        For Each t In ds.Template.GetColumnNames()
            Dim col = New GridViewColumn
            Dim b As New Binding()
            b.Converter = New MGFileConverter()
            b.ConverterParameter = t
            b.Mode = BindingMode.OneWay
            col.DisplayMemberBinding = b
            col.Header = t
            grid.Columns.Add(col)
        Next
    End Sub

    Sub RefreshAndImport()
        log.Info("Staring Importing...")

        ds.RefreshFileSources()

        For Each plugin As IMGTaggingPlugin In ds.TaggingPlugins
            Dim fp As New FileProgress(plugin.GetFriendlyName())
            Dim t As New System.Threading.Thread(AddressOf ShowWindow)
            t.SetApartmentState(System.Threading.ApartmentState.STA)
            t.Start(fp)
            plugin.Process(fp)
            t.Join()
        Next

        log.Info("Done")
    End Sub

    Sub ShowWindow(ByVal fp As Object)
        Dim window = New ImportProgressDisplay(CType(fp, FileProgress))
        window.ShowDialog()
    End Sub

#Region "Writing tags"

    Public Sub WriteAllTags(ByVal ds As MGDataStore)
        For Each file As MGFile In ds.Files
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

#End Region

#Region "Event handlers"

    Private Sub btnNew_Click(ByVal sender As System.Object, ByVal e As System.Windows.RoutedEventArgs) Handles btnNewDataStore.Click
        Dim template = New MetaGeta.DataStore.TVShowDataStoreTemplate()
        ds = dsm.NewDataStore("TV Shows", template)

        ds.AddTaggingPlugin(GetType(MetaGeta.MediaInfoPlugin.MediaInfoPlugin))
        ds.AddTaggingPlugin(GetType(MetaGeta.TVShowPlugin.EducatedGuessImporter))
        ds.AddTaggingPlugin(GetType(MetaGeta.TVDBPlugin.TVDBPlugin))

        ds.AddFileSourcePlugin(GetType(MetaGeta.DirectoryFileSourcePlugin.DirectoryFileSourcePlugin))

        ds.SetPluginSetting(ds.FileSourcePlugins.Single(), "DirectoriesToWatch", "F:\ipod\")
        ds.SetPluginSetting(ds.FileSourcePlugins.Single(), "Extensions", "mp4")
    End Sub

    Private Sub btnRemoveDataStore_Click(ByVal sender As System.Object, ByVal e As System.Windows.RoutedEventArgs) Handles btnRemoveDataStore.Click

    End Sub

    Private Sub btnImport_Click(ByVal sender As System.Object, ByVal e As System.Windows.RoutedEventArgs) Handles btnImport.Click
        Dim t As New System.Threading.Thread(AddressOf RefreshAndImport)
        t.SetApartmentState(System.Threading.ApartmentState.STA)
        t.Start()
    End Sub

#End Region

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
