using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Diagnostics;

namespace NanoMigratorLibrary
{
	public class Migrator
	{
		readonly static string[] MIGRATION_FILE_EXTENSIONS = { ".sql", ".exe", ".cmd", ".bat", ".json" };

		readonly string migrationsTable;
		readonly IDictionary<string, ConnectionData> connectionStrings;
		bool simulate;

		readonly public MetaMigration[] metaMigrations;

		readonly Action<string> log;

		public Migrator(Config config, string group, Action<string> log)
		{
			this.log = log;

			this.migrationsTable = config.migrationsTable;
			if (group != null) this.connectionStrings = config.connectionGroups[group].ToDictionary(x => x.Key, x => new ConnectionData(x.Value.driver, x.Value.toConnectionString(Path.GetDirectoryName(config.filePath))));

			var migrations = loadMigrations(Path.Combine(Path.GetDirectoryName(config.filePath) ?? "", config.migrationsDirectory));
			ensureNoDuplicates(migrations.Where(x => x.isForward).ToArray());
			ensureNoDuplicates(migrations.Where(x => !x.isForward).ToArray());
			metaMigrations = migrations
								.Where(x => x.isForward)
								.Select(x => new MetaMigration
								{
									connectionName = x.connectionName,
									index = x.index,
									up = x,
									down = migrations.FirstOrDefault(y => !y.isForward && y.connectionName == x.connectionName && y.index == x.index)
								})
								.OrderBy(x => x.index)
								.ThenBy(x => x.connectionName)
								.ToArray();
		}

		public void migrate(int version, bool simulate)
		{
			this.simulate = simulate;

			if (metaMigrations.Length == 0) return;

			if (version == -1) version = metaMigrations[metaMigrations.Length - 1].index;

			var curVersions = metaMigrations
								.Select(x => x.connectionName)
								.Distinct()
								.Select(connectionName => new
								{
									connectionName,
									curVer = connectionStrings.ContainsKey(connectionName) ? getCurVersion(connectionStrings[connectionName]) : 0
								})
								.ToDictionary(x => x.connectionName, x => x.curVer);

			var migrationsRev = getMigrationsRev(curVersions, version);
			var migrationsFor = getMigrationsFor(curVersions, version);

			foreach (var connectionName in migrationsRev.Select(x => x.connectionName).Distinct().OrderBy(x => x))
			{
				log("Revert " + connectionName + ": " + curVersions[connectionName] + " => " + version);
			}
			foreach (var connectionName in migrationsFor.Select(x => x.connectionName).Distinct().OrderBy(x => x))
			{
				log("Forward " + connectionName + ": " + curVersions[connectionName] + " => " + version);
			}

			applyMigrationsRev(migrationsRev);
			applyMigrationsFor(migrationsFor);
		}

		public Dictionary<string, int> status()
		{
			log("Connections:");
			foreach (var connectionName in connectionStrings.Keys)
			{
				log("\t" + connectionName + ": " + connectionStrings[connectionName].driver + ":" + connectionStrings[connectionName].connectionString);
			}
			log("");

			var r = new Dictionary<string, int>();
			foreach (var connectionName in connectionStrings.Keys)
			{
				if (metaMigrations.Any(x => x.connectionName == connectionName))
				{
					var curVer = getCurVersion(connectionStrings[connectionName]);
					var lastVer = metaMigrations.Where(x => x.connectionName == connectionName).Select(x => x.index).Max();
					log(connectionName + ": " + curVer + " / " + lastVer);

					r[connectionName] = curVer;
				}
			}

			return r;
		}

		void applyMigrationsRev(PreparedMigration[] preparedMigrations)
		{
			foreach (var pm in preparedMigrations)
			{
				log("\nDOWN " + pm.connectionName + " #" + pm.index + (pm.migration != null ? ": " + pm.migration.description : ""));
				if (pm.migration == null)
				{
					throw new MigratorException("Down file for migration #" + pm.index + " is not defined.");
				}

				var prevIndex = metaMigrations
									.Where(x => x.connectionName == pm.connectionName)
									.Where(x => x.index < pm.index)
									.DefaultIfEmpty()
									.Max(x => (int?)x.index) ?? 0;

				Debug.Assert(pm.migration != null);
				Debug.Assert(!pm.migration.isForward);

				if (!connectionStrings.ContainsKey(pm.connectionName)) throw new MigratorException("Unknow connection '" + pm.connectionName + "'.");

				applyMigration(connectionStrings[pm.connectionName], pm.migration, prevIndex);
			}
		}

		void applyMigrationsFor(PreparedMigration[] preparedMigrations)
		{
			foreach (var pm in preparedMigrations)
			{
				log("\nUP " + pm.connectionName + " #" + pm.index + (pm.migration != null ? ": " + pm.migration.description : ""));

				Debug.Assert(pm.migration != null);
				Debug.Assert(pm.migration.isForward);

				if (!connectionStrings.ContainsKey(pm.connectionName)) throw new MigratorException("Unknow connection '" + pm.connectionName + "'.");

				applyMigration(connectionStrings[pm.connectionName], pm.migration, pm.index);
			}
		}

