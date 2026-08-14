namespace Elements.Core.Tests;

/// <summary>
/// A creator and cleanup class for directories to create temp files and directories during tests.
///
/// Cleanup happens on the following run, artifacts are left in the temp directory.
/// </summary>
[TestClass]
public static class TempFiles
{
    private static readonly string TempRoot;

    static string TempRootPath() => Path.Combine(AppContext.BaseDirectory, "TestTemp");

    static TempFiles()
    {
        TempRoot = Path.Combine(TempRootPath(),
            $"{DateTime.UtcNow:yyyy-MM-dd_HH.mm.ss}_{Path.GetRandomFileName()}");
    }

    [AssemblyInitialize]
    public static void AssemblyInit(TestContext testContext)
    {
        if (!Directory.Exists(TempRootPath()))
        {
            return;
        }
        
        foreach (var dir in Directory.EnumerateDirectories(TempRootPath()))
        {
            try
            {
                Directory.Delete(dir, true);
            }
            catch (Exception e)
            {
                testContext.WriteLine($"Failed to delete previous test directory {dir}: {e.Message}");
            }
        }
    }

    /// <summary>
    /// Creates a temp directory for a test.
    /// 
    /// The contents of the directory are deleted when the next run begins.
    /// <paramref name="path"/> should be used to prevent tests from running into each other.
    /// </summary>
    /// <param name="path">The relative path segments to append to the temp directory.</param>
    /// <returns>The full directory path the test can mutate.</returns>
    public static string CreateTempDir(params ReadOnlySpan<string> path)
    {
        var leafPath = Path.Combine(path);
        var fullPath = Path.GetFullPath(Path.Combine(TempRoot, leafPath));
        if (TempRoot != fullPath &&
            !fullPath.StartsWith(TempRoot + Path.DirectorySeparatorChar, StringComparison.Ordinal))
        {
            throw new ArgumentException(
                $"Temp path '{leafPath}' moved out of the temp directory, which is not allowed.", nameof(path));
        }

        Directory.CreateDirectory(fullPath);
        return fullPath;
    }

    /// <summary>
    /// Create a temp directory for the test; bucketed by the type name provided (normally the test fixture type).
    /// 
    /// The contents of the directory are deleted when the next run begins.
    /// <paramref name="path"/> should be used to prevent tests from running into each other.
    /// </summary>
    /// <param name="path">The relative path segments to append to the temp directory.</param>
    /// <typeparam name="T">A type to isolate temp files, the name of the type goes between the temp dir root and the path segments provided.</typeparam>
    /// <returns>The full directory path the test can mutate.</returns>
    public static string CreateTempDir<T>(params ReadOnlySpan<string> path)
    {
        return CreateTempDir([typeof(T).Name, .. path]);
    }
}

[TestClass]
public class TempFilesTests
{
    [TestMethod]
    public void CreateTempDir_EmptyPath_RootTempDirExists()
    {
        var path = TempFiles.CreateTempDir();
        Assert.IsTrue(Directory.Exists(path));
    }

    [TestMethod]
    public void CreateTempDir_WithoutSpecifier_CreatesNestedTempDir()
    {
        var path = TempFiles.CreateTempDir("Tacos", "Trees");
        Assert.IsTrue(Directory.Exists(path));
        Assert.EndsWith("Trees", path);
    }

    [TestMethod]
    public void CreateTempDir_WithType_CreatesSpecializedTargetDir()
    {
        var path = TempFiles.CreateTempDir<TempFilesTests>();
        Assert.IsTrue(Directory.Exists(path));
    }

    [TestMethod]
    public void CreateTempDir_RootedPath_ThrowsArgumentException()
    {
        Assert.Throws<ArgumentException>(() => TempFiles.CreateTempDir("/Tacos", "Trees"));
    }

    [TestMethod]
    public void CreateTempDir_UpwardsPath_ThrowsArgumentException()
    {
        Assert.Throws<ArgumentException>(() => TempFiles.CreateTempDir("..", "Trees"));
    }
}
