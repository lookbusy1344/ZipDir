using System.Diagnostics;
using Xunit.Abstractions;

namespace ZipDir.Tests;

public class ZipDirTests
{
	private readonly ITestOutputHelper testOutputHelper;

	public ZipDirTests(ITestOutputHelper testOutputHelper) => this.testOutputHelper = testOutputHelper;

	[Fact]
	public void ProcessTestsFolder_ShouldReturnExpectedFiles()
	{
		// Arrange
		var projectRoot = GetProjectRoot();
		var testsPath = Path.Combine(projectRoot, "tests");
		var startInfo = new ProcessStartInfo {
			FileName = GetZipDirExecutablePath(),
			Arguments = $"-f \"{testsPath}\"",
			UseShellExecute = false,
			RedirectStandardOutput = true,
			RedirectStandardError = true,
			CreateNoWindow = true,
			WorkingDirectory = projectRoot
		};

		// Act
		using var process = Process.Start(startInfo);
		Assert.NotNull(process);

		var output = process.StandardOutput.ReadToEnd();
		var error = process.StandardError.ReadToEnd();
		process.WaitForExit();

		// Assert
		Assert.Equal(0, process.ExitCode);
		Assert.Empty(error);

		// Verify expected files are found
		Assert.Contains("Archive1.zip/PicoArgs.cs", output);
		Assert.Contains("Archive1.zip/Program.cs", output);
		Assert.Contains("Archive2.zip/PicoArgs.cs", output);
		Assert.Contains("Archive2.zip/Program.cs", output);
		Assert.Contains("Archive2.zip/Archive1.zip/PicoArgs.cs", output);
		Assert.Contains("Archive2.zip/Archive1.zip/Program.cs", output);

		// Verify it reports finding 2 zip files
		Assert.Contains("2 zip file(s) identified", output);
	}

	[Fact]
	public void ProcessTestsFolder_RawMode_ShouldReturnOnlyFilePaths()
	{
		// Arrange
		var projectRoot = GetProjectRoot();
		var testsPath = Path.Combine(projectRoot, "tests");
		var startInfo = new ProcessStartInfo {
			FileName = GetZipDirExecutablePath(),
			Arguments = $"-f \"{testsPath}\" -r",
			UseShellExecute = false,
			RedirectStandardOutput = true,
			RedirectStandardError = true,
			CreateNoWindow = true,
			WorkingDirectory = projectRoot
		};

		// Act
		using var process = Process.Start(startInfo);
		Assert.NotNull(process);

		var output = process.StandardOutput.ReadToEnd();
		var error = process.StandardError.ReadToEnd();
		process.WaitForExit();

		// Assert
		Assert.Equal(0, process.ExitCode);
		Assert.Empty(error);

		// In raw mode, should not contain header information
		Assert.DoesNotContain("ZipDir - list contents", output);
		Assert.DoesNotContain("zip file(s) identified", output);

		// Should still contain the file paths
		Assert.Contains("Archive1.zip/PicoArgs.cs", output);
		Assert.Contains("Archive2.zip/Program.cs", output);
	}

	[Fact]
	public void ProcessTestsFolder_SingleThreadMode_ShouldWork()
	{
		// Arrange
		var projectRoot = GetProjectRoot();
		var testsPath = Path.Combine(projectRoot, "tests");
		var startInfo = new ProcessStartInfo {
			FileName = GetZipDirExecutablePath(),
			Arguments = $"-f \"{testsPath}\" -s",
			UseShellExecute = false,
			RedirectStandardOutput = true,
			RedirectStandardError = true,
			CreateNoWindow = true,
			WorkingDirectory = projectRoot
		};

		// Act
		using var process = Process.Start(startInfo);
		Assert.NotNull(process);

		var output = process.StandardOutput.ReadToEnd();
		var error = process.StandardError.ReadToEnd();
		process.WaitForExit();

		// Assert
		if (process.ExitCode != 0) {
			testOutputHelper.WriteLine($"Exit code: {process.ExitCode}");
			testOutputHelper.WriteLine($"Error output: {error}");
			testOutputHelper.WriteLine($"Standard output: {output}");
		}

		Assert.Equal(0, process.ExitCode);
		Assert.Empty(error);

		// Verify it's using single-thread mode
		Assert.Contains("Single thread mode", output);

		// Should still find the expected files
		Assert.Contains("Archive1.zip/PicoArgs.cs", output);
		Assert.Contains("Archive2.zip/Program.cs", output);
	}

