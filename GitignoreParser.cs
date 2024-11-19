using System;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.IO;
using System.Linq;
using System.Runtime.InteropServices;
using System.Text;
using System.Text.RegularExpressions;
using static GitignoreParserNet.RegexPatterns;

namespace GitignoreParserNet;

/// <summary>
/// A simple yet complete .gitignore parser for .NET
/// </summary>
/// <remarks>
/// This is a .NET port of the npm package gitignore-parser by Ger Hobbelt.
/// </remarks>
/// <seealso href="https://github.com/GerHobbelt/gitignore-parser"/>
/// <seealso href="https://github.com/GerHobbelt"/>
public sealed class GitignoreParser
{
    private static readonly string[] LINEBREAKS = ["\r\n", "\r", "\n"];

    private readonly (Regex Merged, Regex[] Individual) Positives;
    private readonly (Regex Merged, Regex[] Individual) Negatives;

    /// <summary>
    /// Parses a string containing the gitignore rules.
    /// </summary>
    /// <param name="content">The string containing the gitignore rules.</param>
    /// <param name="compileRegex">If <see langword="true"/>, the Regex objects will be compiled to improve consecutive uses.</param>
    public GitignoreParser(string content, bool compileRegex = false)
    {
        (Positives, Negatives) = Parse(content, compileRegex);
    }

    /// <summary>
    /// Parses a file containing the gitignore rules.
    /// </summary>
    /// <param name="gitignorePath">Path to the file containing the gitignore rules.</param>
    /// <param name="fileEncoding">The encoding applied to the contents of the file.</param>
    /// <param name="compileRegex">If <see langword="true"/>, the Regex objects will be compiled to improve consecutive uses.</param>
    public GitignoreParser(string gitignorePath, Encoding fileEncoding, bool compileRegex = false)
    {
        (Positives, Negatives) = Parse(File.ReadAllText(gitignorePath, fileEncoding), compileRegex);
    }

    /// <summary>
    /// Parses the given gitignore content and returns regex objects for matching positive and negativ filters.
    /// </summary>
    /// <param name="content">The string containing the gitignore rules.</param>
    /// <param name="compileRegex">If <see langword="true"/>, the Regex objects will be compiled to improve consecutive uses.</param>
    /// <returns><see cref="Regex"/> objects for positive and negative matching for the given gitignore rules.</returns>
    public static ((Regex Merged, Regex[] Individual) positives, (Regex Merged, Regex[] Individual) negatives) Parse(string content, bool compileRegex = false)
    {
        var regexOptions = compileRegex ? RegexOptions.Compiled : RegexOptions.None;

        (List<string> positive, List<string> negative) = content
            .Split(LINEBREAKS, StringSplitOptions.None)
            .Select(line => line.Trim())
            .Where(line => !string.IsNullOrWhiteSpace(line) && !line.StartsWith('#'))
            .Aggregate<string, (List<string>, List<string>), (List<string>, List<string>)>(
                ([], []),
                ((List<string> positive, List<string> negative) lists, string line) =>
                {
                    if (line.StartsWith('!'))
                        lists.negative.Add(line.Substring(1));
                    else
                        lists.positive.Add(line);
                    return (lists.positive, lists.negative);
                },
                ((List<string> positive, List<string> negative) lists) => lists
            );

        static (Regex Merged, Regex[] Individual) Submatch(List<string> list, RegexOptions regexOptions)
        {
            if (list.Count == 0)
            {
                return (MatchEmptyRegex, Array.Empty<Regex>());
            }
            else
            {
                var reList = list.OrderBy(str => str).Select(PrepareRegexPattern).ToList();
                return (
                    new Regex($"(?:{string.Join(")|(?:", reList)})", regexOptions),
                    reList.Select(s => new Regex(s, regexOptions)).ToArray()
                );
            }
        }

        return (Submatch(positive, regexOptions), Submatch(negative, regexOptions));
    }

