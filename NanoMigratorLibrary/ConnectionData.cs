using System;
using System.Data.SqlClient;
using System.Text.RegularExpressions;
using MongoDB.Driver;
using MySql.Data.MySqlClient;
using Oracle.ManagedDataAccess.Client;

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
				
				case DriverType.Oracle:
				{
					var builder = new OracleConnectionStringBuilder(connectionString);
                    var re = new Regex("SERVICE_NAME\\s*=\\s*([a-zA-Z0-9.]+)");
                    database = re.Match(builder.DataSource).Groups[1].Value;
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

		public IDatabaseConnection createConnection()
		{
			switch (driver)
			{
				case DriverType.MySql:     return new MySqlConnection(connectionString);
				case DriverType.SqlServer: return new SqlServerConnection(connectionString);
				case DriverType.Oracle:    return new OracleConnection(connectionString);
				case DriverType.MongoDB:   return new MongoConnection(connectionString);
			}
			throw new Exception("Unknow driver: " + driver + ".");
		}

		public bool isSupportSQL()
		{
			switch (driver)
			{
				case DriverType.MySql:     return true;
				case DriverType.SqlServer: return true;
				case DriverType.Oracle: return true;
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
				case DriverType.Oracle:    return false;
				case DriverType.MongoDB:   return true;
			}
			throw new Exception("Unknow driver: " + driver + ".");
		}
	}
}
