using System;
using System.Data.SqlClient;
using MongoDB.Driver;
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
				
				case DriverType.MongoDB:
				{
					var builder = new MongoUrl(connectionString);
					host = builder.Server.Host;
					port = (uint)builder.Server.Port;
					user = builder.Username;
					password = builder.Password;
					database = builder.DatabaseName;
					break;
				}
				
				default:
					throw new Exception("Unknow driver: " + driver + ".");
			}
		}

		public IUniversalConnection createConnection()
		{
			switch (driver)
			{
				case DriverType.MySql:     return new UniversalSqlConnection(new MySqlConnection(connectionString));
				case DriverType.SqlServer: return new UniversalSqlConnection(new SqlConnection(connectionString));
				case DriverType.MongoDB:   return new UniversalMongoConnection(connectionString);
			}
			throw new Exception("Unknow driver: " + driver + ".");
		}

		public bool isSupportSQL()
		{
			switch (driver)
			{
				case DriverType.MySql:     return true;
				case DriverType.SqlServer: return true;
				case DriverType.MongoDB:   return false;
			}
			throw new Exception("Unknow driver: " + driver + ".");
		}

		public bool isSupportJSON()
		{
			switch (driver)
			{
				case DriverType.MySql:     return false;
				case DriverType.SqlServer: return false;
				case DriverType.MongoDB:   return true;
			}
			throw new Exception("Unknow driver: " + driver + ".");
		}
	}
}
