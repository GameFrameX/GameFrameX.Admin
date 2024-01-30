using GameFrameX.Core;
using GameFrameX.Core.SqlSugar;
using GameFrameX.Entity.Client;

namespace GameFrameX.Application.SeedData;

/// <summary>
/// 客户端全局信息配置表 表种子数据
/// </summary>
public class ClientGlobalInfoSeedData: ISqlSugarEntitySeedData<ClientGlobalInfo>
{
    /// <summary>
    /// 种子数据
    /// </summary>
    /// <returns></returns>
    public IEnumerable<ClientGlobalInfo> HasData()
    {
        string recordsJSON = @"
            [
			  {
			    ""Channel"": ""default"",
			    ""Platform"": ""default"",
			    ""Version"": ""default"",
			    ""Package"": ""default"",
			    ""Language"": ""default"",
			    ""CheckResourceUrl"": ""http://127.0.0.1/Bundles/com.ALianBlank.GFX/1.0.0/WebGL/DefaultPackage/20240130192515/"",
			    ""CheckAppVersionUrl"": ""http://127.0.0.1:5244/api/GameAppVersion/GetInfo"",
			    ""Content"": ""{}"",
			    ""Description"": ""默认"",
			    ""CreateTime"": ""2024-01-30 19:13:59"",
			    ""UpdateTime"": ""2024-01-30 19:29:01"",
			    ""CreateUserId"": 1300000000101,
			    ""CreateUserName"": ""超级管理员"",
			    ""UpdateUserId"": 1300000000101,
			    ""UpdateUserName"": ""超级管理员"",
			    ""IsDelete"": false,
			    ""Id"": 31858697374789
			  }
			]
        ";
        List<ClientGlobalInfo> records = Newtonsoft.Json.JsonConvert.DeserializeObject<List<ClientGlobalInfo>>(recordsJSON);
        
        #region 处理 JsonIgnore 的Property
        
        records[0].CreateUser = null;
        records[0].UpdateUser = null;
                
        #endregion
        
        // 后处理数据的特殊字段
		//for (int i = 0; i < records.Count; i++) { }

        return records;
    }
}