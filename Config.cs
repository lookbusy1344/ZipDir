namespace ZipDir;

/// <summary>
/// Configuration for the zipdir command line
/// Folder is a string so the whole record has value semantics. A DirectoryInfo does not support value semantics.
/// </summary>
public record class ZipDirConfig(string Folder, string Pattern, IReadOnlyList<string> Excludes, bool Raw);

/// <summary>
/// Unit type for help requested
/// </summary>
public readonly record struct HelpRequested();

/// <summary>
/// Exception thrown when help is requested
/// </summary>
public class HelpException : Exception
{
	public HelpException(string message) : base(message) { }

	public HelpException()
	{
	}

	public HelpException(string message, Exception innerException) : base(message, innerException)
	{
	}
}