    /// <summary>
    /// Parses a gitignore file and filters the files/directories inside the given directory recursively.
    /// </summary>
    /// <param name="content">The string containing the gitignore rules.</param>
    /// <param name="directoryPath">The directory path to the contents of which to apply the gitignore rules.</param>
    /// <param name="compileRegex">If <see langword="true"/>, the Regex objects will be compiled to improve consecutive uses.</param>
    /// <returns>Files and directories filtered with the given gitignore rules.</returns>
    public static (IEnumerable<string> Accepted, IEnumerable<string> Denied) Parse(string content, string directoryPath, bool compileRegex = false)
    {
        DirectoryInfo directory = new(directoryPath);
        GitignoreParser parser = new(content, compileRegex);

        var fileResults = parser.ProcessFiles(directory);
        var accepted = fileResults.Where(x => x.Accepted).Select(x => x.FilePath).ToArray();
        var denied = fileResults.Where(x => x.Denied).Select(x => x.FilePath).ToArray();

        return (accepted, denied);
    }

    /// <summary>
    /// Parses a gitignore file and filters the files/directories inside the given directory recursively.
    /// If no directory is given, the parent directory of the gitignore file is taken.
    /// </summary>
    /// <param name="gitignorePath">Path to the gitignore file.</param>
    /// <param name="fileEncoding">The encoding applied to the contents of the file.</param>
    /// <param name="compileRegex">If <see langword="true"/>, the Regex objects will be compiled to improve consecutive uses.</param>
    /// <returns>Files and directories filtered with the given gitignore rules.</returns>
    /// <exception cref="DirectoryNotFoundException">Couldn't find the parent dirrectory for <paramref name="gitignorePath"/>.</exception>
    public static (IEnumerable<string> Accepted, IEnumerable<string> Denied) Parse(string gitignorePath, Encoding fileEncoding, string? directoryPath = null, bool compileRegex = false)
    {
        DirectoryInfo directory = directoryPath != null
            ? new(directoryPath)
            : (new FileInfo(gitignorePath).Directory ?? throw new DirectoryNotFoundException($"Couldn't find the parent dirrectory for \"{gitignorePath}\""));

        GitignoreParser parser = new(gitignorePath, fileEncoding, compileRegex);

        var fileResults = parser.ProcessFiles(directory);
        var accepted = fileResults.Where(x => x.Accepted).Select(x => x.FilePath).ToArray();
        var denied = fileResults.Where(x => x.Denied).Select(x => x.FilePath).ToArray();

        return (accepted, denied);
    }

    /// <summary>
    /// Returns a list of relative paths of all subdirectories and files under the given directory (including the given directory itself).
    /// </summary>
    /// <param name="directory">The directory to traverse.</param>
    /// <returns>The list of relative paths of subdirectories and files.</returns>
    private static string[] ListFiles(DirectoryInfo directory)
    {
        var directoryPath = directory.FullName;

        return ((IEnumerable<string>)["/"]).Concat(
            Directory.GetFiles(directoryPath, "*.*", SearchOption.AllDirectories)
                .Select(f => f.Substring(directoryPath.Length + 1))
        ).ToArray();
    }

    /// <summary>
    /// Returns a tuple containing information about the acceptance or denieal of a file.
    /// </summary>
    /// <param name="directory">The directory to traverse.</param>
    /// <returns>A tuple containing information about the acceptance or denieal of a file.</returns>
    private (string FilePath, bool Accepted, bool Denied)[] ProcessFiles(DirectoryInfo directory)
    {
        var files = ListFiles(directory);
        return files.Select(f => (FilePath: f, Accepted: Accepts(f), Denied: Denies(f))).ToArray();
    }

