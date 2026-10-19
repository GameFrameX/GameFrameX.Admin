using GameFrameX.Core.Export;
using Microsoft.AspNetCore.Mvc;
using MiniExcelLibs;

namespace GameFrameX.Core.Tests.Export;

/// <summary>
/// DataExportHelper 列白名单投影与敏感字段过滤测试。
/// 断言：仅输出存在对应属性的白名单列；标记 ExportSensitive 的字段即便被请求也强制剔除；
/// 列顺序遵循前端白名单顺序（去掉被剔除项后的相对顺序）。
/// </summary>
public class DataExportHelperTests
{
    [Fact]
    public async Task BuildExcel_FiltersSensitiveAndUnknown_AndPreservesWhitelistOrder()
    {
        var rows = new List<SampleRow>
        {
            new() { Id = 1, Name = "Alice", Secret = "shh", },
            new() { Id = 2, Name = "Bob", Secret = "shh2", },
        };
        var columns = new List<ExportColumnRequest>
        {
            new() { Field = "Name", Header = "名称", },
            new() { Field = "Secret", Header = "密钥", },
            new() { Field = "NonExistent", Header = "幽灵", },
            new() { Field = "Id", Header = "编号", },
        };

        var result = await DataExportHelper.BuildExcelResultAsync(rows, columns, "测试");
        var fileStreamResult = Assert.IsType<FileStreamResult>(result);
        Assert.EndsWith("测试.xlsx", fileStreamResult.FileDownloadName);

        fileStreamResult.FileStream.Position = 0;
        var rowsBack = MiniExcel.Query(fileStreamResult.FileStream, true)
                                .Cast<IDictionary<string, object>>()
                                .ToList();

        Assert.Equal(2, rowsBack.Count);
        Assert.Equal("名称,编号", string.Join(",", rowsBack[0].Keys));
        Assert.Equal("Alice", rowsBack[0]["名称"]?.ToString());
        Assert.Equal("1", rowsBack[0]["编号"]?.ToString());
        Assert.False(rowsBack[0].ContainsKey("密钥"), "敏感字段必须被剔除，不能进入导出文件");
        Assert.False(rowsBack[0].ContainsKey("幽灵"), "不存在属性的白名单项必须被剔除");
    }

    [Fact]
    public async Task BuildExcel_EmptyColumns_ProducesHeaderOnlySheet()
    {
        var rows = new List<SampleRow> { new() { Id = 1, Name = "A", Secret = "s", }, };

        var result = await DataExportHelper.BuildExcelResultAsync(rows, new List<ExportColumnRequest>(), "空");

        var fileStreamResult = Assert.IsType<FileStreamResult>(result);
        Assert.EndsWith("空.xlsx", fileStreamResult.FileDownloadName);
    }

    public class SampleRow
    {
        public long Id { get; set; }

        public string Name { get; set; } = null!;

        [ExportSensitive] public string Secret { get; set; } = null!;
    }
}