using GameFrameX.Entity.Base;
using GameFrameX.Foundation.Orm.Entity.Filter;

namespace GameFrameX.Core.Service.Base;

/// <summary>
/// 带 Name 过滤和选择器的通用 CRUD 服务基类。
/// 适用于实体具有 Name 属性（实现 ISelectFilter）的标准 CRUD 场景。
/// </summary>
[ApiDescriptionSettings(Order = 100)]
public abstract class GenericCrudSelectService<TEntity, TAddRequest, TUpdateRequest, TResponse, TPageRequest>
    : GenericCrudService<TEntity, TAddRequest, TUpdateRequest, TResponse, TPageRequest>
    where TEntity : EntityBasic, ISelectFilter, new()
    where TAddRequest : BaseAddRequest
    where TUpdateRequest : ICrudUpdateRequest
    where TPageRequest : BasePageRequest
{
    protected GenericCrudSelectService(SqlSugarRepository<TEntity> repository) : base(repository)
    {
    }

    /// <summary>
    /// 构建分页查询条件，默认按 SearchKey 过滤 Name 字段
    /// </summary>
    protected override ISugarQueryable<TResponse> BuildPageQuery(TPageRequest input)
    {
        return Repository.AsQueryable()
                         .Where(u => u.IsDeleted == false)
                         .WhereIF(!string.IsNullOrWhiteSpace(input.SearchKey),
                                  u => (u.Name ?? string.Empty).Contains((input.SearchKey ?? string.Empty).Trim()))
                         .Select<TResponse>();
    }

    /// <summary>
    /// 获取选择器列表
    /// </summary>
    [HttpPost(CrudActionRoutes.Select)]
    public virtual async Task<List<BaseSelectPageResponse>> SelectAsync(BaseSelectPageRequest input)
    {
        var searchKey = input.SearchKey?.Trim() ?? string.Empty;
        var pageSize = Math.Min(input.PageSize > 0 ? input.PageSize : BaseSelectPageRequest.DefaultPageSize, BaseSelectPageRequest.MaxPageSize);
        var query = await Repository.AsQueryable()
                                    .Where(u => u.IsDeleted == false)
                                    .WhereIF(!string.IsNullOrWhiteSpace(searchKey), u =>
                                                 (u.Name ?? string.Empty).Contains(searchKey))
                                    .Take(pageSize)
                                    .ToListAsync();
        var list = new List<BaseSelectPageResponse>(query.Count);
        list.AddRange(query.Select(ts => new BaseSelectPageResponse { Id = ts.Id, Value = ts.Name ?? string.Empty, Label = ts.Description ?? string.Empty, }));
        return list;
    }

    /// <summary>
    /// 获取选择器列表（字符串 ID）
    /// </summary>
    [HttpPost(CrudActionRoutes.SelectString)]
    public virtual async Task<List<BaseSelectPageStringResponse>> SelectStringAsync(BaseSelectPageRequest input)
    {
        var searchKey = input.SearchKey?.Trim() ?? string.Empty;
        var pageSize = Math.Min(input.PageSize > 0 ? input.PageSize : BaseSelectPageRequest.DefaultPageSize, BaseSelectPageRequest.MaxPageSize);
        var query = await Repository.AsQueryable()
                                    .Where(u => u.IsDeleted == false)
                                    .WhereIF(!string.IsNullOrWhiteSpace(searchKey), u =>
                                                 (u.Name ?? string.Empty).Contains(searchKey))
                                    .Take(pageSize)
                                    .ToListAsync();
        var list = new List<BaseSelectPageStringResponse>(query.Count);
        list.AddRange(query.Select(ts => new BaseSelectPageStringResponse { Id = ts.Id.ToString(), Value = ts.Name ?? string.Empty, Label = ts.Description, }));
        return list;
    }

    /// <summary>
    /// 名称唯一性校验
    /// </summary>
    protected async Task CheckNameUniqueAsync(string name, long? excludeId = null)
    {
        var exists = excludeId.HasValue
                         ? await Repository.IsAnyAsync(x => x.Name == name && x.Id != excludeId.Value)
                         : await Repository.IsAnyAsync(x => x.Name == name);
        if (exists)
        {
            throw BusinessError.Oh(CommonErrorCode.RecordAlreadyExists);
        }
    }
}