// Copyright 2009 Will Thomas
// 
// This file is part of MetaGeta.
// 
// MetaGeta is free software: you can redistribute it and/or modify
// it under the terms of the GNU General Public License as published by
// the Free Software Foundation, either version 3 of the License, or
// (at your option) any later version.
// 
// MetaGeta is distributed in the hope that it will be useful,
// but WITHOUT ANY WARRANTY; without even the implied warranty of
// MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE. See the
// GNU General Public License for more details.
// 
// You should have received a copy of the GNU General Public License
// along with MetaGeta. If not, see <http://www.gnu.org/licenses/>.

#region

using System;
using System.Collections.Generic;
using System.Data.Common;
using System.Data.SQLite;
using System.Reflection;
using System.Threading;
using log4net;
using MetaGeta.DataStore;
using MetaGeta.DataStore.Database;
using MetaGeta.Utilities;

#endregion

public class DataMapper : IDataMapper {
    private static readonly ILog log = LogManager.GetLogger(MethodBase.GetCurrentMethod().DeclaringType);

    private static readonly LocalDataStoreSlot s_ConnectionSlot;

    private readonly string m_FileName;
    private readonly List<DbConnection> s_Connections = new List<DbConnection>();

    static DataMapper() {
        s_ConnectionSlot = Thread.AllocateDataSlot();
    }

    public DataMapper(string filename) {
        m_FileName = filename;
    }

    private DbConnection Connection {
        get {
            var conn = (DbConnection)Thread.GetData(s_ConnectionSlot);
            if (conn == null) {
                conn = new SQLiteConnection();
                conn.ConnectionString = string.Format("Data Source={0}", m_FileName);
                log.InfoFormat("New connection: \"{0}\", opening...", conn.ConnectionString);
                conn.Open();

                Thread.SetData(s_ConnectionSlot, conn);
                s_Connections.Add(conn);
            }
            return conn;
        }
    }

    #region IDataMapper Members

    public void Initialize() {
        CheckDatabaseVersion();
    }

    public void Close() {
        //Must be synchronous...
        foreach (DbConnection conn in s_Connections)
            conn.Close();
    }

    #endregion

    private void CheckDatabaseVersion() {
        long version = 0;
        using (DbCommand cmd = Connection.CreateCommand()) {
            cmd.CommandText = "PRAGMA user_version";
            version = (long)cmd.ExecuteScalar();
        }

        log.InfoFormat("Found user_version \"{0}\".", version);

        if (version < 1) {
            log.Info("Version 0, so creating tables...");
            string createTablesSql =
                @"
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
";
            using (DbCommand cmd = Connection.CreateCommand()) {
                cmd.CommandText = createTablesSql;
                cmd.ExecuteNonQuery();
            }
            version = 1;
            log.Info("OK");
        }
        if (version == 1) {
            log.Info("Version 1, so upgrading...");
            string createTablesSql =
                @"
		                CREATE TABLE [GlobalSetting](
			                    [GlobalSettingID] integer primary key autoincrement,
			                    [Name] varchar, 
			                    [Value] varchar,
			                    [DefaultValue] varchar,
			                    [Type] varchar
			                );
			                CREATE UNIQUE INDEX [ixGlobalSetting] ON [GlobalSetting]([Name]);
			                PRAGMA user_version = 2;
";
            using (DbCommand cmd = Connection.CreateCommand()) {
                cmd.CommandText = createTablesSql;
                cmd.ExecuteNonQuery();
            }
            version = 2;
            log.Info("OK");
        }
        if (version == 2) {
            log.Info("Version 2 found, upgrading to version 3 and deleting all files and tags...");
            string createTablesSql =
                @"
                DROP INDEX [ixTag];
                DROP TABLE [Tag];
                DELETE FROM [File];

                CREATE TABLE [Tag](
                    [TagID] integer primary key autoincrement,
                    [FileID] integer references [File]([FileID]), 
                    [Name] varchar, 
                    [Value] none,
                    [Type] varchar
                );
                
                CREATE INDEX [ixTag_FileID] ON [Tag]([FileID]);
                CREATE INDEX [ixTag_Name] ON [Tag]([Name]);

                PRAGMA user_version = 3;
";
            using (DbCommand cmd = Connection.CreateCommand()) {
                cmd.CommandText = createTablesSql;
                cmd.ExecuteNonQuery();
            }
            version = 3;
            log.Info("OK");
        }
        if (version > 3) {
            log.FatalFormat("Unknown user_version: \"{0}\".", version);
            throw new Exception("Unknown database version.");
        }
    }

    private static T? ReadNullableValueType<T>(DbDataReader rdr, int index) where T : struct {
        object val = rdr.GetValue(index);
        if (ReferenceEquals(val, DBNull.Value))
            return null;
        else
            return (T?)val;
    }

