using GameFrameX.Core;
using GameFrameX.Core.SqlSugar;
using GameFrameX.Entity.Client;

namespace GameFrameX.Application.SeedData;

/// <summary>
/// 客户端语言配置表 表种子数据
/// </summary>
public class ClientLanguageSeedData: ISqlSugarEntitySeedData<ClientLanguage>
{
    /// <summary>
    /// 种子数据
    /// </summary>
    /// <returns></returns>
    public IEnumerable<ClientLanguage> HasData()
    {
        string recordsJSON = """
                             
                                         [
                             			  {
                             			    "Name": "ChineseSimplified",
                             			    "Description": "简体中文",
                             			    "CreateTime": "2024-01-23 14:08:41",
                             			    "UpdateTime": "2024-01-23 20:50:35",
                             			    "CreateUserId": 1300000000101,
                             			    "CreateUserName": "超级管理员",
                             			    "UpdateUserId": null,
                             			    "UpdateUserName": null,
                             			    "IsDelete": false,
                             			    "Id": 31699179000000
                             			  },
                             			  {
                             			    "Name": "ChineseTraditional",
                             			    "Description": "繁体中文",
                             			    "CreateTime": "2024-01-23 14:08:41",
                             			    "UpdateTime": "2024-01-23 20:50:35",
                             			    "CreateUserId": 1300000000101,
                             			    "CreateUserName": "超级管理员",
                             			    "UpdateUserId": null,
                             			    "UpdateUserName": null,
                             			    "IsDelete": false,
                             			    "Id": 31699179000001
                             			  },
                             			  {
                             			    "Name": "English",
                             			    "Description": "英语",
                             			    "CreateTime": "2024-01-23 14:08:41",
                             			    "UpdateTime": "2024-01-23 20:50:35",
                             			    "CreateUserId": 1300000000101,
                             			    "CreateUserName": "超级管理员",
                             			    "UpdateUserId": null,
                             			    "UpdateUserName": null,
                             			    "IsDelete": false,
                             			    "Id": 31699179000002
                             			  },
                             			  {
                             			    "Name": "Afrikaans",
                             			    "Description": "南非荷兰语",
                             			    "CreateTime": "2024-01-23 14:08:41",
                             			    "UpdateTime": "2024-01-23 20:50:35",
                             			    "CreateUserId": 1300000000101,
                             			    "CreateUserName": "超级管理员",
                             			    "UpdateUserId": null,
                             			    "UpdateUserName": null,
                             			    "IsDelete": false,
                             			    "Id": 31699179000003
                             			  },
                             			  {
                             			    "Name": "Albanian",
                             			    "Description": "阿尔巴尼亚语",
                             			    "CreateTime": "2024-01-23 14:08:41",
                             			    "UpdateTime": "2024-01-23 20:50:35",
                             			    "CreateUserId": 1300000000101,
                             			    "CreateUserName": "超级管理员",
                             			    "UpdateUserId": null,
                             			    "UpdateUserName": null,
                             			    "IsDelete": false,
                             			    "Id": 31699179000004
                             			  },
                             			  {
                             			    "Name": "Arabic",
                             			    "Description": "阿拉伯语",
                             			    "CreateTime": "2024-01-23 14:08:41",
                             			    "UpdateTime": "2024-01-23 20:50:35",
                             			    "CreateUserId": 1300000000101,
                             			    "CreateUserName": "超级管理员",
                             			    "UpdateUserId": null,
                             			    "UpdateUserName": null,
                             			    "IsDelete": false,
                             			    "Id": 31699179000005
                             			  },
                             			  {
                             			    "Name": "Basque",
                             			    "Description": "巴斯克语",
                             			    "CreateTime": "2024-01-23 14:08:41",
                             			    "UpdateTime": "2024-01-23 20:50:35",
                             			    "CreateUserId": 1300000000101,
                             			    "CreateUserName": "超级管理员",
                             			    "UpdateUserId": null,
                             			    "UpdateUserName": null,
                             			    "IsDelete": false,
                             			    "Id": 31699179000006
                             			  },
                             			  {
                             			    "Name": "Belarusian",
                             			    "Description": "白俄罗斯语",
                             			    "CreateTime": "2024-01-23 14:08:41",
                             			    "UpdateTime": "2024-01-23 20:50:35",
                             			    "CreateUserId": 1300000000101,
                             			    "CreateUserName": "超级管理员",
                             			    "UpdateUserId": null,
                             			    "UpdateUserName": null,
                             			    "IsDelete": false,
                             			    "Id": 31699179000007
                             			  },
                             			  {
                             			    "Name": "Bulgarian",
                             			    "Description": "保加利亚语",
                             			    "CreateTime": "2024-01-23 14:08:41",
                             			    "UpdateTime": "2024-01-23 20:50:35",
                             			    "CreateUserId": 1300000000101,
                             			    "CreateUserName": "超级管理员",
                             			    "UpdateUserId": null,
                             			    "UpdateUserName": null,
                             			    "IsDelete": false,
                             			    "Id": 31699179000008
                             			  },
                             			  {
                             			    "Name": "Catalan",
                             			    "Description": "加泰罗尼亚语",
                             			    "CreateTime": "2024-01-23 14:08:41",
                             			    "UpdateTime": "2024-01-23 20:50:35",
                             			    "CreateUserId": 1300000000101,
                             			    "CreateUserName": "超级管理员",
                             			    "UpdateUserId": null,
                             			    "UpdateUserName": null,
                             			    "IsDelete": false,
                             			    "Id": 31699179000009
                             			  },
                             			  {
                             			    "Name": "German",
                             			    "Description": "德语",
                             			    "CreateTime": "2024-01-23 14:08:41",
                             			    "UpdateTime": "2024-01-23 20:50:35",
                             			    "CreateUserId": 1300000000101,
                             			    "CreateUserName": "超级管理员",
                             			    "UpdateUserId": null,
                             			    "UpdateUserName": null,
                             			    "IsDelete": false,
                             			    "Id": 31699179000010
                             			  },
                             			  {
                             			    "Name": "Croatian",
                             			    "Description": "克罗地亚语",
                             			    "CreateTime": "2024-01-23 14:08:41",
                             			    "UpdateTime": "2024-01-23 20:50:35",
                             			    "CreateUserId": 1300000000101,
                             			    "CreateUserName": "超级管理员",
                             			    "UpdateUserId": null,
                             			    "UpdateUserName": null,
                             			    "IsDelete": false,
                             			    "Id": 31699179000011
                             			  },
                             			  {
                             			    "Name": "Czech",
                             			    "Description": "捷克语",
                             			    "CreateTime": "2024-01-23 14:08:41",
                             			    "UpdateTime": "2024-01-23 20:50:35",
                             			    "CreateUserId": 1300000000101,
                             			    "CreateUserName": "超级管理员",
                             			    "UpdateUserId": null,
                             			    "UpdateUserName": null,
                             			    "IsDelete": false,
                             			    "Id": 31699179000012
                             			  },
                             			  {
                             			    "Name": "Danish",
                             			    "Description": "丹麦语",
                             			    "CreateTime": "2024-01-23 14:08:41",
                             			    "UpdateTime": "2024-01-23 20:50:35",
                             			    "CreateUserId": 1300000000101,
                             			    "CreateUserName": "超级管理员",
                             			    "UpdateUserId": null,
                             			    "UpdateUserName": null,
                             			    "IsDelete": false,
                             			    "Id": 31699179000013
                             			  },
                             			  {
                             			    "Name": "Dutch",
                             			    "Description": "荷兰语",
                             			    "CreateTime": "2024-01-23 14:08:41",
                             			    "UpdateTime": "2024-01-23 20:50:35",
                             			    "CreateUserId": 1300000000101,
                             			    "CreateUserName": "超级管理员",
                             			    "UpdateUserId": null,
                             			    "UpdateUserName": null,
                             			    "IsDelete": false,
                             			    "Id": 31699179000014
                             			  },
                             			  {
                             			    "Name": "Estonian",
                             			    "Description": "爱沙尼亚语",
                             			    "CreateTime": "2024-01-23 14:08:41",
                             			    "UpdateTime": "2024-01-23 20:50:35",
                             			    "CreateUserId": 1300000000101,
                             			    "CreateUserName": "超级管理员",
                             			    "UpdateUserId": null,
                             			    "UpdateUserName": null,
                             			    "IsDelete": false,
                             			    "Id": 31699179000015
                             			  },
                             			  {
                             			    "Name": "Faroese",
                             			    "Description": "法罗语",
                             			    "CreateTime": "2024-01-23 14:08:41",
                             			    "UpdateTime": "2024-01-23 20:50:35",
                             			    "CreateUserId": 1300000000101,
                             			    "CreateUserName": "超级管理员",
                             			    "UpdateUserId": null,
                             			    "UpdateUserName": null,
                             			    "IsDelete": false,
                             			    "Id": 31699179000016
                             			  },
                             			  {
                             			    "Name": "Finnish",
                             			    "Description": "芬兰语",
                             			    "CreateTime": "2024-01-23 14:08:41",
                             			    "UpdateTime": "2024-01-23 20:50:35",
                             			    "CreateUserId": 1300000000101,
                             			    "CreateUserName": "超级管理员",
                             			    "UpdateUserId": null,
                             			    "UpdateUserName": null,
                             			    "IsDelete": false,
                             			    "Id": 31699179000017
                             			  },
                             			  {
                             			    "Name": "French",
                             			    "Description": "法语",
                             			    "CreateTime": "2024-01-23 14:08:41",
                             			    "UpdateTime": "2024-01-23 20:50:35",
                             			    "CreateUserId": 1300000000101,
                             			    "CreateUserName": "超级管理员",
                             			    "UpdateUserId": null,
                             			    "UpdateUserName": null,
                             			    "IsDelete": false,
                             			    "Id": 31699179000018
                             			  },
                             			  {
                             			    "Name": "Georgian",
                             			    "Description": "格鲁吉亚语",
                             			    "CreateTime": "2024-01-23 14:08:41",
                             			    "UpdateTime": "2024-01-23 20:50:35",
                             			    "CreateUserId": 1300000000101,
                             			    "CreateUserName": "超级管理员",
                             			    "UpdateUserId": null,
                             			    "UpdateUserName": null,
                             			    "IsDelete": false,
                             			    "Id": 31699179000019
                             			  },
                             			  {
                             			    "Name": "Greek",
                             			    "Description": "希腊语",
                             			    "CreateTime": "2024-01-23 14:08:41",
                             			    "UpdateTime": "2024-01-23 20:50:35",
                             			    "CreateUserId": 1300000000101,
                             			    "CreateUserName": "超级管理员",
                             			    "UpdateUserId": null,
                             			    "UpdateUserName": null,
                             			    "IsDelete": false,
                             			    "Id": 31699179000020
                             			  },
                             			  {
                             			    "Name": "Hebrew",
                             			    "Description": "希伯来语",
                             			    "CreateTime": "2024-01-23 14:08:41",
                             			    "UpdateTime": "2024-01-23 20:50:35",
                             			    "CreateUserId": 1300000000101,
                             			    "CreateUserName": "超级管理员",
                             			    "UpdateUserId": null,
                             			    "UpdateUserName": null,
                             			    "IsDelete": false,
                             			    "Id": 31699179000021
                             			  },
                             			  {
                             			    "Name": "Hungarian",
                             			    "Description": "匈牙利语",
                             			    "CreateTime": "2024-01-23 14:08:41",
                             			    "UpdateTime": "2024-01-23 20:50:35",
                             			    "CreateUserId": 1300000000101,
                             			    "CreateUserName": "超级管理员",
                             			    "UpdateUserId": null,
                             			    "UpdateUserName": null,
                             			    "IsDelete": false,
                             			    "Id": 31699179000022
                             			  },
                             			  {
                             			    "Name": "Icelandic",
                             			    "Description": "冰岛语",
                             			    "CreateTime": "2024-01-23 14:08:41",
                             			    "UpdateTime": "2024-01-23 20:50:35",
                             			    "CreateUserId": 1300000000101,
                             			    "CreateUserName": "超级管理员",
                             			    "UpdateUserId": null,
                             			    "UpdateUserName": null,
                             			    "IsDelete": false,
                             			    "Id": 31699179000023
                             			  },
                             			  {
                             			    "Name": "Indonesian",
                             			    "Description": "印尼语",
                             			    "CreateTime": "2024-01-23 14:08:41",
                             			    "UpdateTime": "2024-01-23 20:50:35",
                             			    "CreateUserId": 1300000000101,
                             			    "CreateUserName": "超级管理员",
                             			    "UpdateUserId": null,
                             			    "UpdateUserName": null,
                             			    "IsDelete": false,
                             			    "Id": 31699179000024
                             			  },
                             			  {
                             			    "Name": "Italian",
                             			    "Description": "意大利语",
                             			    "CreateTime": "2024-01-23 14:08:41",
                             			    "UpdateTime": "2024-01-23 20:50:35",
                             			    "CreateUserId": 1300000000101,
                             			    "CreateUserName": "超级管理员",
                             			    "UpdateUserId": null,
                             			    "UpdateUserName": null,
                             			    "IsDelete": false,
                             			    "Id": 31699179000025
                             			  },
                             			  {
                             			    "Name": "Japanese",
                             			    "Description": "日语",
                             			    "CreateTime": "2024-01-23 14:08:41",
                             			    "UpdateTime": "2024-01-23 20:50:35",
                             			    "CreateUserId": 1300000000101,
                             			    "CreateUserName": "超级管理员",
                             			    "UpdateUserId": null,
                             			    "UpdateUserName": null,
                             			    "IsDelete": false,
                             			    "Id": 31699179000026
                             			  },
                             			  {
                             			    "Name": "Korean",
                             			    "Description": "韩语",
                             			    "CreateTime": "2024-01-23 14:08:41",
                             			    "UpdateTime": "2024-01-23 20:50:35",
                             			    "CreateUserId": 1300000000101,
                             			    "CreateUserName": "超级管理员",
                             			    "UpdateUserId": null,
                             			    "UpdateUserName": null,
                             			    "IsDelete": false,
                             			    "Id": 31699179000027
                             			  },
                             			  {
                             			    "Name": "Latvian",
                             			    "Description": "拉脱维亚语",
                             			    "CreateTime": "2024-01-23 14:08:41",
                             			    "UpdateTime": "2024-01-23 20:50:35",
                             			    "CreateUserId": 1300000000101,
                             			    "CreateUserName": "超级管理员",
                             			    "UpdateUserId": null,
                             			    "UpdateUserName": null,
                             			    "IsDelete": false,
                             			    "Id": 31699179000028
                             			  },
                             			  {
                             			    "Name": "Lithuanian",
                             			    "Description": "立陶宛语",
                             			    "CreateTime": "2024-01-23 14:08:41",
                             			    "UpdateTime": "2024-01-23 20:50:35",
                             			    "CreateUserId": 1300000000101,
                             			    "CreateUserName": "超级管理员",
                             			    "UpdateUserId": null,
                             			    "UpdateUserName": null,
                             			    "IsDelete": false,
                             			    "Id": 31699179000029
                             			  },
                             			  {
                             			    "Name": "Macedonian",
                             			    "Description": "马其顿语",
                             			    "CreateTime": "2024-01-23 14:08:41",
                             			    "UpdateTime": "2024-01-23 20:50:35",
                             			    "CreateUserId": 1300000000101,
                             			    "CreateUserName": "超级管理员",
                             			    "UpdateUserId": null,
                             			    "UpdateUserName": null,
                             			    "IsDelete": false,
                             			    "Id": 31699179000030
                             			  },
                             			  {
                             			    "Name": "Malayalam",
                             			    "Description": "马拉雅拉姆语",
                             			    "CreateTime": "2024-01-23 14:08:41",
                             			    "UpdateTime": "2024-01-23 20:50:35",
                             			    "CreateUserId": 1300000000101,
                             			    "CreateUserName": "超级管理员",
                             			    "UpdateUserId": null,
                             			    "UpdateUserName": null,
                             			    "IsDelete": false,
                             			    "Id": 31699179000031
                             			  },
                             			  {
                             			    "Name": "Norwegian",
                             			    "Description": "挪威语",
                             			    "CreateTime": "2024-01-23 14:08:41",
                             			    "UpdateTime": "2024-01-23 20:50:35",
                             			    "CreateUserId": 1300000000101,
                             			    "CreateUserName": "超级管理员",
                             			    "UpdateUserId": null,
                             			    "UpdateUserName": null,
                             			    "IsDelete": false,
                             			    "Id": 31699179000032
                             			  },
                             			  {
                             			    "Name": "Persian",
                             			    "Description": "波斯语",
                             			    "CreateTime": "2024-01-23 14:08:41",
                             			    "UpdateTime": "2024-01-23 20:50:35",
                             			    "CreateUserId": 1300000000101,
                             			    "CreateUserName": "超级管理员",
                             			    "UpdateUserId": null,
                             			    "UpdateUserName": null,
                             			    "IsDelete": false,
                             			    "Id": 31699179000033
                             			  },
                             			  {
                             			    "Name": "Polish",
                             			    "Description": "波兰语",
                             			    "CreateTime": "2024-01-23 14:08:41",
                             			    "UpdateTime": "2024-01-23 20:50:35",
                             			    "CreateUserId": 1300000000101,
                             			    "CreateUserName": "超级管理员",
                             			    "UpdateUserId": null,
                             			    "UpdateUserName": null,
                             			    "IsDelete": false,
                             			    "Id": 31699179000034
                             			  },
                             			  {
                             			    "Name": "PortugueseBrazil",
                             			    "Description": "巴西葡萄牙语",
                             			    "CreateTime": "2024-01-23 14:08:41",
                             			    "UpdateTime": "2024-01-23 20:50:35",
                             			    "CreateUserId": 1300000000101,
                             			    "CreateUserName": "超级管理员",
                             			    "UpdateUserId": null,
                             			    "UpdateUserName": null,
                             			    "IsDelete": false,
                             			    "Id": 31699179000035
                             			  },
                             			  {
                             			    "Name": "PortuguesePortugal",
                             			    "Description": "葡萄牙语",
                             			    "CreateTime": "2024-01-23 14:08:41",
                             			    "UpdateTime": "2024-01-23 20:50:35",
                             			    "CreateUserId": 1300000000101,
                             			    "CreateUserName": "超级管理员",
                             			    "UpdateUserId": null,
                             			    "UpdateUserName": null,
                             			    "IsDelete": false,
                             			    "Id": 31699179000036
                             			  },
                             			  {
                             			    "Name": "Romanian",
                             			    "Description": "罗马尼亚语",
                             			    "CreateTime": "2024-01-23 14:08:41",
                             			    "UpdateTime": "2024-01-23 20:50:35",
                             			    "CreateUserId": 1300000000101,
                             			    "CreateUserName": "超级管理员",
                             			    "UpdateUserId": null,
                             			    "UpdateUserName": null,
                             			    "IsDelete": false,
                             			    "Id": 31699179000037
                             			  },
                             			  {
                             			    "Name": "Russian",
                             			    "Description": "俄语",
                             			    "CreateTime": "2024-01-23 14:08:41",
                             			    "UpdateTime": "2024-01-23 20:50:35",
                             			    "CreateUserId": 1300000000101,
                             			    "CreateUserName": "超级管理员",
                             			    "UpdateUserId": null,
                             			    "UpdateUserName": null,
                             			    "IsDelete": false,
                             			    "Id": 31699179000038
                             			  },
                             			  {
                             			    "Name": "SerboCroatian",
                             			    "Description": "塞尔维亚克罗地亚语",
                             			    "CreateTime": "2024-01-23 14:08:41",
                             			    "UpdateTime": "2024-01-23 20:50:35",
                             			    "CreateUserId": 1300000000101,
                             			    "CreateUserName": "超级管理员",
                             			    "UpdateUserId": null,
                             			    "UpdateUserName": null,
                             			    "IsDelete": false,
                             			    "Id": 31699179000039
                             			  },
                             			  {
                             			    "Name": "SerbianCyrillic",
                             			    "Description": "塞尔维亚西里尔语",
                             			    "CreateTime": "2024-01-23 14:08:41",
                             			    "UpdateTime": "2024-01-23 20:50:35",
                             			    "CreateUserId": 1300000000101,
                             			    "CreateUserName": "超级管理员",
                             			    "UpdateUserId": null,
                             			    "UpdateUserName": null,
                             			    "IsDelete": false,
                             			    "Id": 31699179000040
                             			  },
                             			  {
                             			    "Name": "SerbianLatin",
                             			    "Description": "塞尔维亚拉丁语",
                             			    "CreateTime": "2024-01-23 14:08:41",
                             			    "UpdateTime": "2024-01-23 20:50:35",
                             			    "CreateUserId": 1300000000101,
                             			    "CreateUserName": "超级管理员",
                             			    "UpdateUserId": null,
                             			    "UpdateUserName": null,
                             			    "IsDelete": false,
                             			    "Id": 31699179000041
                             			  },
                             			  {
                             			    "Name": "Slovak",
                             			    "Description": "斯洛伐克语",
                             			    "CreateTime": "2024-01-23 14:08:41",
                             			    "UpdateTime": "2024-01-23 20:50:35",
                             			    "CreateUserId": 1300000000101,
                             			    "CreateUserName": "超级管理员",
                             			    "UpdateUserId": null,
                             			    "UpdateUserName": null,
                             			    "IsDelete": false,
                             			    "Id": 31699179000042
                             			  },
                             			  {
                             			    "Name": "Slovenian",
                             			    "Description": "斯洛文尼亚语",
                             			    "CreateTime": "2024-01-23 14:08:41",
                             			    "UpdateTime": "2024-01-23 20:50:35",
                             			    "CreateUserId": 1300000000101,
                             			    "CreateUserName": "超级管理员",
                             			    "UpdateUserId": null,
                             			    "UpdateUserName": null,
                             			    "IsDelete": false,
                             			    "Id": 31699179000043
                             			  },
                             			  {
                             			    "Name": "Spanish",
                             			    "Description": "西班牙语",
                             			    "CreateTime": "2024-01-23 14:08:41",
                             			    "UpdateTime": "2024-01-23 20:50:35",
                             			    "CreateUserId": 1300000000101,
                             			    "CreateUserName": "超级管理员",
                             			    "UpdateUserId": null,
                             			    "UpdateUserName": null,
                             			    "IsDelete": false,
                             			    "Id": 31699179000044
                             			  },
                             			  {
                             			    "Name": "Swedish",
                             			    "Description": "瑞典语",
                             			    "CreateTime": "2024-01-23 14:08:41",
                             			    "UpdateTime": "2024-01-23 20:50:35",
                             			    "CreateUserId": 1300000000101,
                             			    "CreateUserName": "超级管理员",
                             			    "UpdateUserId": null,
                             			    "UpdateUserName": null,
                             			    "IsDelete": false,
                             			    "Id": 31699179000045
                             			  },
                             			  {
                             			    "Name": "Thai",
                             			    "Description": "泰语",
                             			    "CreateTime": "2024-01-23 14:08:41",
                             			    "UpdateTime": "2024-01-23 20:50:35",
                             			    "CreateUserId": 1300000000101,
                             			    "CreateUserName": "超级管理员",
                             			    "UpdateUserId": null,
                             			    "UpdateUserName": null,
                             			    "IsDelete": false,
                             			    "Id": 31699179000046
                             			  },
                             			  {
                             			    "Name": "Turkish",
                             			    "Description": "土耳其语",
                             			    "CreateTime": "2024-01-23 14:08:41",
                             			    "UpdateTime": "2024-01-23 20:50:35",
                             			    "CreateUserId": 1300000000101,
                             			    "CreateUserName": "超级管理员",
                             			    "UpdateUserId": null,
                             			    "UpdateUserName": null,
                             			    "IsDelete": false,
                             			    "Id": 31699179000047
                             			  },
                             			  {
                             			    "Name": "Ukrainian",
                             			    "Description": "乌克兰语",
                             			    "CreateTime": "2024-01-23 14:08:41",
                             			    "UpdateTime": "2024-01-23 20:50:35",
                             			    "CreateUserId": 1300000000101,
                             			    "CreateUserName": "超级管理员",
                             			    "UpdateUserId": null,
                             			    "UpdateUserName": null,
                             			    "IsDelete": false,
                             			    "Id": 31699179000048
                             			  },
                             			  {
                             			    "Name": "Vietnamese",
                             			    "Description": "越南语",
                             			    "CreateTime": "2024-01-23 14:08:41",
                             			    "UpdateTime": "2024-01-23 20:50:35",
                             			    "CreateUserId": 1300000000101,
                             			    "CreateUserName": "超级管理员",
                             			    "UpdateUserId": null,
                             			    "UpdateUserName": null,
                             			    "IsDelete": false,
                             			    "Id": 31699179000049
                             			  },
                             			  {
                             			    "Name": "default",
                             			    "Description": "默认缺省语言",
                             			    "CreateTime": "2024-01-30 10:00:33",
                             			    "UpdateTime": null,
                             			    "CreateUserId": 1300000000101,
                             			    "CreateUserName": "超级管理员",
                             			    "UpdateUserId": null,
                             			    "UpdateUserName": null,
                             			    "IsDelete": false,
                             			    "Id": 31850196630341
                             			  }
                             			]
                                     
                             """;
        List<ClientLanguage> records = Newtonsoft.Json.JsonConvert.DeserializeObject<List<ClientLanguage>>(recordsJSON);
        
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
                
        records[6].CreateUser = null;
        records[6].UpdateUser = null;
                
        records[7].CreateUser = null;
        records[7].UpdateUser = null;
                
        records[8].CreateUser = null;
        records[8].UpdateUser = null;
                
        records[9].CreateUser = null;
        records[9].UpdateUser = null;
                
        records[10].CreateUser = null;
        records[10].UpdateUser = null;
                
        records[11].CreateUser = null;
        records[11].UpdateUser = null;
                
        records[12].CreateUser = null;
        records[12].UpdateUser = null;
                
        records[13].CreateUser = null;
        records[13].UpdateUser = null;
                
        records[14].CreateUser = null;
        records[14].UpdateUser = null;
                
        records[15].CreateUser = null;
        records[15].UpdateUser = null;
                
        records[16].CreateUser = null;
        records[16].UpdateUser = null;
                
        records[17].CreateUser = null;
        records[17].UpdateUser = null;
                
        records[18].CreateUser = null;
        records[18].UpdateUser = null;
                
        records[19].CreateUser = null;
        records[19].UpdateUser = null;
                
        records[20].CreateUser = null;
        records[20].UpdateUser = null;
                
        records[21].CreateUser = null;
        records[21].UpdateUser = null;
                
        records[22].CreateUser = null;
        records[22].UpdateUser = null;
                
        records[23].CreateUser = null;
        records[23].UpdateUser = null;
                
        records[24].CreateUser = null;
        records[24].UpdateUser = null;
                
        records[25].CreateUser = null;
        records[25].UpdateUser = null;
                
        records[26].CreateUser = null;
        records[26].UpdateUser = null;
                
        records[27].CreateUser = null;
        records[27].UpdateUser = null;
                
        records[28].CreateUser = null;
        records[28].UpdateUser = null;
                
        records[29].CreateUser = null;
        records[29].UpdateUser = null;
                
        records[30].CreateUser = null;
        records[30].UpdateUser = null;
                
        records[31].CreateUser = null;
        records[31].UpdateUser = null;
                
        records[32].CreateUser = null;
        records[32].UpdateUser = null;
                
        records[33].CreateUser = null;
        records[33].UpdateUser = null;
                
        records[34].CreateUser = null;
        records[34].UpdateUser = null;
                
        records[35].CreateUser = null;
        records[35].UpdateUser = null;
                
        records[36].CreateUser = null;
        records[36].UpdateUser = null;
                
        records[37].CreateUser = null;
        records[37].UpdateUser = null;
                
        records[38].CreateUser = null;
        records[38].UpdateUser = null;
                
        records[39].CreateUser = null;
        records[39].UpdateUser = null;
                
        records[40].CreateUser = null;
        records[40].UpdateUser = null;
                
        records[41].CreateUser = null;
        records[41].UpdateUser = null;
                
        records[42].CreateUser = null;
        records[42].UpdateUser = null;
                
        records[43].CreateUser = null;
        records[43].UpdateUser = null;
                
        records[44].CreateUser = null;
        records[44].UpdateUser = null;
                
        records[45].CreateUser = null;
        records[45].UpdateUser = null;
                
        records[46].CreateUser = null;
        records[46].UpdateUser = null;
                
        records[47].CreateUser = null;
        records[47].UpdateUser = null;
                
        records[48].CreateUser = null;
        records[48].UpdateUser = null;
                
        records[49].CreateUser = null;
        records[49].UpdateUser = null;
                
        records[50].CreateUser = null;
        records[50].UpdateUser = null;
                
        #endregion
        
        // 后处理数据的特殊字段
		//for (int i = 0; i < records.Count; i++) { }

        return records;
    }
}