using System.IO.Compression;

namespace ZipDir.Tests;

public class ZipUtilsTests
{
	[Fact]
	public void IsZipArchiveFilename_WithZipExtension_ShouldReturnTrue()
	{
		// Arrange
		var filename = "test.zip";

		// Act
		var result = ZipUtils.IsZipArchiveFilename(filename);

		// Assert
		Assert.True(result);
	}

	[Fact]
	public void IsZipArchiveFilename_WithUppercaseExtension_ShouldReturnTrue()
	{
		// Arrange
		var filename = "test.ZIP";

		// Act
		var result = ZipUtils.IsZipArchiveFilename(filename);

		// Assert
		Assert.True(result);
	}

	[Fact]
	public void IsZipArchiveFilename_WithMixedCaseExtension_ShouldReturnTrue()
	{
		// Arrange
		var filename = "test.ZiP";

		// Act
		var result = ZipUtils.IsZipArchiveFilename(filename);

		// Assert
		Assert.True(result);
	}

	[Fact]
	public void IsZipArchiveFilename_WithNonZipExtension_ShouldReturnFalse()
	{
		// Arrange
		var filename = "test.txt";

		// Act
		var result = ZipUtils.IsZipArchiveFilename(filename);

		// Assert
		Assert.False(result);
	}

	[Fact]
	public void IsZipArchiveFilename_WithNoExtension_ShouldReturnFalse()
	{
		// Arrange
		var filename = "testfile";

		// Act
		var result = ZipUtils.IsZipArchiveFilename(filename);

		// Assert
		Assert.False(result);
	}

	[Fact]
	public void IsZipArchiveFilename_WithZipInFilename_ShouldReturnFalse()
	{
		// Arrange
		var filename = "zipfile.txt";

		// Act
		var result = ZipUtils.IsZipArchiveFilename(filename);

		// Assert
		Assert.False(result);
	}

	[Fact]
	public void EntryFilename_ShouldCombineContainerAndEntryPath()
	{
		// Arrange
		var tempZipPath = Path.GetTempFileName();
		File.Delete(tempZipPath); // Delete the empty file created by GetTempFileName()
		try {
			// Create a temporary zip file with an entry
			using (var archive = ZipFile.Open(tempZipPath, ZipArchiveMode.Create)) {
				var entry = archive.CreateEntry("folder/test.txt");
				using var writer = new StreamWriter(entry.Open());
				writer.Write("test content");
			}

			// Open the zip to get the entry
			using var readArchive = ZipFile.OpenRead(tempZipPath);
			var zipEntry = readArchive.Entries.First();

			// Act
			var result = ZipUtils.EntryFilename("container.zip", zipEntry);

			// Assert
			Assert.Equal("container.zip/folder/test.txt", result);
		}
		finally {
			if (File.Exists(tempZipPath)) {
				File.Delete(tempZipPath);
			}
		}
	}

	[Fact]
	public void EntryFilename_WithBackslashes_ShouldNormalizeToForwardSlashes()
	{
		// Arrange
		var tempZipPath = Path.GetTempFileName();
		File.Delete(tempZipPath); // Delete the empty file created by GetTempFileName()
		try {
			// Create a temporary zip file with an entry that has backslashes
			using (var archive = ZipFile.Open(tempZipPath, ZipArchiveMode.Create)) {
				var entry = archive.CreateEntry("folder\\subfolder\\test.txt");
				using var writer = new StreamWriter(entry.Open());
				writer.Write("test content");
			}

			// Open the zip to get the entry
			using var readArchive = ZipFile.OpenRead(tempZipPath);
			var zipEntry = readArchive.Entries.First();

			// Act
			var result = ZipUtils.EntryFilename("container.zip", zipEntry);

			// Assert - should normalize backslashes to forward slashes
			Assert.DoesNotContain("\\", result);
			Assert.Contains("/", result);
		}
		finally {
			if (File.Exists(tempZipPath)) {
				File.Delete(tempZipPath);
			}
		}
	}

	[Fact]
	public void IsZipArchiveContent_WithValidZipFile_ShouldReturnTrue()
	{
		// Arrange
		var tempZipPath = Path.GetTempFileName();
		File.Delete(tempZipPath); // Delete the empty file created by GetTempFileName()
		try {
			// Create a valid zip file
			using (var archive = ZipFile.Open(tempZipPath, ZipArchiveMode.Create)) {
				var entry = archive.CreateEntry("test.txt");
				using var writer = new StreamWriter(entry.Open());
				writer.Write("test content");
			}

			// Act
			var result = ZipUtils.IsZipArchiveContent(tempZipPath);

			// Assert
			Assert.True(result);
		}
		finally {
			if (File.Exists(tempZipPath)) {
				File.Delete(tempZipPath);
			}
		}
	}

	[Fact]
	public void IsZipArchiveContent_WithNonZipFile_ShouldReturnFalse()
	{
		// Arrange
		var tempFilePath = Path.GetTempFileName();
		try {
			// Write non-zip content
			File.WriteAllText(tempFilePath, "This is not a zip file");

			// Act
			var result = ZipUtils.IsZipArchiveContent(tempFilePath);

			// Assert
			Assert.False(result);
		}
		finally {
			if (File.Exists(tempFilePath)) {
				File.Delete(tempFilePath);
			}
		}
	}

	[Fact]
	public void IsZipArchiveContent_WithNonExistentFile_ShouldReturnFalse()
	{
		// Arrange
		var nonExistentPath = Path.Combine(Path.GetTempPath(), Guid.NewGuid().ToString());

		// Act
		var result = ZipUtils.IsZipArchiveContent(nonExistentPath);

		// Assert
		Assert.False(result);
	}

	[Fact]
	public void IsZipArchiveContent_WithEmptyFile_ShouldReturnFalse()
	{
		// Arrange
		var tempFilePath = Path.GetTempFileName();
		try {
			// Create empty file
			File.WriteAllText(tempFilePath, string.Empty);

			// Act
			var result = ZipUtils.IsZipArchiveContent(tempFilePath);

			// Assert
			Assert.False(result);
		}
		finally {
			if (File.Exists(tempFilePath)) {
				File.Delete(tempFilePath);
			}
		}
	}

	[Fact]
	public void IsZipArchiveContent_WithZipMagicNumberOnly_ShouldReturnTrue()
	{
		// Arrange
		var tempFilePath = Path.GetTempFileName();
		try {
			// Write just the ZIP magic number (PK\x03\x04)
			var magicBytes = new byte[] { 0x50, 0x4B, 0x03, 0x04 };
			File.WriteAllBytes(tempFilePath, magicBytes);

			// Act
			var result = ZipUtils.IsZipArchiveContent(tempFilePath);

			// Assert
			Assert.True(result);
		}
		finally {
			if (File.Exists(tempFilePath)) {
				File.Delete(tempFilePath);
			}
		}
	}
}
