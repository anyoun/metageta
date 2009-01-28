Public Class DataMapper
    Private Shared ReadOnly log As log4net.ILog = log4net.LogManager.GetLogger(Reflection.MethodBase.GetCurrentMethod().DeclaringType)

    Private Shared ReadOnly s_ConnectionSlot As LocalDataStoreSlot
    Private Shared ReadOnly s_Connections As New List(Of DbConnection)

    Private ReadOnly Property Connection() As DbConnection
        Get
            Dim conn = CType(Thread.GetData(s_ConnectionSlot), DbConnection)
            If conn Is Nothing Then
                conn = DbProviderFactories.GetFactory("System.Data.SQLite").CreateConnection()
                conn.ConnectionString = "Data Source=metageta.db3"
                log.InfoFormat("New connection: ""{0}"", opening...", conn.ConnectionString)
                conn.Open()

                Thread.SetData(s_ConnectionSlot, conn)
                s_Connections.Add(conn)
            End If
            Return conn
        End Get
    End Property

    Shared Sub New()
        s_ConnectionSlot = Thread.AllocateDataSlot()
    End Sub

    Public Sub New()

    End Sub

    Public Sub Initialize()
        CheckDatabaseVersion()
    End Sub

    Public Sub Close()
        'Must be synchronous...
        For Each conn In s_Connections
            conn.Close()
        Next
    End Sub

    Private Sub CheckDatabaseVersion()
        Dim version As Long
        Using cmd = Connection.CreateCommand()
            cmd.CommandText = "PRAGMA user_version"
            version = CType(cmd.ExecuteScalar(), Long)
        End Using

        log.InfoFormat("Found user_version ""{0}"".", version)

        If version < 1 Then
            log.Info("Version 0, so creating tables...")
            Dim createTablesSql = <string>
                CREATE TABLE [DataStore](
                    [DatastoreID] integer primary key autoincrement, 
                    [Name] varchar,
                    [Description] varchar,
                    [TemplateName] varchar
                );
                CREATE TABLE [PluginSetting](
                    [PluginSettingID] integer primary key autoincrement,
                    [DatastoreID] integer references [DataStore]([DatastoreID]), 
                    [PluginTypeName] varchar,
                    [Name] varchar, 
                    [Value] varchar
                );
                CREATE UNIQUE INDEX [ixPluginSetting] ON [PluginSetting]([DatastoreID], [PluginTypeName], [Name]);
                CREATE TABLE [File](
                    [FileID] integer primary key autoincrement,
                    [DatastoreID] integer references [DataStore]([DatastoreID])
                );
                CREATE TABLE [Tag](
                    [TagID] integer primary key autoincrement,
                    [FileID] integer references [File]([FileID]), 
                    [Name] varchar, 
                    [Value] varchar
                );
                CREATE UNIQUE INDEX [ixTag] ON [Tag]([FileID], [Name]);
                PRAGMA user_version = 1;
            </string>.Value
            Using cmd = Connection.CreateCommand()
                cmd.CommandText = createTablesSql
                cmd.ExecuteNonQuery()
            End Using
            log.Info("OK")
        ElseIf version > 1 Then
            log.FatalFormat("Unknown user_version: ""{0}"".", version)
            Throw New Exception("Unknown database version.")
        End If
    End Sub

#Region "Creating"
    Public Sub WriteNewDataStore(ByVal dataStore As MGDataStore)
        Using cmd = Connection.CreateCommand()
            cmd.CommandText = "INSERT INTO [DataStore]([Name], [Description], [TemplateName]) VALUES(?,?,?);SELECT last_insert_rowid() AS [ID]"
            cmd.AddParam(dataStore.Name)
            cmd.AddParam(dataStore.Description)
            cmd.AddParam(dataStore.Template.GetName())
            dataStore.ID = CType(cmd.ExecuteScalar(), Long)
        End Using
    End Sub
    Public Sub WriteNewFile(ByVal file As MGFile, ByVal dataStore As MGDataStore)
        Using cmd = Connection.CreateCommand()
            cmd.CommandText = "INSERT INTO [File]([DatastoreID]) VALUES(?);SELECT last_insert_rowid() AS [ID]"
            cmd.AddParam(dataStore.ID)
            file.ID = CType(cmd.ExecuteScalar(), Long)
        End Using
    End Sub
#End Region

