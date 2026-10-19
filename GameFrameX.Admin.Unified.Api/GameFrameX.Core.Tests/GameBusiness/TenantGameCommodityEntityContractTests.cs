using System.Text.Json;
using GameFrameX.Application.Game.Goods.GameCommodity.Dto;
using GameFrameX.Core.Extension.Json;

namespace GameFrameX.Core.Tests.GameBusiness;

/// <summary>
/// C391（GFX-923 / Api#291）：游戏商品 Add/Page DTO 与前端真实提交形态的 JSON 绑定契约。
/// 前端 EditDialog 的 number 是 TextField（提交 JS string）；categoryId 取自目录 select 接口
/// （BaseSelectPageResponse.Id 按 Id 契约序列化为 string，回填后提交 string；curl 等客户端可能
/// 提交数字）。任一字段类型错位都会让整个 body 反序列化失败、input 静默为 null → 500 NRE。
/// </summary>
public class TenantGameCommodityEntityContractTests
{
    private static JsonSerializerOptions CreateOptions()
    {
        return ProjectJsonSerializerOptions.CreateDefault();
    }

    [Fact]
    public void AddRequest_FrontendShapedStringPayload_Deserializes()
    {
        const string payload = "{\"appId\":\"1300000000001\",\"status\":\"OnSale\",\"categoryId\":\"7451234567890123\",\"cost\":false,\"number\":\"5\"}";

        var request = JsonSerializer.Deserialize<AddTenantGameCommodityEntityRequest>(payload, CreateOptions());

        Assert.NotNull(request);
        Assert.Equal(1300000000001, request.AppId);
        Assert.Equal("OnSale", request.Status);
        Assert.Equal(7451234567890123, request.CategoryId);
        Assert.Equal(5, request.Number);
    }

    [Fact]
    public void AddRequest_NumericCategoryIdPayload_Deserializes()
    {
        const string payload = "{\"appId\":1300000000001,\"status\":\"OnSale\",\"categoryId\":7451234567890123,\"cost\":true,\"number\":5}";

        var request = JsonSerializer.Deserialize<AddTenantGameCommodityEntityRequest>(payload, CreateOptions());

        Assert.NotNull(request);
        Assert.Equal(7451234567890123, request.CategoryId);
        Assert.Equal(5, request.Number);
    }

    [Fact]
    public void AddRequest_DoesNotCarryIdProperty()
    {
        // AddRequest 不携带主键（新增语义；Id 由后端雪花分配），对齐 catalogue AddRequest 既定模式
        Assert.Null(typeof(AddTenantGameCommodityEntityRequest).GetProperty("Id"));
    }

    [Fact]
    public void PageRequest_TextSearchNumberFilter_Deserializes()
    {
        const string payload = "{\"page\":1,\"pageSize\":20,\"appId\":\"1300000000001\",\"number\":\"5\",\"categoryId\":\"7451234567890123\"}";

        var request = JsonSerializer.Deserialize<PageTenantGameCommodityEntityRequest>(payload, CreateOptions());

        Assert.NotNull(request);
        Assert.Equal(5, request.Number);
        Assert.Equal(7451234567890123, request.CategoryId);
    }

    [Fact]
    public void Response_CategoryId_IsLongAndWrittenAsStringByIdContract()
    {
        // CategoryId 为 long（目录 Id 语义），出站按 Id 后缀契约字符串化——
        // 前端 getCategoryName 的 item.id === categoryId 严格相等匹配不受影响
        var options = CreateOptions();

        var json = JsonSerializer.Serialize(new TenantGameCommodityEntityResponse
        {
            Id = 4664278385747429000,
            CategoryId = 7451234567890123,
        }, options);

        Assert.Contains("\"CategoryId\":\"7451234567890123\"", json);
    }
}
