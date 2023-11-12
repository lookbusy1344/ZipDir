using PicoArgs_dotnet;

namespace ZipDir;

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
	private static bool raw;

	private static int Main(string[] args)
	{
		var ver = GitVersion.VersionInfo.Get();

		try
		{
			var parsed = ParseCommandLine(args);

			Program.raw = parsed.Raw;

			if (!raw)
				Console.WriteLine($"ZipDir - list contents of zip files {ver.GetVersionHash(12)}");

			WriteMessage($"Folder: {parsed.Folder}, pattern: {parsed.Pattern}", true);
			Searcher.SearchFolder(parsed.Folder, parsed.Pattern, parsed.Excludes);
			return 0;
		}
		catch (HelpException)
		{
			// --help has been requested
			Console.WriteLine($"ZipDir - list contents of zip files {ver.GetVersionHash(20)}");
			Console.WriteLine(CommandLineMessage);
			return 0;
		}
		catch (Exception ex)
		{
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
		if (help)
		{
			// if we want help, just bail here. Supress the warning about not using other parameters
			pico.SuppressCheck = true;
			throw new HelpException();
		}

		// parse the rest of the command line
		var raw = pico.Contains("-r", "--raw");
		var folder = Searcher.NormalizeFolder(pico.GetParamOpt("-f", "--folder") ?? ".");
		var pattern = pico.GetParamOpt("-p", "--pattern") ?? "*.zip";
		var excludes = pico.GetMultipleParams("-e", "--exclude");

		return new ZipDirConfig(folder, pattern, excludes, raw);
	}

	/// <summary>
	/// Display helpful message is not in --raw mode
	/// </summary>
	public static void WriteMessage(string msg, bool blankLine = false)
	{
		if (!raw)
		{
			Console.WriteLine(msg);
			if (blankLine) Console.WriteLine();
		}
	}

	private const string CommandLineMessage = """
          Usage: ZipDir.exe [options]
          
          Options:
            -f, --folder <path>   Folder to search (default ".")
            -p, --pattern <str>   Zip file pattern (default "*.zip")
            -e, --exclude <str>   Exclude patterns, can be specified multiple times "-e backup -e documents"
            -r, --raw             Raw output, for piping
            -h, --help, -?        Help information

          Example:
            ZipDir.exe -f .
            ZipDir.exe --folder \your\docs --pattern *.zip --exclude backup --exclude documents
          """;
}
