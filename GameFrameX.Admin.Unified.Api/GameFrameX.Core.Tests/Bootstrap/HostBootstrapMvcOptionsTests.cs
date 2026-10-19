using GameFrameX.Core.Bootstrap;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Options;

namespace GameFrameX.Core.Tests.Bootstrap;

/// <summary>
/// AddHostControllers 的 MvcOptions 契约测试：关闭 non-nullable string 的隐式 required
/// 校验（2026-09-10 线上事故：客户端对 DeviceModel/PackageName 等字段发 null/空串，
/// 隐式校验直接 400，GameUserArchive/Upload 与 GameRemoteConfig/GetInfo 全断，
/// 波及所有已发客户端版本）。此项被误删时测试红。
/// </summary>
public sealed class HostBootstrapMvcOptionsTests
{
    [Fact]
    public void AddHostControllers_SuppressesImplicitRequiredForNonNullableStrings()
    {
        var services = new ServiceCollection();

        services.AddHostControllers();

        using (var provider = services.BuildServiceProvider())
        {
            var options = provider.GetRequiredService<IOptions<MvcOptions>>().Value;
            Assert.True(options.SuppressImplicitRequiredAttributeForNonNullableReferenceTypes,
                "隐式 required 必须保持关闭：客户端（含存量老包）会对 string 字段发 null/空串");
        }
    }
}
