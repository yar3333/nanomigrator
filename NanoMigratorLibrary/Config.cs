using System;
using System.Collections.Generic;
using System.IO;
using Newtonsoft.Json;

namespace NanoMigratorLibrary
{
	public class Config
	{
		[JsonIgnore]
		public string filePath;

		public string migrationsDirectory { get; set; }
		public string migrationsTable { get; set; }

		public Dictionary<string, Dictionary<string, ConfigConnection>> connectionGroups { get; set; }

		public Config(string filePath, string migrationsDirectory, string migrationsTable, Dictionary<string, Dictionary<string, ConfigConnection>> connectionGroups)
		{
			this.filePath = filePath;
			this.migrationsDirectory = migrationsDirectory;
			this.migrationsTable = migrationsTable;
			this.connectionGroups = connectionGroups;
		}

		public Config(string filePath = null)
		{
			if (filePath == null)
			{
				filePath = Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.MyDocuments), "databases.nmjson");

				if (!File.Exists(filePath))
				{
					connectionGroups = new Dictionary<string, Dictionary<string, ConfigConnection>>();
					connectionGroups.Add("development", new Dictionary<string, ConfigConnection>());
					connectionGroups.Add("production", new Dictionary<string, ConfigConnection>());

					migrationsDirectory = "migrations";
					migrationsTable = "migrations";
				}
				else
				{
					load(filePath);
				}
			}
			else
			{
				if (!File.Exists(filePath)) throw new FileNotFoundException("File '" + filePath + "' is not found.");
				load(filePath);
			}

			this.filePath = filePath;
		}

		void load(string filePath)
		{
			JsonConvert.PopulateObject(File.ReadAllText(filePath), this);
		}

		public Config clone()
		{
			var connectionGroupsClone = new Dictionary<string, Dictionary<string, ConfigConnection>>();
			foreach (var kv in connectionGroups)
			{
				connectionGroupsClone.Add(kv.Key, cloneGroup(kv.Value));
			}
			return new Config(filePath, migrationsDirectory, migrationsTable, connectionGroupsClone);
		}

		static Dictionary<string, ConfigConnection> cloneGroup(Dictionary<string, ConfigConnection> group)
		{
			var groupClone = new Dictionary<string, ConfigConnection>();
			foreach (var kv in group)
			{
				groupClone.Add(kv.Key, kv.Value.clone());
			}
			return groupClone;
		}
	}
}