    /// <summary>
    /// Tests whether the given file/directory passes the gitignore filters.
    /// </summary>
    /// <param name="input">The file/directory path to test.</param>
    /// <param name="expected">If not <see langword="null"/>, when the result of the method doesn't match the expected, print</param>
    /// <returns>
    /// <see langword="true"/> when the given `input` path <strong>passes</strong> the gitignore filters,
    /// i.e. when the given input path is <strong>denied</strong> (<i>ignored</i>).
    /// </returns>
    /// <remarks>
    /// <list type="bullet">
    /// <item>
    /// <description>
    /// You <strong>must</strong> postfix a input directory with a slash
    /// ('/') to ensure the gitignore rules can be applied conform spec.
    /// </description>
    /// </item>
    /// <item>
    /// <description>
    /// You <strong>may</strong> prefix a input directory with a slash ('/')
    /// when that directory is 'rooted' in the search directory.
    /// </description>
    /// </item>
    /// </list>
    /// </remarks>
#if DEBUG
    public bool Accepts(string input, bool? expected = null)
#else
    public bool Accepts(string input)
#endif
    {
        if (RuntimeInformation.IsOSPlatform(OSPlatform.Windows))
            input = input.Replace('\\', '/');

        if (!input.StartsWith('/'))
            input = "/" + input;

        var acceptTest = Negatives.Merged.IsMatch(input);
        var denyTest = Positives.Merged.IsMatch(input);
        var returnVal = acceptTest || !denyTest;

        // See the test/fixtures/gitignore.manpage.txt near line 680 (grep for "uber-nasty"):
        // to resolve chained rules which reject, then accept, we need to establish
        // the precedence of both accept and reject parts of the compiled gitignore by
        // comparing match lengths.
        // Since the generated consolidated regexes are lazy, we must loop through all lines' regexes instead:
#if DEBUG
        Match? acceptMatch = null, denyMatch = null;
#endif
        if (acceptTest && denyTest)
        {
            int acceptLength = 0, denyLength = 0;
            foreach (var re in Negatives.Individual)
            {
                var m = re.Match(input);
                if (m.Success && acceptLength < m.Value.Length)
                {
#if DEBUG
                    acceptMatch = m;
#endif
                    acceptLength = m.Value.Length;
                }
            }
            foreach (var re in Positives.Individual)
            {
                var m = re.Match(input);
                if (m.Success && denyLength < m.Value.Length)
                {
#if DEBUG
                    denyMatch = m;
#endif
                    denyLength = m.Value.Length;
                }
            }
            returnVal = acceptLength >= denyLength;
        }
#if DEBUG
        if (expected != null && expected != returnVal)
        {
            Diagnose(
                "accepts",
                input,
                (bool)expected,
                Negatives.Merged,
                acceptTest,
                acceptMatch,
                Positives.Merged,
                denyTest,
                denyMatch,
                "(Accept || !Deny)",
                returnVal
            );
        }
#endif
        return returnVal;
    }

    /// <summary>
    /// Tests whether the given files/directories pass the gitignore filters.
    /// </summary>
    /// <param name="inputs">The file/directory paths to test.</param>
    /// <returns>
    /// <see cref="IEnumerable{string}"/> with the paths that <strong>pass</strong> the gitignore filters,
    /// i.e. the paths that are <strong>denied</strong> (<i>ignored</i>).
    /// </returns>
    /// <remarks>
    /// <list type="bullet">
    /// <item>
    /// <description>
    /// You <strong>must</strong> postfix a input directory with a slash
    /// ('/') to ensure the gitignore rules can be applied conform spec.
    /// </description>
    /// </item>
    /// <item>
    /// <description>
    /// You <strong>may</strong> prefix a input directory with a slash ('/')
    /// when that directory is 'rooted' in the search directory.
    /// </description>
    /// </item>
    /// </list>
    /// </remarks>
    public IEnumerable<string> Accepted(IEnumerable<string> inputs)
    {
        return inputs.Where(f => Accepts(f));
    }

