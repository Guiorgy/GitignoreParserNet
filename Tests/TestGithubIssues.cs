using GitignoreParserNet;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using System.Text;

namespace Tests.TestGithubIssues;

internal static class TestHelper
{
    public static void gitignoreAccepts(GitignoreParser gitignore, string str, bool expected)
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

    public static void gitignoreDenies(GitignoreParser gitignore, string str, bool expected)
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
}

[TestClass]
public class TestCaseA
{
    [TestMethod(DisplayName = "should only accept a/2/a")]
    public void OnlyAcceptA2A()
    {
        var gitignore = new GitignoreParser(@"..\..\..\a\.gitignore-fixture", Encoding.UTF8);
        TestHelper.gitignoreAccepts(gitignore, "a/2/a", true);
        TestHelper.gitignoreAccepts(gitignore, "a/3/a", false);
    }
}

[TestClass]
public class TestIssueN11
{
    [TestMethod(DisplayName = "should not fail test A")]
    public void TestA()
    {
        var gitignore = new GitignoreParser("foo.txt");
        TestHelper.gitignoreDenies(gitignore, "foo.txt", true);
        TestHelper.gitignoreAccepts(gitignore, "foo.txt", false);
    }

    [TestMethod(DisplayName = "should not fail test B")]
    public void TestB()
    {
        var gitignore = new GitignoreParser("foo.txt");
        TestHelper.gitignoreDenies(gitignore, "a/foo.txt", true);
        TestHelper.gitignoreAccepts(gitignore, "a/foo.txt", false);
    }
}

[TestClass]
public class TestIssueN12
{
    [TestMethod(DisplayName = "should not fail test A")]
    public void TestA()
    {
        var gitignore = new GitignoreParser("/ajax/libs/bPopup/*b*");
        TestHelper.gitignoreAccepts(gitignore, "/ajax/libs/bPopup/0.9.0", true);  //output false
    }

    [TestMethod(DisplayName = "should not fail test B")]
    public void TestB()
    {
        var gitignore = new GitignoreParser("/ajax/libs/jquery-form-validator/2.2");
        TestHelper.gitignoreAccepts(gitignore, "/ajax/libs/jquery-form-validator/2.2.43", true); //output false
    }

    [TestMethod(DisplayName = "should not fail test C")]
    public void TestC()
    {
        var gitignore = new GitignoreParser("/ajax/libs/punycode/2.0");
        TestHelper.gitignoreAccepts(gitignore, "/ajax/libs/punycode/2.0.0", true); //output false
    }

    [TestMethod(DisplayName = "should not fail test D")]
    public void TestD()
    {
        var gitignore = new GitignoreParser("/ajax/libs/typescript/*dev*");
        TestHelper.gitignoreAccepts(gitignore, "/ajax/libs/typescript/2.0.6-insiders.20161014", true); //output false
    }
}

[TestClass]
public class TestIssueN14
{
    [TestMethod(DisplayName = "should not fail test A")]
    public void TestA()
    {
        var gitignore = new GitignoreParser("node-modules");
        TestHelper.gitignoreDenies(gitignore, "packages/my-package/node-modules", true);
        TestHelper.gitignoreAccepts(gitignore, "packages/my-package/node-modules", false);

        TestHelper.gitignoreDenies(gitignore, "packages/my-package/node-modules/a", true);
        TestHelper.gitignoreAccepts(gitignore, "packages/my-package/node-modules/a", false);

        TestHelper.gitignoreDenies(gitignore, "node-modules/a", true);
        TestHelper.gitignoreAccepts(gitignore, "node-modules/a", false);
    }
}

#if DEBUG
[TestClass]
// when using the `expected` argument, accepts/denies should fire the `Diagnose()` callback
public class TestDiagnose
{
    [TestMethod(DisplayName = "Accepts() should fail and call Diagnose()")]
    public void TestA()
    {
        var gitignore = new GitignoreParser("node_modules");
        int count = 0;
        gitignore.OnExpectedMatchFail += (sender, e) => count++;

        Assert.IsFalse(gitignore.Accepts("node_modules", true  /* INTENTIONALLY WRONG */), "Accepts() call is expected to return FALSE!");
        Assert.AreEqual(1, count, "gitignore.Diagnose() should have been invoked once from inside the Accepts() call!");
        // the next one is matching the expectation, hence no Diagnose() call:
        Assert.IsFalse(gitignore.Accepts("node_modules", false), "Accepts() call is expected to return FALSE!");
        Assert.AreEqual(1, count, "gitignore.Diagnose() should have been invoked once from inside the Accepts() call!");

        Assert.IsTrue(gitignore.Denies("node_modules", false  /* INTENTIONALLY WRONG */), "Denies() call is expected to return TRUE!");
        Assert.AreEqual(2, count, "gitignore.Diagnose() should have been invoked once from inside the Denies() call!");
        // the next one is matching the expectation, hence no Diagnose() call:
        Assert.IsTrue(gitignore.Denies("node_modules", true), "Denies() call is expected to return TRUE!");
        Assert.AreEqual(2, count, "gitignore.Diagnose() should have been invoked once from inside the Denies() call!");

        Assert.IsTrue(gitignore.Inspects("node_modules", false  /* INTENTIONALLY WRONG */), "Inspects() call is expected to return TRUE!");
        Assert.AreEqual(3, count, "gitignore.Diagnose() should have been invoked once from inside the Inspects() call!");
        // the next one is matching the expectation, hence no Diagnose() call:
        Assert.IsTrue(gitignore.Inspects("node_modules", true), "Inspects() call is expected to return TRUE!");
        Assert.AreEqual(3, count, "gitignore.Diagnose() should have been invoked once from inside the Inspects() call!");
    }
}
#endif

/* TODO: Consider using the "fixtures/gitignore.manpage.txt" file to generate more tests. */
