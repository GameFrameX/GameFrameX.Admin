using GameFrameX.Application.System.Plugin.Dto;
using GameFrameX.Foundation.Http.Normalization;

namespace GameFrameX.Application.System.Plugin;

/// <summary>
/// 系统动态插件服务
/// </summary>
[ApiDescriptionSettings(Order = 245)]
public class SystemPluginService : IAdminApiController
{
    private readonly IAdminApiRuntimeChangeProvider _provider;
    private readonly SqlSugarRepository<SystemPluginEntity> _systemPluginRepository;

    public SystemPluginService(IAdminApiRuntimeChangeProvider provider,
        SqlSugarRepository<SystemPluginEntity> systemPluginRepository)
    {
        _provider = provider;
        _systemPluginRepository = systemPluginRepository;
    }

    /// <summary>
    /// 获取动态插件列表
    /// </summary>
    /// <param name="input"></param>
    /// <returns></returns>
    [DisplayName("获取动态插件列表")]
    [ApiDescriptionSettings(Name = "Page")]
    public async Task<SqlSugarPagedList<SystemPluginResponse>> PageAsync(PagePluginRequest input)
    {
        return await _systemPluginRepository.AsQueryable()
                                     .WhereIF(!string.IsNullOrWhiteSpace(input.Name), u => u.Name.Contains(input.Name))
                                     .OrderBy(u => u.OrderNo)
                                     .Select<SystemPluginResponse>()
                                     .ToPagedListAsync(input.Page, input.PageSize);
    }

    /// <summary>
    /// 增加动态插件
    /// </summary>
    /// <param name="input"></param>
    /// <returns></returns>
    [ApiDescriptionSettings(Name = "Add")]
    [HttpPost]
    [DisplayName("增加动态插件")]
    public async Task AddPluginAsync(AddPluginRequest input)
    {
        var isExist = await _systemPluginRepository.IsAnyAsync(u => u.Name == input.Name || u.AssemblyName == input.AssemblyName);
        if (isExist)
        {
            throw BusinessError.Oh(PluginErrorCode.PluginAlreadyExists);
        }

        // 添加动态程序集/接口
        input.AssemblyName = (await CompileAssemblyAsync(new CompileAssemblyRequest { CsharpCode = input.CsharpCode, AssemblyName = input.AssemblyName })).Data.AssemblyName;

        await _systemPluginRepository.InsertAsync(input.Adapt<SystemPluginEntity>());
    }

    /// <summary>
    /// 更新动态插件
    /// </summary>
    /// <param name="input"></param>
    /// <returns></returns>
    [ApiDescriptionSettings(Name = "Update")]
    [HttpPost]
    [DisplayName("更新动态插件")]
    public async Task UpdatePluginAsync(UpdatePluginRequest input)
    {
        var isExist = await _systemPluginRepository.IsAnyAsync(u => (u.Name == input.Name || u.AssemblyName == input.AssemblyName) && u.Id != input.Id);
        if (isExist)
        {
            throw BusinessError.Oh(PluginErrorCode.PluginAlreadyExists);
        }

        // 先移除再添加动态程序集/接口
        RemoveAssembly(input.AssemblyName);
        input.AssemblyName = (await CompileAssemblyAsync(new CompileAssemblyRequest { CsharpCode = input.CsharpCode })).Data.AssemblyName;

        await _systemPluginRepository.AsUpdateable(input.Adapt<SystemPluginEntity>()).IgnoreColumns(true).ExecuteCommandWithOptLockAsync(true);
    }

    /// <summary>
    /// 删除动态插件
    /// </summary>
    /// <param name="input"></param>
    /// <returns></returns>
    [ApiDescriptionSettings(Name = "Delete")]
    [HttpPost]
    [DisplayName("删除动态插件")]
    public async Task DeletePluginAsync(DeletePluginRequest input)
    {
        var plugin = await _systemPluginRepository.GetByIdAsync(input.Id);
        if (plugin == null)
        {
            return;
        }

        // 移除动态程序集/接口
        RemoveAssembly(plugin.AssemblyName);

        await _systemPluginRepository.DeleteAsync(u => u.Id == input.Id);
    }

    /// <summary>
    /// 添加动态程序集/接口
    /// </summary>
    /// <param name="input">编译输入（C# 源代码 + 可选程序集名称）</param>
    /// <returns></returns>
    [DisplayName("添加动态程序集/接口")]
    [ApiDescriptionSettings(Name = "CompileAssembly")]
    [HttpPost("compileAssembly")]
    public Task<HttpJsonResultData<CompileAssemblyResponse>> CompileAssemblyAsync(CompileAssemblyRequest input)
    {
        // 编译 C# 代码并返回动态程序集
        var dynamicAssembly = DynamicCSharpCompiler.Compile(input.CsharpCode, input.AssemblyName);

        // 将程序集添加进动态 WebAPI 应用部件
        _provider.AddAssembliesWithNotifyChanges(dynamicAssembly);

        // 返回动态程序集名称
        return Task.FromResult(HttpJsonResultData<CompileAssemblyResponse>.Success(new CompileAssemblyResponse { AssemblyName = dynamicAssembly.GetName().Name ?? string.Empty, }));
    }

    /// <summary>
    /// 移除动态程序集/接口
    /// </summary>
    [ApiDescriptionSettings(Name = "RemoveAssembly")]
    [HttpPost]
    [DisplayName("移除动态程序集/接口")]
    public void RemoveAssembly(string? assemblyName)
    {
        if (string.IsNullOrWhiteSpace(assemblyName))
        {
            return;
        }

        _provider.RemoveAssembliesWithNotifyChanges(assemblyName);
    }
}