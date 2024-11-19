using System.Text.RegularExpressions;

namespace GitignoreParserNet;

internal static
#if NET7_0_OR_GREATER
    partial
#endif
    class RegexPatterns
{
    //language=Regex
    private const string MatchEmptyRegexPattern = "$^";
    //language=Regex
    private const string RangeRegexPattern = @"^((?:[^\[\\]|(?:\\.))*)\[((?:[^\]\\]|(?:\\.))*)\]";
    //language=Regex
    private const string BackslashRegexPattern = @"\\(.)";
    //language=Regex
    private const string SpecialCharactersRegexPattern = @"[\-\[\]\{\}\(\)\+\.\\\^\$\|]";
    //language=Regex
    private const string QuestionMarkRegexPattern = @"\?";
    //language=Regex
    private const string SlashDoubleAsteriksSlashRegexPattern = @"\/\*\*\/";
    //language=Regex
    private const string DoubleAsteriksSlashRegexPattern = @"^\*\*\/";
    //language=Regex
    private const string SlashDoubleAsteriksRegexPattern = @"\/\*\*$";
    //language=Regex
    private const string DoubleAsteriksRegexPattern = @"\*\*";
    //language=Regex
    private const string SlashAsteriksEndOrSlashRegexPattern = @"\/\*(\/|$)";
    //language=Regex
    private const string AsteriksRegexPattern = @"\*";
    //language=Regex
    private const string SlashRegexPattern = @"\/";

#if NET7_0_OR_GREATER
    [GeneratedRegex(MatchEmptyRegexPattern)]
    private static partial Regex GeneratedMatchEmptyRegex();
    [GeneratedRegex(RangeRegexPattern)]
    private static partial Regex GeneratedRangeRegex();
    [GeneratedRegex(BackslashRegexPattern)]
    private static partial Regex GeneratedBackslashRegex();
    [GeneratedRegex(SpecialCharactersRegexPattern)]
    private static partial Regex GeneratedSpecialCharactersRegex();
    [GeneratedRegex(QuestionMarkRegexPattern)]
    private static partial Regex GeneratedQuestionMarkRegex();
    [GeneratedRegex(SlashDoubleAsteriksSlashRegexPattern)]
    private static partial Regex GeneratedSlashDoubleAsteriksSlashRegex();
    [GeneratedRegex(DoubleAsteriksSlashRegexPattern)]
    private static partial Regex GeneratedDoubleAsteriksSlashRegex();
    [GeneratedRegex(SlashDoubleAsteriksRegexPattern)]
    private static partial Regex GeneratedSlashDoubleAsteriksRegex();
    [GeneratedRegex(DoubleAsteriksRegexPattern)]
    private static partial Regex GeneratedDoubleAsteriksRegex();
    [GeneratedRegex(SlashAsteriksEndOrSlashRegexPattern)]
    private static partial Regex GeneratedSlashAsteriksEndOrSlashRegex();
    [GeneratedRegex(AsteriksRegexPattern)]
    private static partial Regex GeneratedAsteriksRegex();
    [GeneratedRegex(SlashRegexPattern)]
    private static partial Regex GeneratedSlashRegex();

    public static readonly Regex MatchEmptyRegex = GeneratedMatchEmptyRegex();
    public static readonly Regex RangeRegex = GeneratedRangeRegex();
    public static readonly Regex BackslashRegex = GeneratedBackslashRegex();
    public static readonly Regex SpecialCharactersRegex = GeneratedSpecialCharactersRegex();
    public static readonly Regex QuestionMarkRegex = GeneratedQuestionMarkRegex();
    public static readonly Regex SlashDoubleAsteriksSlashRegex = GeneratedSlashDoubleAsteriksSlashRegex();
    public static readonly Regex DoubleAsteriksSlashRegex = GeneratedDoubleAsteriksSlashRegex();
    public static readonly Regex SlashDoubleAsteriksRegex = GeneratedSlashDoubleAsteriksRegex();
    public static readonly Regex DoubleAsteriksRegex = GeneratedDoubleAsteriksRegex();
    public static readonly Regex SlashAsteriksEndOrSlashRegex = GeneratedSlashAsteriksEndOrSlashRegex();
    public static readonly Regex AsteriksRegex = GeneratedAsteriksRegex();
    public static readonly Regex SlashRegex = GeneratedSlashRegex();
#else
    public static readonly Regex MatchEmptyRegex = new(MatchEmptyRegexPattern, RegexOptions.Compiled);
    public static readonly Regex RangeRegex = new(RangeRegexPattern, RegexOptions.Compiled);
    public static readonly Regex BackslashRegex = new(BackslashRegexPattern, RegexOptions.Compiled);
    public static readonly Regex SpecialCharactersRegex = new(SpecialCharactersRegexPattern, RegexOptions.Compiled);
    public static readonly Regex QuestionMarkRegex = new(QuestionMarkRegexPattern, RegexOptions.Compiled);
    public static readonly Regex SlashDoubleAsteriksSlashRegex = new(SlashDoubleAsteriksSlashRegexPattern, RegexOptions.Compiled);
    public static readonly Regex DoubleAsteriksSlashRegex = new(DoubleAsteriksSlashRegexPattern, RegexOptions.Compiled);
    public static readonly Regex SlashDoubleAsteriksRegex = new(SlashDoubleAsteriksRegexPattern, RegexOptions.Compiled);
    public static readonly Regex DoubleAsteriksRegex = new(DoubleAsteriksRegexPattern, RegexOptions.Compiled);
    public static readonly Regex SlashAsteriksEndOrSlashRegex = new(SlashAsteriksEndOrSlashRegexPattern, RegexOptions.Compiled);
    public static readonly Regex AsteriksRegex = new(AsteriksRegexPattern, RegexOptions.Compiled);
    public static readonly Regex SlashRegex = new(SlashRegexPattern, RegexOptions.Compiled);
#endif
}
