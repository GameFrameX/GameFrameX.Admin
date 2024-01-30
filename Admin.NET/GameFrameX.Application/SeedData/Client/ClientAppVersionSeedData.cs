using GameFrameX.Core;
using GameFrameX.Core.SqlSugar;
using GameFrameX.Entity.Client;

namespace GameFrameX.Application.SeedData;

/// <summary>
/// 客户端程序版本配置表 表种子数据
/// </summary>
public class ClientAppVersionSeedData: ISqlSugarEntitySeedData<ClientAppVersion>
{
    /// <summary>
    /// 种子数据
    /// </summary>
    /// <returns></returns>
    public IEnumerable<ClientAppVersion> HasData()
    {
        string recordsJSON = @"
            [
			  {
			    ""Channel"": ""default"",
			    ""Platform"": ""default"",
			    ""Version"": ""1.0.0"",
			    ""Package"": ""default"",
			    ""Language"": ""default"",
			    ""OldVersion"": ""1.0.0"",
			    ""DownloadUrl"": ""http://127.0.0.1/Bundles/com.ALianBlank.GFX/1.0.0/WebGL/DefaultPackage/20240130192515/"",
			    ""IsForce"": false,
			    ""Announcement"": ""xxxx"",
			    ""Description"": ""zzz"",
			    ""CreateTime"": ""2024-01-30 19:05:15"",
			    ""UpdateTime"": ""2024-01-30 19:27:59"",
			    ""CreateUserId"": 1300000000101,
			    ""CreateUserName"": ""超级管理员"",
			    ""UpdateUserId"": 1300000000101,
			    ""UpdateUserName"": ""超级管理员"",
			    ""IsDelete"": false,
			    ""Id"": 31858563436357
			  },
			  {
			    ""Channel"": ""default"",
			    ""Platform"": ""default"",
			    ""Version"": ""1.0.0"",
			    ""Package"": ""default"",
			    ""Language"": ""default"",
			    ""OldVersion"": ""1.0.0"",
			    ""DownloadUrl"": ""1"",
			    ""IsForce"": false,
			    ""Announcement"": ""ass"",
			    ""Description"": ""aaa"",
			    ""CreateTime"": ""2024-01-30 19:19:08"",
			    ""UpdateTime"": null,
			    ""CreateUserId"": 1300000000101,
			    ""CreateUserName"": ""超级管理员"",
			    ""UpdateUserId"": null,
			    ""UpdateUserName"": null,
			    ""IsDelete"": false,
			    ""Id"": 31858776550213
			  },
			  {
			    ""Channel"": ""default"",
			    ""Platform"": ""WebGL"",
			    ""Version"": ""1.0.0"",
			    ""Package"": ""default"",
			    ""Language"": ""default"",
			    ""OldVersion"": ""1.0.0"",
			    ""DownloadUrl"": ""111"",
			    ""IsForce"": false,
			    ""Announcement"": ""11"",
			    ""Description"": ""111"",
			    ""CreateTime"": ""2024-01-30 19:19:37"",
			    ""UpdateTime"": null,
			    ""CreateUserId"": 1300000000101,
			    ""CreateUserName"": ""超级管理员"",
			    ""UpdateUserId"": null,
			    ""UpdateUserName"": null,
			    ""IsDelete"": false,
			    ""Id"": 31858783902789
			  },
			  {
			    ""Channel"": ""default"",
			    ""Platform"": ""default"",
			    ""Version"": ""default"",
			    ""Package"": ""default"",
			    ""Language"": ""default"",
			    ""OldVersion"": ""default"",
			    ""DownloadUrl"": ""111"",
			    ""IsForce"": false,
			    ""Announcement"": ""11"",
			    ""Description"": ""111"",
			    ""CreateTime"": ""2024-01-30 19:23:12"",
			    ""UpdateTime"": null,
			    ""CreateUserId"": 1300000000101,
			    ""CreateUserName"": ""超级管理员"",
			    ""UpdateUserId"": null,
			    ""UpdateUserName"": null,
			    ""IsDelete"": false,
			    ""Id"": 31858839149637
			  }
			]
        ";
        List<ClientAppVersion> records = Newtonsoft.Json.JsonConvert.DeserializeObject<List<ClientAppVersion>>(recordsJSON);
        
        #region 处理 JsonIgnore 的Property
        
        records[0].CreateUser = null;
        records[0].UpdateUser = null;
                
        records[1].CreateUser = null;
        records[1].UpdateUser = null;
                
        records[2].CreateUser = null;
        records[2].UpdateUser = null;
                
        records[3].CreateUser = null;
        records[3].UpdateUser = null;
                
        #endregion
        
        // 后处理数据的特殊字段
		//for (int i = 0; i < records.Count; i++) { }

        return records;
    }
}