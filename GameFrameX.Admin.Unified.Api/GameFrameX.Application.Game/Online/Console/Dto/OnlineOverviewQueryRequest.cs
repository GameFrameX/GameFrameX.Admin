namespace GameFrameX.Application.Game.Online.Console.Dto;

/// <summary>
/// 在线总览快照查询线缆请求（vault:C9 契约；mock 先行，联调以 vault 详表为准）。
/// <para>快照即时生成，无过滤参数；作用域三元组由 OnlineScopeContext 注入，不在线缆体内重复。</para>
/// </summary>
public class OnlineOverviewQueryRequest
{
}
