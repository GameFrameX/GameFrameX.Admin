// 麻省理工学院许可证
// 
// 版权所有 (c) 2021-2023 zuohuaijun，大名科技（天津）有限公司  联系电话/微信：18020030720  QQ：515096995
// 
// 特此免费授予获得本软件的任何人以处理本软件的权利，但须遵守以下条件：在所有副本或重要部分的软件中必须包括上述版权声明和本许可声明。
// 
// 软件按“原样”提供，不提供任何形式的明示或暗示的保证，包括但不限于对适销性、适用性和非侵权的保证。
// 在任何情况下，作者或版权持有人均不对任何索赔、损害或其他责任负责，无论是因合同、侵权或其他方式引起的，与软件或其使用或其他交易有关。

using Furion.DependencyInjection;
using Furion.DynamicApiController;
using Furion.FriendlyException;
using GameFrameX.Core.BaseInputOutput;
using GameFrameX.Core.Enum;
using GameFrameX.Core.Extension;
using GameFrameX.Core.SqlSugar;
using GameFrameX.Entity;
using Mapster;
using Microsoft.AspNetCore.Mvc;

namespace GameFrameX.Application.Base.Service;

/// <summary>
/// 选择器基础服务
/// </summary>
/// <typeparam name="T"></typeparam>
[ApiDescriptionSettings(Order = 100)]
public class BaseService<T> : IDynamicApiController, ITransient where T : EntityBase, new()
{
    protected readonly SqlSugarRepository<T> Repository;

    protected BaseService(SqlSugarRepository<T> repository)
    {
        Repository = repository;
    }

    // /// <summary>
    // /// 分页查询版本管理
    // /// </summary>
    // /// <param name="input"></param>
    // /// <returns></returns>
    // [HttpPost]
    // [ApiDescriptionSettings(Name = "Page")]
    // public async Task<SqlSugarPagedList<ClientVersionOutput>> Page(BasePageInput input)
    // {
    //     var query = _rep.AsQueryable()
    //             .WhereIF(!string.IsNullOrWhiteSpace(input.SearchKey), u =>
    //                 u.Name.Contains(input.SearchKey.Trim())
    //             )
    //             .WhereIF(!string.IsNullOrWhiteSpace(input.Name), u => u.Name.Contains(input.Name.Trim()))
    //             .Select<ClientVersionOutput>()
    //         ;
    //     query = query.OrderBuilder(input, "", "CreateTime");
    //     return await query.ToPagedListAsync(input.Page, input.PageSize);
    // }

    /// <summary>
    /// 增加
    /// </summary>
    /// <param name="input"></param>
    /// <returns></returns>
    // [HttpPost]
    // [ApiDescriptionSettings(Name = "Add")]
    protected async Task InnerAdd(BaseAddInput input)
    {
        var entity = input.Adapt<T>();
        await Repository.InsertAsync(entity);
    }

    /// <summary>
    /// 删除
    /// </summary>
    /// <param name="input"></param>
    /// <returns></returns>
    [HttpPost]
    [ApiDescriptionSettings(Name = "Delete")]
    public async Task Delete(BaseDeleteInput input)
    {
        var entity = await Repository.GetFirstAsync(u => u.Id == input.Id) ?? throw Oops.Oh(ErrorCodeEnum.D1002);
        await Repository.FakeDeleteAsync<T>(entity); //假删除
        //await _rep.DeleteAsync(entity);   //真删除
    }

    /// <summary>
    /// 更新版本管理
    /// </summary>
    /// <param name="input"></param>
    /// <returns></returns>
    protected async Task InnerUpdate(BaseUpdateInput input)
    {
        var entity = input.Adapt<T>();
        await Repository.AsUpdateable(entity).IgnoreColumns(ignoreAllNullColumns: true).ExecuteCommandAsync();
    }

    /// <summary>
    /// 根据主键ID获取详细信息
    /// </summary>
    /// <param name="input"></param>
    /// <returns></returns>
    // [HttpGet]
    // [ApiDescriptionSettings(Name = "Detail")]
    protected async Task<T> InnerGet([FromQuery] QueryByIdInput input)
    {
        return await Repository.GetFirstAsync(u => u.Id == input.Id);
    }

    /// <summary>
    /// 获取数据列表
    /// </summary>
    /// <param name="input"></param>
    /// <returns></returns>
    // [HttpGet]
    // [ApiDescriptionSettings(Name = "List")]
    // public async Task<List<T>> InnerList([FromQuery] BasePageInput input)
    // {
    //     return await Repository.AsQueryable().Select<IBasePageOutput<T>>().ToListAsync();
    // }
}