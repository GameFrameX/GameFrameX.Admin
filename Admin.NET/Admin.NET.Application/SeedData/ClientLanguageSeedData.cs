namespace Admin.NET.Application.SeedData;

/// <summary>
///  表种子数据
/// </summary>
public class ClientLanguageSeedData : ISqlSugarEntitySeedData<ClientLanguage>
{
    /// <summary>
    /// 种子数据
    /// </summary>
    /// <returns></returns>
    public IEnumerable<ClientLanguage> HasData()
    {
        return new[]
        {
            new ClientLanguage()
            {
                Name = "SimplifiedChinese",
                Description = "简体中文",
                CreateTime = DateTime.Parse("2024-01-23 14:08:40"),
                UpdateTime = null,
                CreateUserId = 1300000000101,
                CreateUserName = "超级管理员",
                UpdateUserId = null,
                UpdateUserName = null,
                IsDelete = false,
                Id = 31699179249989
            },
        };
    }
}