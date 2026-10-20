using GameFrameX.Core.Service.User;
using GameFrameX.Entity.Enum;
using GameFrameX.Entity.Global;

namespace GameFrameX.Application.System.ChannelDefinition;

/// <summary>
/// 全局渠道定义服务，仅管理系统渠道
/// </summary>
[ApiDescriptionSettings(Order = 100)]
public class GlobalChannelDefinitionService
    : ChannelDefinitionManagementServiceBase<GlobalChannelDefinitionEntity>
{
    /// <summary>
    /// 全局渠道定义服务
    /// </summary>
    public GlobalChannelDefinitionService(
        SqlSugarRepository<GlobalChannelDefinitionEntity> repository,
        CurrentUserContext currentUserContext)
        : base(repository, repository, currentUserContext)
    {
    }

    /// <inheritdoc />
    protected override ChannelScopeEnum ManagedScope
    {
        get { return ChannelScopeEnum.System; }
    }
}