Imports System.IO
Imports System.Text
Imports log4net.Config
Imports System.Reflection
Imports System.ComponentModel

Partial Public Class MainWindow
    Implements INotifyPropertyChanged
    Private Shared ReadOnly log As ILog = LogManager.GetLogger(MethodBase.GetCurrentMethod().DeclaringType)

    Public Sub New()
        DataStoreManager.IsInDesignMode = DesignerProperties.GetIsInDesignMode(Me)
        InitializeComponent()
        lbDataStores.SelectedIndex = -1
    End Sub

    Private Sub MainWindow_Loaded(ByVal sender As Object, ByVal e As RoutedEventArgs) Handles Window1.Loaded
        OnMyPropertyChanged("SelectedDataStoreColumnsView")
    End Sub

    Private Sub MainWindow_Closing(ByVal sender As Object, ByVal e As EventArgs) Handles Window1.Closing
        DataStoreManager.Shutdown()
    End Sub

    Private Sub RightHandGrid_DataContextChanged(ByVal sender As System.Object, ByVal e As System.Windows.DependencyPropertyChangedEventArgs) Handles RightHandGrid.DataContextChanged
        OnMyPropertyChanged("SelectedDataStoreColumnsView")
    End Sub

    Private ReadOnly Property DataStoreManager() As DataStoreManager
        Get
            Return CType(Me.DataContext, DataStoreManager)
        End Get
    End Property

    Private ReadOnly Property SelectedDataStore() As MGDataStore
        Get
            Return CType(lbDataStores.SelectedItem, MGDataStore)
        End Get
    End Property

    Public ReadOnly Property SelectedDataStoreColumnsView() As ViewBase
        Get
            Dim grid As New GridView()
            If Not SelectedDataStore Is Nothing Then
                grid.Columns.Clear()

                For Each t In SelectedDataStore.Template.GetColumnNames()
                    Dim col = New GridViewColumn
                    Dim b As New Binding()
                    b.Converter = New MGFileConverter()
                    b.ConverterParameter = t
                    b.Mode = BindingMode.OneWay
                    col.DisplayMemberBinding = b
                    col.Header = t
                    grid.Columns.Add(col)
                Next
            End If
            Return grid
        End Get
    End Property

    Sub RefreshAndImport(ByVal ds As Object)
        log.Info("Staring Importing...")

        CType(ds, MGDataStore).RefreshFileSources()

        log.Info("Done")
        MessageBox.Show("Import Complete", "Import")
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
        Dim ds = DataStoreManager.NewDataStore("TV Shows", template)

        ds.SetPluginSetting(ds.FileSourcePlugins.Single(), "DirectoriesToWatch", "F:\ipod\")
        ds.SetPluginSetting(ds.FileSourcePlugins.Single(), "Extensions", "mp4")
    End Sub

    Private Sub btnRemoveDataStore_Click(ByVal sender As System.Object, ByVal e As System.Windows.RoutedEventArgs) Handles btnRemoveDataStore.Click
        DataStoreManager.DataStores.Remove(SelectedDataStore)
    End Sub

    Private Sub btnImport_Click(ByVal sender As System.Object, ByVal e As System.Windows.RoutedEventArgs) Handles btnImport.Click
        Dim t As New System.Threading.Thread(AddressOf RefreshAndImport)
        t.SetApartmentState(System.Threading.ApartmentState.STA)
        t.Start(SelectedDataStore)
    End Sub

#End Region

    Private Sub OnMyPropertyChanged(ByVal name As String)
        RaiseEvent PropertyChanged(Me, New PropertyChangedEventArgs(name))
    End Sub
    Public Event PropertyChanged(ByVal sender As Object, ByVal e As System.ComponentModel.PropertyChangedEventArgs) Implements System.ComponentModel.INotifyPropertyChanged.PropertyChanged
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
