using Newtonsoft.Json;
using Newtonsoft.Json.Converters;

namespace NanoMigratorLibrary
{
	[JsonConverter(typeof(StringEnumConverter))]
	public enum DriverType
	{
		MySql = 0,
		SqlServer = 1
	}
}
