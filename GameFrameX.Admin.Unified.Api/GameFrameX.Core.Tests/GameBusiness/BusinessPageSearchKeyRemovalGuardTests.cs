namespace GameFrameX.Core.Tests.GameBusiness;

/// <summary>
/// 业务分页 SearchKey 重复过滤删除守护（C329 / GFX-739）。B佬 2026-09-10 拍板：业务分页查询
/// 只保留明确条件字段，与同列明确字段（Name/Title 等）模糊查询等价的 SearchKey 分支全部删除
/// （16 个 Service + GameRole/GameUser 分页 DTO 去遮蔽 + TimeJob 补 Name 明确条件，main eec47f0d 落地）。
/// <para>与日志侧 <c>LogPageSearchKeyFilterTests</c>（C327 / GFX-737，日志页 searchKey 是唯一模糊条件，保留）
/// 合成完整策略锁定。本守护为源码扫描：防止新增/复制 Service 时同列重复的 SearchKey 过滤回潮。</para>
/// <para>保留项不受本守护约束：3 个 Service 的 SelectAsync 下拉搜索 searchKey（GameAppAssetPackage /
/// GameAppAssetPackageVersion / GameAppPackage）；页面无明确名称字段、关键词为唯一模糊条件的分页
/// （等同日志判例，见 C329 notes.md 审计表）。</para>
/// </summary>
public sealed class BusinessPageSearchKeyRemovalGuardTests
{
    /// <summary>GFX-739 清理触及的 17 个 Service（16 个删重复 SearchKey WhereIF + TimeJob 补 Name 条件）。</summary>
    private static readonly string[] CleanedServices =
    {
        "TenantGameAnnouncementEntityService",
        "TenantGameBannerAnnouncementEntityService",
        "TenantGameAppEntityService",
        "TenantGameAppAssetPackageService",
        "TenantGameAppAssetPackageVersionService",
        "TenantGameAppPackageService",
        "TenantGameAreaEntityService",
        "TenantGameAreaClassifyEntityService",
        "TenantGameDockerTagEntityService",
        "TenantGameGoodsCatalogueEntityService",
        "TenantGameMailTemplateCategoryEntityService",
        "TenantGameOrderEntityService",
        "TenantGameOrderFulfillmentEntityService",
        "TenantGameRedeemCodeChannelEntityService",
        "TenantGameRoleEntityService",
        "TenantGameTimeJobEntityService",
        "TenantGameUserEntityService",
    };

    /// <summary>SelectAsync 下拉搜索保留 searchKey 用法的 3 个 Service（issue 明确保留项，整文件断言豁免）。</summary>
    private static readonly string[] ServicesWithSelectAsyncKeep =
    {
        "TenantGameAppAssetPackageService",
        "TenantGameAppAssetPackageVersionService",
        "TenantGameAppPackageService",
    };

    [Fact]
    public void CleanedServices_BuildPageQuery_DoesNotReferenceSearchKey()
    {
        foreach (var serviceName in CleanedServices)
        {
            var source = ReadSource(serviceName);
            var body = ExtractMethodBody(source, "BuildPageQuery");

            Assert.True(body.Length > 0, $"{serviceName} 未找到 BuildPageQuery 方法体。");
            Assert.DoesNotContain("SearchKey", body, StringComparison.Ordinal);
        }
    }

    [Fact]
    public void ServicesWithoutSelectAsyncKeep_FileContainsNoInputSearchKey()
    {
        var noKeepServices = CleanedServices.Where(u => !ServicesWithSelectAsyncKeep.Contains(u)).ToArray();
        Assert.Equal(14, noKeepServices.Length);

        foreach (var serviceName in noKeepServices)
        {
            var source = ReadSource(serviceName);
            Assert.DoesNotContain("input.SearchKey", source, StringComparison.Ordinal);
        }
    }

    [Fact]
    public void RoleAndUserPageRequests_HaveNoSearchKeyShadowing()
    {
        // 回归 BasePageRequest.SearchKey 基类字段后，分页 DTO 本地不得再出现任何 SearchKey 声明（含 new 遮蔽）。
        var source = ReadSource("PageTenantGameRoleEntityRequest") + "\n" + ReadSource("PageTenantGameUserEntityRequest");
        Assert.DoesNotContain("SearchKey", source, StringComparison.Ordinal);
    }

    [Fact]
    public void TimeJob_KeepsExplicitNameCondition()
    {
        // TimeJob 原本没有 SearchKey 条件（前端关键词框是死 UI）：删除关键词框的同时补 Name 明确条件 + DTO 属性。
        var requestSource = ReadSource("PageTenantGameTimeJobEntityRequest");
        Assert.Contains("public string Name", requestSource, StringComparison.Ordinal);
        Assert.DoesNotContain("SearchKey", requestSource, StringComparison.Ordinal);

        var serviceBody = ExtractMethodBody(ReadSource("TenantGameTimeJobEntityService"), "BuildPageQuery");
        Assert.Contains("input.Name", serviceBody, StringComparison.Ordinal);
    }

    private static string ReadSource(string typeName)
    {
        return File.ReadAllText(FindSourceFile(typeName).FullName);
    }

    /// <summary>从测试输出目录逐级向上按文件名唯一定位源文件（沿用 ApiEndpointCatalogSyncTests 模式）。</summary>
    private static FileInfo FindSourceFile(string typeName)
    {
        var directory = new DirectoryInfo(AppContext.BaseDirectory);
        while (directory != null)
        {
            var candidates = Directory.EnumerateFiles(directory.FullName, typeName + ".cs", SearchOption.AllDirectories)
                                      .Where(u => !u.Contains($"{Path.DirectorySeparatorChar}bin{Path.DirectorySeparatorChar}", StringComparison.Ordinal))
                                      .Where(u => !u.Contains($"{Path.DirectorySeparatorChar}obj{Path.DirectorySeparatorChar}", StringComparison.Ordinal))
                                      .Select(u => new FileInfo(u))
                                      .ToArray();
            if (candidates.Length == 1)
            {
                return candidates[0];
            }

            directory = directory.Parent;
        }

        throw new FileNotFoundException($"找不到 {typeName}.cs。");
    }

    /// <summary>提取指定方法源码体：定位方法名首次出现位置，从其后第一个 '{' 起做花括号配对至闭合。</summary>
    private static string ExtractMethodBody(string source, string methodName)
    {
        var signatureIndex = source.IndexOf(methodName, StringComparison.Ordinal);
        if (signatureIndex < 0)
        {
            return string.Empty;
        }

        var openIndex = source.IndexOf('{', signatureIndex);
        if (openIndex < 0)
        {
            return string.Empty;
        }

        var depth = 0;
        for (var i = openIndex; i < source.Length; i++)
        {
            if (source[i] == '{')
            {
                depth++;
            }
            else if (source[i] == '}')
            {
                depth--;
                if (depth == 0)
                {
                    return source.Substring(openIndex, i - openIndex + 1);
                }
            }
        }

        return string.Empty;
    }
}
