using GameFrameX.Core.Const;
using GameFrameX.Core.Const.MenuSeed;
using GameFrameX.Core.SqlSugar;
using GameFrameX.Core.Extension.Const;
using GameFrameX.Core.Service.User;
using GameFrameX.Core.Utility;
using SqlSugar;
using GameFrameX.Entity.Enum;
using GameFrameX.Foundation.Utility.DistributedSystem.Snowflake;

namespace GameFrameX.Core.Bootstrap;

/// <summary>
/// 新装首个业务管理员种子引导（C406 / F3）。
/// <para>Web.Api 初始化（旧库守卫 + 建表 + 种子）之后执行：
/// ①<see cref="CommonConst.SysAdminRole"/> 角色不存在时创建，并按 <see cref="RoleMenuService"/> 同口径
/// （超管专属子树不授予、<c>AllowAdminWhenSuperAdminOnly</c> 可读选择器照常授予）授予全部非超管种子菜单；
/// ②<see cref="InitialAccountEnvironmentVariable"/> 与 <see cref="InitialPasswordEnvironmentVariable"/>
/// 成对设置且库中尚无任何业务管理员（<see cref="UserEntity"/>）时创建该账号（Argon2id）并绑定管理员角色。</para>
/// <para>幂等口径：账号已存在不重建、不覆盖密码；角色菜单授权每次启动补齐全部缺失的种子菜单授权——
/// 运营在管理端删掉 <c>sys_admin</c> 的某条种子菜单授权后，下次重启会被补回（模板角色的手工授权不持久，
/// 与 C353 口径一致）。需要收窄权限时新建自定义角色，不要直接修改 <c>sys_admin</c>；
/// 这与 global_menu 种子的 upsert 语义同向：菜单行与模板角色授权都由启动期种子保证。</para>
/// <para>口令永不落入日志；密码不满足平台密码强度策略时 fail-closed 终止启动（错误信息不含密码）。</para>
/// </summary>
public static class InitialAdminBootstrap
{
    /// <summary>
    /// 首个业务管理员账号注入环境变量名（与密码变量成对设置才生效）。
    /// </summary>
    public const string InitialAccountEnvironmentVariable = "GAMEFRAMEX_ADMIN_INITIAL_ACCOUNT";

    /// <summary>
    /// 首个业务管理员初始密码注入环境变量名（与账号变量成对设置才生效）。
    /// </summary>
    public const string InitialPasswordEnvironmentVariable = "GAMEFRAMEX_ADMIN_INITIAL_PASSWORD";

    /// <summary>
    /// 执行引导。须在主库建表与种子初始化完成之后（依赖 MenuSeedCatalog 已 LoadFromDb）调用。
    /// </summary>
    /// <param name="sqlSugarClient">SqlSugar 客户端（取主库连接）。</param>
    public static async Task RunAsync(ISqlSugarClient sqlSugarClient)
    {
        var db = sqlSugarClient.AsTenant().GetConnectionScope(SqlSugarConst.MainConfigId);

        var roleId = await EnsureSysAdminRoleAsync(db);
        await GrantAdminMenusAsync(db, roleId);
        await EnsureInitialAccountAsync(db, roleId);
    }

    /// <summary>
    /// 确保 <see cref="CommonConst.SysAdminRole"/> 角色存在，返回角色 Id。
    /// </summary>
    private static async Task<long> EnsureSysAdminRoleAsync(ISqlSugarClient db)
    {
        var role = await db.Queryable<RoleEntity>().Where(u => u.Code == CommonConst.SysAdminRole).FirstAsync();
        if (role != null)
        {
            return role.Id;
        }

        role = new RoleEntity
        {
            Id = SnowFlakeIdHelper.Instance.NextId(),
            Name = "系统管理员",
            Code = CommonConst.SysAdminRole,
            OrderNo = 1,
            Remark = "新装引导创建的内置管理员角色，授权全部非超管菜单",
            IsEnabled = true,
        };
        await db.Insertable(role).ExecuteCommandAsync();
        var roleId = role.Id;

        Console.WriteLine($"[InitialAdmin] 已创建管理员角色 {CommonConst.SysAdminRole} 并授权全部非超管种子菜单。");
        return roleId;
    }

