using GitignoreParserNet;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using System.IO;
using System.Text;

namespace Tests.TestParser;

[TestClass]
[TestCategory("Customers")]
public class TestAccepts
{
    readonly string fixture;
    readonly string noNegativeFixture;

    readonly GitignoreParser gitignore;
    readonly GitignoreParser gitignoreNoNegatives;

    public TestAccepts()
    {
        fixture = File.ReadAllText(@"..\..\..\.gitignore-fixture", Encoding.UTF8);
        noNegativeFixture = File.ReadAllText(@"..\..\..\.gitignore-no-negatives", Encoding.UTF8);
        gitignore = new GitignoreParser(fixture);
        gitignoreNoNegatives = new GitignoreParser(noNegativeFixture);
    }

    private void gitignoreAccepts(string str, bool expected)
    {
        Assert.AreEqual(
            expected,
#if DEBUG
            gitignore.Accepts(str, expected)
#else
            gitignore.Accepts(str)
#endif
            , $"Expected '{str}' to be {(expected ? "accepted" : "rejected")}!"
        );
    }

    private void gitignoreNoNegativesAccepts(string str, bool expected)
    {
        Assert.AreEqual(
            expected,
#if DEBUG
            gitignoreNoNegatives.Accepts(str, expected)
#else
            gitignoreNoNegatives.Accepts(str)
#endif
            , $"Expected '{str}' to be {(expected ? "accepted" : "rejected")} when we use gitignoreNoNegatives!"
        );
    }

    [TestMethod(DisplayName = "should accept the given filenames")]
    public void AcceptGivenFilenames()
    {
        gitignoreAccepts("test/index.js", true);
        gitignoreAccepts("wat/test/index.js", true);
        gitignoreNoNegativesAccepts("test/index.js", true);
        gitignoreNoNegativesAccepts("node_modules.json", true);
    }

    [TestMethod(DisplayName = "should not accept the given filenames")]
    public void NotAcceptGivenFilenames()
    {
        gitignoreAccepts("test.swp", false);
        gitignoreAccepts("foo/test.swp", false);
        gitignoreAccepts("node_modules/wat.js", false);
        gitignoreAccepts("foo/bar.wat", false);
        gitignoreNoNegativesAccepts("node_modules/wat.js", false);
    }

    [TestMethod(DisplayName = "should not accept the given directories")]
    public void NotAcceptGivenDirectories()
    {
        gitignoreAccepts("nonexistent", false);
        gitignoreAccepts("nonexistent/bar", false);
        gitignoreNoNegativesAccepts("node_modules", false);
    }

    [TestMethod(DisplayName = "should accept unignored files in ignored directories")]
    public void AcceptUnignoredFilesInIgnoredDirectories()
    {
        gitignoreAccepts("nonexistent/foo", true);
    }

    [TestMethod(DisplayName = "should accept nested unignored files in ignored directories")]
    public void AcceptNestedUnignoredFilesInIgnoredDirectories()
    {
        gitignoreAccepts("nonexistent/foo/wat", true);
    }
}

[TestClass]
[TestCategory("Customers")]
public class TestDenies
{
    readonly string fixture;
    readonly string noNegativeFixture;

    readonly GitignoreParser gitignore;
    readonly GitignoreParser gitignoreNoNegatives;

    public TestDenies()
    {
        fixture = File.ReadAllText(@"..\..\..\.gitignore-fixture", Encoding.UTF8);
        noNegativeFixture = File.ReadAllText(@"..\..\..\.gitignore-no-negatives", Encoding.UTF8);
        gitignore = new GitignoreParser(fixture);
        gitignoreNoNegatives = new GitignoreParser(noNegativeFixture);
    }

    private void gitignoreDenies(string str, bool expected)
    {
        Assert.AreEqual(
            expected,
#if DEBUG
            gitignore.Denies(str, expected)
#else
            gitignore.Denies(str)
#endif
            , $"Expected '{str}' to be {(expected ? "denied" : "accepted")}!"
        );
    }

    private void gitignoreNoNegativesDenies(string str, bool expected)
    {
        Assert.AreEqual(
            expected,
#if DEBUG
            gitignoreNoNegatives.Denies(str, expected)
#else
            gitignoreNoNegatives.Denies(str)
#endif
            , $"Expected '{str}' to be {(expected ? "denied" : "accepted")} when we use gitignoreNoNegatives!"
        );
    }

