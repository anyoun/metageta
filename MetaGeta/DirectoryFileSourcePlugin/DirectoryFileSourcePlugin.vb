Imports System.ComponentModel
Imports System.Collections.ObjectModel

Public Class DirectoryFileSourcePlugin
    Implements IMGFileSourcePlugin, IMGPluginBase

    Private Shared ReadOnly log As log4net.ILog = log4net.LogManager.GetLogger(Reflection.MethodBase.GetCurrentMethod().DeclaringType)

    Private m_DataStore As MGDataStore
    Private m_ID As Long

    Private m_DirectoriesToWatch As ReadOnlyCollection(Of String)
    Private m_Extensions As ReadOnlyCollection(Of String)

    Public ReadOnly Property ID() As Long Implements IMGPluginBase.PluginID
        Get
            Return m_ID
        End Get
    End Property

    Public ReadOnly Property FriendlyName() As String Implements DataStore.IMGPluginBase.FriendlyName
        Get
            Return "Directory File Source Plugin"
        End Get
    End Property
    Public ReadOnly Property UniqueName() As String Implements DataStore.IMGPluginBase.UniqueName
        Get
            Return "DirectoryFileSourcePlugin"
        End Get
    End Property
    Public ReadOnly Property Version() As Version Implements DataStore.IMGPluginBase.Version
        Get
            Return New Version(1, 0, 0, 0)
        End Get
    End Property

    Public Sub Startup(ByVal dataStore As MGDataStore, ByVal id As Long) Implements IMGPluginBase.Startup
        m_DataStore = dataStore
    End Sub
    Public Sub Shutdown() Implements IMGPluginBase.Shutdown

    End Sub

    Public Function GetFilesToAdd() As ICollection(Of Uri) Implements IMGFileSourcePlugin.GetFilesToAdd
        Dim fileNameToFileDict = New Dictionary(Of String, MGFile)
        For Each t In m_DataStore.GetAllTagOnFiles(MGFile.FileNameKey)
            fileNameToFileDict(New Uri(t.First.Value).LocalPath) = t.Second
        Next

        Dim newFiles As New List(Of Uri)

        For Each fi In GetAllFilesInWatchedDirectories()
            Dim mgFile As MGFile = Nothing
            If fileNameToFileDict.TryGetValue(fi.FullName, mgFile) Then
                'Already exits
                log.DebugFormat("File ""{0}"" already exists.", fi.FullName)
            Else
                'New files
                log.DebugFormat("New files: ""{0}"".", fi.FullName)
                newFiles.Add(New Uri(fi.FullName))
            End If
        Next

        Return newFiles
    End Function

    Private Function GetAllFilesInWatchedDirectories() As ICollection(Of FileInfo)
        Dim packedDirectories = m_DataStore.GetPluginSetting(Me, "DirectoriesToWatch")
        If packedDirectories Is Nothing Then packedDirectories = ""
        Dim directories = packedDirectories.Split(";"c)
        Dim packedExtensions = m_DataStore.GetPluginSetting(Me, "Extensions")
        If packedExtensions Is Nothing Then packedExtensions = ""
        Dim extensions = From s In packedExtensions.Split(";"c) Select "." + s
        Dim extensionsLookup As New LookupSet(Of String)(extensions)
        Dim files As New List(Of FileInfo)
        For Each d In directories
            files.AddRange(New DirectoryInfo(d).GetFiles("*", SearchOption.AllDirectories))
        Next
        files.RemoveAll(Function(f) Not extensionsLookup.Contains(Path.GetExtension(f.FullName)))
        Return files
    End Function

    <Settings("Directories To Watch", New String() {}, SettingType.DirectoryList, "Locations")> _
    Public Property DirectoriesToWatch() As ReadOnlyCollection(Of String)
        Get
            Return m_DirectoriesToWatch
        End Get
        Set(ByVal value As ReadOnlyCollection(Of String))
            If value IsNot m_DirectoriesToWatch Then
                m_DirectoriesToWatch = value
                RaiseEvent SettingChanged(Me, New PropertyChangedEventArgs("DirectoriesToWatch"))
            End If
        End Set
    End Property

    <Settings("Extensions To Watch", New String() {}, SettingType.ExtensionList, "Locations")> _
    Public Property Extensions() As ReadOnlyCollection(Of String)
        Get
            Return m_Extensions
        End Get
        Set(ByVal value As ReadOnlyCollection(Of String))
            If value IsNot m_Extensions Then
                m_Extensions = value
                RaiseEvent SettingChanged(Me, New PropertyChangedEventArgs("Extensions"))
            End If
        End Set
    End Property

    Public Event SettingChanged(ByVal sender As Object, ByVal e As PropertyChangedEventArgs) Implements DataStore.IMGPluginBase.SettingChanged
End Class
