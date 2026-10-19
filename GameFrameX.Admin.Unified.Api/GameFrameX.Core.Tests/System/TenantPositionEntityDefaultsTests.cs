using GameFrameX.Entity.Tenant;

namespace GameFrameX.Core.Tests.System;

// GFX-918（C387）：tenant_position_entity.remark 列 NOT NULL，而新建职位表单备注非必填、
// 未填时 JSON 省略字段——实体初始化器必须提供空串默认值，否则 Add 落库违反非空约束
// （23502）。本测试锁定该默认值，改回 null! 即红。
public class TenantPositionEntityDefaultsTests
{
    [Fact]
    public void TenantPositionEntity_RemarkDefaultsToEmptyString()
    {
        Assert.Equal(string.Empty, new TenantPositionEntity().Remark);
    }
}
