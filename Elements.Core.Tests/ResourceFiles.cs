using System.Runtime.CompilerServices;

namespace Elements.Core.Tests;

/// <summary>
/// An accessor for file paths for files checked into the repo.
///
/// This doesn't work if a test executable is moved to another system.
/// This is also affected by DeterministicSourcePaths and ContinuousIntegrationBuild props.
/// 
/// To fix this, we should probably change to preserveNewest content items in the csproj.
/// This requires static files to be in the repo first, otherwise the csproj will include nonexistent assets,
/// and the directory won't be created.
/// </summary>
[TestClass]
public static class ResourceFiles
{
    private static readonly Lazy<string> CsprojDir = new (() => {
        var rootPath = Path.GetFullPath(ThisStaticFilePath());
        var current = rootPath;
        while (!File.Exists(Path.Combine(current, typeof(ResourceFiles).Assembly.GetName().Name + ".csproj")))
        {
            current = Path.GetDirectoryName(current);
            if (current == null)
            {
                throw new ArgumentException(
                    $"Could not find Elements.Core.Tests.csproj under '{rootPath}', tests that load local resource will probably fail.");
            }
        }

        return current;
    });

    private static string ThisStaticFilePath([CallerFilePath] string filePath = "") => filePath;

    /// <summary>
    /// Returns the path to a resource in the Elements.Core.Tests project.
    /// Resources are relative to the Elements.Core.Tests directory.
    /// </summary>
    /// <param name="pathSegments">The segments to append to the path, normally some kind of resource directory, then file name.</param>
    /// <returns>The fully formed file path.</returns>
    public static string GetResourcePath(params string[] pathSegments)
    {
        return Path.GetFullPath(Path.Combine(CsprojDir.Value, Path.Combine(pathSegments)));
    }

    /// <summary>
    /// Returns a path to a resource relative to the file calling the method.
    /// </summary>
    /// <param name="fileName">The name of the file to load, this can contain relative paths, but be careful about platform-specific symbols.</param>
    /// <param name="sourceFile">The path to the file calling this method, DO NOT PROVIDE THIS ARGUMENT.</param>
    /// <returns>The fully formed file path.</returns>
    public static string GetLocalResourcePath(string fileName, [CallerFilePath] string sourceFile = "")
    {
        // This is belts and suspenders, sourceFile is a full path,
        // so combining the csproj dir to sourceFile would only result in sourceFile.
        // If someone manually provides the sourceFile parameter, this helps avoid breaking out of the directory.
        // This should also allow files from other assemblies to be loaded (like the benchmarks).
        return Path.GetFullPath(Path.Combine(CsprojDir.Value, sourceFile, "..", fileName));
    }
}

[TestClass]
public class ResourceFilesTests
{
    [TestMethod]
    public void GetResourcePath_ExistingFile_ReturnsPathToFile()
    {
        var path = ResourceFiles.GetResourcePath("ResourceFiles.cs");
        Assert.IsTrue(File.Exists(path));
    }

    [TestMethod]
    public void GetLocalResourcePath_ExistingFile_ReturnsPathToFile()
    {
        var path = ResourceFiles.GetLocalResourcePath("ResourceFiles.cs");
        Assert.IsTrue(File.Exists(path));
    }
}