    private static T ReadNullable<T>(DbDataReader rdr, int index) where T : class {
        object val = rdr.GetValue(index);
        if (ReferenceEquals(val, DBNull.Value))
            return null;
        else
            return (T)val;
    }

    #region "Creating"

    public void WriteNewDataStore(MGDataStore dataStore) {
        using (DbTransaction tran = Connection.BeginTransaction()) {
            using (DbCommand cmd = Connection.CreateCommand()) {
                cmd.CommandText = "INSERT INTO [DataStore]([Name], [Description], [TemplateName]) VALUES(?,?,?);SELECT last_insert_rowid() AS [ID]";
                cmd.Transaction = tran;
                cmd.AddParam(dataStore.Name);
                cmd.AddParam(dataStore.Description);
                cmd.AddParam(dataStore.Template.GetName());
                dataStore.ID = (long)cmd.ExecuteScalar();
            }
            foreach (IMGPluginBase plugin in dataStore.Plugins) {
                using (DbCommand cmd = Connection.CreateCommand()) {
                    cmd.CommandText = "INSERT INTO [Plugin]([DatastoreID], [PluginTypeName]) VALUES(?,?);SELECT last_insert_rowid() AS [ID]";
                    cmd.Transaction = tran;
                    cmd.AddParam(dataStore.ID);
                    string typeName = string.Format("{0}, {1}", plugin.GetType().FullName, plugin.GetType().Assembly.GetName().Name);
                    cmd.AddParam(typeName);
                    var id = (long)cmd.ExecuteScalar();
                    dataStore.StartupPlugin(plugin, id, true);
                }
            }
            tran.Commit();
        }
    }

    public void WriteNewFiles(IEnumerable<MGFile> files, MGDataStore dataStore) {
        using (DbTransaction tran = Connection.BeginTransaction()) {
            foreach (MGFile file in files) {
                using (DbCommand cmd = Connection.CreateCommand()) {
                    cmd.Transaction = tran;
                    cmd.CommandText = "INSERT INTO [File]([DatastoreID]) VALUES(?);SELECT last_insert_rowid() AS [ID]";
                    cmd.AddParam(dataStore.ID);
                    file.ID = (long)cmd.ExecuteScalar();
                }
            }
            tran.Commit();
        }
    }

    #endregion

    #region "Reading"

    public IEnumerable<MGDataStore> GetDataStores(IDataStoreOwner owner) {
        log.Debug("Loading DataStores...");
        var dataStores = new List<MGDataStore>();
        using (DbCommand cmd = Connection.CreateCommand()) {
            cmd.CommandText = "SELECT [DatastoreID], [Name], [Description], [TemplateName] FROM [DataStore]";
            using (DbDataReader rdr = cmd.ExecuteReader()) {
                while (rdr.Read()) {
                    long id = rdr.GetInt64(0);
                    string name = rdr.GetString(1);
                    string description = rdr.Get<string>(2);
                    string templateName = rdr.GetString(3);
                    IDataStoreTemplate template = TemplateFinder.GetTemplateByName(templateName);
                    var ds = new MGDataStore(owner, this);
                    using (ds.SuspendUpdates()) {
                        ds.Name = name;
                        ds.ID = id;
                        ds.Template = template;
                        ds.Description = description;
                    }
                    dataStores.Add(ds);
                }
            }
        }

        log.Debug("Loading Plugins...");
        foreach (MGDataStore ds in dataStores) {
            using (DbCommand cmd = Connection.CreateCommand()) {
                cmd.CommandText = "SELECT [PluginID], [PluginTypeName] FROM [Plugin] WHERE [DataStoreID] = ? ORDER BY [PluginID] ASC";
                cmd.AddParam(ds.ID);
                using (DbDataReader rdr = cmd.ExecuteReader()) {
                    while (rdr.Read()) {
                        long pluginID = rdr.GetInt64(0);
                        string typeName = rdr.GetString(1);
                        Type t = Type.GetType(typeName);
                        if (t == null)
                            throw new Exception(string.Format("Can't find type \"{0}\".", typeName));
                        var plugin = (IMGPluginBase)Activator.CreateInstance(t);
                        ds.AddExistingPlugin(plugin, pluginID);
                    }
                }
            }
        }

        return dataStores;
    }

