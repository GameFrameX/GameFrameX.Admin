using GameFrameX.Core.Const;
using GameFrameX.Core.Const.MenuSeed;
using GameFrameX.Core.SqlSugar;

namespace GameFrameX.Application.SeedData.System;

/// <summary>
/// 系统用户角色表种子数据
/// </summary>
public class TenantUserRoleSeedData : ISqlSugarEntitySeedData<TenantUserRoleEntity>
{
    /// <summary>
    /// 种子数据
    /// </summary>
    /// <returns></returns>
    public IEnumerable<TenantUserRoleEntity> HasData()
    {
        var usersByAccount = TenantUserSeedData.BuildData(string.Empty)
                                               .ToDictionary(user => user.Account, user => user.Id, StringComparer.Ordinal);
        var rolesByCode = TenantDefaultDataTemplates.RoleTemplates
                                                    .ToDictionary(role => role.Code, role => role.SeedId, StringComparer.Ordinal);

        return rolesByCode.Keys
                          .Where(usersByAccount.ContainsKey)
                          .Select(roleCode =>
                          {
                              var userId = usersByAccount[roleCode];
                              var roleId = rolesByCode[roleCode];
                              return new TenantUserRoleEntity
                              {
                                  Id = StableSeedId.UserRole(TenantDefaultDataTemplates.MainTenantId, userId, roleId),
                                  UserId = userId,
                                  RoleId = roleId,
                              };
                          })
                          .Prepend(new TenantUserRoleEntity
                          {
                              Id = StableSeedId.UserRole(TenantDefaultDataTemplates.MainTenantId, usersByAccount["admin"], rolesByCode[TenantDefaultDataTemplates.AdminRoleCode]),
                              UserId = usersByAccount["admin"],
                              RoleId = rolesByCode[TenantDefaultDataTemplates.AdminRoleCode],
                          })
                          .ToArray();
    }
}