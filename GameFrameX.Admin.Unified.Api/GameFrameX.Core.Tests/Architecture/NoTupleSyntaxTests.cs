using System.Text.RegularExpressions;

namespace GameFrameX.Core.Tests.Architecture;

public sealed class NoTupleSyntaxTests
{
    private static readonly string RepositoryRoot = GetRepositoryRoot();

    // C118 (GFX-121): 后端 C# 元组语法已全部替换为具名 sealed class/DTO，基线清空，本测试恢复为“零容忍”架构关卡。
    // 新增任何元组语法（ValueTuple / Tuple<> / 元组表达式 / 元组解构）都会直接失败，必须用具名类型替代。
    private static readonly Dictionary<string, int> AllowedTupleSyntaxBaseline = new(StringComparer.Ordinal)
    {
    };

    [Fact]
    public void BackendSource_DoesNotIntroduceNewTupleSyntax()
    {
        var actual = FindTupleSyntax().GroupBy(hit => hit).ToDictionary(group => group.Key, group => group.Count(), StringComparer.Ordinal);
        var newHits = actual
                      .Where(hit => !AllowedTupleSyntaxBaseline.TryGetValue(hit.Key, out var expectedCount) || hit.Value > expectedCount)
                      .Select(hit => $"{hit.Key} count={hit.Value}")
                      .Order(StringComparer.Ordinal)
                      .ToList();

        Assert.True(
            newHits.Count == 0,
            "C# tuple syntax is not allowed in backend code. Use a named record/class/DTO instead. New hits:" + Environment.NewLine + string.Join(Environment.NewLine, newHits));
    }

    private static IEnumerable<string> FindTupleSyntax()
    {
        foreach (var file in Directory.EnumerateFiles(RepositoryRoot, "*.cs", SearchOption.AllDirectories))
        {
            if (ShouldSkip(file))
            {
                continue;
            }

            var relativePath = Path.GetRelativePath(RepositoryRoot, file).Replace(Path.DirectorySeparatorChar, '/');
            foreach (var line in File.ReadLines(file))
            {
                var code = StripLineComment(line).Trim();
                if (code.Length == 0 || !ContainsTupleSyntax(code))
                {
                    continue;
                }

                yield return $"{relativePath}|{code}";
            }
        }
    }

    private static bool ContainsTupleSyntax(string code)
    {
        // ponytail: This line-based rule blocks tuple shapes used in this repo; upgrade to a Roslyn analyzer if multiline tuple syntax becomes a real case.
        return code.Contains("ValueTuple", StringComparison.Ordinal)
               || code.Contains("Tuple<", StringComparison.Ordinal)
               || code.Contains("Tuple.Create", StringComparison.Ordinal)
               || Regex.IsMatch(code, @"\b(?:Task|ValueTask|IEnumerable|IReadOnlyList|IReadOnlyCollection|List)<\s*\([^;{}]*,[^;{}]*\)")
               || Regex.IsMatch(code, @"\b(?:public|private|internal|protected)\s+(?:static\s+)?(?:async\s+)?\([^;{}=]*,[^;{}=]*\)\s+\w+\s*\(")
               || Regex.IsMatch(code, @"\bvar\s+\([^;{}]*,[^;{}]*\)\s*=")
               || IsTupleReturn(code, "yield return")
               || IsTupleReturn(code, "return");
    }

    private static bool IsTupleReturn(string code, string keyword)
    {
        var keywordIndex = code.IndexOf(keyword, StringComparison.Ordinal);
        if (keywordIndex < 0)
        {
            return false;
        }

        var openIndex = keywordIndex + keyword.Length;
        while (openIndex < code.Length && char.IsWhiteSpace(code[openIndex]))
        {
            openIndex++;
        }

        if (openIndex >= code.Length || code[openIndex] != '(')
        {
            return false;
        }

        var depth = 0;
        var hasTopLevelComma = false;
        for (var i = openIndex; i < code.Length; i++)
        {
            var current = code[i];
            if (current == '(')
            {
                depth++;
                continue;
            }

            if (current == ')')
            {
                depth--;
                if (depth == 0)
                {
                    return hasTopLevelComma && code[(i + 1)..].Trim() == ";";
                }

                continue;
            }

            if (current == ',' && depth == 1)
            {
                hasTopLevelComma = true;
            }
        }

        return false;
    }

    private static bool ShouldSkip(string file)
    {
        var relativePath = Path.GetRelativePath(RepositoryRoot, file).Replace(Path.DirectorySeparatorChar, '/');
        return relativePath.Contains("/bin/", StringComparison.Ordinal)
               || relativePath.Contains("/obj/", StringComparison.Ordinal)
               || relativePath == "GameFrameX.Core.Tests/Architecture/NoTupleSyntaxTests.cs";
    }

    private static string StripLineComment(string line)
    {
        var inString = false;
        var inChar = false;
        var isVerbatimString = false;

        for (var i = 0; i < line.Length - 1; i++)
        {
            var current = line[i];
            var next = line[i + 1];

            if (inString)
            {
                if (isVerbatimString && current == '"' && next == '"')
                {
                    i++;
                    continue;
                }

                if (current == '"' && (isVerbatimString || !IsEscaped(line, i)))
                {
                    inString = false;
                    isVerbatimString = false;
                }

                continue;
            }

            if (inChar)
            {
                if (current == '\'' && !IsEscaped(line, i))
                {
                    inChar = false;
                }

                continue;
            }

            if (current == '/' && next == '/')
            {
                return line[..i];
            }

            if (current == '"' || (current == '@' && next == '"'))
            {
                inString = true;
                isVerbatimString = current == '@';
                if (isVerbatimString)
                {
                    i++;
                }

                continue;
            }

            if (current == '\'')
            {
                inChar = true;
            }
        }

        return line;
    }

    private static bool IsEscaped(string text, int quoteIndex)
    {
        var slashCount = 0;
        for (var i = quoteIndex - 1; i >= 0 && text[i] == '\\'; i--)
        {
            slashCount++;
        }

        return slashCount % 2 == 1;
    }

    private static string GetRepositoryRoot()
    {
        var directory = new DirectoryInfo(AppContext.BaseDirectory);
        while (directory != null && !File.Exists(Path.Combine(directory.FullName, "GameFrameX.Admin.Unified.slnx")))
        {
            directory = directory.Parent;
        }

        return directory?.FullName ?? throw new DirectoryNotFoundException("Could not find GameFrameX.Admin.Unified.slnx.");
    }
}