    public IList<MGFile> GetFiles(MGDataStore dataStore) {
        var files = new List<MGFile>();
        using (DbCommand cmd = Connection.CreateCommand()) {
            cmd.CommandText =
                @"
SELECT 
    [File].[FileID], 
    [Tag].[Name], 
    [Tag].[Value],
    [Tag].[Type] 
FROM [File] 
    INNER JOIN [Tag] on [Tag].[FileID] = [File].[FileID]
WHERE 1=1
    AND [DatastoreID] = @DataStoreID
    --AND [Tag].[Value] IS NOT NULL ORDER BY [File].[FileID]
";
            cmd.SetParam("@DataStoreID", dataStore.ID);
            var persister = new TagCollectionPersister() {
                KeyColumn = "FileID",
                TagNameColumn = "Name",
                TagValueColumn = "Value",
                TagTypeColumn = "Type",
            };
            Dictionary<long, MGTagCollection> idsToTags;
            using (DbDataReader rdr = cmd.ExecuteReader()) {
                idsToTags = persister.ReadTags<long>(rdr);
            }
            foreach (var pair in idsToTags) {
                files.Add(new MGFile(pair.Key, dataStore, pair.Value));
            }
        }
        return files;
    }

    public IList<Tuple<MGTag, MGFile>> GetTagOnAllFiles(MGDataStore dataStore, string tagName) {
        var files = new List<Tuple<MGTag, MGFile>>();
        foreach (var file in GetFiles(dataStore)) {
            if(file.Tags.ContainsKey(tagName))
                files.Add(new Tuple<MGTag, MGFile>(file.Tags.GetTag(tagName), file));
        }
        return files;
    }

    #endregion

    #region "Writing Changes"

    public void WriteFile(MGFile file) {
        using (DbTransaction tran = Connection.BeginTransaction()) {
            using (DbCommand cmd = Connection.CreateCommand()) {
                cmd.Transaction = tran;
                cmd.CommandText = "DELETE FROM [Tag] WHERE [FileID] = ?";
                cmd.AddParam(file.ID);
                cmd.ExecuteNonQuery();
            }

            using (DbCommand cmd = Connection.CreateCommand()) {
                cmd.Transaction = tran;
                cmd.CommandText = "INSERT INTO [Tag]([FileID], [Name], [Value], [Type]) VALUES(@FileID, @Name, @Value, @Type)";
                var persister = new TagCollectionPersister() {
                                                                 KeyColumn = "FileID",
                                                                 TagNameColumn = "Name",
                                                                 TagValueColumn = "Value",
                                                                 TagTypeColumn = "Type",
                                                             };
                cmd.SetParam("@FileID", file.ID);
                persister.WriteTags(file.Tags, cmd);
            }

            tran.Commit();
        }
    }

    public void WriteDataStore(MGDataStore dataStore) {
        using (DbTransaction tran = Connection.BeginTransaction()) {
            using (DbCommand cmd = Connection.CreateCommand()) {
                cmd.CommandText = "UPDATE [DataStore] SET [Name] = ?, [Description] = ? WHERE [DatastoreID] = ?";
                cmd.Transaction = tran;
                cmd.AddParam(dataStore.Name);
                cmd.AddParam(dataStore.Description);
                cmd.AddParam(dataStore.ID);
                cmd.ExecuteNonQuery();
            }
            tran.Commit();
        }
    }

    #endregion

    #region "Removing"

    public void RemoveDataStore(MGDataStore datastore) {
        log.DebugFormat("Removing datastore: {0}", datastore.Name);
        using (DbTransaction tran = Connection.BeginTransaction()) {
            using (DbCommand cmd = Connection.CreateCommand()) {
                cmd.CommandText = "DELETE FROM [Tag] WHERE [FileID] in (SELECT [FileID] FROM [File] WHERE [DatastoreID] = ?)";
                cmd.AddParam(datastore.ID);
                cmd.ExecuteNonQuery();
            }
            using (DbCommand cmd = Connection.CreateCommand()) {
                cmd.CommandText = "DELETE FROM [File] WHERE [DatastoreID] = ?";
                cmd.AddParam(datastore.ID);
                cmd.ExecuteNonQuery();
            }
            using (DbCommand cmd = Connection.CreateCommand()) {
                cmd.CommandText = "DELETE FROM [PluginSetting] WHERE [DatastoreID] = ?";
                cmd.AddParam(datastore.ID);
                cmd.ExecuteNonQuery();
            }
            using (DbCommand cmd = Connection.CreateCommand()) {
                cmd.CommandText = "DELETE FROM [Plugin] WHERE [DatastoreID] = ?";
                cmd.AddParam(datastore.ID);
                cmd.ExecuteNonQuery();
            }
            using (DbCommand cmd = Connection.CreateCommand()) {
                cmd.CommandText = "DELETE FROM [DataStore] WHERE [DatastoreID] = ?";
                cmd.AddParam(datastore.ID);
                cmd.ExecuteNonQuery();
            }
            tran.Commit();
        }
    }

