using System;

namespace NanoMigratorLibrary
{
	public interface IDatabaseConnection : IDisposable
	{
		void ensureMigrationsTableExists(string migrationsTable);
		int getVersion(string migrationsTable);
		void setVersion(string migrationsTable, int version);
		int executeCommand(string text);
	}
}
