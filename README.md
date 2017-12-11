NanoMigrator
============

NanoMigrator is a C# tool to apply migrations (up and down) to database. `MySQL` and `MS SQL Server` are supported.

Features:

	* simple: just a SQL/CMD/BAT/EXE files in specific folder;
	* support several environments (custom connection groups, such as `development` and `production`);
	* support several databases per environment.


Installation
------------

First, install NuGet. Then, install NanoMigrator from the package manager console:
```
PM> Install-Package NanoMigrator
```


Using
-----

First, create config file `databases.nmjson` like next:
```json
{
  "migrationsDirectory": "migrations",
  "migrationsTable": "migrations",
  "connectionGroups": {
    "development": {
    
      "TestSqlServerLocalDatabase": {
        "driver": "SqlServer",
        "connectionString": "Data Source=(localdb)\\MSSQLLocalDB;AttachDbFilename=c:\\mydir\\my_database_file.mdf;User ID=MY_LOGIN;Password=MY_PASSWORD;Initial Catalog=MY_DATABASE"
      },
      
      "TestSqlServerFromAppConfig":
      {
        "driver": "SqlServer",
        "appConfigFile":"mydir\\app.config",
        "connectionName":"MainConnection"
      },
      
    
      "TestMySqlDatabase": {
        "driver": "MySql",
        "connectionString": "server=localhost;user id=root;password=123456;database=MY_DATABASE;persistsecurityinfo=True;charset=utf8"
      }
    }
  }
}
```

Create test migration UP file `migrations\0001_TestSqlServerLocalDatabase_My_first_migration.sql` (change `TestSqlServerLocalDatabase` to your connection name):
```sql
select 1;
```

Create test migration DOWN file `migrations\0001_TestSqlServerLocalDatabase_My_first_migration_DOWN.sql` (change `TestSqlServerLocalDatabase` to your connection name):
```sql
select 1;
```

Next, test connections:
```
PM> NanoMigrator status
```
