using GameFrameX.Core.Service.File;
using GameFrameX.Entity.Tenant.ObjectStorage;

namespace GameFrameX.Core.Tests.ObjectStorage;

/// <summary>
/// TenantFileService.EnsureSingleActiveConfig 守卫单测（P1-1）。
/// 验证上传 Resolver 在 0/多/单 三种活动配置状态下的分支行为，确保唯一活动配置不变量被违反时抛错而非静默取首条。
/// <para>注：守卫内部用 历史框架 <c>BusinessError.Oh</c> 抛友好异常，无 历史框架 host 的纯单测下会触发 Friendly
/// 静态初始化异常，因此本测试只断言“抛/不抛”，消息文本正确性由 code review 把关（见 TenantFileService.EnsureSingleActiveConfig）。</para>
/// </summary>
public class ObjectStorageActiveConfigGuardTests
{
    [Fact]
    public void EnsureSingleActiveConfig_Null_Throws()
    {
#pragma warning disable CS8625 // 测试意图：传入 null 列表验证守卫抛异常
        Assert.ThrowsAny<Exception>(() => TenantFileService.EnsureSingleActiveConfig(null));
#pragma warning restore CS8625
    }

    [Fact]
    public void EnsureSingleActiveConfig_Empty_Throws()
    {
        Assert.ThrowsAny<Exception>(() => TenantFileService.EnsureSingleActiveConfig(new List<TenantObjectStorageConfigEntity>()));
    }

    [Fact]
    public void EnsureSingleActiveConfig_Multiple_Throws()
    {
        var list = new List<TenantObjectStorageConfigEntity>
        {
            new() { Id = 1, Provider = "local", },
            new() { Id = 2, Provider = "aliyun", },
        };

        Assert.ThrowsAny<Exception>(() => TenantFileService.EnsureSingleActiveConfig(list));
    }

    [Fact]
    public void EnsureSingleActiveConfig_Single_ReturnsIt()
    {
        var single = new TenantObjectStorageConfigEntity { Id = 7, Provider = "local", };

        var result = TenantFileService.EnsureSingleActiveConfig(new List<TenantObjectStorageConfigEntity> { single, });

        Assert.Same(single, result);
    }
}