	[Fact]
	public void ProcessTestsFolder_WithMagicNumberDetection_ShouldWork()
	{
		// Arrange
		var projectRoot = GetProjectRoot();
		var testsPath = Path.Combine(projectRoot, "tests");
		var startInfo = new ProcessStartInfo {
			FileName = GetZipDirExecutablePath(),
			Arguments = $"-f \"{testsPath}\" -b",
			UseShellExecute = false,
			RedirectStandardOutput = true,
			RedirectStandardError = true,
			CreateNoWindow = true,
			WorkingDirectory = projectRoot
		};

		// Act
		using var process = Process.Start(startInfo);
		Assert.NotNull(process);

		var output = process.StandardOutput.ReadToEnd();
		var error = process.StandardError.ReadToEnd();
		process.WaitForExit();

		// Assert
		if (process.ExitCode != 0) {
			testOutputHelper.WriteLine($"Exit code: {process.ExitCode}");
			testOutputHelper.WriteLine($"Error output: {error}");
			testOutputHelper.WriteLine($"Standard output: {output}");
		}

		Assert.Equal(0, process.ExitCode);
		Assert.Empty(error);

		// Verify it's using magic number detection
		Assert.Contains("searching by magic number", output);

		// Should still find the expected files
		Assert.Contains("Archive1.zip/PicoArgs.cs", output);
		Assert.Contains("Archive2.zip/Program.cs", output);
	}

	private static string GetProjectRoot()
	{
		// Get the directory where the test assembly is located
		var testAssemblyPath = System.Reflection.Assembly.GetExecutingAssembly().Location;
		var testDir = Path.GetDirectoryName(testAssemblyPath);

		// Navigate to the main project root
		return Path.GetFullPath(Path.Combine(testDir!, "..", "..", "..", ".."));
	}

	private static string GetZipDirExecutablePath()
	{
		var projectRoot = GetProjectRoot();
		var executablePath = Path.Combine(projectRoot, "bin", "Debug", "net9.0", "ZipDir");

		// On Windows, add .exe extension
		if (Environment.OSVersion.Platform == PlatformID.Win32NT) {
			executablePath += ".exe";
		}

		if (!File.Exists(executablePath)) {
			throw new FileNotFoundException($"ZipDir executable not found at: {executablePath}");
		}

		return executablePath;
	}

	[Fact]
	public void Help_ShouldDisplayHelpText()
	{
		// Arrange
		var startInfo = new ProcessStartInfo {
			FileName = GetZipDirExecutablePath(),
			Arguments = "--help",
			UseShellExecute = false,
			RedirectStandardOutput = true,
			RedirectStandardError = true,
			CreateNoWindow = true
		};

		// Act
		using var process = Process.Start(startInfo);
		Assert.NotNull(process);

		var output = process.StandardOutput.ReadToEnd();
		var error = process.StandardError.ReadToEnd();
		process.WaitForExit();

		// Assert
		Assert.Equal(0, process.ExitCode);
		Assert.Empty(error);
		Assert.Contains("Usage: ZipDir [options]", output);
		Assert.Contains("--folder", output);
		Assert.Contains("--pattern", output);
		Assert.Contains("--exclude", output);
		Assert.Contains("--help", output);
	}

	[Fact]
	public void HelpShortForm_ShouldDisplayHelpText()
	{
		// Arrange
		var startInfo = new ProcessStartInfo {
			FileName = GetZipDirExecutablePath(),
			Arguments = "-h",
			UseShellExecute = false,
			RedirectStandardOutput = true,
			RedirectStandardError = true,
			CreateNoWindow = true
		};

		// Act
		using var process = Process.Start(startInfo);
		Assert.NotNull(process);

		var output = process.StandardOutput.ReadToEnd();
		process.WaitForExit();

		// Assert
		Assert.Equal(0, process.ExitCode);
		Assert.Contains("Usage: ZipDir [options]", output);
	}

	[Fact]
	public void VersionInfo_ShouldBeDisplayed()
	{
		// Arrange
		var projectRoot = GetProjectRoot();
		var testsPath = Path.Combine(projectRoot, "tests");
		var startInfo = new ProcessStartInfo {
			FileName = GetZipDirExecutablePath(),
			Arguments = $"-f \"{testsPath}\"",
			UseShellExecute = false,
			RedirectStandardOutput = true,
			RedirectStandardError = true,
			CreateNoWindow = true
		};

		// Act
		using var process = Process.Start(startInfo);
		Assert.NotNull(process);

		var output = process.StandardOutput.ReadToEnd();
		process.WaitForExit();

		// Assert
		Assert.Equal(0, process.ExitCode);
		Assert.Contains("ZipDir - list contents of zip files", output);
	}

