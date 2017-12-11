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

		public string database
		{
			get
			{
				switch (driver)
				{
					case DriverType.MySql: return new MySqlConnectionStringBuilder(connectionString).Database;
					case DriverType.SqlServer: return new SqlConnectionStringBuilder(connectionString).InitialCatalog;
				}
				throw new Exception("Unknow driver: " + driver + ".");
			}
		}

		public ConnectionData(DriverType driver, string connectionString)
		{
			this.driver = driver;
			this.connectionString = connectionString;
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
