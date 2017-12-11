using System;
using System.IO;
using System.Linq;

namespace NanoMigratorLibrary
{
	public class Migration
	{
		static readonly string[] upKeywords = { "up", "for", "forward" };
		static readonly string[] downKeywords = { "down", "rev", "revert" };

		public readonly string filePath;
		public readonly int index;
		public readonly string connectionName;
		public readonly string description;
		public readonly bool isForward;

		public Migration(string filePath)
		{
			this.filePath = filePath;

			var parts = Path.GetFileNameWithoutExtension(filePath).Split('_'); // index_[connectionName]_description[_for/rev]
			if (parts.Length < 2) throw new Exception();
			index = int.Parse(parts[0]);
			if (index <= 0) throw new Exception("File '" + filePath + "': version must be a positive integer.");
			connectionName = parts[1];

			var descriptionWords = parts.Skip(2).ToList();
			if (descriptionWords.Count > 0)
			{
				isForward = true;
				if (downKeywords.Contains(descriptionWords[descriptionWords.Count - 1].ToLowerInvariant()))
				{
					isForward = false;
					descriptionWords.RemoveAt(descriptionWords.Count - 1);
				}
				else
				if (upKeywords.Contains(descriptionWords[descriptionWords.Count - 1].ToLowerInvariant()))
				{
					descriptionWords.RemoveAt(descriptionWords.Count - 1);
				}
			}
			description = string.Join(" ", descriptionWords);
		}
	}
}