	[Fact]
	public void InvalidFolder_ShouldReturnError()
	{
		// Arrange
		var startInfo = new ProcessStartInfo {
			FileName = GetZipDirExecutablePath(),
			Arguments = "-f \"/nonexistent/path/that/does/not/exist\"",
			UseShellExecute = false,
			RedirectStandardOutput = true,
			RedirectStandardError = true,
			CreateNoWindow = true
		};

		// Act
		using var process = Process.Start(startInfo);
		Assert.NotNull(process);

		var output = process.StandardOutput.ReadToEnd();
		var error = process.StandardError.ReadToEnd();
		process.WaitForExit();

		// Assert
		Assert.NotEqual(0, process.ExitCode);
		var combinedOutput = output + error;
		Assert.True(
			combinedOutput.Contains("ERROR") || combinedOutput.Contains("not found") || combinedOutput.Contains("does not exist"),
			$"Expected error message but got: {combinedOutput}"
		);
	}

	[Fact]
	public void EmptyDirectory_ShouldHandleGracefully()
	{
		// Arrange
		var projectRoot = GetProjectRoot();
		var emptyDir = Path.Combine(projectRoot, "tests", "empty_test_dir");
		Directory.CreateDirectory(emptyDir);

		try {
			var startInfo = new ProcessStartInfo {
				FileName = GetZipDirExecutablePath(),
				Arguments = $"-f \"{emptyDir}\"",
				UseShellExecute = false,
				RedirectStandardOutput = true,
				RedirectStandardError = true,
				CreateNoWindow = true
			};

			// Act
			using var process = Process.Start(startInfo);
			Assert.NotNull(process);

			var output = process.StandardOutput.ReadToEnd();
			var error = process.StandardError.ReadToEnd();
			process.WaitForExit();

			// Assert
			Assert.Equal(0, process.ExitCode);
			Assert.Empty(error);
			Assert.Contains("0 zip file(s) identified", output);
		}
		finally {
			// Cleanup
			if (Directory.Exists(emptyDir)) {
				Directory.Delete(emptyDir, true);
			}
		}
	}

	[Fact]
	public void ExcludePattern_ShouldFilterResults()
	{
		// Arrange
		var projectRoot = GetProjectRoot();
		var testsPath = Path.Combine(projectRoot, "tests");
		var startInfo = new ProcessStartInfo {
			FileName = GetZipDirExecutablePath(),
			Arguments = $"-f \"{testsPath}\" -e Archive1.zip",
			UseShellExecute = false,
			RedirectStandardOutput = true,
			RedirectStandardError = true,
			CreateNoWindow = true
		};

		// Act
		using var process = Process.Start(startInfo);
		Assert.NotNull(process);

		var output = process.StandardOutput.ReadToEnd();
		var error = process.StandardError.ReadToEnd();
		process.WaitForExit();

		// Assert
		Assert.Equal(0, process.ExitCode);
		Assert.Empty(error);

		// Should not contain direct Archive1.zip files
		var lines = output.Split('\n');
		// Check that Archive1.zip as a top-level file is excluded
		Assert.DoesNotContain(lines, line => line.Contains("tests/Archive1.zip/") || line.Contains("tests\\Archive1.zip\\"));

		// Should still contain Archive2.zip files (including nested Archive1.zip within it)
		Assert.Contains("Archive2.zip/PicoArgs.cs", output);
		Assert.Contains("Archive2.zip/Program.cs", output);

		// Should report only 1 zip file found (Archive1.zip excluded, only Archive2.zip found)
		Assert.Contains("1 zip file(s) identified", output);
	}

	[Fact]
	public void MultipleExcludePatterns_ShouldFilterAllMatches()
	{
		// Arrange
		var projectRoot = GetProjectRoot();
		var testsPath = Path.Combine(projectRoot, "tests");
		var startInfo = new ProcessStartInfo {
			FileName = GetZipDirExecutablePath(),
			Arguments = $"-f \"{testsPath}\" -e Archive1 -e Archive2",
			UseShellExecute = false,
			RedirectStandardOutput = true,
			RedirectStandardError = true,
			CreateNoWindow = true
		};

		// Act
		using var process = Process.Start(startInfo);
		Assert.NotNull(process);

		var output = process.StandardOutput.ReadToEnd();
		var error = process.StandardError.ReadToEnd();
		process.WaitForExit();

		// Assert
		Assert.Equal(0, process.ExitCode);
		Assert.Empty(error);

		// Should not contain any archive files
		Assert.DoesNotContain("Archive1.zip", output);
		Assert.DoesNotContain("Archive2.zip", output);

		// Should report 0 zip files found
		Assert.Contains("0 zip file(s) identified", output);
	}

