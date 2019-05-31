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
					database = builder.Database;
					break;
				}
				
				case DriverType.SqlServer:
				{
					var builder = new SqlConnectionStringBuilder(connectionString);
					database = builder.InitialCatalog;
					break;
				}
				
				case DriverType.MongoDB:
				{
					var builder = new MongoUrl(connectionString);
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
