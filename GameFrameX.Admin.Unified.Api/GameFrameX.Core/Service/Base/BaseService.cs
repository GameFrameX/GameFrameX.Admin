using GameFrameX.Core.Service.Tenant;
using GameFrameX.Entity.Base;

namespace GameFrameX.Core.Service.Base;

/// <summary>
/// 选择器基础服务
/// </summary>
/// <typeparam name="T"></typeparam>
[ApiDescriptionSettings(Order = 100)]
public class BaseService<T> : IAdminApiController where T : EntityBasic, new()
{
    protected readonly SqlSugarRepository<T> Repository;

    protected BaseService(SqlSugarRepository<T> repository)
    {
        Repository = repository;
    }

    /// <summary>
    /// 增加
    /// </summary>
    /// <param name="input"></param>
    /// <returns></returns>
    protected async Task<T> InnerAddAsync(BaseAddRequest input)
    {
        var entity = input.Adapt<T>();
        // AddRequest 若继承 BaseUpdateRequest（携带 RowVersion null），Adapt 会把实体默认 0 覆盖为 null，
        // 插入后 row_version 为 NULL，首次乐观锁更新会以 null 参数比较（Postgres bigint = text）直接 500。兜底回 0。
        entity.RowVersion ??= 0;
        await Repository.InsertWithDiffLogAsync(entity);
        return entity;
    }

    /// <summary>
    /// 删除
    /// </summary>
    /// <param name="input"></param>
    /// <returns></returns>
    [HttpPost(CrudActionRoutes.Delete)]
    [ApiDescriptionSettings(Name = "Delete")]
    public virtual async Task DeleteAsync(BaseDeleteRequest input)
    {
        // 动态 API 校验管线不跑：缺 rowVersion 时 null 直达 SqlSugar 乐观锁，
        // 以 42883（bigint = text）500 暴露（Api#292）；前置拦截为可读的参数校验失败
        if (input is null || input.RowVersion is null)
        {
            Console.Error.WriteLine("GUARD-292-HIT inputNull=" + (input is null));
            throw BusinessError.Oh(CommonErrorCode.ParameterValidationFailed);
        }
        var entity = await Repository.GetFirstAsync(u => u.Id == input.Id);
        if (entity == null)
        {
            throw BusinessError.Oh(CommonErrorCode.RecordNotFound);
        }

        entity.RowVersion = input.RowVersion;
        await Repository.FakeDeleteAsync(entity);
    }

    /// <summary>
    /// 更新版本管理。
    /// <para>语义（2026-10-20 修正，C395 浏览器覆盖发现）：Update DTO 是字段级契约——
    /// 先取当前行，把 DTO 映射到<b>已加载实体</b>上再整行更新；DTO 未声明的列保持库中原值。
    /// 此前 <c>input.Adapt&lt;T&gt;()</c> 映射到新实体，DTO 未声明的非空值类型列
    /// （如资源包 <c>GameAppId</c>）落默认值 0 后写库，编辑即把行挪出应用作用域（数据损坏）。</para>
    /// </summary>
    /// <param name="input"></param>
    /// <returns></returns>
    protected async Task<T> InnerUpdateAsync<TInput>(TInput input) where TInput : ICrudUpdateRequest
    {
        var entity = await Repository.GetFirstAsync(e => e.Id == input.Id)
                     ?? throw BusinessError.Oh(CommonErrorCode.RecordNotFound);
        input.Adapt(entity);
        entity.Id = input.Id;
        ApplyUpdateAudit(entity);
        await Repository.Context.Updateable(entity)
                        .IgnoreColumns(m => new { m.CreatedTime, m.CreatedId, m.CreatedName, })
                        .IgnoreColumns(true)
                        .EnableDiffLogEvent()
                        .ExecuteCommandWithOptLockAsync(true);
        return entity;
    }

    private static void ApplyUpdateAudit(T entity)
    {
        var userId = TenantOperationContext.CurrentUser?.FindFirst(ClaimConst.UserId)?.Value;
        if (long.TryParse(userId, out var parsedUserId))
        {
            entity.UpdatedId = parsedUserId;
        }

        entity.UpdatedName = GetCurrentUserDisplayName();
        entity.UpdateTime = TimerHelper.UnixTimeSeconds();
    }

    private static string? GetCurrentUserDisplayName()
    {
        var realName = TenantOperationContext.CurrentUser?.FindFirst(ClaimConst.RealName)?.Value;
        if (!string.IsNullOrWhiteSpace(realName))
        {
            return realName;
        }

        return TenantOperationContext.CurrentUser?.FindFirst(ClaimConst.Account)?.Value;
    }

    /// <summary>
    /// 根据主键ID获取详细信息
    /// </summary>
    /// <param name="input"></param>
    /// <returns></returns>
    protected async Task<T> InnerGetAsync([FromQuery] BaseIdRequest input)
    {
        return await Repository.GetFirstAsync(u => u.Id == input.Id);
    }
}