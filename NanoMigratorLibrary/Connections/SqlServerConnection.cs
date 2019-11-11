using System.Collections.Generic;
using System.Data;
using System.Data.SqlClient;
using System.Linq;
using Dapper;

namespace NanoMigratorLibrary
{
	public class SqlServerConnection : BaseSqlConnection
    {
        public SqlServerConnection(string connectionString) : base(new SqlConnection(connectionString)) {}

        override public void ensureMigrationsTableExists(string migrationsTable)
		{
			if (!getTables().Contains(migrationsTable))
			{
				connection.Execute("CREATE TABLE `" + migrationsTable + "`"
							+ " ("
								+"`name` varchar(100) NOT NULL"
								+ ", `value` varchar(1000) NULL"
								+ ", PRIMARY KEY (`name`)"
								+ ")"
							+ " ENGINE=InnoDB DEFAULT CHARSET=utf8");
				connection.Execute("INSERT INTO `" + migrationsTable + "` VALUES ('index', '0')");
			}
		}

        override public int getVersion(string migrationsTable)
		{
			if (!getTables().Contains(migrationsTable)) return 0;
			return connection.ExecuteScalar<int>("SELECT `value` FROM `" + migrationsTable + "` WHERE `name` = 'index'");
		}

        override public void setVersion(string migrationsTable, int version)
		{
			connection.Execute("UPDATE `" + migrationsTable + "` SET `value`='" + version + "' WHERE `name`='index'");
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
			return connection.GetSchema("Tables").Rows.Cast<DataRow>().Select(row => (string)row[2]).ToList();
		}
	}
}
