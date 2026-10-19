using System.Globalization;
using System.Reflection;
using System.Security.Cryptography;
using System.Text;
using System.Text.Json;
using System.Text.Json.Serialization;
using GameFrameX.Core.Enum;
using GameFrameX.Entity.Enum;

namespace GameFrameX.Core.Tests.Enum;

public class EnumContractTests
{
    private const string ExpectedContractSha256 =
        "B6DD6CA19E4C3437CA2071DA0757246E580814D62F5443949BC090E20AEE6AE7";

    private static readonly JsonSerializerOptions StringEnumOptions = new()
    {
        Converters = { new JsonStringEnumConverter(), },
    };

    [Fact]
    public void PublicEnums_NamesAndValues_MatchCompatibilityContract()
    {
        var contract = BuildContract();
        var actualHash = Convert.ToHexString(SHA256.HashData(Encoding.UTF8.GetBytes(contract)));

        Assert.True(
            string.Equals(ExpectedContractSha256, actualHash, StringComparison.Ordinal),
            $"Enum compatibility contract changed. Expected SHA-256 {ExpectedContractSha256}, " +
            $"actual {actualHash}.{Environment.NewLine}{contract}");
    }

    [Fact]
    public void PublicEnumMembers_HaveStableJsonSerializationForms()
    {
        foreach (var enumType in GetPublicEnumTypes())
        {
            foreach (var field in GetEnumFields(enumType))
            {
                var value = field.GetValue(null);
                var numericValue = Convert.ToInt64(value, CultureInfo.InvariantCulture);

                Assert.Equal(
                    numericValue.ToString(CultureInfo.InvariantCulture),
                    JsonSerializer.Serialize(value, enumType));
                Assert.Equal(
                    JsonSerializer.Serialize(field.Name),
                    JsonSerializer.Serialize(value, enumType, StringEnumOptions));
            }
        }
    }

    private static string BuildContract()
    {
        var lines = GetPublicEnumTypes()
            .SelectMany(enumType => GetEnumFields(enumType).Select(field =>
                                                                       $"{enumType.FullName}|{field.Name}|" +
                                                                       $"{Convert.ToInt64(field.GetValue(null), CultureInfo.InvariantCulture)}"));

        return string.Join('\n', lines);
    }

    private static IEnumerable<Type> GetPublicEnumTypes()
    {
        return typeof(AccountTypeEnum).Assembly
                                      .GetExportedTypes()
                                      .Where(type => type.IsEnum)
                                      .OrderBy(type => type.FullName, StringComparer.Ordinal);
    }

    private static IEnumerable<FieldInfo> GetEnumFields(Type enumType)
    {
        return enumType
               .GetFields(BindingFlags.Public | BindingFlags.Static)
               .OrderBy(field => field.Name, StringComparer.Ordinal);
    }
}