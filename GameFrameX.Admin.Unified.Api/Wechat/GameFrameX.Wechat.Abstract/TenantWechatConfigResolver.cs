using GameFrameX.Core.Service.Tenant;
using GameFrameX.Core.Exceptions;
using GameFrameX.Core.SqlSugar;
using GameFrameX.Entity.Tenant.ClientApiConfig;

namespace GameFrameX.Wechat.Abstract;

/// <summary>
/// 微信公众号 / 小程序租户配置解析器。
/// <para>认证后 Wechat 运营方法必须经由本解析器获取当前租户有效凭证，禁止直接读取静态 JSON / 平台 Options 或回退到默认租户 / 主库。</para>
/// <para>解析规则（与 C23 §3.3 一致）：</para>
/// <list type="number">
///   <item><description><c>appId &gt; 0</c> 时仅匹配 <c>TenantId + AppId = appId</c> 的已启用配置。</description></item>
///   <item><description><c>appId</c> 为空或 <c>0</c> 时仅匹配 <c>TenantId + AppId = 0</c> 的租户默认配置；不允许随机选择某个应用级配置。</description></item>
///   <item><description>固定排序：<c>Priority DESC, UpdateTime DESC, Id DESC</c>。</description></item>
/// </list>
/// <para>公众号方法与小程程序方法只能使用各自对应的凭证对；调用方必须显式传入 <see cref="WechatCredentialKind"/>，禁止跨类型补位。</para>
/// </summary>
public class TenantWechatConfigResolver
{
    private readonly SqlSugarRepository<TenantWechatConfigEntity> _repository;

    public TenantWechatConfigResolver(SqlSugarRepository<TenantWechatConfigEntity> repository)
    {
        _repository = repository;
    }

    /// <summary>
    /// 解析当前租户在指定应用上启用的 Wechat 配置。
    /// </summary>
    /// <param name="appId">应用 ID。0 或 null 表示仅匹配租户默认配置（AppId=0）。</param>
    /// <param name="kind">凭据类型（公众号 / 小程序），用于校验返回配置的对应凭证对完整性。</param>
    /// <returns>已启用且凭证对完整的租户配置；找不到或校验失败时抛明确错误。</returns>
    public async Task<TenantWechatConfigEntity> ResolveAsync(long? appId, WechatCredentialKind kind)
    {
        var tenantId = TenantOperationContext.GetEffectiveTenantId();
        if (tenantId == null)
        {
            throw BusinessError.Oh("未解析到有效租户上下文，禁止访问租户微信配置。SuperAdmin 必须显式传入 GameFrameX-Target-Tenant-Id 请求头。");
        }

        var targetAppId = appId ?? 0L;
        var entity = await _repository.AsQueryable()
                                      .Where(u => u.TenantId == tenantId.Value && u.AppId == targetAppId && u.IsEnabled == true)
                                      .OrderByDescending(u => u.Priority)
                                      .OrderByDescending(u => u.UpdateTime)
                                      .OrderByDescending(u => u.Id)
                                      .FirstAsync();

        if (entity == null)
        {
            var layer = targetAppId == 0 ? "租户默认配置（AppId=0）" : $"应用级配置（AppId={targetAppId}）";
            throw BusinessError.Oh($"当前租户未找到{layer}的已启用微信配置。");
        }

        switch (kind)
        {
            case WechatCredentialKind.Wechat:
                if (string.IsNullOrWhiteSpace(entity.WechatAppId) || string.IsNullOrWhiteSpace(entity.WechatAppSecret))
                {
                    throw BusinessError.Oh("当前租户微信公众号凭证对不完整，禁止调用公众号方法。");
                }

                break;
            case WechatCredentialKind.WxOpen:
                if (string.IsNullOrWhiteSpace(entity.WxOpenAppId) || string.IsNullOrWhiteSpace(entity.WxOpenSecret))
                {
                    throw BusinessError.Oh("当前租户微信小程序凭证对不完整，禁止调用小程序方法。");
                }

                break;
        }

        return entity;
    }

    /// <summary>
    /// 按配置 ID 直接读取并校验凭据对完整性；用于 SendTest / 运营方法显式传 ID 的路径，绕过作用域选择。
    /// </summary>
    /// <param name="id">租户微信配置主键。</param>
    /// <param name="kind">凭据类型（公众号 / 小程序）。</param>
    public async Task<TenantWechatConfigEntity> ResolveByIdAsync(long id, WechatCredentialKind kind)
    {
        if (id <= 0)
        {
            throw BusinessError.Oh("租户微信配置 ID 必须大于 0。");
        }

        var tenantId = TenantOperationContext.GetEffectiveTenantId();
        if (tenantId == null)
        {
            throw BusinessError.Oh("未解析到有效租户上下文，禁止访问租户微信配置。");
        }

        var entity = await _repository.AsQueryable()
                                      .Where(u => u.Id == id && u.TenantId == tenantId.Value)
                                      .FirstAsync();
        if (entity == null)
        {
            throw BusinessError.Oh($"租户微信配置不存在或不属于当前租户：Id={id}。");
        }

        if (entity.IsEnabled != true)
        {
            throw BusinessError.Oh("租户微信配置已禁用，禁止使用。");
        }

        switch (kind)
        {
            case WechatCredentialKind.Wechat:
                if (string.IsNullOrWhiteSpace(entity.WechatAppId) || string.IsNullOrWhiteSpace(entity.WechatAppSecret))
                {
                    throw BusinessError.Oh("租户微信公众号凭证对不完整，禁止调用公众号方法。");
                }

                break;
            case WechatCredentialKind.WxOpen:
                if (string.IsNullOrWhiteSpace(entity.WxOpenAppId) || string.IsNullOrWhiteSpace(entity.WxOpenSecret))
                {
                    throw BusinessError.Oh("租户微信小程序凭证对不完整，禁止调用小程序方法。");
                }

                break;
        }

        return entity;
    }
}