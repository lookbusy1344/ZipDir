using System.IO.Compression;

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
				ZipInternals.CheckZipFile(file);
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

internal static class ZipInternals
{
	/// <summary>
	/// Wrapper around zip search to handle nested zips
	/// </summary>
	public static void CheckZipFile(string path, CancellationToken token = default)
	{
		using var archive = ZipFile.OpenRead(path);
		RecursiveArchiveCheck(path, archive, token);
	}

	/// <summary>
	/// Given a zip archive, loop through and list the contents. Recursively calls for nested zips
	/// </summary>
	private static void RecursiveArchiveCheck(string containerName, ZipArchive archive, CancellationToken token)
	{
		foreach (var nestedEntry in archive.Entries)
		{
			token.ThrowIfCancellationRequested();

			if (nestedEntry.FullName.Length == 0) continue;

			if (IsNestedZip(nestedEntry))
			{
				// its another nested zip file, we need to open it and search inside
				var nestedZipName = $"{containerName}/{nestedEntry.FullName}";
				try
				{
					using var nestedStream = nestedEntry.Open();
#pragma warning disable CA2000 // Dispose objects before losing scope - THIS SEEMS TO BE A BUG IN .NET 8
					using var nestedArchive = new ZipArchive(nestedStream);
#pragma warning restore CA2000 // Dispose objects before losing scope

					RecursiveArchiveCheck(nestedZipName, nestedArchive, token);
				}
				catch
				{
					Program.WriteMessage($"Error in nested zip: {nestedZipName}");
				}
			}
			else if (nestedEntry.FullName[^1] is not ('/' or '\\')) // check the last character, so we can ignore folders
				Console.WriteLine(EntryFilename(containerName, nestedEntry));
		}
	}

	/// <summary>
	/// Does this entry represent a nested zip file? Check the extension
	/// </summary>
	private static bool IsNestedZip(ZipArchiveEntry entry) => entry.FullName.EndsWith(".zip", StringComparison.OrdinalIgnoreCase);

	/// <summary>
	/// Build the full path to this entry
	/// </summary>
	private static string EntryFilename(string containerName, ZipArchiveEntry entry)
	{
		if (entry.FullName.Contains('\\'))
		{
			// path separator is '\', so replace with '/' for consistency
			var s = entry.FullName.Replace('\\', '/');
			return $"{containerName}/{s}";
		}
		else
			return $"{containerName}/{entry.FullName}";
	}

	private static readonly byte[] magicNumberZip = [0x50, 0x4B, 0x03, 0x04];

	/// <summary>
	/// Check the magic number of a physical file to see if it is a zip archive
	/// </summary>
	private static bool IsZipArchive(string filePath)
	{
		try
		{
			Span<byte> fileBytes = stackalloc byte[magicNumberZip.Length];

			using var file = new FileStream(filePath, FileMode.Open, FileAccess.Read);
			_ = file.Read(fileBytes);

			return fileBytes.SequenceEqual(magicNumberZip);
		}
		catch (Exception)
		{
			// some error in the zip file
			return false;
		}
	}

	/// <summary>
	/// Check the magic number of a zip entry to see if it is a zip archive
	/// </summary>
	private static bool IsZipArchive(ZipArchiveEntry entry)
	{
		try
		{
			Span<byte> entryBytes = stackalloc byte[magicNumberZip.Length];

			using var entryStream = entry.Open();
			_ = entryStream.Read(entryBytes);

			return entryBytes.SequenceEqual(magicNumberZip);
		}
		catch (Exception)
		{
			// some error in the zip file
			return false;
		}
	}
}