    public void RemoveFiles(IEnumerable<MGFile> files, MGDataStore store) {
        using (DbTransaction tran = Connection.BeginTransaction()) {
            foreach (MGFile file in files) {
                log.DebugFormat("Removing file: {0}", file.ID);
                using (DbCommand cmd = Connection.CreateCommand()) {
                    cmd.Transaction = tran;
                    cmd.CommandText = "DELETE FROM [Tag] WHERE [FileID] = ?";
                    cmd.AddParam(file.ID);
                    cmd.ExecuteNonQuery();
                }
                using (DbCommand cmd = Connection.CreateCommand()) {
                    cmd.Transaction = tran;
                    cmd.CommandText = "DELETE FROM [File] WHERE [FileID] = ?";
                    cmd.AddParam(file.ID);
                    cmd.ExecuteNonQuery();
                }
            }
            tran.Commit();
        }
    }

    #endregion

    #region "Settings"

    public string GetPluginSetting(MGDataStore dataStore, long pluginID, string settingName) {
        using (DbCommand cmd = Connection.CreateCommand()) {
            cmd.CommandText = "SELECT [Value] FROM [PluginSetting] WHERE [DatastoreID] = ? AND [PluginID] = ? AND [Name] = ?";
            cmd.AddParam(dataStore.ID);
            cmd.AddParam(pluginID);
            cmd.AddParam(settingName);
            return (string)cmd.ExecuteScalar();
        }
    }

    public void WritePluginSetting(MGDataStore dataStore, long pluginID, string settingName, string settingValue) {
        using (DbCommand cmd = Connection.CreateCommand()) {
            cmd.CommandText = "INSERT OR REPLACE INTO [PluginSetting]([DatastoreID], [PluginID], [Name], [Value]) VALUES(?, ?, ?, ?)";
            cmd.AddParam(dataStore.ID);
            cmd.AddParam(pluginID);
            cmd.AddParam(settingName);
            cmd.AddParam(settingValue);
            cmd.ExecuteNonQuery();
        }
    }

    public string GetGlobalSetting(string settingName) {
        using (DbCommand cmd = Connection.CreateCommand()) {
            cmd.CommandText = "SELECT coalesce([Value], [DefaultValue]) FROM [GlobalSetting] WHERE [Name] = ?";
            cmd.AddParam(settingName);
            return (string)cmd.ExecuteScalar();
        }
    }

    public void WriteGlobalSetting(string settingName, string settingValue) {
        using (DbCommand cmd = Connection.CreateCommand()) {
            cmd.CommandText = "UPDATE [GlobalSetting] SET [Value] = ? WHERE [Name] = ?";
            cmd.AddParam(settingValue);
            cmd.AddParam(settingName);
            cmd.ExecuteNonQuery();
        }
    }

    //Public Sub CreateGlobalSetting(ByVal setting As GlobalSettingAttribute)
    //    Using tran = Connection.BeginTransaction()
    //        Using cmd = Connection.CreateCommand()
    //            cmd.CommandText = "INSERT OR IGNORE INTO [GlobalSetting]([Name], [Value], [DefaultValue], [Type]) VALUES(?, NULL, ?, ?)"
    //            cmd.AddParam(setting.Name)
    //            cmd.AddParam(setting.DefaultValue)
    //            cmd.AddParam(setting.Type.ToString())
    //            cmd.ExecuteNonQuery()
    //        End Using
    //        Using cmd = Connection.CreateCommand()
    //            cmd.CommandText = "UPDATE [GlobalSetting] SET [DefaultValue] = ?, [Type] = ? WHERE [Name] = ?"
    //            cmd.AddParam(setting.DefaultValue)
    //            cmd.AddParam(setting.Type.ToString())
    //            cmd.AddParam(setting.Name)
    //            cmd.ExecuteNonQuery()
    //        End Using
    //        tran.Commit()
    //    End Using
    //End Sub
    public IList<GlobalSetting> ReadGlobalSettings() {
        var settings = new List<GlobalSetting>();
        using (DbCommand cmd = Connection.CreateCommand()) {
            cmd.CommandText = "SELECT [Name], [Value], [DefaultValue], [Type] FROM [GlobalSetting]";
            using (DbDataReader rdr = cmd.ExecuteReader()) {
                while (rdr.Read()) {
                    string name = rdr.GetString(0);
                    var value = ReadNullable<string>(rdr, 1);
                    string defaultValue = rdr.GetString(2);
                    var type = (GlobalSettingType)Enum.Parse(typeof(GlobalSettingType), rdr.GetString(3));
                    settings.Add(new GlobalSetting(name, value, defaultValue, type));
                }
            }
        }
        return settings;
    }

    #endregion
}