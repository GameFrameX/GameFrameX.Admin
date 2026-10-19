using System.Reflection;
using System.Text.RegularExpressions;
using GameFrameX.Entity;

namespace GameFrameX.Core.Tests.SqlSugar;

public class EntityStringLengthContractTests
{
    [Fact]
    public void All_Constants_Meet256MinimumFloor()
    {
        var constants = GetLengthConstants();

        Assert.NotEmpty(constants);
        Assert.All(constants, field =>
        {
            var value = Convert.ToInt32(field.GetRawConstantValue());
            Assert.True(value >= 256, $"常量 {field.Name}={value} 低于 256 下限（C281/GFX-689 规则：系统最低字符串长度 256）");
        });
    }

    [Fact]
    public void Tier_Set_Consolidated_To_SevenTiers_256To16384()
    {
        var expectedTiers = new[] { "Length256", "Length512", "Length1024", "Length2048", "Length4096", "Length8192", "Length16384" };
        var actualTiers = GetLengthConstants().Select(field => field.Name).OrderBy(name => name).ToArray();

        Assert.Equal(expectedTiers.OrderBy(name => name).ToArray(), actualTiers);
    }

    private static IReadOnlyList<FieldInfo> GetLengthConstants()
    {
        return typeof(EntityStringLength)
            .GetFields(BindingFlags.Public | BindingFlags.Static | BindingFlags.FlattenHierarchy)
            .Where(field => field.IsLiteral && field.FieldType == typeof(int))
            .ToList();
    }

    [Fact]
    public void Backend_LengthAttributes_DoNotUseRawNumberLiterals()
    {
        var roots = new[]
        {
            "GameFrameX.Entity",
            "GameFrameX.Core",
            "GameFrameX.Application",
            "GameFrameX.Application.System",
            "GameFrameX.Application.Game",
        };
        var forbiddenPatterns = new[]
        {
            new Regex(@"\bLength\s*=\s*\d+\b", RegexOptions.Compiled),
            new Regex(@"\b(?:MaxLength|StringLength)\s*\(\s*\d+", RegexOptions.Compiled),
        };

        var failures = roots.SelectMany(FindSourceDirectory)
                            .SelectMany(root => Directory.EnumerateFiles(root, "*.cs", SearchOption.AllDirectories))
                            .Where(path => !path.EndsWith($"{Path.DirectorySeparatorChar}EntityStringLength.cs", StringComparison.Ordinal))
                            .SelectMany(path => FindForbiddenLengthLiterals(path, forbiddenPatterns))
                            .ToList();

        Assert.Empty(failures);
    }

    private static IEnumerable<string> FindSourceDirectory(string directoryName)
    {
        var directory = new DirectoryInfo(AppContext.BaseDirectory);
        while (directory != null)
        {
            var candidate = Path.Combine(directory.FullName, directoryName);
            if (Directory.Exists(candidate))
            {
                yield return candidate;
                yield break;
            }

            directory = directory.Parent;
        }
    }

    private static IEnumerable<string> FindForbiddenLengthLiterals(string path, IReadOnlyCollection<Regex> patterns)
    {
        var lines = File.ReadAllLines(path);
        for (var i = 0; i < lines.Length; i++)
        {
            if (patterns.Any(pattern => pattern.IsMatch(lines[i])))
            {
                yield return $"{Path.GetFileName(path)}:{i + 1}:{lines[i].Trim()}";
            }
        }
    }
}