		PreparedMigration[] getMigrationsRev(IDictionary<string, int> curVersions, int version)
		{
			return metaMigrations.GroupBy(x => x.connectionName)
									.Select
									(
										g => g.Where(x => x.index <= curVersions[g.Key] && x.index > version)
												.Select(x => new PreparedMigration
												{
													connectionName = g.Key,
													index = x.index,
													migration = x.down
												})
									)
									.SelectMany(x => x)
									.OrderByDescending(x => x.index)
									.ThenByDescending(x => x.connectionName)
									.ToArray();
		}

		PreparedMigration[] getMigrationsFor(IDictionary<string, int> curVersions, int version)
		{
			return metaMigrations.GroupBy(x => x.connectionName)
									.Select
									(
										g => g.Where(x => x.index > curVersions[g.Key] && x.index <= version)
												.Select(x => new PreparedMigration
												{
													connectionName = g.Key,
													index = x.index,
													migration = x.up
												})
									)
									.SelectMany(x => x)
									.OrderBy(x => x.index)
									.ThenBy(x => x.connectionName)
									.ToArray();
		}

		void applyMigration(ConnectionData connectionData, Migration migration, int resultVersion)
		{
			if (simulate)
			{
				log("SIMULATE " + migration.filePath);
				return;
			}

			switch (Path.GetExtension(migration.filePath))
			{
				case ".sql":
					if (!connectionData.isSupportSQL()) throw new MigratorException("SQL is not supported for this connection.");
					applyMigrationSQL(connectionData, migration.filePath);
					break;

				case ".json":
					if (!connectionData.isSupportJSON()) throw new MigratorException("JSON is not supported for this connection.");
					applyMigrationSQL(connectionData, migration.filePath);
					break;

				case ".exe":
				case ".cmd":
				case ".bat":
					log(migration.filePath + " \"" + connectionData.driver + ":" + connectionData.connectionString + "\"");
					var exitCode = runCommand(migration.filePath, "\"" + connectionData.driver + ":" + connectionData.connectionString + "\"", connectionStrings.ToDictionary(x => "MIGRATION_DB_" + x.Key, x => x.Value.driver.ToString() + ":" + x.Value.connectionString));
					if (exitCode != 0)
					{
						throw new MigratorException("None-zero exit code " + exitCode + ".");
					}
					break;

				default:
					throw new Exception("Unexpected migration file extension '" + migration.filePath + "'.");
			}

			using (var conn = connectionData.createConnection())
			{
				conn.setVersion(migrationsTable, resultVersion);
			}
		}

		void applyMigrationSQL(ConnectionData connectionData, string sqlFilePath)
		{
			using (var conn = connectionData.createConnection())
			{
				conn.ensureMigrationsTableExists(migrationsTable);

				var sql = File.ReadAllText(sqlFilePath).TrimEnd();
				foreach (var connectionName in connectionStrings.Keys)
				{
					sql = sql.Replace("{" + connectionName + "}", connectionStrings[connectionName].database);
				}

				log(sql);
				try
				{
					var n = conn.executeCommand(sql);
					log("Rows affected: " + n);
				}
				catch (Exception e)
				{
					throw new MigratorException(e.Message);
				}
			}
		}

		int getCurVersion(ConnectionData connectionData)
		{
			using (var conn = connectionData.createConnection())
			{
				return conn.getVersion(migrationsTable);
			}
		}

		static int runCommand(string exeFilePath, string arguments, IDictionary<string, string> env)
		{
			if (new FileInfo(exeFilePath).Length == 0) return 0;

			var p = new Process();

			p.StartInfo.FileName = exeFilePath;
			p.StartInfo.UseShellExecute = false;
			p.StartInfo.Arguments = arguments;

			foreach (var k in env.Keys) p.StartInfo.EnvironmentVariables[k] = env[k];

			p.Start();
			p.WaitForExit();

			var r = p.ExitCode;

			p.Close();

			return r;
		}

		static Migration[] loadMigrations(string dir)
		{
			return Directory.GetFiles(dir)
					.Where(x => MIGRATION_FILE_EXTENSIONS.Contains(Path.GetExtension(x)))
					.Select(x => new Migration(x))
					.Concat(Directory.GetDirectories(dir).Select(loadMigrations).SelectMany(x => x))
					.ToArray();
		}

		static void ensureNoDuplicates(Migration[] migrations)
		{
			for (var i = 0; i < migrations.Length; i++)
			{
				var m = migrations[i];
				var dup = migrations.Skip(i + 1).FirstOrDefault(x => x.index == m.index);
				if (dup != null)
				{
					throw new MigratorException("ERROR: duplicate migration files detected: '" + m.filePath + "' and '" + dup.filePath + "'.");
				}
			}
		}
	}
}
