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
}