    /// <summary>
    /// Tests whether the given files/directories pass the gitignore filters.
    /// </summary>
    /// <param name="directory">The directory to test.</param>
    /// <returns>
    /// <see cref="IEnumerable{string}"/> with the paths that <strong>pass</strong> the gitignore filters,
    /// i.e. the paths that are <strong>denied</strong> (<i>ignored</i>).
    /// </returns>
    /// <remarks>
    /// <list type="bullet">
    /// <item>
    /// <description>
    /// You <strong>must</strong> postfix a input directory with a slash
    /// ('/') to ensure the gitignore rules can be applied conform spec.
    /// </description>
    /// </item>
    /// <item>
    /// <description>
    /// You <strong>may</strong> prefix a input directory with a slash ('/')
    /// when that directory is 'rooted' in the search directory.
    /// </description>
    /// </item>
    /// </list>
    /// </remarks>
    public IEnumerable<string> Accepted(DirectoryInfo directory)
    {
        var files = ListFiles(directory);
        return files.Where(f => Accepts(f));
    }

    /// <summary>
    /// Tests whether the given file/directory fails the gitignore filters.
    /// </summary>
    /// <param name="input">The file/directory path to test.</param>
    /// <param name="expected">If not <see langword="null"/>, when the result of the method doesn't match the expected, print</param>
    /// <returns>
    /// <see langword="true"/> when the given `input` path <strong>fails</strong> the gitignore filters,
    /// i.e. when the given input path is <strong>accepted</strong> (<i>not ignored</i>).
    /// </returns>
    /// <remarks>
    /// <list type="bullet">
    /// <item>
    /// <description>
    /// You <strong>must</strong> postfix a input directory with a slash
    /// ('/') to ensure the gitignore rules can be applied conform spec.
    /// </description>
    /// </item>
    /// <item>
    /// <description>
    /// You <strong>may</strong> prefix a input directory with a slash ('/')
    /// when that directory is 'rooted' in the search directory.
    /// </description>
    /// </item>
    /// </list>
    /// </remarks>
#if DEBUG
    public bool Denies(string input, bool? expected = null)
#else
    public bool Denies(string input)
#endif
    {
        if (RuntimeInformation.IsOSPlatform(OSPlatform.Windows))
            input = input.Replace('\\', '/');

        if (!input.StartsWith('/'))
            input = "/" + input;

        var acceptTest = Negatives.Merged.IsMatch(input);
        var denyTest = Positives.Merged.IsMatch(input);
        // boolean logic:
        //
        // Denies = !Accepts =>
        // Denies = !(Accept || !Deny) =>
        // Denies = (!Accept && !!Deny) =>
        // Denies = (!Accept && Deny)
        var returnVal = !acceptTest && denyTest;

        // See the test/fixtures/gitignore.manpage.txt near line 680 (grep for "uber-nasty"):
        // to resolve chained rules which reject, then accept, we need to establish
        // the precedence of both accept and reject parts of the compiled gitignore by
        // comparing match lengths.
        // Since the generated regexes are all set up to be GREEDY, we can use the
        // consolidated regex for this, instead of having to loop through all lines' regexes:
#if DEBUG
        Match? acceptMatch = null, denyMatch = null;
#endif
        if (acceptTest && denyTest)
        {
            int acceptLength = 0, denyLength = 0;
            foreach (var re in Negatives.Individual)
            {
                var m = re.Match(input);
                if (m.Success && acceptLength < m.Value.Length)
                {
#if DEBUG
                    acceptMatch = m;
#endif
                    acceptLength = m.Value.Length;
                }
            }
            foreach (var re in Positives.Individual)
            {
                var m = re.Match(input);
                if (m.Success && denyLength < m.Value.Length)
                {
#if DEBUG
                    denyMatch = m;
#endif
                    denyLength = m.Value.Length;
                }
            }
            returnVal = acceptLength < denyLength;
        }
#if DEBUG
        if (expected != null && expected != returnVal)
        {
            Diagnose(
                "denies",
                input,
                (bool)expected,
                Negatives.Merged,
                acceptTest,
                acceptMatch,
                Positives.Merged,
                denyTest,
                denyMatch,
                "(!Accept && Deny)",
                returnVal
            );
        }
#endif
        return returnVal;
    }

