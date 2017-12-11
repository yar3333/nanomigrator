using System;
using System.Linq;
using NanoMigratorLibrary;

namespace NanoMigrator
{
	class Program
	{
		static void Main(string[] args)
		{
			var nmjsonFilePath = "databases.nmjson";
			var group = "development";

			if (args.Length == 0)
			{
				Console.WriteLine("Using:");
				Console.WriteLine("    nanomigrator [ -f <config> ] [ -g <group>] migrate [ <version> ]");
				Console.WriteLine("    nanomigrator [ -f <config> ] [ -g <group>] status");
				Console.WriteLine("    nanomigrator [ -f <config> ] [ -g <group>] simulate [ <version> ]");
				Console.WriteLine("");
				Console.WriteLine("Options:");
				Console.WriteLine("    <config> - path to configuration file (default is '" + nmjsonFilePath + "');");
				Console.WriteLine("    <group> - connection group name in configuration file (default is '" + group + "');");
				Console.WriteLine("    <version> - integer version number to switch to (0 for revert all, default is most recent version).");
				return;
			}

			for (var i = 0; i < args.Length - 1; i++)
			{
				if (args[i] == "-f")
				{
					nmjsonFilePath = args[i + 1];
					args = args.Take(i).Concat(args.Skip(i + 2)).ToArray();
				}
			}
			for (var i = 0; i < args.Length - 1; i++)
			{
				if (args[i] == "-g")
				{
					group = args[i + 1];
					args = args.Take(i).Concat(args.Skip(i + 2)).ToArray();
				}
			}

			try
			{
				switch (args[0])
				{
					case "migrate":
						if (args.Length > 2) throw new MigratorException("Too many arguments for 'migrate' command.");
						var version1 = args.Length > 1 ? parseVersion(args[1]) : -1;
						new Migrator(new Config(nmjsonFilePath), group, s => Console.WriteLine(s)).migrate(version1, false);
						break;

					case "simulate":
						if (args.Length > 2) throw new MigratorException("Too many arguments for 'simulate' command.");
						var version2 = args.Length > 1 ? parseVersion(args[1]) : -1;
						new Migrator(new Config(nmjsonFilePath), group, s => Console.WriteLine(s)).migrate(version2, true);
						break;

					case "status":
						new Migrator(new Config(nmjsonFilePath), group, s => Console.WriteLine(s)).status();
						break;

					default:
						throw new MigratorException("Unknow command '" + args[0] + "'.");
				}
			}
			catch (MigratorException e)
			{
				Console.WriteLine("ERROR: " + e.Message);
				Environment.Exit(1);
			}
		}

		static int parseVersion(string strVer)
		{
			try
			{
				return int.Parse(strVer);
			}
			catch
			{
				Console.WriteLine("Version must be a integer.");
				Environment.Exit(1);
				return -1;
			}
		}
	}
}
