using GameFrameX.Core;
using GameFrameX.Core.SqlSugar;
using GameFrameX.Entity.Client;

namespace GameFrameX.Application.SeedData;

/// <summary>
/// 客户端程序版本配置表 表种子数据
/// </summary>
public class ClientVersionSeedData: ISqlSugarEntitySeedData<ClientVersion>
{
    /// <summary>
    /// 种子数据
    /// </summary>
    /// <returns></returns>
    public IEnumerable<ClientVersion> HasData()
    {
        string recordsJSON = @"
            [
			  {
			    ""Name"": ""default"",
			    ""Description"": ""默认缺省版本"",
			    ""CreateTime"": ""2024-01-01 00:00:00"",
			    ""UpdateTime"": ""2024-01-30 19:00:42"",
			    ""CreateUserId"": 1300000000101,
			    ""CreateUserName"": ""超级管理员"",
			    ""UpdateUserId"": null,
			    ""UpdateUserName"": null,
			    ""IsDelete"": false,
			    ""Id"": 31699111322950
			  },
			  {
			    ""Name"": ""1.0.0"",
			    ""Description"": ""初始版本"",
			    ""CreateTime"": ""2024-01-29 11:39:16"",
			    ""UpdateTime"": ""2024-01-30 19:00:42"",
			    ""CreateUserId"": 1300000000101,
			    ""CreateUserName"": ""超级管理员"",
			    ""UpdateUserId"": null,
			    ""UpdateUserName"": null,
			    ""IsDelete"": false,
			    ""Id"": 31699111322949
			  }
			]
        ";
        List<ClientVersion> records = Newtonsoft.Json.JsonConvert.DeserializeObject<List<ClientVersion>>(recordsJSON);
        
        #region 处理 JsonIgnore 的Property
        
        records[0].CreateUser = null;
        records[0].UpdateUser = null;
                
        records[1].CreateUser = null;
        records[1].UpdateUser = null;
                
        #endregion
        
        // 后处理数据的特殊字段
		//for (int i = 0; i < records.Count; i++) { }

        return records;
    }
}