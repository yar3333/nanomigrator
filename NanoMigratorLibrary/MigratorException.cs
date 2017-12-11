using System;

namespace NanoMigratorLibrary
{
	public class MigratorException : Exception
	{
		public MigratorException(string message) : base(message) {}
	}
}