    /// <summary>
    /// Tests whether the given files/directories fail the gitignore filters.
    /// </summary>
    /// <param name="inputs">The file/directory paths to test.</param>
    /// <returns>
    /// <see cref="IEnumerable{string}"/> with the paths that <strong>fail</strong> the gitignore filters,
    /// i.e. the paths that are <strong>accepted</strong> (<i>not ignored</i>).
    /// </returns>
    /// <remarks>
    /// <list type="bullet">
    /// <item>
    /// <description>
    /// You <strong>must</strong> postfix a input directory with a slash
    /// ('/') to ensure the gitignore rules can be applied conform spec.
    /// </description>
    /// </item>
    /// <item>
    /// <description>
    /// You <strong>may</strong> prefix a input directory with a slash ('/')
    /// when that directory is 'rooted' in the search directory.
    /// </description>
    /// </item>
    /// </list>
    /// </remarks>
    public IEnumerable<string> Denied(IEnumerable<string> inputs)
    {
        return inputs.Where(f => Denies(f));
    }

    /// <summary>
    /// Tests whether the given files/subdirectories under the specified directory fail the gitignore filters.
    /// </summary>
    /// <param name="directory">The directory to test.</param>
    /// <returns>
    /// <see cref="IEnumerable{string}"/> with the paths that <strong>fail</strong> the gitignore filters,
    /// i.e. the paths that are <strong>accepted</strong> (<i>not ignored</i>).
    /// </returns>
    /// <remarks>
    /// <list type="bullet">
    /// <item>
    /// <description>
    /// You <strong>must</strong> postfix a input directory with a slash
    /// ('/') to ensure the gitignore rules can be applied conform spec.
    /// </description>
    /// </item>
    /// <item>
    /// <description>
    /// You <strong>may</strong> prefix a input directory with a slash ('/')
    /// when that directory is 'rooted' in the search directory.
    /// </description>
    /// </item>
    /// </list>
    /// </remarks>
    public IEnumerable<string> Denied(DirectoryInfo directory)
    {
        var files = ListFiles(directory);
        return files.Where(f => Denies(f));
    }

    /// <summary>
    /// You can use this method to help construct the decision path when you
    /// process nested gitignore files: gitignore filters in subdirectories
    /// <strong>may</strong> override parent gitignore filters only when
    /// there's actually <strong>any</strong> filter in the child gitignore
    /// after all.
    /// </summary>
    /// <param name="input">The file/directory path to test.</param>
    /// <param name="expected">If not <see langword="null"/>, when the result of the method doesn't match the expected, print</param>
    /// <returns>
    /// <see langword="true"/> when the given `input` path is inspected by the gitignore filters.
    /// </returns>
    /// <remarks>
    /// <list type="bullet">
    /// <item>
    /// <description>
    /// You <strong>must</strong> postfix a input directory with a slash
    /// ('/') to ensure the gitignore rules can be applied conform spec.
    /// </description>
    /// </item>
    /// <item>
    /// <description>
    /// You <strong>may</strong> prefix a input directory with a slash ('/')
    /// when that directory is 'rooted' in the search directory.
    /// </description>
    /// </item>
    /// </list>
    /// </remarks>
#if DEBUG
    public bool Inspects(string input, bool? expected = null)
#else
    public bool Inspects(string input)
#endif
    {
        if (RuntimeInformation.IsOSPlatform(OSPlatform.Windows))
            input = input.Replace('\\', '/');

        if (!input.StartsWith('/'))
            input = "/" + input;

        var acceptTest = Negatives.Merged.IsMatch(input);
        var denyTest = Positives.Merged.IsMatch(input);
        // when any filter 'touches' the input path, it must match,
        // no matter whether it's a deny or accept filter line:
        var returnVal = acceptTest || denyTest;
#if DEBUG
        if (expected != null && expected != returnVal)
        {
            Diagnose(
                "inspects",
                input,
                (bool)expected,
                Negatives.Merged,
                acceptTest,
                null,
                Positives.Merged,
                denyTest,
                null,
                "(Accept || Deny)",
                returnVal
            );
        }
#endif
        return returnVal;
    }

