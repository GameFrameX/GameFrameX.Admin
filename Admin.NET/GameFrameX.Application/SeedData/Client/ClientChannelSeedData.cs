using GameFrameX.Core;
using GameFrameX.Core.SqlSugar;
using GameFrameX.Entity.Client;

namespace GameFrameX.Application.SeedData;

/// <summary>
/// 客户端渠道配置表 表种子数据
/// </summary>
public class ClientChannelSeedData: ISqlSugarEntitySeedData<ClientChannel>
{
    /// <summary>
    /// 种子数据
    /// </summary>
    /// <returns></returns>
    public IEnumerable<ClientChannel> HasData()
    {
        string recordsJSON = @"
            [
			  {
			    ""Name"": ""default"",
			    ""Description"": ""默认缺省渠道"",
			    ""CreateTime"": ""2024-01-30 10:01:12"",
			    ""UpdateTime"": ""2024-01-30 19:00:41"",
			    ""CreateUserId"": 1300000000101,
			    ""CreateUserName"": ""超级管理员"",
			    ""UpdateUserId"": null,
			    ""UpdateUserName"": null,
			    ""IsDelete"": false,
			    ""Id"": 31850206682437
			  }
			]
        ";
        List<ClientChannel> records = Newtonsoft.Json.JsonConvert.DeserializeObject<List<ClientChannel>>(recordsJSON);
        
        #region 处理 JsonIgnore 的Property
        
        records[0].CreateUser = null;
        records[0].UpdateUser = null;
                
        #endregion
        
        // 后处理数据的特殊字段
		//for (int i = 0; i < records.Count; i++) { }

        return records;
    }
}