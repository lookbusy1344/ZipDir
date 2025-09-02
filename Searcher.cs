namespace ZipDir;

using System.IO.Compression;
using System.Runtime.CompilerServices;
using System.Threading.Tasks;

internal static class Searcher
{
	/// <summary>
	/// Find all .zip files in this folder, and list their contents (multi-threaded)
	/// </summary>
	internal static void SearchFolder(ZipDirConfig config)
	{
		var allFiles = Directory.GetFiles(config.Folder, config.Pattern,
			new EnumerationOptions { IgnoreInaccessible = true, RecurseSubdirectories = true, MatchCasing = MatchCasing.CaseInsensitive });

		// filter out any files that match the exclude pattern using proper glob matching
		var files = config.Excludes.Count switch {
			0 => allFiles,
			1 => [.. allFiles.Where(file => !FileMatchesPattern(file, config.Excludes[0]))],
			_ => [.. allFiles.Where(file => !config.Excludes.Any(pattern => FileMatchesPattern(file, pattern)))]
		};

		if (config.ByExtension) {
			Program.WriteMessage($"{files.Length} zip file(s) identified...", config.Raw, true);
		} else {
			Program.WriteMessage($"{files.Length} potential file(s) identified...", config.Raw, true);
		}

		var parallelism = config.SingleThread ? 1 : Environment.ProcessorCount;

		// Use parallelism to process zip files on specified number of cores
		_ = Parallel.ForEach(files, new() { MaxDegreeOfParallelism = parallelism }, file => {
			try {
				if (config.ByExtension || ZipUtils.IsZipArchiveContent(file)) {
					var zip = new ZipInternals(config.ByExtension, config.Raw);
					zip.CheckZipFile(file);
				}
			}
			catch (Exception ex) {
				Program.WriteMessage($"Error in zip: {file} - {ex.Message}", config.Raw);
			}
		});
	}

	/// <summary>
	/// Expand the folder name to a full path, removing things like ..\..
	/// </summary>
	internal static string NormalizeFolder(string folderName)
	{
		var dirinfo = new DirectoryInfo(folderName);
		return dirinfo.FullName;
	}

	/// <summary>
	/// Check if a file path matches an exclusion pattern (supports simple wildcards)
	/// </summary>
	private static bool FileMatchesPattern(string filePath, string pattern)
	{
		// Simple pattern matching - if pattern contains wildcards, use proper matching
		if (pattern.Contains('*') || pattern.Contains('?')) {
			// Convert simple glob pattern to regex for basic wildcard support
			var regexPattern = "^" + System.Text.RegularExpressions.Regex.Escape(pattern)
				.Replace(@"\*", ".*")
				.Replace(@"\?", ".") + "$";
			return System.Text.RegularExpressions.Regex.IsMatch(filePath, regexPattern,
				System.Text.RegularExpressions.RegexOptions.IgnoreCase);
		}

		// Fallback to simple substring matching for non-wildcard patterns
		return filePath.Contains(pattern, StringComparison.OrdinalIgnoreCase);
	}
}

internal sealed class ZipInternals(bool byExtension = true, bool raw = false)
{
	/// <summary>
	/// Wrapper around zip search to handle nested zips
	/// </summary>
	internal void CheckZipFile(string path, CancellationToken token = default)
	{
		using var archive = ZipFile.OpenRead(path);
		RecursiveArchiveCheck(path, archive, token);
	}

	/// <summary>
	/// Given a zip archive, loop through and list the contents. Recursively calls for nested zips
	/// </summary>
	private void RecursiveArchiveCheck(string containerName, ZipArchive archive, CancellationToken token)
	{
		foreach (var nestedEntry in archive.Entries) {
			token.ThrowIfCancellationRequested();

			if (nestedEntry.FullName.Length == 0) {
				continue;
			}

			if (IsZipArchive(nestedEntry)) {
				// its another nested zip file, we need to open it and search inside
				var nestedZipName = $"{containerName}/{nestedEntry.FullName}";
				try {
					using var nestedStream = nestedEntry.Open();
#pragma warning disable CA2000 // Dispose objects before losing scope - THIS SEEMS TO BE A BUG IN .NET 8
					using var nestedArchive = new ZipArchive(nestedStream, ZipArchiveMode.Read, true);
#pragma warning restore CA2000 // Dispose objects before losing scope

					RecursiveArchiveCheck(nestedZipName, nestedArchive, token);
				}
				catch (Exception ex) {
					Program.WriteMessage($"Error in nested zip: {nestedZipName} - {ex.Message}", raw);
				}
			} else if (nestedEntry.FullName[^1] is not ('/' or '\\')) {
				// check the last character, so we can ignore folders
				Console.WriteLine(ZipUtils.EntryFilename(containerName, nestedEntry));
			}
		}
	}

	/// <summary>
	/// Check this file according to extension, or according to content
	/// </summary>
	[MethodImpl(MethodImplOptions.AggressiveInlining)]
	private bool IsZipArchive(ZipArchiveEntry archive) =>
		byExtension ? ZipUtils.IsZipArchiveFilename(archive) : ZipUtils.IsZipArchiveContent(archive);
}
