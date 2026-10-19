using GameFrameX.Core.SqlSugar;

namespace GameFrameX.Core.Tests.SqlSugar;

public class SqlSugarPagedExtensionsTests
{
    [Theory]
    [InlineData(0, 10)]
    [InlineData(-1, 10)]
    public void ToPagedList_WithInvalidPageIndex_Throws(int pageIndex, int pageSize)
    {
        var items = Enumerable.Range(1, 10);

        var exception = Assert.Throws<ArgumentOutOfRangeException>(() => items.ToPagedList(pageIndex, pageSize));

        Assert.Equal("pageIndex", exception.ParamName);
    }

    [Theory]
    [InlineData(1, 0)]
    [InlineData(1, -1)]
    [InlineData(1, SqlSugarPagedExtensions.MaxPageSize + 1)]
    public void ToPagedList_WithInvalidPageSize_Throws(int pageIndex, int pageSize)
    {
        var items = Enumerable.Range(1, 10);

        var exception = Assert.Throws<ArgumentOutOfRangeException>(() => items.ToPagedList(pageIndex, pageSize));

        Assert.Equal("pageSize", exception.ParamName);
    }

    [Fact]
    public void ToPagedList_WithValidPaging_ReturnsExpectedPage()
    {
        var items = Enumerable.Range(1, 10);

        var result = items.ToPagedList(2, 3);

        Assert.Equal(2, result.Page);
        Assert.Equal(3, result.PageSize);
        Assert.Equal(10, result.Total);
        Assert.Equal(4, result.TotalPages);
        Assert.True(result.HasPrevPage);
        Assert.True(result.HasNextPage);
        Assert.Equal(new[] { 4, 5, 6, }, result.Items);
    }
}