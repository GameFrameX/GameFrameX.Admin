using GameFrameX.Core.Service.TenantPackage;
using GameFrameX.Core.Service.User;
using GameFrameX.Entity.Global;

namespace GameFrameX.Core.Service.Base;

/// <summary>
/// 系统管理服务基类，提供公共依赖和基础功能
/// </summary>
/// <remarks>
/// 此基类封装了系统管理服务（如机构、角色、用户服务）的公共依赖和通用功能，
/// 包括用户管理和租户配额检查。派生类只需声明特有的依赖，减少重复代码。
///
/// 注意：此基类不参与自动生命周期标记，由派生类通过项目扫描注册，避免 DI 容器尝试解析抽象类。
/// </remarks>
public abstract class BaseSystemService : IAdminApiController
{
    /// <summary>
    /// 用户管理器，提供当前用户信息和权限验证
    /// </summary>
    protected readonly CurrentUserContext CurrentUserContext;

    /// <summary>
    /// 租户配额助手，提供租户配额检查功能
    /// </summary>
    protected readonly TenantQuotaHelper TenantQuotaHelper;

    /// <summary>
    /// 初始化基类，注入公共依赖
    /// </summary>
    /// <param name="currentUserContext">用户管理器</param>
    /// <param name="tenantQuotaHelper">租户配额助手</param>
    protected BaseSystemService(CurrentUserContext currentUserContext, TenantQuotaHelper tenantQuotaHelper)
    {
        CurrentUserContext = currentUserContext;
        TenantQuotaHelper = tenantQuotaHelper;
    }

    /// <summary>
    /// 检查当前用户是否有操作权限
    /// </summary>
    /// <param name="createdId">资源创建者 ID，可选。如果为 null，则只检查是否为 SuperAdmin</param>
    /// <returns>当前用户是 SuperAdmin，或是资源的创建者时返回 true；否则返回 false</returns>
    /// <remarks>
    /// 权限规则：
    /// - SuperAdmin 拥有所有权限
    /// - 普通用户只能操作自己创建的资源（createdId == 当前用户 ID）
    /// </remarks>
    /// <example>
    /// <code>
    /// // 示例 1：检查是否为 SuperAdmin
    /// if (CheckPermission())
    /// {
    ///     // SuperAdmin 可以执行所有操作
    /// }
    ///
    /// // 示例 2：检查是否可以操作指定资源
    /// if (CheckPermission(resource.CreatedId))
    /// {
    ///     // SuperAdmin 或资源创建者可以操作
    /// }
    /// </code>
    /// </example>
    protected virtual bool CheckPermission(long? createdId = null)
    {
        return CurrentUserContext.SuperAdmin ||
               (createdId.HasValue && createdId.Value == CurrentUserContext.UserId);
    }

    /// <summary>
    /// 获取租户配额包信息
    /// </summary>
    /// <param name="quotaType">配额类型标识（保留参数，用于未来扩展）</param>
    /// <returns>租户配额包信息，如果不存在则返回 null</returns>
    /// <remarks>
    /// 此方法会调用租户配额助手获取当前租户的配额包信息。
    /// 配额包包含了租户的各种资源上限，如用户数、角色数、机构数等。
    ///
    /// 使用场景：
    /// - 在创建新用户/角色/机构前检查配额
    /// - 显示租户当前配额使用情况
    /// </remarks>
    /// <example>
    /// <code>
    /// // 示例：在创建新用户前检查用户数量配额
    /// var package = await GetTenantQuotaPackageAsync();
    /// if (package != null &amp;&amp; package.MaxUserCount > currentUserCount)
    /// {
    ///     // 配额未超限，继续创建用户
    ///     var newUserId = await CreateUser(input);
    /// }
    /// else
    /// {
    ///     throw BusinessError.Oh("用户数已达上限");
    /// }
    /// </code>
    /// </example>
    protected virtual async Task<GlobalPackageEntity?> GetTenantQuotaPackageAsync(string quotaType = "")
    {
        return await TenantQuotaHelper.GetPackageAsync(CurrentUserContext.TenantId);
    }
}