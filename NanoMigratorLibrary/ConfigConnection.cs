using System.IO;
using System.Xml;
using Newtonsoft.Json;

namespace NanoMigratorLibrary
{
	public class ConfigConnection
	{
		public DriverType driver { get; set; }

		[JsonProperty(NullValueHandling = NullValueHandling.Ignore)]
		public string appConfigFile { get; set; }

		[JsonProperty(NullValueHandling = NullValueHandling.Ignore)]
		public string connectionName { get; set; }

		[JsonProperty(NullValueHandling = NullValueHandling.Ignore)]
		public string connectionString { get; set; }

		public string toConnectionString()
		{
			if (string.IsNullOrEmpty(appConfigFile) || string.IsNullOrEmpty(connectionName)) return normalizeConnectionString(connectionString);

			var doc = new XmlDocument();
			try
			{
				doc.Load(appConfigFile);
			}
			catch (FileNotFoundException)
			{
				throw new MigratorException("ERROR: XML file '" + appConfigFile + "' is not found.");
			}
			catch (XmlException e)
			{
				throw new MigratorException("ERROR: Invalid XML file '" + appConfigFile + "' - " + e.Message);
			}

			var nodes = doc.SelectNodes("/configuration/connectionStrings/add");
			foreach (XmlElement node in nodes)
			{
				if (node.GetAttribute("name") != connectionName) continue;
				return normalizeConnectionString(node.GetAttribute("connectionString"));
			}

			throw new MigratorException("ERROR: Connection '" + connectionName + "' is not found in file  '" + appConfigFile + "'.");
		}

		public string toDetails()
		{
			if (string.IsNullOrEmpty(appConfigFile) || string.IsNullOrEmpty(connectionName)) return driver + ":" + connectionString;
			return driver + ":" + appConfigFile + " | " + connectionName;
		}

		public ConfigConnection clone()
		{
			return new ConfigConnection
			{
				driver = driver,
				appConfigFile = appConfigFile,
				connectionName = connectionName,
				connectionString = connectionString
			};
		}

		string normalizeConnectionString(string s)
		{
			var builder = new System.Data.Common.DbConnectionStringBuilder();
			builder.ConnectionString = s.Trim();
			if (builder.ContainsKey("provider connection string"))
			{
				s = (string)builder["provider connection string"];
			}
			return s;
		}
	}
}