    [SuppressMessage("Major Code Smell", "S1121:Assignments should not be made from within sub-expressions")]
    private static string PrepareRegexPattern(string pattern)
    {
        // https://git-scm.com/docs/gitignore#_pattern_format
        //
        // * ...
        //
        // * If there is a separator at the beginning or middle (or both) of the pattern,
        //   then the pattern is relative to the directory level of the particular
        //   .gitignore file itself.
        //   Otherwise the pattern may also match at any level below the .gitignore level.
        //
        // * ...
        //
        // * For example, a pattern `doc/frotz/` matches `doc/frotz` directory, but
        //   not `a/doc/frotz` directory; however `frotz/` matches `frotz` and `a/frotz`
        //   that is a directory (all paths are relative from the .gitignore file).
        //
#if DEBUG
        string input = pattern;
#endif
        var reBuilder = new StringBuilder();
        bool rooted = false, directory = false;
        if (pattern.StartsWith('/'))
        {
            rooted = true;
            pattern = pattern.Substring(1);
        }
        if (pattern.EndsWith('/'))
        {
            directory = true;
            pattern = pattern.Substring(0, pattern.Length - 1);
        }

        string transpileRegexPart(string _re)
        {
            if (_re.Length == 0) return _re;
            // unescape for these will be escaped again in the subsequent `.Replace(...)`,
            // whether they were escaped before or not:
            _re = BackslashRegex.Replace(_re, "$1");
            // escape special regex characters:
            _re = SpecialCharactersRegex.Replace(_re, @"\$&");
            _re = QuestionMarkRegex.Replace(_re, "[^/]");
            _re = SlashDoubleAsteriksSlashRegex.Replace(_re, "(?:/|(?:/.+/))");
            _re = DoubleAsteriksSlashRegex.Replace(_re, "(?:|(?:.+/))");
            _re = SlashDoubleAsteriksRegex.Replace(_re, _ =>
            {
                directory = true;       // `a/**` should match `a/`, `a/b/` and `a/b`, the latter by implication of matching directory `a/`
                return "(?:|(?:/.+))";  // `a/**` also accepts `a/` itself
            });
            _re = DoubleAsteriksRegex.Replace(_re, ".*");
            // `a/*` should match `a/b` and `a/b/` but NOT `a` or `a/`
            // meanwhile, `a/*/` should match `a/b/` and `a/b/c` but NOT `a` or `a/` or `a/b`
            _re = SlashAsteriksEndOrSlashRegex.Replace(_re, "/[^/]+$1");
            _re = AsteriksRegex.Replace(_re, "[^/]*");
            _re = SlashRegex.Replace(_re, @"\/");
            return _re;
        }

        // keep character ranges intact:
        Regex rangeRe = RangeRegex;
        // ^ could have used the 'y' sticky flag, but there's some trouble with infinite loops inside
        //   the matcher below then...
        for (Match match; (match = rangeRe.Match(pattern)).Success;)
        {
            if (match.Groups[1].Value.Contains('/'))
            {
                rooted = true;
                // ^ cf. man page:
                //
                //   If there is a separator at the beginning or middle (or both)
                //   of the pattern, then the pattern is relative to the directory
                //   level of the particular .gitignore file itself. Otherwise
                //   the pattern may also match at any level below the .gitignore level.
            }
            reBuilder.Append(transpileRegexPart(match.Groups[1].Value));
            reBuilder.Append('[').Append(match.Groups[2].Value).Append(']');

            pattern = pattern.Substring(match.Length);
        }
        if (!string.IsNullOrWhiteSpace(pattern))
        {
            if (pattern.Contains('/'))
            {
                rooted = true;
                // ^ cf. man page:
                //
                //   If there is a separator at the beginning or middle (or both)
                //   of the pattern, then the pattern is relative to the directory
                //   level of the particular .gitignore file itself. Otherwise
                //   the pattern may also match at any level below the .gitignore level.
            }
            reBuilder.Append(transpileRegexPart(pattern));
        }

        // prep regexes assuming we'll always prefix the check string with a '/':
        reBuilder.Prepend(rooted ? @"^\/" : @"\/");
        // cf spec:
        //
        //   If there is a separator at the end of the pattern then the pattern
        //   will only match directories, otherwise the pattern can match
        //   **both files and directories**.                   (emphasis mine)
        // if `directory`: match the directory itself and anything within
        // otherwise: match the file itself, or, when it is a directory, match the directory and anything within
        reBuilder.Append(directory ? @"\/" : @"(?:$|\/)");

        // regex validation diagnostics: better to check if the part is valid
        // then to discover it's gone haywire in the big conglomerate at the end.

        string re = reBuilder.ToString();

#if DEBUG
        try
        {
#pragma warning disable S1481 // Unused local variables should be removed
            Regex regex = new($"(?:{re})"); // throws ArgumentException if a regular expression parsing error occurred.
#pragma warning restore S1481 // Unused local variables should be removed
        }
        catch (ArgumentException ex)
        {
            Console.WriteLine("Failed regex: \n\tinput: {0}\n\tregex: {1}\n\texception: {2}", input, re, ex);
        }
#endif

        return re;
    }

#if DEBUG
    public sealed class OnExpectedMatchFailEventArgs(
        string query,
        string input,
        bool expected,
        Regex acceptRe,
        bool acceptTest,
        Match? acceptMatch,
        Regex denyRe,
        bool denyTest,
        Match? denyMatch,
        string combine,
        bool returnVal) : EventArgs
    {
        public string Query { get; set; } = query;
        public string Input { get; set; } = input;
        public bool Expected { get; set; } = expected;
        public Regex AcceptRe { get; set; } = acceptRe;
        public bool AcceptTest { get; set; } = acceptTest;
        public Match? AcceptMatch { get; set; } = acceptMatch;
        public Regex DenyRe { get; set; } = denyRe;
        public bool DenyTest { get; set; } = denyTest;
        public Match? DenyMatch { get; set; } = denyMatch;
        public string Combine { get; set; } = combine;
        public bool ReturnVal { get; set; } = returnVal;
    }

