using System.Security.Cryptography;
using System.Text;
using GameFrameX.Application.Game.App.GameAppServerConfig.Dto;

namespace GameFrameX.Application.Game.App.GameAppServerConfig;

/// <summary>
/// 游戏运营配置管理服务（登录 Config md5 差异下发内容源的运营维护面）
/// </summary>
[ApiDescriptionSettings(ClientConst.GroupName, Order = 100)]
public class TenantGameAppServerConfigService : GenericCrudService<Entity.Tenant.Game.ServerConfig.TenantGameAppServerConfigEntity, AddGameAppServerConfigRequest, UpdateGameAppServerConfigRequest, GameAppServerConfigResponse, PageGameAppServerConfigRequest>
{
    public TenantGameAppServerConfigService(SqlSugarRepository<Entity.Tenant.Game.ServerConfig.TenantGameAppServerConfigEntity> repository) : base(repository)
    {
    }

    /// <summary>
    /// 构建分页查询条件：应用作用域必填；租户默认基线行（AppId=null）不在管理面出现，同远程配置先例
    /// </summary>
    protected override ISugarQueryable<GameAppServerConfigResponse> BuildPageQuery(PageGameAppServerConfigRequest input)
    {
        var appId = RequireAppId(input.AppId, "请选择应用后再维护游戏运营配置");
        var searchKey = input.SearchKey?.Trim() ?? string.Empty;
        var key = input.Key?.Trim() ?? string.Empty;
        return Repository.AsQueryable()
                         .WhereIF(!string.IsNullOrEmpty(searchKey), u => u.Key.Contains(searchKey) || (u.Description ?? string.Empty).Contains(searchKey))
                         .WhereIF(!string.IsNullOrEmpty(key), u => u.Key == key)
                         .WhereIF(input.Enabled.HasValue, u => u.Enabled == input.Enabled.Value)
                         .Where(u => u.AppId == appId)
                         .Select<GameAppServerConfigResponse>();
    }

    /// <summary>
    /// 新增前：应用作用域 + 同键查重 + 内容归一化与 md5 版本号重算
    /// </summary>
    protected override async Task OnAddingAsync(AddGameAppServerConfigRequest input)
    {
        input.AppId = RequireAppId(input.AppId, "请选择应用后再新增游戏运营配置");
        input.Key = (input.Key ?? string.Empty).Trim();
        await EnsureKeyUniqueAsync(input.AppId.Value, input.Key, null);
        NormalizeAndStamp(input);
    }

    /// <summary>
    /// 更新前：记录存在 + 作用域锁定（不可跨应用改挂）+ 同键查重（排除自身）+ 内容归一化与 md5 重算
    /// </summary>
    protected override async Task OnUpdatingAsync(UpdateGameAppServerConfigRequest input)
    {
        var existing = await Repository.GetFirstAsync(u => u.Id == input.Id);
        if (existing == null)
        {
            throw BusinessError.Oh(CommonErrorCode.RecordNotFound);
        }

        input.AppId = existing.AppId;
        RequireAppId(input.AppId, "游戏运营配置不属于当前应用");
        input.Key = (input.Key ?? string.Empty).Trim();
        await EnsureKeyUniqueAsync(existing.AppId, input.Key, input.Id);
        NormalizeAndStamp(input);
    }

    /// <summary>
    /// 同一应用下配置键唯一性校验（AppId + Key 组合唯一；excludeId 用于更新时排除自身）
    /// </summary>
    private async Task EnsureKeyUniqueAsync(long? appId, string key, long? excludeId)
    {
        var duplicated = excludeId == null
            ? await Repository.GetFirstAsync(u => u.AppId == appId && u.Key == key)
            : await Repository.GetFirstAsync(u => u.AppId == appId && u.Key == key && u.Id != excludeId);
        if (duplicated != null)
        {
            throw BusinessError.Oh("同一应用下配置键已存在：" + key);
        }
    }

    /// <summary>
    /// md5 版本号构造保证：对归一化后的 Config 计算 MD5 hex 写回 Md5 字段，前端传入值一律忽略。
    /// 登录 md5 差异下发协议中 md5 是版本号（内容变 md5 必变 → 客户端下次登录拉全量），
    /// 由服务端在保存路径统一重算，消除「改内容忘换 md5」的人工同步失败模式
    /// </summary>
    private static void NormalizeAndStamp(GameAppServerConfigBaseRequest input)
    {
        if (string.IsNullOrWhiteSpace(input.Config))
        {
            throw BusinessError.Oh("配置内容不能为空");
        }

        try
        {
            using (var document = JsonDocument.Parse(input.Config))
            {
                // 归一化：按固定 writer 选项重序列化，消除空白与转义差异，保证同内容 md5 稳定
                input.Config = JsonSerializer.Serialize(document.RootElement);
            }
        }
        catch (JsonException)
        {
            throw BusinessError.Oh("配置内容必须是合法 JSON");
        }

        input.Md5 = Convert.ToHexString(MD5.HashData(Encoding.UTF8.GetBytes(input.Config))).ToLowerInvariant();
    }

    /// <summary>
    /// 应用作用域必填守卫（null 或小于等于 0 拒绝；Nullable 与字面量比较对 null 恒 false，须显式判）
    /// </summary>
    private static long RequireAppId(long? appId, string message)
    {
        var resolvedAppId = appId ?? 0;
        if (resolvedAppId <= 0)
        {
            throw BusinessError.Oh(message);
        }

        return resolvedAppId;
    }
}