#Region "Reading"
    Public Function GetDataStores() As IEnumerable(Of MGDataStore)
        log.Debug("Loading DataStores...")
        Dim dataStores As New List(Of MGDataStore)
        Using cmd = Connection.CreateCommand()
            cmd.CommandText = "SELECT [DatastoreID], [Name], [Description], [TemplateName] FROM [DataStore]"
            Using rdr = cmd.ExecuteReader()
                While rdr.Read()
                    Dim id = rdr.GetInt64(0)
                    Dim name = rdr.GetString(1)
                    Dim description = rdr.GetString(2)
                    Dim templateName = rdr.GetString(3)
                    Dim template = TemplateFinder.GetTemplateByName(templateName)
                    Dim ds As New MGDataStore(template, name, Me) With { _
                        .ID = id, _
                        .Description = description _
                    }
                    dataStores.Add(ds)
                End While
            End Using
        End Using
        Return dataStores
    End Function
    Public Function GetTag(ByVal file As MGFile, ByVal tagName As String) As String
        Using cmd = Connection.CreateCommand()
            cmd.CommandText = "SELECT [Value] FROM [Tag] WHERE [FileID] = ? AND [Name] = ?"
            cmd.AddParam(file.ID)
            cmd.AddParam(tagName)
            Return CType(cmd.ExecuteScalar(), String)
        End Using
    End Function
    Public Function GetFiles(ByVal dataStore As MGDataStore) As IList(Of MGFile)
        Dim files As New List(Of MGFile)
        Using cmd = Connection.CreateCommand()
            cmd.CommandText = "SELECT [FileID] FROM [File] WHERE [DatastoreID] = ?"
            cmd.AddParam(dataStore.ID)
            Using rdr = cmd.ExecuteReader()
                While rdr.Read()
                    files.Add(New MGFile(rdr.GetInt64(0), dataStore))
                End While
            End Using
        End Using
        Return files
    End Function
    Public Function GetAllTagOnFiles(ByVal dataStore As MGDataStore, ByVal tagName As String) As IList(Of Tuple(Of MGTag, MGFile))
        Dim files As New List(Of Tuple(Of MGTag, MGFile))
        Using cmd = Connection.CreateCommand()
            cmd.CommandText = "SELECT [File].[FileID], [Tag].[Value] FROM [File] LEFT OUTER JOIN [Tag] on [Tag].[FileID] = [File].[FileID] WHERE [DatastoreID] = ? AND [Tag].[Name] = ?"
            cmd.AddParam(dataStore.ID)
            cmd.AddParam(tagName)
            Using rdr = cmd.ExecuteReader()
                While rdr.Read()
                    Dim fileID = rdr.GetInt64(0)
                    Dim tagValue = rdr.GetString(1)
                    files.Add(New Tuple(Of MGTag, MGFile)(New MGTag(tagName, tagValue), New MGFile(fileID, dataStore)))
                End While
            End Using
        End Using
        Return files
    End Function
    Public Function GetAllTags(ByVal fileId As Long) As MGTagCollection
        Dim tags As New List(Of MGTag)
        Using cmd = Connection.CreateCommand()
            cmd.CommandText = "SELECT [Name], [Value] FROM [Tag] WHERE [FileID] = ?"
            cmd.AddParam(fileId)
            Using rdr = cmd.ExecuteReader()
                While rdr.Read()
                    tags.Add(New MGTag(rdr.GetString(0), rdr.GetString(1)))
                End While
            End Using
        End Using
        Return New MGTagCollection(tags)
    End Function
#End Region

#Region "Writing Changes"
    Public Sub WriteTag(ByVal file As MGFile, ByVal tagName As String, ByVal tagValue As String)
        Using cmd = Connection.CreateCommand()
            cmd.CommandText = "INSERT OR REPLACE INTO [Tag]([FileID], [Name], [Value]) VALUES(?, ?, ?)"
            cmd.AddParam(file.ID)
            cmd.AddParam(tagName)
            cmd.AddParam(tagValue)
            cmd.ExecuteNonQuery()
        End Using
    End Sub
#End Region

    Public Function GetPluginSetting(ByVal dataStore As MGDataStore, ByVal pluginName As String, ByVal settingName As String) As String
        Using cmd = Connection.CreateCommand()
            cmd.CommandText = "SELECT [Value] FROM [PluginSetting] WHERE [DatastoreID] = ? AND [PluginTypeName] = ? AND [Name] = ?"
            cmd.AddParam(dataStore.ID)
            cmd.AddParam(pluginName)
            cmd.AddParam(settingName)
            Return CType(cmd.ExecuteScalar(), String)
        End Using
    End Function
    Public Sub WritePluginSetting(ByVal dataStore As MGDataStore, ByVal pluginName As String, ByVal settingName As String, ByVal settingValue As String)
        Using cmd = Connection.CreateCommand()
            cmd.CommandText = "INSERT OR REPLACE INTO [PluginSetting]([DatastoreID], [PluginTypeName], [Name], [Value]) VALUES(?, ?, ?, ?)"
            cmd.AddParam(dataStore.ID)
            cmd.AddParam(pluginName)
            cmd.AddParam(settingName)
            cmd.AddParam(settingValue)
            cmd.ExecuteNonQuery()
        End Using
    End Sub

End Class
