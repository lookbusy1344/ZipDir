using System.IO.Compression;

namespace ZipDir;

internal static class Searcher
{
	/// <summary>
	/// Find all .zip files in this folder, and list their contents
	/// </summary>
	public static void SearchFolder(string path, string filespec, IReadOnlyList<string> exclude)
	{
		var allfiles = Directory.GetFiles(path, filespec, new EnumerationOptions { IgnoreInaccessible = true, RecurseSubdirectories = true, MatchCasing = MatchCasing.CaseInsensitive });

		// filter out any files that match the exclude pattern. This is a micro-optimization
		var files = exclude.Count switch
		{
			0 => allfiles,
			1 => allfiles.Where(file => !file.Contains(exclude[0], StringComparison.OrdinalIgnoreCase))
				.ToArray(),
			_ => allfiles.Where(file => !exclude.Any(toexclude => file.Contains(toexclude, StringComparison.OrdinalIgnoreCase)))
				.ToArray()
		};

		Program.WriteMessage($"{files.Length} zip file(s) identified...", true);

		// for each file
		foreach (var file in files)
		{
			try
			{
				ZipInternals.CheckZipFile(file, CancellationToken.None);
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
	public static string NormalizeFolder(string foldername)
	{
		var dirinfo = new DirectoryInfo(foldername);
		return dirinfo.FullName;
	}
}

internal static class ZipInternals
{
	/// <summary>
	/// Wrapper around zip search to handle nested zips
	/// </summary>
	public static void CheckZipFile(string path, CancellationToken token)
	{
		using var archive = ZipFile.OpenRead(path);
		RecursiveArchiveCheck(path, archive, token);
	}

	/// <summary>
	/// Given a zip archive, loop through and list the contents. Recursively calls for nested zips
	/// </summary>
	private static void RecursiveArchiveCheck(string containername, ZipArchive archive, CancellationToken token)
	{
		foreach (var nestedEntry in archive.Entries)
		{
			token.ThrowIfCancellationRequested();

			if (nestedEntry.FullName.Length == 0) continue;
			var lastChar = nestedEntry.FullName[^1];

			if (nestedEntry.FullName.EndsWith(".zip", StringComparison.OrdinalIgnoreCase))
			{
				// its another nested zip file, we need to open it and search inside
				var nestedZipName = $"{containername}/{nestedEntry.FullName}";
				try
				{
					using var nestedStream = nestedEntry.Open();
					using var nestedArchive = new ZipArchive(nestedStream);

					RecursiveArchiveCheck(nestedZipName, nestedArchive, token);
				}
				catch
				{
					Program.WriteMessage($"Error in nested zip: {nestedZipName}");
				}
			}
			else if (lastChar is not ('/' or '\\')) // ignore folders
			{
				string filename;
				if (nestedEntry.FullName.Contains('\\'))
				{
					// path separator is '\', so replace with '/' for consistency
					var s = nestedEntry.FullName.Replace('\\', '/');
					filename = $"{containername}/{s}";
				}
				else
					filename = $"{containername}/{nestedEntry.FullName}";

				Console.WriteLine(filename);
			}
		}
	}
}
