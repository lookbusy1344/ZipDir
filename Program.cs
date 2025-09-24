namespace ZipDir;

using PicoArgs_dotnet;

/*	Displayed syntax should probably be something like:
 *	\path\to\zip.zip/path/to/file.txt
 *	..or for nested zips:
 *	\path\to\zip1.zip/nested.zip/path/to/file.txt
 *	..where
 *	[\path\to\zip1.zip] [/nested.zip/path/to/file.txt]
 *
 *	Physical files use backslash, zip entries use forward slash
 */

internal static class Program
{
	private static int Main(string[] args)
	{
		var ver = GitVersion.VersionInfo.Get();

		try {
			var parsed = ParseCommandLine(args);

			if (!parsed.Raw) {
				Console.WriteLine($"ZipDir - list contents of zip files {ver.GetVersionHash(12)}");
				Console.WriteLine(parsed.SingleThread ? "Single thread mode" : "Multi-thread mode");
			}

			var str = parsed.ByExtension ? "extension" : "magic number";
			WriteMessage($"Folder: {parsed.Folder}, pattern: {parsed.Pattern}, searching by {str}", parsed.Raw, true);
			Searcher.SearchFolder(parsed);
			return 0;
		}
		catch (HelpException) {
			// --help has been requested
			Console.WriteLine($"ZipDir - list contents of zip files {ver.GetVersionHash(20)}");
			Console.WriteLine(CommandLineMessage);
			return 0;
		}
		catch (Exception ex) {
			// any other exception
			Console.WriteLine($"ERROR: {ex.Message}\r\n");
			Console.WriteLine($"ZipDir - list contents of zip files {ver.GetVersionHash(12)}");
			Console.WriteLine(CommandLineMessage);
			return 1;
		}
	}

	/// <summary>
	/// Wrap the call to PicoArgs in a using block, so it automatically throws if there are any errors
	/// </summary>
	private static ZipDirConfig ParseCommandLine(string[] args)
	{
		using var pico = new PicoArgsDisposable(args);

		var help = pico.Contains("-h", "-?", "--help");
		if (help) {
			// if we want help, just bail here. Suppress the warning about not using other parameters
			pico.SuppressCheck = true;
			throw new HelpException();
		}

		// parse the rest of the command line
		var rawOutput = pico.Contains("-r", "--raw");
		var singleThread = pico.Contains("-s", "--single-thread");
		var byExtension = !pico.Contains("-b", "--byte");
		var folder = Searcher.NormalizeFolder(pico.GetParamOpt("-f", "--folder") ?? ".");
		var pattern = pico.GetParamOpt("-p", "--pattern");
		var excludes = (IReadOnlyList<string>)pico.GetMultipleParams("-e", "--exclude");

		// if no pattern is specified:
		// when searching by extension, the default should be *.zip
		// when searching by magic number, the default should be *
		pattern ??= byExtension ? "*.zip" : "*";

		return new(byExtension, folder, pattern, excludes, rawOutput, singleThread);
	}

	/// <summary>
	/// Display helpful message if not in --raw mode
	/// </summary>
	internal static void WriteMessage(string msg, bool raw = false, bool blankLine = false)
	{
		if (!raw) {
			Console.WriteLine(msg);
			if (blankLine) {
				Console.WriteLine();
			}
		}
	}

	private const string CommandLineMessage = """
											  Usage: ZipDir [options]

											  Options:
											    -f, --folder <path>   Folder to search (default ".")
											    -p, --pattern <str>   Zip file pattern (default "*.zip")
											    -e, --exclude <str>   Exclude patterns, can be specified multiple times "-e backup -e documents"
											    -b, --byte            Identify zip files by magic number, not extension
											    -r, --raw             Raw output, for piping
											    -s, --single-thread   Use a single thread for processing
											    -h, --help, -?        Help information

											  Example:
											    ZipDir -f .
											    ZipDir --folder \your\docs --pattern *.zip --exclude backup --exclude documents
											  """;
}
