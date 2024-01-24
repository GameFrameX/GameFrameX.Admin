using GameFrameX.Application.Game;
using GameFrameX.Core.Extension;
using GameFrameX.Core.SqlSugar;
using GameFrameX.Entity.Client;

namespace GameFrameX.Application.SeedData.Client;

/// <summary>
///  表种子数据
/// </summary>
public class ClientVersionSeedData : ISqlSugarEntitySeedData<ClientVersion>
{
    /// <summary>
    /// 种子数据
    /// </summary>
    /// <returns></returns>
    public IEnumerable<ClientVersion> HasData()
    {
        return new[]
        {
            new ClientVersion()
            {
                Name = "1.0.0",
                Description = "初始版本",
                CreateTime = "2024-01-23 14:04:15".ParseToDateTime(),
                UpdateTime = null,
                CreateUserId = 1300000000101,
                CreateUserName = "超级管理员",
                UpdateUserId = null,
                UpdateUserName = null,
                IsDelete = false,
                Id = 31699111322949
            }
        };
    }
}