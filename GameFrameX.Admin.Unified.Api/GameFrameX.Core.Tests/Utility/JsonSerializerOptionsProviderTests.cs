using System.Text.Json;
using GameFrameX.Core.Utility;

namespace GameFrameX.Core.Tests.Utility;

public class JsonSerializerOptionsProviderTests
{
    [Fact]
    public void Configure_PreservesAdminJsonContract()
    {
        var options = new JsonSerializerOptions();

        JsonSerializerOptionsProvider.Configure(options);

        var json = JsonSerializer.Serialize(new JsonContract
        {
            DisplayName = "Admin",
            CreatedTime = new DateTime(2026, 7, 10, 8, 9, 10, DateTimeKind.Local),
        }, options);

        Assert.Contains("\"DisplayName\":\"Admin\"", json);
        Assert.Contains("\"CreatedTime\":\"2026-07-10 08:09:10\"", json);
    }

    [Fact]
    public void Configure_IgnoresReferenceCycles()
    {
        var options = new JsonSerializerOptions();
        JsonSerializerOptionsProvider.Configure(options);
        var value = new CyclicJsonContract { Name = "root", };
        value.Self = value;

        var json = JsonSerializer.Serialize(value, options);

        Assert.Contains("\"Name\":\"root\"", json);
    }

    [Fact]
    public void Configure_WritesIdentifierLongsAsStringsAndReadsThemBack()
    {
        var options = new JsonSerializerOptions();
        JsonSerializerOptionsProvider.Configure(options);

        var json = JsonSerializer.Serialize(new IdentifierJsonContract
        {
            Id = 4664278385747429000,
            TenantId = 1300000000001,
            CreatedTime = 1784073600,
        }, options);

        Assert.Contains("\"Id\":\"4664278385747429000\"", json);
        Assert.Contains("\"TenantId\":\"1300000000001\"", json);
        Assert.Contains("\"CreatedTime\":1784073600", json);

        var value = JsonSerializer.Deserialize<IdentifierJsonContract>(
            "{\"Id\":\"4664278385747429000\",\"TenantId\":\"1300000000001\",\"CreatedTime\":1784073600}",
            options);

        Assert.NotNull(value);
        Assert.Equal(4664278385747429000, value.Id);
        Assert.Equal(1300000000001, value.TenantId);
        Assert.Equal(1784073600, value.CreatedTime);
    }

    private sealed class JsonContract
    {
        public string DisplayName { get; init; } = string.Empty;

        public DateTime CreatedTime { get; init; }
    }

    private sealed class CyclicJsonContract
    {
        public string Name { get; init; } = string.Empty;

        public CyclicJsonContract? Self { get; set; }
    }

    private sealed class IdentifierJsonContract
    {
        public long Id { get; init; }

        public long? TenantId { get; init; }

        public long CreatedTime { get; init; }
    }
}