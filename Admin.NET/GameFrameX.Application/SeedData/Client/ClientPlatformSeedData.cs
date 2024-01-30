using GameFrameX.Core;
using GameFrameX.Core.SqlSugar;
using GameFrameX.Entity.Client;

namespace GameFrameX.Application.SeedData;

/// <summary>
/// 客户端平台配置表 表种子数据
/// </summary>
public class ClientPlatformSeedData: ISqlSugarEntitySeedData<ClientPlatform>
{
    /// <summary>
    /// 种子数据
    /// </summary>
    /// <returns></returns>
    public IEnumerable<ClientPlatform> HasData()
    {
        string recordsJSON = @"
            [
			  {
			    ""Name"": ""default"",
			    ""Description"": ""默认缺省平台"",
			    ""CreateTime"": ""2024-01-30 10:00:55"",
			    ""UpdateTime"": ""2024-01-30 19:00:41"",
			    ""CreateUserId"": 1300000000101,
			    ""CreateUserName"": ""超级管理员"",
			    ""UpdateUserId"": null,
			    ""UpdateUserName"": null,
			    ""IsDelete"": false,
			    ""Id"": 31850202486597
			  },
			  {
			    ""Name"": ""Android"",
			    ""Description"": ""安卓平台"",
			    ""CreateTime"": ""2024-01-30 10:25:19"",
			    ""UpdateTime"": ""2024-01-30 19:00:41"",
			    ""CreateUserId"": 1300000000101,
			    ""CreateUserName"": ""超级管理员"",
			    ""UpdateUserId"": null,
			    ""UpdateUserName"": null,
			    ""IsDelete"": false,
			    ""Id"": 31850577223493
			  },
			  {
			    ""Name"": ""iOS"",
			    ""Description"": ""IPhone或Ipad"",
			    ""CreateTime"": ""2024-01-30 10:25:41"",
			    ""UpdateTime"": ""2024-01-30 19:00:41"",
			    ""CreateUserId"": 1300000000101,
			    ""CreateUserName"": ""超级管理员"",
			    ""UpdateUserId"": null,
			    ""UpdateUserName"": null,
			    ""IsDelete"": false,
			    ""Id"": 31850582821957
			  },
			  {
			    ""Name"": ""Windows"",
			    ""Description"": ""PC平台"",
			    ""CreateTime"": ""2024-01-30 10:25:52"",
			    ""UpdateTime"": ""2024-01-30 19:00:41"",
			    ""CreateUserId"": 1300000000101,
			    ""CreateUserName"": ""超级管理员"",
			    ""UpdateUserId"": null,
			    ""UpdateUserName"": null,
			    ""IsDelete"": false,
			    ""Id"": 31850585613125
			  },
			  {
			    ""Name"": ""MacOs"",
			    ""Description"": ""MacOs平台"",
			    ""CreateTime"": ""2024-01-30 10:26:13"",
			    ""UpdateTime"": ""2024-01-30 19:00:41"",
			    ""CreateUserId"": 1300000000101,
			    ""CreateUserName"": ""超级管理员"",
			    ""UpdateUserId"": null,
			    ""UpdateUserName"": null,
			    ""IsDelete"": false,
			    ""Id"": 31850590968133
			  },
			  {
			    ""Name"": ""WebGL"",
			    ""Description"": ""Web网页平台"",
			    ""CreateTime"": ""2024-01-30 10:26:45"",
			    ""UpdateTime"": ""2024-01-30 19:00:41"",
			    ""CreateUserId"": 1300000000101,
			    ""CreateUserName"": ""超级管理员"",
			    ""UpdateUserId"": null,
			    ""UpdateUserName"": null,
			    ""IsDelete"": false,
			    ""Id"": 31850599172933
			  }
			]
        ";
        List<ClientPlatform> records = Newtonsoft.Json.JsonConvert.DeserializeObject<List<ClientPlatform>>(recordsJSON);
        
        #region 处理 JsonIgnore 的Property
        
        records[0].CreateUser = null;
        records[0].UpdateUser = null;
                
        records[1].CreateUser = null;
        records[1].UpdateUser = null;
                
        records[2].CreateUser = null;
        records[2].UpdateUser = null;
                
        records[3].CreateUser = null;
        records[3].UpdateUser = null;
                
        records[4].CreateUser = null;
        records[4].UpdateUser = null;
                
        records[5].CreateUser = null;
        records[5].UpdateUser = null;
                
        #endregion
        
        // 后处理数据的特殊字段
		//for (int i = 0; i < records.Count; i++) { }

        return records;
    }
}