using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Data.Common;

namespace NanoMigratorLibrary.DatabaseDrivers
{
	static class DbConnectionTools
	{
		public static List<string> getTables(this DbConnection conn)
		{
			return conn.GetSchema("Tables").Rows.Cast<DataRow>().Select(row => (string)row[2]).ToList();
		}
	}
}
