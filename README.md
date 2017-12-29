NanoMigrator
============

NanoMigrator is a C# tool to apply migrations (up and down) to database. `MySQL` and `MS SQL Server` are supported.

Features:

  * simple: just a SQL/CMD/BAT/EXE files in specific folder;
  * support several environments (custom connection groups, such as `development` and `production`);
  * support several databases per environment.

Also, you can use [GUI](http://nanomigrator.haqteam.com/).

[Make a donation to help improve NanoMigrator CLI & GUI =>](https://www.paypal.me/nanomigrator/8USD?locale.x=en_US&country.x=RU)

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
SELECT 1;
```

Create test migration DOWN file `migrations\0001_TestSqlServerLocalDatabase_My_first_migration_DOWN.sql` (change `TestSqlServerLocalDatabase` to your connection name):
```sql
SELECT 2;
```

Next, test connections:
```
PM> NanoMigrator status
```

To simulate migration, run:
```
PM> NanoMigrator simulate
```

To migrate to the last version, run:
```
PM> NanoMigrator migrate
```

To revert all migrations, run:
```
PM> NanoMigrator migrate 0
```

Run `NanoMigrator` without arguments to get more help.

Migration files
---------------

Migration files are `*.sql/*.cmd/*.bat/*.exe` files in folder, specified in `migrationsDirectory` parameter of config file (subfolders are also scanned).
Each file name must be in the next format:
```
index_name_description_postfix.ext
```
Where:

  * `index` is a positive integer number (leading zeroes are possible);
  * `name` is connection name (`TestSqlServerLocalDatabase`, `TestSqlServerFromAppConfig` or `TestMySqlDatabase` for config file listed above);
  * `description` is a transaction description text;
  * `postfix` is a optional part (may be: `UP`/`FOR` for forward migration file or `DOWN`/`REV` for revert migration file); when ommited then forward migration file is assumed.

NanoMigrator run `*.cmd/*.bat/.exe` migrations in the next maner:

  * argument %1 - active connection string in `driver:connectionString` format (for example: `MySql:server=localhost;user id=root;password=123456;database=MY_DATABASE;persistsecurityinfo=True;charset=utf8`);
  * environment variables `MIGRATION_DB_<CONNECTION_NAME>` - connection strings to all known databases in `driver:connectionString` format.
