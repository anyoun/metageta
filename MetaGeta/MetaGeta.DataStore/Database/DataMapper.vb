Imports System.Data.SQLite
Imports MetaGeta.DataStore.Database

Public Class DataMapper
    Implements IDataMapper
    Private Shared ReadOnly log As log4net.ILog = log4net.LogManager.GetLogger(Reflection.MethodBase.GetCurrentMethod().DeclaringType)

    Private Shared ReadOnly s_ConnectionSlot As LocalDataStoreSlot

    Private ReadOnly s_Connections As New List(Of DbConnection)
    Private ReadOnly m_FileName As String

    Private ReadOnly Property Connection() As DbConnection
        Get
            Dim conn = CType(Thread.GetData(s_ConnectionSlot), DbConnection)
            If conn Is Nothing Then
                conn = New SQLiteConnection()
                conn.ConnectionString = String.Format("Data Source={0}", m_FileName)
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

    Public Sub New(ByVal filename As String)
        m_FileName = filename
    End Sub

    Public Sub Initialize() Implements IDataMapper.Initialize
        CheckDatabaseVersion()
    End Sub

    Public Sub Close() Implements IDataMapper.Close
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
                CREATE TABLE [Plugin](
                    [PluginID] integer primary key autoincrement, 
                    [DatastoreID] integer references [DataStore]([DatastoreID]), 
                    [PluginTypeName] varchar
                );
                CREATE UNIQUE INDEX [ixPlugin] ON [Plugin]([DatastoreID], [PluginTypeName]);
                CREATE TABLE [PluginSetting](
                    [PluginSettingID] integer primary key autoincrement,
                    [DatastoreID] integer references [DataStore]([DatastoreID]), 
                    [PluginID] integer references [DataStore]([DatastoreID]), 
                    [Name] varchar, 
                    [Value] varchar
                );
                CREATE UNIQUE INDEX [ixPluginSetting] ON [PluginSetting]([DatastoreID], [PluginID], [Name]);
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
            version = 1
            log.Info("OK")
        End If
        If version = 1 Then
            log.Info("Version 1, so upgrading...")
            Dim createTablesSql = <string>
                CREATE TABLE [GlobalSetting](
                    [GlobalSettingID] integer primary key autoincrement,
                    [Name] varchar, 
                    [Value] varchar,
                    [DefaultValue] varchar,
                    [Type] varchar
                );
                CREATE UNIQUE INDEX [ixGlobalSetting] ON [GlobalSetting]([Name]);
                PRAGMA user_version = 2;
            </string>.Value
            Using cmd = Connection.CreateCommand()
                cmd.CommandText = createTablesSql
                cmd.ExecuteNonQuery()
            End Using
            version = 2
            log.Info("OK")
        End If
        If version > 2 Then
            log.FatalFormat("Unknown user_version: ""{0}"".", version)
            Throw New Exception("Unknown database version.")
        End If
    End Sub

#Region "Creating"
    Public Sub WriteNewDataStore(ByVal dataStore As MGDataStore) Implements IDataMapper.WriteNewDataStore
        Using tran = Connection.BeginTransaction()
            Using cmd = Connection.CreateCommand()
                cmd.CommandText = "INSERT INTO [DataStore]([Name], [Description], [TemplateName]) VALUES(?,?,?);SELECT last_insert_rowid() AS [ID]"
                cmd.Transaction = tran
                cmd.AddParam(dataStore.Name)
                cmd.AddParam(dataStore.Description)
                cmd.AddParam(dataStore.Template.GetName())
                dataStore.ID = CType(cmd.ExecuteScalar(), Long)
            End Using
            For Each plugin In dataStore.Plugins
                Using cmd = Connection.CreateCommand()
                    cmd.CommandText = "INSERT INTO [Plugin]([DatastoreID], [PluginTypeName]) VALUES(?,?);SELECT last_insert_rowid() AS [ID]"
                    cmd.Transaction = tran
                    cmd.AddParam(dataStore.ID)
                    Dim typeName = String.Format("{0}, {1}", plugin.GetType().FullName, plugin.GetType().Assembly.GetName().Name)
                    cmd.AddParam(typeName)
                    Dim id = CType(cmd.ExecuteScalar(), Long)
                    dataStore.StartupPlugin(plugin, id, True)
                End Using
            Next
            tran.Commit()
        End Using
    End Sub
    Public Sub WriteNewFiles(ByVal files As IEnumerable(Of MGFile), ByVal dataStore As MGDataStore) Implements IDataMapper.WriteNewFiles
        Using tran = Connection.BeginTransaction()
            For Each file In files
                Using cmd = Connection.CreateCommand()
                    cmd.Transaction = tran
                    cmd.CommandText = "INSERT INTO [File]([DatastoreID]) VALUES(?);SELECT last_insert_rowid() AS [ID]"
                    cmd.AddParam(dataStore.ID)
                    file.ID = CType(cmd.ExecuteScalar(), Long)
                End Using
            Next
            tran.Commit()
        End Using
    End Sub
