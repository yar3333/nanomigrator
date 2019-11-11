using System.Collections.Generic;
using Dapper;

namespace NanoMigratorLibrary
{
	public class OracleConnection : BaseSqlConnection
    {
        public OracleConnection(string connectionString) : base(new Oracle.ManagedDataAccess.Client.OracleConnection(connectionString)) {}

        override public void ensureMigrationsTableExists(string migrationsTable)
		{
			if (!getTables().Contains(migrationsTable))
			{
				connection.Execute(@"CREATE TABLE """ + migrationsTable + @""" ("
								            + @"""name"" VARCHAR2(100) NOT NULL"
								            + @", ""value"" VARCHAR2(1000)"
								            + @", CONSTRAINT """ + migrationsTable + @"_pk"" PRIMARY KEY (""name"")"
                                        + ")");
				connection.Execute(@"INSERT INTO """ + migrationsTable + @""" VALUES ('index', '0')");
			}
		}

        override public int getVersion(string migrationsTable)
		{
			if (!getTables().Contains(migrationsTable)) return 0;
			return connection.ExecuteScalar<int>(@"SELECT ""value"" FROM """ + migrationsTable + @""" WHERE ""name"" = 'index'");
		}

        override public void setVersion(string migrationsTable, int version)
		{
			connection.Execute(@"UPDATE """ + migrationsTable + @""" SET ""value""='" + version + @"' WHERE ""name""='index'");
		}

        override public int executeCommand(string text)
		{
			var command = connection.CreateCommand();
			command.CommandTimeout = int.MaxValue;
			command.CommandText = text;
			return command.ExecuteNonQuery();
		}

		List<string> getTables()
        {
            var r = new List<string>();
            var reader = connection.ExecuteReader("SELECT table_name FROM user_tables");
            while (reader.Read())
            {
                r.Add(reader["table_name"].ToString());
            }
            return r;
        }
	}
}
