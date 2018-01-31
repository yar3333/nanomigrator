using System.Linq;
using MongoDB.Bson;
using MongoDB.Driver;

namespace NanoMigratorLibrary
{
	public class UniversalMongoConnection : IUniversalConnection
	{
		protected readonly IMongoDatabase db;

		public UniversalMongoConnection(string connectionString)
		{
			var client = new MongoClient(connectionString);
			db = client.GetDatabase(MongoUrl.Create(connectionString).DatabaseName);
		}

		public void Dispose() {}

		public void ensureMigrationsTableExists(string migrationsTable)
		{
			if (db.GetCollection<MigrationsTableRow>(migrationsTable).AsQueryable().Where(x => x.name == "index").Count() == 0)
			{
				db.GetCollection<MigrationsTableRow>(migrationsTable).InsertOne(new MigrationsTableRow { name = "index", value = "0" });
			}
		}

		public int getVersion(string migrationsTable)
		{
			var row = db.GetCollection<MigrationsTableRow>(migrationsTable).AsQueryable().SingleOrDefault(x => x.name == "index");
			return row != null ? int.Parse(row.value) : 0;
		}

		public void setVersion(string migrationsTable, int version)
		{
			db.GetCollection<MigrationsTableRow>(migrationsTable)
				.UpdateOne(Builders<MigrationsTableRow>.Filter.Where(x => x.name == "index"), Builders<MigrationsTableRow>.Update.Set(x => x.value, version.ToString()));
		}

		public int executeCommand(string text)
		{
			var command = new JsonCommand<BsonDocument>(text);
			var result = db.RunCommand(command);
			return (int)result["ok"].AsDouble;
		}
	}

	class MigrationsTableRow
	{
		public ObjectId _id { get; set; }

		public string name { get; set; }
		public string value { get; set; }
	}
}