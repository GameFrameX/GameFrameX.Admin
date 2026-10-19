using System.Text.Json;
using GameFrameX.Core.Export;

namespace GameFrameX.Core.Tests.Export;

/// <summary>
/// 数据导出 DTO（DataExportRequest/ExportColumnRequest/ExportRangeEnum）序列化契约测试。
/// 断言前端 camelCase JSON 能正确反序列化为强类型字段与枚举，保障 C146 前端 → C147 后端契约。
/// </summary>
public class DataExportInputSerializationTests
{
    private static readonly JsonSerializerOptions Options = new()
    {
        PropertyNameCaseInsensitive = true,
    };

    [Fact]
    public void CamelCaseJson_Deserializes_ToExpectedFieldsAndTypes()
    {
        var json = "{\"query\":{\"page\":2,\"pageSize\":50,\"searchKey\":\"x\"}," +
                   "\"columns\":[{\"field\":\"Name\",\"header\":\"名称\"},{\"field\":\"Id\",\"header\":\"编号\"}]," +
                   "\"range\":1}";

        var input = JsonSerializer.Deserialize<DataExportRequest<SampleQuery>>(json, Options);

        Assert.NotNull(input);
        Assert.NotNull(input.Query);
        Assert.Equal(2, input.Query.Page);
        Assert.Equal(50, input.Query.PageSize);
        Assert.Equal("x", input.Query.SearchKey);
        Assert.Equal(2, input.Columns.Count);
        Assert.Equal("Name", input.Columns[0].Field);
        Assert.Equal("名称", input.Columns[0].Header);
        Assert.Equal("Id", input.Columns[1].Field);
        Assert.Equal("编号", input.Columns[1].Header);
        Assert.Equal(ExportRangeEnum.CurrentPage, input.Range);
    }

    [Fact]
    public void MissingRange_DefaultsToAll()
    {
        var json = "{\"query\":{\"page\":1},\"columns\":[]}";

        var input = JsonSerializer.Deserialize<DataExportRequest<SampleQuery>>(json, Options);

        Assert.NotNull(input);
        Assert.Equal(ExportRangeEnum.All, input.Range);
        Assert.NotNull(input.Query);
        Assert.Empty(input.Columns);
    }

    [Fact]
    public void Defaults_AllRangeAndInstantiatedQuery()
    {
        var input = new DataExportRequest<SampleQuery>();

        Assert.Equal(ExportRangeEnum.All, input.Range);
        Assert.NotNull(input.Query);
        Assert.Empty(input.Columns);
    }

    public class SampleQuery
    {
        public int Page { get; set; }

        public int PageSize { get; set; }

        public string SearchKey { get; set; } = null!;
    }
}