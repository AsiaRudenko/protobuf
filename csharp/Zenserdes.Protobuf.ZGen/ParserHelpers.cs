using Google.Protobuf.Reflection;

using System;
using System.Collections.Generic;
using System.IO;

#nullable enable

namespace Zenserdes.Protobuf.ZenGen
{
	public static class ParserHelpers
	{
		public static List<FileDescriptorProto>? Parse(string file, string @namespace)
		{
			// https://github.com/protobuf-net/protobuf-net/blob/master/src/protogen/Program.cs#L136

			var filePath = Path.GetFullPath(file);
			var directoryPath = Path.GetDirectoryName(filePath);

			var fileDescriptorSet = new FileDescriptorSet
			{
				DefaultPackage = @namespace
			};

			fileDescriptorSet.AddImportPath(directoryPath);

			if (!fileDescriptorSet.Add(file, true))
			{
				Console.WriteLine($"Couldn't add file: {file}");
				return null;
			}

			fileDescriptorSet.Process();
			var errors = fileDescriptorSet.GetErrors();

			if (errors.Length > 0)
			{
				foreach (var error in errors)
				{
					throw new InvalidOperationException(error.ToString());
				}
			}

			return fileDescriptorSet.Files;
		}

		public static FileDescriptorProto? ParseText(string fileText, string @namespace)
		{
			// lol :)
			// TODO: figure out how to parse proto files in memory
			var tempName = Path.GetTempFileName();
			File.WriteAllText(tempName, fileText);

			Directory.SetCurrentDirectory(Path.GetDirectoryName(tempName));
			var result = Parse(Path.GetFileName(tempName), @namespace)?[0];

			File.Delete(tempName);
			return result;
		}
	}
}