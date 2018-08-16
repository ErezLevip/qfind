using System.Collections.Generic;
using qfind.Interfaces;
using System.Data;
using System.Data.Common;
using Microsoft.Data.Sqlite;
using System;
using System.Threading.Tasks;
using qfind.Entities;
using System.IO;
using System.Linq;

namespace qfind.Implementations
{
    public class SqliteAdapter : IDbAdapter
    {
        private string _connectionString;
        public SqliteAdapter(DbConfigSection config)
        {
            _connectionString = config.ConnectionString;
        }
        public async Task<List<Index>> GetAllIndexes()
        {
            const string SELECT_ALL_COMMAND = @"SELECT search_key,name,extention,date_created,folder FROM Indexes";

            var indexes = new List<Index>();
            using (var connection = new SqliteConnection(_connectionString))
            {
                connection.Open();

                var command = connection.CreateCommand();
                command.CommandText = SELECT_ALL_COMMAND;
                using (var reader = await command.ExecuteReaderAsync())
                {
                    while (reader.Read())
                    {
                        indexes.Add(new Index
                        {
                            SearchKey = reader.GetString(0),
                            Name = reader.GetString(1),
                            Extention = reader.GetString(2),
                            DateCreated = reader.GetDateTime(3),
                            Folder = reader.GetString(4)
                        });
                    }
                }
            }
            return indexes;
        }

        public async Task<List<Selection>> GetAllPreviousSelections()
        {
            const string SELECT_ALL_COMMAND = @"SELECT search_key,count FROM Selections";

            var selections = new List<Selection>();
            using (var connection = new SqliteConnection(_connectionString))
            {
                connection.Open();

                // replace this with sys table query
                try
                {
                    await CreateSelectionsTable(connection);
                }
                catch { }

                var command = connection.CreateCommand();
                command.CommandText = SELECT_ALL_COMMAND;
                using (var reader = await command.ExecuteReaderAsync())
                {
                    while (reader.Read())
                    {
                        selections.Add(new Selection(reader.GetInt32(1), reader.GetString(0)));
                    }
                }
            }
            return selections;
        }

        public void SetIndexes(IEnumerable<Index> indexes, bool overrideAll)
        {
            const string INSERT_COMMAND = @"INSERT INTO Indexes (search_key,name,extention,date_created,folder)
                        VALUES(@key,@name,@extetion,@dateCreated,@folder)";

            using (var connection = new SqliteConnection(_connectionString))
            {
                connection.Open();

                // replace this with sys table query
                if (overrideAll)
                {
                    try
                    {
                        var DROP_INDEX_TABLE_COMMAND = @"DROP TABLE Indexes";
                        var dropCommand = connection.CreateCommand();
                        dropCommand.CommandText = DROP_INDEX_TABLE_COMMAND;
                        dropCommand.ExecuteNonQuery();
                    }
                    catch { }
                }

                try
                {
                    var CREATE_INDEX_TABLE_COMMAND = @"CREATE TABLE Indexes (
                                        search_key NVARCHAR(1000) NOT NULL,
                                        name VARCHAR(100) NOT NULL,
                                        extention NVARCHAR(20) NOT NULL,
                                        date_created DATETIME NOT NULL ,
                                        folder NVARCHAR(1000) NOT NULL,
                                        PRIMARY KEY (search_key) 
                                     )";
                    var createCommand = connection.CreateCommand();
                    createCommand.CommandText = CREATE_INDEX_TABLE_COMMAND;
                    createCommand.ExecuteNonQuery();
                }
                catch { }

                using (var transaction = connection.BeginTransaction())
                {
                    foreach (var idx in indexes)
                    {
                        var command = connection.CreateCommand();
                        command.CommandText = INSERT_COMMAND;
                        command.Transaction = transaction;
                        command.Parameters.AddWithValue("@key", idx.SearchKey);
                        command.Parameters.AddWithValue("@name", idx.Name);
                        command.Parameters.AddWithValue("@extetion", idx.Extention);
                        command.Parameters.AddWithValue("@dateCreated", DateTime.Now);
                        command.Parameters.AddWithValue("@folder", idx.Folder);
                        command.ExecuteNonQuery();
                    }
                    transaction.Commit();
                }
            }
        }