    [TestMethod(DisplayName = "should deny the given filenames")]
    public void DenyGivenFilenames()
    {
        gitignoreDenies("test.swp", true);
        gitignoreDenies("foo/test.swp", true);
        gitignoreDenies("node_modules/wat.js", true);
        gitignoreDenies("foo/bar.wat", true);
        gitignoreNoNegativesDenies("node_modules/wat.js", true);
    }

    [TestMethod(DisplayName = "should not deny the given filenames")]
    public void NotDenyGivenFilenames()
    {
        gitignoreDenies("test/index.js", false);
        gitignoreDenies("wat/test/index.js", false);
        gitignoreNoNegativesDenies("test/index.js", false);
        gitignoreNoNegativesDenies("wat/test/index.js", false);
        gitignoreNoNegativesDenies("node_modules.json", false);
    }

    [TestMethod(DisplayName = "should deny the given directories")]
    public void DenyGivenDirectories()
    {
        gitignoreDenies("nonexistent", true);
        gitignoreDenies("nonexistent/bar", true);
        gitignoreNoNegativesDenies("node_modules", true);
        gitignoreNoNegativesDenies("node_modules/foo", true);
    }

    [TestMethod(DisplayName = "should not deny unignored files in ignored directories")]
    public void NotDenyUnignoredFilesInIgnoredDirectories()
    {
        gitignoreDenies("nonexistent/foo", false);
    }

    [TestMethod(DisplayName = "should not deny nested unignored files in ignored directories")]
    public void NotDenyNestedUnignoredFilesInIgnoredDirectories()
    {
        gitignoreDenies("nonexistent/foo/wat", false);
    }
}

[TestClass]
[TestCategory("Customers")]
public class TestInspects
{
    readonly string fixture;
    readonly string noNegativeFixture;

    readonly GitignoreParser gitignore;
    readonly GitignoreParser gitignoreNoNegatives;

    public TestInspects()
    {
        fixture = File.ReadAllText(@"..\..\..\.gitignore-fixture", Encoding.UTF8);
        noNegativeFixture = File.ReadAllText(@"..\..\..\.gitignore-no-negatives", Encoding.UTF8);
        gitignore = new GitignoreParser(fixture);
        gitignoreNoNegatives = new GitignoreParser(noNegativeFixture);
    }

    private void gitignoreIsInspected(string str, bool expected)
    {
        Assert.AreEqual(
            expected,
#if DEBUG
            gitignore.Inspects(str, expected)
#else
            gitignore.Inspects(str)
#endif
            , $"Expected '{str}' to {(expected ? "be inspected" : "NOT be inspected")}!"
        );
    }

    private void gitignoreNoNegativesIsInspected(string str, bool expected)
    {
        Assert.AreEqual(
            expected,
#if DEBUG
            gitignoreNoNegatives.Inspects(str, expected)
#else
            gitignoreNoNegatives.Inspects(str)
#endif
            , $"Expected '{str}' to {(expected ? "be inspected" : "NOT be inspected")} when we use gitignoreNoNegatives!"
        );
    }

    [TestMethod(DisplayName = "should return false for directories not mentioned by .gitignore")]
    public void FalseForDirectoriesNotInGitignore()
    {
        gitignoreIsInspected("lib", false);
        gitignoreIsInspected("lib/foo/bar", false);
        gitignoreNoNegativesIsInspected("lib", false);
        gitignoreNoNegativesIsInspected("lib/foo/bar", false);
    }

    [TestMethod(DisplayName = "should return true for directories explicitly mentioned by .gitignore")]
    public void TrueForDirectoriesInGitignore()
    {
        gitignoreIsInspected("baz", true);
        gitignoreIsInspected("baz/wat/foo", true);
        gitignoreNoNegativesIsInspected("node_modules", true);
    }

    [TestMethod(DisplayName = "should return true for ignored directories that have exceptions")]
    public void TrueForIgnoredDirecrotiesWithExceptions()
    {
        gitignoreIsInspected("nonexistent", true);
        gitignoreIsInspected("nonexistent/foo", true);
        gitignoreIsInspected("nonexistent/foo/bar", true);
    }

    [TestMethod(DisplayName = "should return true for non exceptions of ignored subdirectories")]
    public void TrueForNonExceptionsOfIgnoredSubdirectories()
    {
        gitignoreIsInspected("nonexistent/wat", true);
        gitignoreIsInspected("nonexistent/wat/foo", true);
        gitignoreNoNegativesIsInspected("node_modules/wat/foo", true);
    }
}
