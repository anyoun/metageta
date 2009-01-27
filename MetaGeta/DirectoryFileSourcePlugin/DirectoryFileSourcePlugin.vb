Public Class DirectoryFileSourcePlugin
    Implements IMGFileSourcePlugin

    Private Shared ReadOnly log As log4net.ILog = log4net.LogManager.GetLogger(Reflection.MethodBase.GetCurrentMethod().DeclaringType)

    Private m_DataStore As MGDataStore

    Public Function GetFriendlyName() As String Implements DataStore.IMGPluginBase.GetFriendlyName
        Return "Directory File Source Plugin"
    End Function
    Public Function GetUniqueName() As String Implements DataStore.IMGPluginBase.GetUniqueName
        Return "DirectoryFileSourcePlugin"
    End Function
    Public Function GetVersion() As System.Version Implements DataStore.IMGPluginBase.GetVersion
        Return New Version(1, 0, 0, 0)
    End Function

    Public Sub Startup(ByVal dataStore As MGDataStore) Implements IMGPluginBase.Startup
        m_DataStore = dataStore
    End Sub
    Public Sub Shutdown() Implements IMGPluginBase.Shutdown

    End Sub

    Public Function GetFilesToAdd() As ICollection(Of Uri) Implements IMGFileSourcePlugin.GetFilesToAdd
        Dim fileNameToFileDict = New Dictionary(Of String, MGFile)
        For Each t In m_DataStore.GetAllTagOnFiles(MGFile.FileNameKey)
            fileNameToFileDict(t.First.Value) = t.Second
        Next

        Dim newFiles As New List(Of Uri)

        For Each fi In GetAllFilesInWatchedDirectories()
            Dim mgFile As MGFile
            If fileNameToFileDict.TryGetValue(fi.FullName, mgFile) Then
                'Already exits
                log.DebugFormat("File ""{0}"" already exists.")
            Else
                'New files
                log.DebugFormat("New files: ""{0}"".", fi.FullName)
                newFiles.Add(New Uri(fi.FullName))
            End If
        Next

        Return newFiles
    End Function

    Private Function GetAllFilesInWatchedDirectories() As ICollection(Of FileInfo)
        Dim directories = m_DataStore.GetPluginSetting(Me, "DirectoriesToWatch").Split(";"c)
        Dim extensions = From s In m_DataStore.GetPluginSetting(Me, "Extensions").Split(";"c) Select "." + s
        Dim extensionsLookup As New LookupSet(Of String)(extensions)
        Dim files As New List(Of FileInfo)
        For Each d In directories
            files.AddRange(New DirectoryInfo(d).GetFiles("*", SearchOption.AllDirectories))
        Next
        files.RemoveAll(Function(f) Not extensionsLookup.Contains(Path.GetExtension(f.FullName)))
        Return files
    End Function


End Class