    /// <summary>
    /// This event will be invoked if any of `Accepts()`, `Denies()` or `Inspects()`
    /// fail to match the expected result.
    /// </summary>
    public event EventHandler<OnExpectedMatchFailEventArgs>? OnExpectedMatchFail;

    /// <summary>
    /// Helper invoked when any of `Accepts()`, `Denies()` or `Inspects()`
    /// fail, to help the developer analyze what is going on inside:
    /// some gitignore spec bits are non-intuitive / non-trivial, after all.
    /// </summary>
    private void Diagnose(
        string query,
        string input,
        bool expected,
        Regex acceptRe,
        bool acceptTest,
        Match? acceptMatch,
        Regex denyRe,
        bool denyTest,
        Match? denyMatch,
        string combine,
        bool returnVal
    )
    {
        if (OnExpectedMatchFail != null)
        {
            OnExpectedMatchFail(this,
                new OnExpectedMatchFailEventArgs(
                    query,
                    input,
                    expected,
                    acceptRe,
                    acceptTest,
                    acceptMatch,
                    denyRe,
                    denyTest,
                    denyMatch,
                    combine,
                    returnVal
                )
            );
            return;
        }

        Console.WriteLine(
            $$"""
            '{{query}}': {
                'query': '{{query}}',
                'input': '{{input}}',
                'expected': '{{expected}}',
                'acceptRe': '{{acceptRe}}',
                'acceptTest': '{{acceptTest}}',
                'acceptMatch': '{{acceptMatch}}',
                'denyRe': '{{denyRe}}',
                'denyTest': '{{denyTest}}',
                'denyMatch': '{{denyMatch}}',
                'combine': '{{combine}}',
                'returnVal': '{{returnVal}}'
            }
            """
        );
    }
#endif
}
