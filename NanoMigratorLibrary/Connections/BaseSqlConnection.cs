using System;
using System.Data.Common;

namespace NanoMigratorLibrary
{
    public abstract class BaseSqlConnection : IDatabaseConnection
    {
        protected DbConnection connection;

        public BaseSqlConnection(DbConnection connection)
        {
            this.connection = connection;

            try { connection.Open(); } catch (Exception e) { throw new MigratorException(e.Message); }
        }

        public void Dispose()
        {
            connection?.Dispose();
            connection = null;
        }

        public abstract void ensureMigrationsTableExists(string migrationsTable);
        public abstract int getVersion(string migrationsTable);
        public abstract void setVersion(string migrationsTable, int version);
        public abstract int executeCommand(string text);
    }
}