#End Region

#Region "Reading"
    Public Function GetDataStores(ByVal owner As IDataStoreOwner) As IEnumerable(Of MGDataStore) Implements IDataMapper.GetDataStores
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
                    Dim ds As New MGDataStore(owner, Me)
                    Using (ds.SuspendUpdates())
                        ds.Name = name
                        ds.ID = id
                        ds.Template = template
                        ds.Description = description
                    End Using
                    dataStores.Add(ds)
                End While
            End Using
        End Using

        log.Debug("Loading Plugins...")
        For Each ds In dataStores
            Using cmd = Connection.CreateCommand()
                cmd.CommandText = "SELECT [PluginID], [PluginTypeName] FROM [Plugin] WHERE [DataStoreID] = ? ORDER BY [PluginID] ASC"
                cmd.AddParam(ds.ID)
                Using rdr = cmd.ExecuteReader()
                    While rdr.Read()
                        Dim pluginID = rdr.GetInt64(0)
                        Dim typeName = rdr.GetString(1)
                        Dim t = Type.GetType(typeName)
                        If t Is Nothing Then
                            Throw New Exception(String.Format("Can't find type ""{0}"".", typeName))
                        End If
                        Dim plugin = CType(Activator.CreateInstance(t), IMGPluginBase)
                        ds.AddExistingPlugin(plugin, pluginID)
                    End While
                End Using
            End Using
        Next

        Return dataStores
    End Function
    Public Function GetFiles(ByVal dataStore As MGDataStore) As IList(Of MGFile) Implements IDataMapper.GetFiles
        Dim files As New List(Of MGFile)
        Using cmd = Connection.CreateCommand()
            cmd.CommandText = "SELECT [File].[FileID], [Tag].[Name], [Tag].[Value] FROM [File] LEFT OUTER JOIN [Tag] on [Tag].[FileID] = [File].[FileID] WHERE [DatastoreID] = ? AND [Tag].[Value] IS NOT NULL ORDER BY [File].[FileID]"
            cmd.AddParam(dataStore.ID)
            Using rdr = cmd.ExecuteReader()
                Dim file As MGFile = Nothing
                While rdr.Read()
                    Dim nextId = rdr.GetInt64(0)
                    If file Is Nothing OrElse file.ID <> nextId Then
                        If file IsNot Nothing Then
                            files.Add(file)
                        End If
                        file = New MGFile(nextId, dataStore)
                    End If
                    file.Tags.SetValue(rdr.GetString(1), rdr.GetString(2))
                End While
            End Using
        End Using
        Return files
    End Function
    Public Function GetTagOnAllFiles(ByVal dataStore As MGDataStore, ByVal tagName As String) As IList(Of Tuple(Of MGTag, MGFile)) Implements IDataMapper.GetTagOnAllFiles
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
#End Region

