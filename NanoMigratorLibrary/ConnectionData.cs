using System;
using System.Data.Common;
using System.Data.SqlClient;
using MySql.Data.MySqlClient;

namespace NanoMigratorLibrary
{
	public class ConnectionData
	{
		public readonly DriverType driver;
		public readonly string connectionString;

		public readonly string host;
		public readonly uint port;
		public readonly string user;
		public readonly string password;
		public readonly string database;

		public ConnectionData(DriverType driver, string connectionString)
		{
			this.driver = driver;
			this.connectionString = connectionString;

			switch (driver)
			{
				case DriverType.MySql:
				{
					var builder = new MySqlConnectionStringBuilder(connectionString);
					host = builder.Server;
					port = builder.Port;
					user = builder.UserID;
					password = builder.Password;
					database = builder.Database;
					break;
				}
				
				case DriverType.SqlServer:
				{
					var builder = new SqlConnectionStringBuilder(connectionString);
					host = builder.DataSource.Split(',')[0];
					port = builder.DataSource.Split(',').Length >= 2 ? uint.Parse(builder.DataSource.Split(',')[1]) : 1433;
					user = builder.UserID;
					password = builder.Password;
					database = builder.InitialCatalog;
					break;
				}
				
				default:
					throw new Exception("Unknow driver: " + driver + ".");
			}
		}

		public DbConnection createConnection()
		{
			switch (driver)
			{
				case DriverType.MySql:     return new MySqlConnection(connectionString);
				case DriverType.SqlServer: return new SqlConnection(connectionString);
			}
			throw new Exception("Unknow driver: " + driver + ".");
		}
	}
}
