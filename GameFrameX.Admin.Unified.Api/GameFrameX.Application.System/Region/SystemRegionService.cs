using AngleSharp;
using AngleSharp.Html.Dom;
using GameFrameX.Application.System.Region.Dto;

namespace GameFrameX.Application.System.Region;

/// <summary>
/// 系统行政区域服务
/// </summary>
[ApiDescriptionSettings(Order = 310)]
public class SystemRegionService : IAdminApiController
{
    private readonly SqlSugarRepository<SystemRegionEntity> _systemRegionRepository;

    // Url地址-国家统计局行政区域2023年
    private readonly string _url = "http://www.stats.gov.cn/sj/tjbz/tjyqhdmhcxhfdm/2023/index.html";

    public SystemRegionService(SqlSugarRepository<SystemRegionEntity> systemRegionRepository)
    {
        _systemRegionRepository = systemRegionRepository;
    }

    private static void ValidateRegionInput(AddRegionRequest input)
    {
        if ((input.Code?.Length ?? 0) > 32)
        {
            throw BusinessError.Oh("行政代码不能超过32个字符");
        }

        if ((input.CityCode?.Length ?? 0) > 6)
        {
            throw BusinessError.Oh("区号不能超过6个字符");
        }
    }

    /// <summary>
    /// 获取行政区域分页列表
    /// </summary>
    /// <param name="input"></param>
    /// <returns></returns>
    [DisplayName("获取行政区域分页列表")]
    [ApiDescriptionSettings(Name = "Page")]
    public async Task<SqlSugarPagedList<SystemRegionResponse>> PageAsync(PageRegionRequest input)
    {
        var name = input.Name?.Trim() ?? string.Empty;
        var code = input.Code?.Trim() ?? string.Empty;
        return await _systemRegionRepository.AsQueryable()
                                     .WhereIF(input.Pid > 0, u => u.Pid == input.Pid || u.Id == input.Pid)
                                     .WhereIF(!string.IsNullOrWhiteSpace(name), u => (u.Name ?? string.Empty).Contains(name))
                                     .WhereIF(!string.IsNullOrWhiteSpace(code), u => (u.Code ?? string.Empty).Contains(code))
                                     .Select<SystemRegionResponse>()
                                     .ToPagedListAsync(input.Page, input.PageSize);
    }

    /// <summary>
    /// 获取行政区域列表
    /// </summary>
    /// <param name="input"></param>
    /// <returns></returns>
    [DisplayName("获取行政区域列表")]
    [ApiDescriptionSettings(Name = "GetList")]
    public async Task<List<SystemRegionResponse>> GetListAsync([FromQuery] RegionRequest input)
    {
        return await _systemRegionRepository.AsQueryable()
                                     .Where(u => u.Pid == input.Id)
                                     .Select<SystemRegionResponse>()
                                     .ToListAsync();
    }

    /// <summary>
    /// 增加行政区域
    /// </summary>
    /// <param name="input"></param>
    /// <returns></returns>
    [ApiDescriptionSettings(Name = "Add")]
    [HttpPost]
    [DisplayName("增加行政区域")]
    public async Task<long> AddRegionAsync(AddRegionRequest input)
    {
        ValidateRegionInput(input);

        var isExist = await _systemRegionRepository.IsAnyAsync(u => u.Name == input.Name && u.Code == input.Code);
        if (isExist)
        {
            throw BusinessError.Oh(RegionErrorCode.RegionAlreadyExists);
        }

        var systemRegion = input.Adapt<SystemRegionEntity>();
        // AddRequest 未携带 RowVersion 时 Adapt 会把实体默认 0 覆盖为 null，插入后 row_version 为 NULL，
        // 首次乐观锁更新以 null 参数比较（Postgres bigint = text）直接 500（同 InnerAddAsync 兜底）
        systemRegion.RowVersion ??= 0;
        var newRegion = await _systemRegionRepository.AsInsertable(systemRegion).ExecuteReturnEntityAsync();
        return newRegion.Id;
    }

    /// <summary>
    /// 更新行政区域
    /// </summary>
    /// <param name="input"></param>
    /// <returns></returns>
    [ApiDescriptionSettings(Name = "Update")]
    [HttpPost]
    [DisplayName("更新行政区域")]
    public async Task UpdateRegionAsync(UpdateRegionRequest input)
    {
        ValidateRegionInput(input);

        if (input.Pid != 0)
        {
            var pRegion = await _systemRegionRepository.GetFirstAsync(u => u.Id == input.Pid);
            _ = pRegion ?? throw BusinessError.Oh(OrganizationErrorCode.ParentOrganizationNotFound);
        }

        if (input.Id == input.Pid)
        {
            throw BusinessError.Oh(RegionErrorCode.NodeIdEqualsParentForbidden);
        }

        var systemRegion = await _systemRegionRepository.GetFirstAsync(u => u.Id == input.Id);
        var isExist = await _systemRegionRepository.IsAnyAsync(u => u.Name == input.Name && u.Code == input.Code && u.Id != systemRegion.Id);
        if (isExist)
        {
            throw BusinessError.Oh(RegionErrorCode.RegionAlreadyExists);
        }

        //// 父Id不能为自己的子节点
        //var regionTreeList = await _systemRegionRepository.AsQueryable().ToChildListAsync(u => u.Pid, input.Id, true);
        //var childIdList = regionTreeList.Select(u => u.Id).ToList();
        //if (childIdList.Contains(input.Pid))
        //    throw BusinessError.Oh(RegionErrorCode.NodeIdEqualsParentForbidden);

        await _systemRegionRepository.AsUpdateable(input.Adapt<SystemRegionEntity>()).IgnoreColumns(true).ExecuteCommandWithOptLockAsync(true);
    }