#Region "Writing Changes"
    Public Sub WriteFile(ByVal file As MGFile) Implements IDataMapper.WriteFile
        Using tran = Connection.BeginTransaction()
            Using cmd = Connection.CreateCommand()
                cmd.Transaction = tran
                cmd.CommandText = "DELETE FROM [Tag] WHERE [FileID] = ?"
                cmd.AddParam(file.ID)
                cmd.ExecuteNonQuery()
            End Using
            For Each tag In file.Tags
                Using cmd = Connection.CreateCommand()
                    cmd.Transaction = tran
                    cmd.CommandText = "INSERT INTO [Tag]([FileID], [Name], [Value]) VALUES(?, ?, ?)"
                    cmd.AddParam(file.ID)
                    cmd.AddParam(tag.Name)
                    cmd.AddParam(tag.Value)
                    cmd.ExecuteNonQuery()
                End Using
            Next
            tran.Commit()
        End Using
    End Sub
    Public Sub WriteDataStore(ByVal dataStore As MGDataStore) Implements IDataMapper.WriteDataStore
        Using tran = Connection.BeginTransaction()
            Using cmd = Connection.CreateCommand()
                cmd.CommandText = "UPDATE [DataStore] SET [Name] = ?, [Description] = ? WHERE [DatastoreID] = ?"
                cmd.Transaction = tran
                cmd.AddParam(dataStore.Name)
                cmd.AddParam(dataStore.Description)
                cmd.AddParam(dataStore.ID)
                cmd.ExecuteNonQuery()
            End Using
            tran.Commit()
        End Using
    End Sub
#End Region

#Region "Removing"
    Public Sub RemoveDataStore(ByVal datastore As MGDataStore) Implements IDataMapper.RemoveDataStore
        log.DebugFormat("Removing datastore: {0}", datastore.Name)
        Using tran = Connection.BeginTransaction()
            Using cmd = Connection.CreateCommand()
                cmd.CommandText = "DELETE FROM [Tag] WHERE [FileID] in (SELECT [FileID] FROM [File] WHERE [DatastoreID] = ?)"
                cmd.AddParam(datastore.ID)
                cmd.ExecuteNonQuery()
            End Using
            Using cmd = Connection.CreateCommand()
                cmd.CommandText = "DELETE FROM [File] WHERE [DatastoreID] = ?"
                cmd.AddParam(datastore.ID)
                cmd.ExecuteNonQuery()
            End Using
            Using cmd = Connection.CreateCommand()
                cmd.CommandText = "DELETE FROM [PluginSetting] WHERE [DatastoreID] = ?"
                cmd.AddParam(datastore.ID)
                cmd.ExecuteNonQuery()
            End Using
            Using cmd = Connection.CreateCommand()
                cmd.CommandText = "DELETE FROM [Plugin] WHERE [DatastoreID] = ?"
                cmd.AddParam(datastore.ID)
                cmd.ExecuteNonQuery()
            End Using
            Using cmd = Connection.CreateCommand()
                cmd.CommandText = "DELETE FROM [DataStore] WHERE [DatastoreID] = ?"
                cmd.AddParam(datastore.ID)
                cmd.ExecuteNonQuery()
            End Using
            tran.Commit()
        End Using
    End Sub

    Public Sub RemoveFiles(ByVal files As IEnumerable(Of MGFile), ByVal store As MGDataStore) Implements IDataMapper.RemoveFiles
        Using tran = Connection.BeginTransaction()
            For Each file In files
                log.DebugFormat("Removing file: {0}", file.ID)
                Using cmd = Connection.CreateCommand()
                    cmd.Transaction = tran
                    cmd.CommandText = "DELETE FROM [Tag] WHERE [FileID] = ?"
                    cmd.AddParam(file.ID)
                    cmd.ExecuteNonQuery()
                End Using
                Using cmd = Connection.CreateCommand()
                    cmd.Transaction = tran
                    cmd.CommandText = "DELETE FROM [File] WHERE [FileID] = ?"
                    cmd.AddParam(file.ID)
                    cmd.ExecuteNonQuery()
                End Using
            Next
            tran.Commit()
        End Using
    End Sub
#End Region

