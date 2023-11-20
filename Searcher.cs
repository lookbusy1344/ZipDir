using System.IO.Compression;
using System.Runtime.CompilerServices;

namespace ZipDir;

internal static class Searcher
{
	/// <summary>
	/// Find all .zip files in this folder, and list their contents
	/// </summary>
	public static void SearchFolderByExt(string path, string fileSpec, IReadOnlyList<string> exclude)
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
				var zip = new ZipInternals(true);
				zip.CheckZipFile(file);
			}
			catch
			{
				Program.WriteMessage($"Error in zip: {file}");
			}
		}
	}

	public static void SearchFolderByContent(string path, string fileSpec, IReadOnlyList<string> exclude)
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

		// for each file, check if it contains the magic bytes
		foreach (var file in files)
		{
			if (!ZipUtils.IsZipArchiveContent(file)) continue;

			try
			{
				var zip = new ZipInternals(false);
				zip.CheckZipFile(file);
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

internal sealed class ZipInternals(bool byExtension = true)
{
	/// <summary>
	/// Wrapper around zip search to handle nested zips
	/// </summary>
	public void CheckZipFile(string path, CancellationToken token = default)
	{
		using var archive = ZipFile.OpenRead(path);
		RecursiveArchiveCheck(path, archive, token);
	}

	/// <summary>
	/// Given a zip archive, loop through and list the contents. Recursively calls for nested zips
	/// </summary>
	private void RecursiveArchiveCheck(string containerName, ZipArchive archive, CancellationToken token)
	{
		foreach (var nestedEntry in archive.Entries)
		{
			token.ThrowIfCancellationRequested();

			if (nestedEntry.FullName.Length == 0) continue;

			if (IsZipArchive(nestedEntry))
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
				Console.WriteLine(ZipUtils.EntryFilename(containerName, nestedEntry));
		}
	}

	/// <summary>
	/// Check this file according to extension, or according to content
	/// </summary>
	[MethodImpl(MethodImplOptions.AggressiveInlining)]
	private bool IsZipArchive(ZipArchiveEntry archive) => byExtension ? ZipUtils.IsZipArchiveFilename(archive) : ZipUtils.IsZipArchiveContent(archive);
}
