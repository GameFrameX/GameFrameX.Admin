using GameFrameX.Core.SqlSugar;
using GameFrameX.Entity.Enum;

namespace GameFrameX.Core.Tests.SqlSugar;

public sealed class DataScopeFallbackTests
{
    [Theory]
    [InlineData(DataScopeEnum.All, DataScopeEnum.All)]
    [InlineData(DataScopeEnum.Dept, DataScopeEnum.Self)]
    [InlineData(DataScopeEnum.DeptChild, DataScopeEnum.Self)]
    [InlineData(DataScopeEnum.Define, DataScopeEnum.Self)]
    [InlineData(DataScopeEnum.Self, DataScopeEnum.Self)]
    public void ScopeResolution_MapsOrganizationValuesToSelf(DataScopeEnum configured, DataScopeEnum expected)
    {
        // 这里只验证解析；当前没有 EntityBasicData 子类，不能声称业务行已被过滤。
        Assert.Equal((int)expected, SqlSugarFilter.NormalizeDataScope((int)configured));
    }
}