#Region "Settings"

    Public Function GetPluginSetting(ByVal dataStore As MGDataStore, ByVal pluginID As Long, ByVal settingName As String) As String Implements IDataMapper.GetPluginSetting
        Using cmd = Connection.CreateCommand()
            cmd.CommandText = "SELECT [Value] FROM [PluginSetting] WHERE [DatastoreID] = ? AND [PluginID] = ? AND [Name] = ?"
            cmd.AddParam(dataStore.ID)
            cmd.AddParam(pluginID)
            cmd.AddParam(settingName)
            Return CType(cmd.ExecuteScalar(), String)
        End Using
    End Function
    Public Sub WritePluginSetting(ByVal dataStore As MGDataStore, ByVal pluginID As Long, ByVal settingName As String, ByVal settingValue As String) Implements IDataMapper.WritePluginSetting
        Using cmd = Connection.CreateCommand()
            cmd.CommandText = "INSERT OR REPLACE INTO [PluginSetting]([DatastoreID], [PluginID], [Name], [Value]) VALUES(?, ?, ?, ?)"
            cmd.AddParam(dataStore.ID)
            cmd.AddParam(pluginID)
            cmd.AddParam(settingName)
            cmd.AddParam(settingValue)
            cmd.ExecuteNonQuery()
        End Using
    End Sub

    Public Function GetGlobalSetting(ByVal settingName As String) As String Implements IDataMapper.GetGlobalSetting
        Using cmd = Connection.CreateCommand()
            cmd.CommandText = "SELECT coalesce([Value], [DefaultValue]) FROM [GlobalSetting] WHERE [Name] = ?"
            cmd.AddParam(settingName)
            Return CType(cmd.ExecuteScalar(), String)
        End Using
    End Function
    Public Sub WriteGlobalSetting(ByVal settingName As String, ByVal settingValue As String) Implements IDataMapper.WriteGlobalSetting
        Using cmd = Connection.CreateCommand()
            cmd.CommandText = "UPDATE [GlobalSetting] SET [Value] = ? WHERE [Name] = ?"
            cmd.AddParam(settingValue)
            cmd.AddParam(settingName)
            cmd.ExecuteNonQuery()
        End Using
    End Sub
    'Public Sub CreateGlobalSetting(ByVal setting As GlobalSettingAttribute)
    '    Using tran = Connection.BeginTransaction()
    '        Using cmd = Connection.CreateCommand()
    '            cmd.CommandText = "INSERT OR IGNORE INTO [GlobalSetting]([Name], [Value], [DefaultValue], [Type]) VALUES(?, NULL, ?, ?)"
    '            cmd.AddParam(setting.Name)
    '            cmd.AddParam(setting.DefaultValue)
    '            cmd.AddParam(setting.Type.ToString())
    '            cmd.ExecuteNonQuery()
    '        End Using
    '        Using cmd = Connection.CreateCommand()
    '            cmd.CommandText = "UPDATE [GlobalSetting] SET [DefaultValue] = ?, [Type] = ? WHERE [Name] = ?"
    '            cmd.AddParam(setting.DefaultValue)
    '            cmd.AddParam(setting.Type.ToString())
    '            cmd.AddParam(setting.Name)
    '            cmd.ExecuteNonQuery()
    '        End Using
    '        tran.Commit()
    '    End Using
    'End Sub
    Public Function ReadGlobalSettings() As IList(Of GlobalSetting) Implements IDataMapper.ReadGlobalSettings
        Dim settings As New List(Of GlobalSetting)
        Using cmd = Connection.CreateCommand()
            cmd.CommandText = "SELECT [Name], [Value], [DefaultValue], [Type] FROM [GlobalSetting]"
            Using rdr = cmd.ExecuteReader()
                While rdr.Read()
                    Dim name = rdr.GetString(0)
                    Dim value = ReadNullable(Of String)(rdr, 1)
                    Dim defaultValue = rdr.GetString(2)
                    Dim type = CType([Enum].Parse(GetType(GlobalSettingType), rdr.GetString(3)), GlobalSettingType)
                    settings.Add(New GlobalSetting(name, value, defaultValue, type))
                End While
            End Using
        End Using
        Return settings
    End Function
#End Region

    Private Shared Function ReadNullableValueType(Of T As Structure)(ByVal rdr As DbDataReader, ByVal index As Integer) As T?
        Dim val = rdr.GetValue(index)
        If val Is DBNull.Value Then
            Return Nothing
        Else
            Return CType(val, T?)
        End If
    End Function
    Private Shared Function ReadNullable(Of T As Class)(ByVal rdr As DbDataReader, ByVal index As Integer) As T
        Dim val = rdr.GetValue(index)
        If val Is DBNull.Value Then
            Return Nothing
        Else
            Return CType(val, T)
        End If
    End Function
End Class
