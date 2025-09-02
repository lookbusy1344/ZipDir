namespace ZipDir;

/// <summary>
/// Configuration for the zipdir command line
/// Folder is a string so the whole record has value semantics. A DirectoryInfo does not support value semantics.
/// </summary>
internal sealed record class ZipDirConfig(
	bool ByExtension,
	string Folder,
	string Pattern,
	IReadOnlyList<string> Excludes,
	bool Raw,
	bool SingleThread)
{
	/// <summary>
	/// Manually implementing Equals so IReadOnlyList Excludes is compared by value
	/// </summary>
	public bool Equals(ZipDirConfig? other) => other != null
	                                           && ByExtension == other.ByExtension
	                                           && Folder == other.Folder
	                                           && Pattern == other.Pattern
	                                           && Excludes.SequenceEqual(other
		                                           .Excludes) // this is the reason we cant use default Equals
	                                           && Raw == other.Raw
	                                           && SingleThread == other.SingleThread;

	public override int GetHashCode()
	{
		var hash = new HashCode();
		hash.Add(ByExtension);
		hash.Add(Folder);
		hash.Add(Pattern);
		hash.Add(Raw);
		hash.Add(SingleThread);

		// Hash each exclude pattern individually for better distribution
		foreach (var exclude in Excludes) {
			hash.Add(exclude);
		}

		return hash.ToHashCode();
	}
}

/// <summary>
/// Exception thrown when help is requested
/// </summary>
public class HelpException : Exception
{
	public HelpException(string message) : base(message) { }

	public HelpException() { }

	public HelpException(string message, Exception innerException) : base(message, innerException) { }
}
