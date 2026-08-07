using System;
using System.IO;
using System.Runtime.CompilerServices;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace Elements.Core.Tests;

/// <summary>
/// Accessor for test resources, this only works if the project is running via the `dotnet test` command,
/// or similar build systems that run under the source directory.
/// </summary>
[TestClass]
public static class TestFileLoader
{
    private static readonly string CsprojDir;
    private static readonly string TempRoot;

    private static string ThisStaticFilePath([CallerFilePath] string filePath = "") => filePath;

    static TestFileLoader()
    {
        var current = Path.GetFullPath(ThisStaticFilePath());
        while (!File.Exists(Path.Combine(current, typeof(TestFileLoader).Assembly.GetName().Name + ".csproj")))
        {
            current = Path.GetDirectoryName(current);
            if (current == null)
            {
                Assert.Fail(
                    "Could not find Elements.Core.Tests.csproj, tests that load local resource will probably fail.");
            }
        }

        CsprojDir = current;

        TempRoot = Path.Combine(AppContext.BaseDirectory, "TestTemp",
            $"{DateTime.UtcNow:yyyy-MM-dd_HH.mm.ss}_{Path.GetRandomFileName()}");
        Directory.CreateDirectory(TempRoot);
    }
    
    [AssemblyCleanup]
    public static void AssemblyCleanup()
    {
        // Comment this if you need test files to diagnose with
        Directory.Delete(TempRoot, true);
    }

    /// <summary>
    /// Returns the path to a resource in the Elements.Core.Tests project.
    /// Resources are relative to the Elements.Core.Tests directory.
    /// </summary>
    /// <param name="pathSegments">The segments to append to the path, normally some kind of resource directory, then file name.</param>
    /// <returns>The fully formed file path.</returns>
    public static string GetResourcePath(params string[] pathSegments)
    {
        return Path.GetFullPath(Path.Combine(CsprojDir, Path.Combine(pathSegments)));
    }

    /// <summary>
    /// Returns a path to a resource relative to the file calling the method.
    /// </summary>
    /// <param name="fileName">The name of the file to load, this can contain relative paths, but be careful about platform-specific symbols.</param>
    /// <param name="sourceFile">The path to the file calling this method, DO NOT PROVIDE THIS ARGUMENT.</param>
    /// <returns>The fully formed file path.</returns>
    public static string GetLocalResourcePath(string fileName, [CallerFilePath] string sourceFile = "")
    {
        // This is a bit belts and suspenders, source file is a full path,
        // but better be safe in case someone passes a relative path to sourceFile directly.
        // This should also allow files from other assemblies to be loaded (like the benchmarks).
        return Path.GetFullPath(Path.Combine(CsprojDir, sourceFile, "..", fileName));
    }

    /// <summary>
    /// Creates a temp directory for a test.
    /// <paramref name="path"/> should be used to prevent tests from running into each other.
    /// </summary>
    /// <param name="path">The relative path segments to append to the temp directory.</param>
    /// <returns>The full directory path the test can mutate.</returns>
    public static string CreateTempDir(params string[] path)
    {
        var fullPath = Path.Combine(TempRoot, Path.Combine(path));
        Directory.CreateDirectory(fullPath);
        return fullPath;
    }

    /// <summary>
    /// Create a temp directory for the test; bucketed by the type name provided (normally the test fixture type).
    /// <paramref name="path"/> should be used to prevent tests from running into each other.
    /// </summary>
    /// <param name="path">The relative path segments to append to the temp directory.</param>
    /// <typeparam name="T">A name that goes between the temp dir root and the path segments provided.</typeparam>
    /// <returns>The full directory path the test can mutate.</returns>
    public static string CreateTempDir<T>(params string[] path)
    {
        var fullPath = Path.Combine(TempRoot, typeof(T).Name, Path.Combine(path));
        Directory.CreateDirectory(fullPath);
        return fullPath;
    }
}

/// <summary>
/// Verification tests to ensure the environment is working correctly, if these fail... best of luck.
/// </summary>
[TestClass]
public class TestFileLoaderTests
{
    [TestMethod]
    public void GetResourcePathReturnsCorrectPath()
    {
        var path = TestFileLoader.GetResourcePath("TestFileLoader.cs");
        Assert.IsTrue(File.Exists(path));
    }

    [TestMethod]
    public void GetLocalResourcePathReturnsCorrectPath()
    {
        var path = TestFileLoader.GetLocalResourcePath("TestFileLoader.cs");
        Assert.IsTrue(File.Exists(path));
    }

    [TestMethod]
    public void CreateTempDirCreatesTargetDir()
    {
        var path = TestFileLoader.CreateTempDir();
        Assert.IsTrue(Directory.Exists(path));
    }

    [TestMethod]
    public void CreateTempDirWithTypeCreatesTargetDir()
    {
        var path = TestFileLoader.CreateTempDir<TestFileLoaderTests>();
        Assert.IsTrue(Directory.Exists(path));
    }
}
