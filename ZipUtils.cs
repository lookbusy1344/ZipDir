namespace ZipDir;

using System.IO.Compression;

internal static class ZipUtils
{
	/// <summary>
	/// Magic number for a zip file. ReadOnlySpan is immutable and will not reallocate in this setting
	/// See Framework Design Guidelines, 3rd Edition, sec 9.12 page 438
	/// </summary>
	private static ReadOnlySpan<byte> MagicNumberZip => [0x50, 0x4B, 0x03, 0x04];

	/// <summary>
	/// Is this a zip file? Check the extension
	/// </summary>
	internal static bool IsZipArchiveFilename(string filename) =>
		filename.EndsWith(".zip", StringComparison.OrdinalIgnoreCase);

	/// <summary>
	/// Does this entry represent a nested zip file? Check the extension
	/// </summary>
	internal static bool IsZipArchiveFilename(ZipArchiveEntry entry) => IsZipArchiveFilename(entry.FullName);

	/// <summary>
	/// Build the full path to this entry
	/// </summary>
	internal static string EntryFilename(string containerName, ZipArchiveEntry entry)
	{
		if (entry.FullName.Contains('\\')) {
			// path separator is '\', so replace with '/' for consistency
			var s = entry.FullName.Replace('\\', '/');
			return $"{containerName}/{s}";
		}

		return $"{containerName}/{entry.FullName}";
	}

	/// <summary>
	/// Check the magic number of a physical file to see if it is a zip archive
	/// </summary>
	internal static bool IsZipArchiveContent(string filePath)
	{
		try {
			using var fileStream = new FileStream(filePath, FileMode.Open, FileAccess.Read);
			return CheckZipStream(fileStream);
		}
		catch (UnauthorizedAccessException) {
			// Access denied to file
			return false;
		}
		catch (FileNotFoundException) {
			// File doesn't exist
			return false;
		}
		catch (DirectoryNotFoundException) {
			// Directory doesn't exist
			return false;
		}
		catch (IOException) {
			// File in use or other IO error
			return false;
		}
	}

	/// <summary>
	/// Check the magic number of a zip entry to see if it is a zip archive
	/// </summary>
	internal static bool IsZipArchiveContent(ZipArchiveEntry entry)
	{
		try {
			using var entryStream = entry.Open();
			return CheckZipStream(entryStream);
		}
		catch (InvalidDataException) {
			// Corrupted zip entry
			return false;
		}
		catch (NotSupportedException) {
			// Unsupported compression method
			return false;
		}
		catch (IOException) {
			// Stream read error
			return false;
		}
	}

	/// <summary>
	/// Check if this open stream contains the magic bytes for a zip archive
	/// </summary>
	private static bool CheckZipStream(Stream stream)
	{
		Span<byte> contents = stackalloc byte[MagicNumberZip.Length]; // avoid heap allocation
		return stream.Read(contents) >= MagicNumberZip.Length && contents.SequenceEqual(MagicNumberZip);
	}
}
