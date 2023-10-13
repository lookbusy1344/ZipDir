/*		SafeRef<T>, a reference that cannot be null
 *		This is a struct containing a single reference
 *
 *		Most cases are caught when constructing with a null reference, but occasionally you need to check the reference when you use it
 *		
 *		Normally it cannot be constructed with a null reference, eg:
 *		  string? s = null;
 *		  var safe = new SafeRef<string>(s);	// runtime exception on construction
 *		  Console.Writeline(safe.Ref);	// cannot be null
 *		
 *		However, you can get round this:
 *		  var arr = new SafeRef<string>[5]; // oh no, 5 null references!
 *		  var safe = arr[0];
 *		  var s = safe.Ref;	// runtime exception on use (debug builds)
 */

namespace ZipDir;

#pragma warning disable CA1815 // Override equals and operator equals on value types

/// <summary>
/// Wrapper around a reference, to ensure it is not null
/// </summary>
[System.Diagnostics.DebuggerDisplay("{GetRef}")]
public readonly struct SafeRef<T> where T : class
{
	/// <summary>
	/// Construct a SafeRef from a reference, which must be non-null
	/// </summary>
	public SafeRef(T reference) => innerref = reference ?? throw new ArgumentNullException(nameof(reference));

	/// <summary>
	/// Backing field for reference. Usually nulls are caught in the constructor, but not always
	/// </summary>
	private readonly T? innerref;

#if DEBUG
	/// <summary>
	/// The underlying reference, guaranteed to be non-null in debug builds
	/// </summary>
	public T Ref => innerref ?? throw new NotSupportedException("SafeRef is null");
#else
	/// <summary>
	/// The underlying reference, no run-time check in release builds
	/// </summary>
	public T Ref => innerref!;
#endif

	/// <summary>
	/// Default constructor is not supported
	/// </summary>
	public SafeRef() =>
		throw new NotSupportedException("SafeRef default constructor is not supported");

	/// <summary>
	/// Implicit conversion to the underlying reference
	/// </summary>
	public static implicit operator T(SafeRef<T> safeReference) => safeReference.Ref;

	/// <summary>
	/// Get the underlying reference
	/// </summary>
	public T FromSafeRef() => this.Ref;
}

#pragma warning restore CA1815 // Override equals and operator equals on value types
