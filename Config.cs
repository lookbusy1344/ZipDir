﻿namespace ZipDir;

/// <summary>
/// Configuration for the zipdir command line
/// Folder is a string so the whole record has value semantics. A DirectoryInfo does not support value semantics.
/// </summary>
internal sealed record class ZipDirConfig(bool ByExtension, string Folder, string Pattern, IReadOnlyList<string> Excludes, bool Raw)
{
	/// <summary>
	/// Manually implementing Equals so IReadOnlyList Excludes is compared by value
	/// </summary>
	public bool Equals(ZipDirConfig? other) => other != null
		&& ByExtension == other.ByExtension
		&& Folder == other.Folder
		&& Pattern == other.Pattern
		&& Excludes.SequenceEqual(other.Excludes)   // this is the reason we cant use default Equals
		&& Raw == other.Raw;

	public override int GetHashCode() => HashCode.Combine(ByExtension, Folder, Pattern, Raw); // dont use Combine(.., Excludes, ..) because it doesnt work!
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