    /// <summary>
    /// 补齐管理员角色缺失的非超管种子菜单授权（与 RoleMenuService.ExcludeSuperAdminOnlyMenusAsync 同口径）；
    /// 已删除的种子授权会在下次启动补回（模板角色手工授权不持久）。
    /// </summary>
    private static async Task GrantAdminMenusAsync(ISqlSugarClient db, long roleId)
    {
        var reservedMenuIds = MenuSeedCatalog.SuperAdminOnlySubtreeMenuIds;
        var readableMenuIds = MenuSeedCatalog.AdminReadableSuperAdminOnlyMenuIds;
        var grantableMenuIds = MenuSeedCatalog.AllDefinitions
                                              .Select(u => StableSeedId.Menu(u.Key))
                                              .Where(id => !reservedMenuIds.Contains(id) || readableMenuIds.Contains(id))
                                              .ToHashSet();

        var grantedMenuIds = (await db.Queryable<RoleMenuEntity>().Where(u => u.RoleId == roleId).Select(u => u.MenuId).ToListAsync()).ToHashSet();
        var missingMenuIds = grantableMenuIds.Where(id => !grantedMenuIds.Contains(id)).ToArray();
        if (missingMenuIds.Length == 0)
        {
            return;
        }

        await db.Insertable(missingMenuIds.Select(menuId => new RoleMenuEntity
        {
            Id = SnowFlakeIdHelper.Instance.NextId(),
            RoleId = roleId,
            MenuId = menuId,
            IsEnabled = true,
        }).ToList()).ExecuteCommandAsync();

        Console.WriteLine($"[InitialAdmin] 管理员角色 {CommonConst.SysAdminRole} 补授 {missingMenuIds.Length} 个种子菜单（缺失授权补齐，手工删除的授权会在重启后恢复）。");
    }

    /// <summary>
    /// 按环境变量引导首个业务管理员账号；密码强度 fail-closed；全部日志不含口令。
    /// </summary>
    private static async Task EnsureInitialAccountAsync(ISqlSugarClient db, long roleId)
    {
        var account = Environment.GetEnvironmentVariable(InitialAccountEnvironmentVariable);
        var password = Environment.GetEnvironmentVariable(InitialPasswordEnvironmentVariable);
        var accountConfigured = !string.IsNullOrWhiteSpace(account);
        var passwordConfigured = !string.IsNullOrWhiteSpace(password);

        // 含软删行一并计数：账号唯一性以物理行为准，软删残留同样阻止重建，避免重启复活同名账号。
        var existingAdminCount = await db.Queryable<UserEntity>().Filter(null, true).CountAsync();

        if (!accountConfigured && !passwordConfigured)
        {
            if (existingAdminCount == 0)
            {
                Console.WriteLine($"[Warning] 尚未初始化业务管理员：设置环境变量 {InitialAccountEnvironmentVariable} 与 {InitialPasswordEnvironmentVariable} 后重启即可引导创建首个业务管理员。");
            }

            return;
        }

        if (!accountConfigured || !passwordConfigured)
        {
            Console.WriteLine($"[Warning] 环境变量 {InitialAccountEnvironmentVariable} 与 {InitialPasswordEnvironmentVariable} 必须成对设置，本次不引导创建业务管理员。");
            return;
        }

        if (existingAdminCount > 0)
        {
            Console.WriteLine($"[Warning] 库中已存在业务管理员（{existingAdminCount} 个），环境变量 {InitialAccountEnvironmentVariable} / {InitialPasswordEnvironmentVariable} 被忽略：不重建账号、不覆盖密码。");
            return;
        }

        if (CryptogramUtility.StrongPassword && !System.Text.RegularExpressions.Regex.IsMatch(password!, CryptogramUtility.PasswordStrengthValidation))
        {
            // fail-closed：拒绝带着弱口令启动；错误信息只带变量名与策略提示，不带密码。
            throw new InvalidOperationException(
                $"环境变量 {InitialPasswordEnvironmentVariable} 的初始密码不满足平台密码强度策略：{CryptogramUtility.PasswordStrengthValidationMsg}。请调整密码后重启。");
        }

        var user = new UserEntity
        {
            Id = SnowFlakeIdHelper.Instance.NextId(),
            Account = account!.Trim(),
            Password = CryptogramUtility.HashPassword(password!),
            NickName = "初始业务管理员",
            RealName = "初始业务管理员",
            Avatar = string.Empty,
            AccountType = AccountTypeEnum.SystemAdmin,
            Remark = "新装环境变量引导创建的首个业务管理员",
            IsEnabled = true,
        };
        UserService.NormalizeNewUser(user);
        await db.Insertable(user).ExecuteCommandAsync();
        var userId = user.Id;
        await db.Insertable(new UserRoleEntity
        {
            Id = SnowFlakeIdHelper.Instance.NextId(),
            UserId = userId,
            RoleId = roleId,
            IsEnabled = true,
        }).ExecuteCommandAsync();

        Console.WriteLine($"[InitialAdmin] 已按环境变量引导创建首个业务管理员账号 {user.Account} 并绑定角色 {CommonConst.SysAdminRole}，请立即登录并按需修改密码。");
    }
}
