Public Class DataStoreManager
    Implements INotifyPropertyChanged

    Private ReadOnly m_DbConnection As DbConnection
    Private ReadOnly m_DataStores As New ObservableCollection(Of MGDataStore)

    Public Sub New()
        m_DbConnection = DbProviderFactories.GetFactory("System.Data.SQLite").CreateConnection()
    End Sub

    Public Sub Startup()
        m_DbConnection.ConnectionString = "Data Source=metageta.db3"
        m_DbConnection.Open()

        Dim version As Long
        Using cmd = m_DbConnection.CreateCommand()
            cmd.CommandText = "PRAGMA user_version"
            version = CType(cmd.ExecuteScalar(), Long)
        End Using

        If version < 1 Then
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
                PRAGMA user_version = 1;
            </string>.Value
            Using cmd = m_DbConnection.CreateCommand()
                cmd.CommandText = createTablesSql
                cmd.ExecuteNonQuery()
            End Using
        End If

        Dim files As New List(Of MGFile)
        Using cmd = m_DbConnection.CreateCommand()
            cmd.CommandText = "SELECT [DatastoreID], [Name], [Description], [TemplateName] FROM [DataStore]"
            Using rdr = cmd.ExecuteReader()
                While rdr.Read()
                    Dim id = rdr.GetInt64(0)
                    Dim name = rdr.GetString(1)
                    Dim description = rdr.GetString(2)
                    Dim templateName = rdr.GetString(3)
                    Dim template = TemplateFinder.GetTemplateByName(templateName)
                    Dim ds As New MGDataStore(id, template, name, m_DbConnection)
                    ds.Description = description
                    m_DataStores.Add(ds)
                End While
            End Using
        End Using
    End Sub

    Public Sub Shutdown()
        For Each ds In m_DataStores
            ds.Close()
        Next

        m_DbConnection.Close()
    End Sub

    Public Function NewDataStore(ByVal name As String, ByVal template As IDataStoreTemplate) As MGDataStore
        Dim dataStoreID As Long
        Using cmd = m_DbConnection.CreateCommand()
            cmd.CommandText = "INSERT INTO [DataStore]([Name], [Description], [TemplateName]) VALUES(?,?,?);SELECT last_insert_rowid() AS [ID]"
            cmd.AddParam(name)
            cmd.AddParam("")
            cmd.AddParam(template.GetName())
            dataStoreID = CType(cmd.ExecuteScalar(), Long)
        End Using

        Dim data As New MGDataStore(dataStoreID, template, name, m_DbConnection)
        m_DataStores.Add(data)
        OnDataStoresChanged()
        Return data
    End Function

    Public ReadOnly Property DataStores() As ObservableCollection(Of MGDataStore)
        Get
            Return m_DataStores
        End Get
    End Property

    Private Sub OnDataStoresChanged()
        OnPropertyChanged("DataStores")
    End Sub
    Private Sub OnPropertyChanged(ByVal name As String)
        RaiseEvent PropertyChanged(Me, New PropertyChangedEventArgs(name))
    End Sub
    Public Event PropertyChanged(ByVal sender As Object, ByVal e As System.ComponentModel.PropertyChangedEventArgs) Implements System.ComponentModel.INotifyPropertyChanged.PropertyChanged
End Class