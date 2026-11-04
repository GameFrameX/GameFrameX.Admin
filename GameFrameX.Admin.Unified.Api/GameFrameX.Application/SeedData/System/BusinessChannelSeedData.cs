using GameFrameX.Core.SqlSugar;
using GameFrameX.Entity.Enum;
using GameFrameX.Entity.Tenant;

namespace GameFrameX.Application.SeedData.System;

/// <summary>主库业务默认渠道，编码与系统渠道字典分开。</summary>
public sealed class BusinessChannelSeedData : ISqlSugarEntitySeedData<TenantChannelDefinitionEntity>
{
    public IEnumerable<TenantChannelDefinitionEntity> HasData()
    {
        return new[]
        {
            new TenantChannelDefinitionEntity
            {
                Id = 31950206982437, Code = "business-default", Name = "默认业务渠道",
                Scope = ChannelScopeEnum.Tenant, PlatformCode = "all", ChannelType = ChannelTypeEnum.Custom,
                IsDefault = true, IsEnabled = true, Description = "新装默认业务渠道",
                CreatedTime = 1745836800, CreatedId = 1300000000101, CreatedName = "超级管理员",
            },
        };
    }
}