    /// <summary>
    /// 删除行政区域
    /// </summary>
    /// <param name="input"></param>
    /// <returns></returns>
    [ApiDescriptionSettings(Name = "Delete")]
    [HttpPost]
    [DisplayName("删除行政区域")]
    public async Task DeleteRegionAsync(DeleteRegionRequest input)
    {
        var regionTreeList = await _systemRegionRepository.AsQueryable().ToChildListAsync(u => u.Pid, input.Id, true);
        var regionIdList = regionTreeList.Select(u => u.Id).ToList();
        await _systemRegionRepository.DeleteAsync(u => regionIdList.Contains(u.Id));
    }

    /// <summary>
    /// 同步行政区域
    /// </summary>
    /// <returns></returns>
    [DisplayName("同步行政区域")]
    [ApiDescriptionSettings(Name = "Sync")]
    public async Task SyncAsync()
    {
        await _systemRegionRepository.DeleteAsync(u => u.Id > 0);

        var context = BrowsingContext.New(Configuration.Default.WithDefaultLoader());
        var dom = await context.OpenAsync(_url);

        // 省级
        var itemList = dom.QuerySelectorAll("table.provincetable tr.provincetr td a");
        foreach (IHtmlAnchorElement item in itemList)
        {
            var region = await _systemRegionRepository.InsertReturnEntityAsync(new SystemRegionEntity
            {
                Pid = 0,
                Name = item.TextContent,
                Remark = item.Href,
                Level = 1,
            });

            // 市级
            if (string.IsNullOrEmpty(item.Href))
            {
                continue;
            }

            var dom1 = await context.OpenAsync(item.Href);
            var itemList1 = dom1.QuerySelectorAll("table.citytable tr.citytr td a");
            for (var i1 = 0; i1 < itemList1.Length; i1 += 2)
            {
                var item1 = (IHtmlAnchorElement)itemList1[i1 + 1];
                var region1 = await _systemRegionRepository.InsertReturnEntityAsync(new SystemRegionEntity
                {
                    Pid = region.Id,
                    Name = item1.TextContent,
                    Code = itemList1[i1].TextContent,
                    Remark = item1.Href,
                    Level = 2,
                });

                // 区县级
                if (string.IsNullOrEmpty(item1.Href))
                {
                    continue;
                }

                var dom2 = await context.OpenAsync(item1.Href);
                var itemList2 = dom2.QuerySelectorAll("table.countytable tr.countytr td a");
                for (var i2 = 0; i2 < itemList2.Length; i2 += 2)
                {
                    var item2 = (IHtmlAnchorElement)itemList2[i2 + 1];
                    var region2 = await _systemRegionRepository.InsertReturnEntityAsync(new SystemRegionEntity
                    {
                        Pid = region1.Id,
                        Name = item2.TextContent,
                        Code = itemList2[i2].TextContent,
                        Remark = item2.Href,
                        Level = 3,
                    });

                    // 街道级
                    if (string.IsNullOrEmpty(item2.Href))
                    {
                        continue;
                    }

                    var dom3 = await context.OpenAsync(item2.Href);
                    var itemList3 = dom3.QuerySelectorAll("table.towntable tr.towntr td a");
                    for (var i3 = 0; i3 < itemList3.Length; i3 += 2)
                    {
                        var item3 = (IHtmlAnchorElement)itemList3[i3 + 1];
                        var region3 = await _systemRegionRepository.InsertReturnEntityAsync(new SystemRegionEntity
                        {
                            Pid = region2.Id,
                            Name = item3.TextContent,
                            Code = itemList3[i3].TextContent,
                            Remark = item3.Href,
                            Level = 4,
                        });

                        // 村级
                        if (string.IsNullOrEmpty(item3.Href))
                        {
                            continue;
                        }

                        var dom4 = await context.OpenAsync(item3.Href);
                        var itemList4 = dom4.QuerySelectorAll("table.villagetable tr.villagetr td");
                        for (var i4 = 0; i4 < itemList4.Length; i4 += 3)
                        {
                            await _systemRegionRepository.InsertAsync(new SystemRegionEntity
                            {
                                Pid = region3.Id,
                                Name = itemList4[i4 + 2].TextContent,
                                Code = itemList4[i4].TextContent,
                                CityCode = itemList4[i4 + 1].TextContent,
                                Level = 5,
                            });
                        }
                    }
                }
            }
        }
    }
}