	[Fact]
	public void PathWithSpaces_ShouldHandleCorrectly()
	{
		// Arrange
		var projectRoot = GetProjectRoot();
		var dirWithSpaces = Path.Combine(projectRoot, "tests", "dir with spaces");
		Directory.CreateDirectory(dirWithSpaces);

		try {
			// Create a test zip file in the directory
			var zipPath = Path.Combine(dirWithSpaces, "test.zip");
			using (var archive = System.IO.Compression.ZipFile.Open(zipPath, System.IO.Compression.ZipArchiveMode.Create)) {
				var entry = archive.CreateEntry("test.txt");
				using var writer = new StreamWriter(entry.Open());
				writer.Write("test content");
			}

			var startInfo = new ProcessStartInfo {
				FileName = GetZipDirExecutablePath(),
				Arguments = $"-f \"{dirWithSpaces}\"",
				UseShellExecute = false,
				RedirectStandardOutput = true,
				RedirectStandardError = true,
				CreateNoWindow = true
			};

			// Act
			using var process = Process.Start(startInfo);
			Assert.NotNull(process);

			var output = process.StandardOutput.ReadToEnd();
			var error = process.StandardError.ReadToEnd();
			process.WaitForExit();

			// Assert
			Assert.Equal(0, process.ExitCode);
			Assert.Empty(error);
			Assert.Contains("test.zip/test.txt", output);
			Assert.Contains("1 zip file(s) identified", output);
		}
		finally {
			// Cleanup
			if (Directory.Exists(dirWithSpaces)) {
				Directory.Delete(dirWithSpaces, true);
			}
		}
	}

	[Fact]
	public void CustomPattern_ShouldFilterByPattern()
	{
		// Arrange
		var projectRoot = GetProjectRoot();
		var testsPath = Path.Combine(projectRoot, "tests");
		var startInfo = new ProcessStartInfo {
			FileName = GetZipDirExecutablePath(),
			Arguments = $"-f \"{testsPath}\" -p Archive1.zip",
			UseShellExecute = false,
			RedirectStandardOutput = true,
			RedirectStandardError = true,
			CreateNoWindow = true
		};

		// Act
		using var process = Process.Start(startInfo);
		Assert.NotNull(process);

		var output = process.StandardOutput.ReadToEnd();
		var error = process.StandardError.ReadToEnd();
		process.WaitForExit();

		// Assert
		Assert.Equal(0, process.ExitCode);
		Assert.Empty(error);

		// Should contain Archive1 files
		Assert.Contains("Archive1.zip/PicoArgs.cs", output);

		// Should not contain Archive2 files (not matching pattern)
		Assert.DoesNotContain("Archive2.zip/PicoArgs.cs", output);

		// Should report only 1 zip file found
		Assert.Contains("1 zip file(s) identified", output);
	}

	[Fact]
	public void CombinedOptions_ShouldWorkTogether()
	{
		// Arrange
		var projectRoot = GetProjectRoot();
		var testsPath = Path.Combine(projectRoot, "tests");
		var startInfo = new ProcessStartInfo {
			FileName = GetZipDirExecutablePath(),
			Arguments = $"-f \"{testsPath}\" -r -s",
			UseShellExecute = false,
			RedirectStandardOutput = true,
			RedirectStandardError = true,
			CreateNoWindow = true
		};

		// Act
		using var process = Process.Start(startInfo);
		Assert.NotNull(process);

		var output = process.StandardOutput.ReadToEnd();
		var error = process.StandardError.ReadToEnd();
		process.WaitForExit();

		// Assert
		Assert.Equal(0, process.ExitCode);
		Assert.Empty(error);

		// Raw mode should not show headers
		Assert.DoesNotContain("ZipDir - list contents", output);
		Assert.DoesNotContain("Single thread mode", output);
		Assert.DoesNotContain("zip file(s) identified", output);

		// Should still contain file paths
		Assert.Contains("Archive1.zip/PicoArgs.cs", output);
	}
}
