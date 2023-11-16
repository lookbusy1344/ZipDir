﻿using System.IO.Compression;

namespace ZipDir;

internal static class Searcher
{
	/// <summary>
	/// Find all .zip files in this folder, and list their contents
	/// </summary>
	public static void SearchFolder(string path, string fileSpec, IReadOnlyList<string> exclude)
	{
		var allFiles = Directory.GetFiles(path, fileSpec, new EnumerationOptions { IgnoreInaccessible = true, RecurseSubdirectories = true, MatchCasing = MatchCasing.CaseInsensitive });

		// filter out any files that match the exclude pattern. This is a micro-optimization
		var files = exclude.Count switch
		{
			0 => allFiles,
			1 => allFiles.Where(file => !file.Contains(exclude[0], StringComparison.OrdinalIgnoreCase))
				.ToArray(),
			_ => allFiles.Where(file => !exclude.Any(toExclude => file.Contains(toExclude, StringComparison.OrdinalIgnoreCase)))
				.ToArray()
		};

		Program.WriteMessage($"{files.Length} zip file(s) identified...", true);

		// for each file
		foreach (var file in files)
		{
			try
			{
				var zip = new ZipInternals(file);
				zip.CheckZipFile();
			}
			catch
			{
				Program.WriteMessage($"Error in zip: {file}");
			}
		}
	}

	/// <summary>
	/// Expand the folder name to a full path, removing things like ..\..
	/// </summary>
	public static string NormalizeFolder(string folderName)
	{
		var dirinfo = new DirectoryInfo(folderName);
		return dirinfo.FullName;
	}
}

internal class ZipInternals(string path, CancellationToken token = default)
{
	/// <summary>
	/// Wrapper around zip search to handle nested zips
	/// </summary>
	public void CheckZipFile()
	{
		using var archive = ZipFile.OpenRead(path);
		RecursiveArchiveCheck(path, archive);
	}

	/// <summary>
	/// Given a zip archive, loop through and list the contents. Recursively calls for nested zips
	/// </summary>
	private void RecursiveArchiveCheck(string containerName, ZipArchive archive)
	{
		foreach (var nestedEntry in archive.Entries)
		{
			token.ThrowIfCancellationRequested();

			if (nestedEntry.FullName.Length == 0) continue;
			var lastChar = nestedEntry.FullName[^1];

			if (nestedEntry.FullName.EndsWith(".zip", StringComparison.OrdinalIgnoreCase))
			{
				// its another nested zip file, we need to open it and search inside
				var nestedZipName = $"{containerName}/{nestedEntry.FullName}";
				try
				{
					using var nestedStream = nestedEntry.Open();
					using var nestedArchive = new ZipArchive(nestedStream);

					RecursiveArchiveCheck(nestedZipName, nestedArchive);
				}
				catch
				{
					Program.WriteMessage($"Error in nested zip: {nestedZipName}");
				}
			}
			else if (lastChar is not ('/' or '\\')) // ignore folders
			{
				string fileName;
				if (nestedEntry.FullName.Contains('\\'))
				{
					// path separator is '\', so replace with '/' for consistency
					var s = nestedEntry.FullName.Replace('\\', '/');
					fileName = $"{containerName}/{s}";
				}
				else
					fileName = $"{containerName}/{nestedEntry.FullName}";

				Console.WriteLine(fileName);
			}
		}
	}
}
