using System.Text.Json;
using GameFrameX.Core.Extension.Json;

namespace GameFrameX.Core.Tests.Utility;

/// <summary>
/// C270：Hub/Client host 场景下仅应用 long Id 字符串化契约（不动其他 JSON 选项）的行为锁定。
/// </summary>
public class ProjectJsonSerializerOptionsHostContractTests
{
    private sealed class HostJsonContract
    {
        public long Id { get; set; }

        public long? TenantId { get; set; }

        public long CreatedTime { get; set; }

        public string Name { get; set; }
    }

    [Fact]
    public void ApplyLongIdentifierStringContract_WritesIdentifierLongsAsStringsAndReadsThemBack()
    {
        var options = new JsonSerializerOptions { PropertyNamingPolicy = null, };
        ProjectJsonSerializerOptions.ApplyLongIdentifierStringContract(options);

        var json = JsonSerializer.Serialize(new HostJsonContract
        {
            Id = 4664278385747429000,
            TenantId = 1300000000001,
            CreatedTime = 1784073600,
            Name = "hub",
        }, options);

        Assert.Contains("\"Id\":\"4664278385747429000\"", json);
        Assert.Contains("\"TenantId\":\"1300000000001\"", json);
        // 非 Id 结尾 long 不受契约影响，保持数字
        Assert.Contains("\"CreatedTime\":1784073600", json);
        Assert.Contains("\"Name\":\"hub\"", json);

        // 反向宽容读取：字符串与数字形式的 Id 均可绑定（既有客户端提交数字不破坏）
        var fromString = JsonSerializer.Deserialize<HostJsonContract>("{\"Id\":\"4664278385747429000\",\"TenantId\":null,\"CreatedTime\":0,\"Name\":null}", options);
        Assert.NotNull(fromString);
        Assert.Equal(4664278385747429000, fromString.Id);

        var fromNumber = JsonSerializer.Deserialize<HostJsonContract>("{\"Id\":100,\"TenantId\":null,\"CreatedTime\":0,\"Name\":null}", options);
        Assert.NotNull(fromNumber);
        Assert.Equal(100, fromNumber.Id);
    }

    [Fact]
    public void ApplyLongIdentifierStringContract_DoesNotTouchOtherOptions()
    {
        var options = new JsonSerializerOptions { PropertyNamingPolicy = null, };
        ProjectJsonSerializerOptions.ApplyLongIdentifierStringContract(options);

        // 只挂 long→string 契约：PascalCase 策略与 DateTime 行为不被 Apply 覆盖（DateTime 仍走默认 ISO 格式）
        var json = JsonSerializer.Serialize(new HostJsonContract
        {
            Id = 1,
            CreatedTime = 2,
            Name = "x",
        }, options);

        Assert.Contains("\"Id\":\"1\"", json);
        Assert.Null(options.PropertyNamingPolicy);
        Assert.Empty(options.Converters);
    }
}
