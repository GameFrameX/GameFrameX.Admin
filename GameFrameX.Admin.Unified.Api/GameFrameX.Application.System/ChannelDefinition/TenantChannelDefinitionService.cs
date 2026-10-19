using GameFrameX.Entity.Tenant.Game.App;
using GameFrameX.Application.System.ChannelDefinition.Dto;
using GameFrameX.Core.Export;
using GameFrameX.Core.Service.User;
using GameFrameX.Entity.Enum;
using GameFrameX.Entity.Global;

namespace GameFrameX.Application.System.ChannelDefinition;

/// <summary>
/// 租户渠道定义服务，仅管理当前租户渠道
/// </summary>
[ApiDescriptionSettings(Order = 101)]
public class TenantChannelDefinitionService
    : ChannelDefinitionManagementServiceBase<TenantChannelDefinitionEntity>
{
    /// <summary>
    /// 租户渠道定义服务
    /// </summary>
    public TenantChannelDefinitionService(
        SqlSugarRepository<TenantChannelDefinitionEntity> repository,
        SqlSugarRepository<GlobalChannelDefinitionEntity> globalChannelRepository,
        SqlSugarRepository<TenantGameAppPackageEntity> packageRepository,
        CurrentUserContext currentUserContext)
        : base(repository, globalChannelRepository, packageRepository, currentUserContext)
    {
    }

    /// <inheritdoc />
    protected override ChannelScopeEnum ManagedScope
    {
        get { return ChannelScopeEnum.Tenant; }
    }

    /// <summary>
    /// 获取当前用户可用渠道
    /// </summary>
    [HttpGet]
    [ApiDescriptionSettings(Name = "Available")]
    public Task<List<AvailableChannelDefinitionResponse>> AvailableAsync([FromQuery] BaseSelectPageRequest input)
    {
        return QueryAvailableAsync(input);
    }

    /// <summary>
    /// 导出当前租户渠道定义。复用既有 BuildPageQuery（强制 TenantId 过滤），
    /// 仅导出请求列白名单中、未标记 <see cref="ExportSensitiveAttribute"/> 的字段，
    /// 租户账号不会跨租户泄露，超管不触发租户耦合导出。
    /// </summary>
    /// <param name="input">导出请求：查询条件 + 可见列白名单 + 范围（全量受 MaxExportRows 上限保护）。</param>
    /// <returns>.xlsx 文件流结果。</returns>
    [HttpPost(CrudActionRoutes.Export)]
    [ApiDescriptionSettings(Name = "Export")]
    [NonUnify]
    [DisplayName("导出租户渠道定义")]
    public Task<IActionResult> ExportAsync(DataExportRequest<PageChannelDefinitionRequest> input)
    {
        return ExportCoreAsync(input.Query, input.Columns, input.Range, "租户渠道定义");
    }
}