        public async Task SetSelection(Selection selection)
        {
            const string INSERT_COMMAND = @"INSERT INTO Selections (search_key,count)
                        VALUES(@key,@count)";

            using (var connection = new SqliteConnection(_connectionString))
            {
                connection.Open();

                // replace this with sys table query
                try
                {
                    await CreateSelectionsTable(connection);
                }
                catch { }


                var command = connection.CreateCommand();
                command.CommandText = INSERT_COMMAND;
                command.Connection = connection;
                command.Parameters.AddWithValue("@key", selection.SearchKey);
                command.Parameters.AddWithValue("@count", selection.Count);
                await command.ExecuteNonQueryAsync();
            }
        }
        public async Task ClearnSelections()
        {
            using (var connection = new SqliteConnection(_connectionString))
            {
                connection.Open();

                var DROP_INDEX_TABLE_COMMAND = @"DROP TABLE Indexes";
                var dropCommand = connection.CreateCommand();
                dropCommand.CommandText = DROP_INDEX_TABLE_COMMAND;
                await dropCommand.ExecuteNonQueryAsync();

                var CREATE_INDEX_TABLE_COMMAND = @"CREATE TABLE Indexes (
                                        search_key NVARCHAR(1000) NOT NULL,
                                        name VARCHAR(100) NOT NULL,
                                        extention NVARCHAR(20) NOT NULL,
                                        date_created DATETIME NOT NULL ,
                                        folder NVARCHAR(1000) NOT NULL,
                                        PRIMARY KEY (search_key) 
                                     )";
                var createCommand = connection.CreateCommand();
                createCommand.CommandText = CREATE_INDEX_TABLE_COMMAND;
                await createCommand.ExecuteNonQueryAsync();

                await CreateSelectionsTable(connection);
            }

        }
        private async Task CreateSelectionsTable(SqliteConnection connection = null)
        {
            Func<SqliteConnection, Task> creationAction = async (SqliteConnection conn) =>
            {
                var CREATE_SELECTIONS_TABLE_COMMAND = @"CREATE TABLE Selections (
                                        search_key NVARCHAR(1000) NOT NULL,
                                        count INT NOT NULL
                                     )";
                var createCommand = conn.CreateCommand();
                createCommand.CommandText = CREATE_SELECTIONS_TABLE_COMMAND;
                await createCommand.ExecuteNonQueryAsync();
            };

            if (connection == null)
            {
                using (connection = new SqliteConnection(_connectionString))
                {
                    connection.Open();
                    await creationAction(connection);
                    return;
                }
            }
            await creationAction(connection);
        }

        public async Task UpdateIndexes(IEnumerable<UpdateIndex> updates)
        {
            ///const string SELECT_COMMAND = "SELECT search_key FROM Indexes WHERE search_key = @old LIMIT 1";
            const string INSERT_COMMAND = @"INSERT INTO Indexes (search_key,name,extention,date_created,folder)
                        VALUES(@key,@name,@extetion,@dateCreated,@folder)";

            const string DELETE_COMMAND = "DELETE FROM Indexes where search_key = @old";
            using (var connection = new SqliteConnection(_connectionString))
            {
                connection.Open();

                foreach (var idx in updates)
                {
                    var deleteCommand = connection.CreateCommand();
                    deleteCommand.Parameters.AddWithValue("@old", idx.SearchKey);
                    deleteCommand.CommandText = DELETE_COMMAND;
                    await deleteCommand.ExecuteNonQueryAsync();

                    // var selectCommand = connection.CreateCommand();
                    // selectCommand.Parameters.Add(new SqliteParameter("@old", idx.SearchKey));
                    // selectCommand.CommandText = SELECT_COMMAND;
                    // var reader = selectCommand.ExecuteReader();
                    // var update = reader.Read();

                    if (!string.IsNullOrEmpty(idx.NewSearchKey))
                    {
                        var command = connection.CreateCommand();
                        command.CommandText = INSERT_COMMAND;
                        command.Parameters.AddWithValue("@key", idx.NewSearchKey);
                        command.Parameters.AddWithValue("@name", idx.Name);
                        command.Parameters.AddWithValue("@extetion", idx.Extention);
                        command.Parameters.AddWithValue("@dateCreated", DateTime.Now);
                        command.Parameters.AddWithValue("@folder", idx.Folder);
                        command.ExecuteNonQuery();
                    }
                }
            }
        }

        public async Task UpdateIndexes(UpdateIndex update)
        {
            await UpdateIndexes(new List<UpdateIndex> { update });
        }

        public async Task ClearMappings()
        {
            using (var connection = new SqliteConnection(_connectionString))
            {
                connection.Open();

                var DROP_INDEX_TABLE_COMMAND = @"DROP TABLE Indexes";
                var dropCommand = connection.CreateCommand();
                dropCommand.CommandText = DROP_INDEX_TABLE_COMMAND;
                dropCommand.ExecuteNonQuery();

                var CREATE_INDEX_TABLE_COMMAND = @"CREATE TABLE Indexes (
                                        search_key NVARCHAR(1000) NOT NULL,
                                        name VARCHAR(100) NOT NULL,
                                        extention NVARCHAR(20) NOT NULL,
                                        date_created DATETIME NOT NULL ,
                                        folder NVARCHAR(1000) NOT NULL,
                                        PRIMARY KEY (search_key) 
                                     )";
                var createCommand = connection.CreateCommand();
                createCommand.CommandText = CREATE_INDEX_TABLE_COMMAND;
                await createCommand.ExecuteNonQueryAsync();
            }
        }
    }
}