using GameFrameX.Core.SqlSugar;
using GameFrameX.Entity.Global;

namespace GameFrameX.Application.SeedData.System;

/// <summary>
/// 系统字典值表种子数据
/// </summary>
public class GlobalDictionaryDataSeedData : ISqlSugarEntitySeedData<GlobalDictionaryDataEntity>
{
    /// <summary>
    /// 种子数据
    /// </summary>
    /// <returns></returns>
    public IEnumerable<GlobalDictionaryDataEntity> HasData()
    {
        return new[]
        {
            new GlobalDictionaryDataEntity { Id = 1300000000101, DictionaryTypeId = 1300000000101, Value = "输入框", Code = "Input", OrderNo = 100, Remark = "输入框", IsEnabled = true, CreatedTime = DateTimeOffset.Parse("2022-02-10 00:00:00").ToUnixTimeSeconds(), },
            new GlobalDictionaryDataEntity { Id = 1300000000102, DictionaryTypeId = 1300000000101, Value = "外键", Code = "fk", OrderNo = 100, Remark = "外键", IsEnabled = true, CreatedTime = DateTimeOffset.Parse("2022-02-10 00:00:00").ToUnixTimeSeconds(), },
            new GlobalDictionaryDataEntity { Id = 1300000000103, DictionaryTypeId = 1300000000101, Value = "日期选择", Code = "DatePicker", OrderNo = 100, Remark = "日期选择", IsEnabled = true, CreatedTime = DateTimeOffset.Parse("2022-02-10 00:00:00").ToUnixTimeSeconds(), },
            new GlobalDictionaryDataEntity { Id = 1300000000112, DictionaryTypeId = 1300000000101, Value = "日期时间选择", Code = "DateTimePicker", OrderNo = 100, Remark = "日期时间选择", IsEnabled = true, CreatedTime = DateTimeOffset.Parse("2022-02-10 00:00:00").ToUnixTimeSeconds(), },
            new GlobalDictionaryDataEntity { Id = 1300000000113, DictionaryTypeId = 1300000000101, Value = "时间选择", Code = "TimePicker", OrderNo = 100, Remark = "时间选择", IsEnabled = true, CreatedTime = DateTimeOffset.Parse("2022-02-10 00:00:00").ToUnixTimeSeconds(), },
            new GlobalDictionaryDataEntity { Id = 1300000000104, DictionaryTypeId = 1300000000101, Value = "选择器", Code = "Select", OrderNo = 100, Remark = "选择器", IsEnabled = true, CreatedTime = DateTimeOffset.Parse("2022-02-10 00:00:00").ToUnixTimeSeconds(), },
            new GlobalDictionaryDataEntity { Id = 1300000000105, DictionaryTypeId = 1300000000101, Value = "数字输入框", Code = "InputNumber", OrderNo = 100, Remark = "数字输入框", IsEnabled = true, CreatedTime = DateTimeOffset.Parse("2022-02-10 00:00:00").ToUnixTimeSeconds(), },
            new GlobalDictionaryDataEntity { Id = 1300000000106, DictionaryTypeId = 1300000000101, Value = "文本域", Code = "InputTextArea", OrderNo = 100, Remark = "文本域", IsEnabled = true, CreatedTime = DateTimeOffset.Parse("2022-02-10 00:00:00").ToUnixTimeSeconds(), },
            new GlobalDictionaryDataEntity { Id = 1300000000107, DictionaryTypeId = 1300000000101, Value = "上传", Code = "Upload", OrderNo = 100, Remark = "上传", IsEnabled = true, CreatedTime = DateTimeOffset.Parse("2022-02-10 00:00:00").ToUnixTimeSeconds(), },
            new GlobalDictionaryDataEntity { Id = 1300000000108, DictionaryTypeId = 1300000000101, Value = "树选择", Code = "ApiTreeSelect", OrderNo = 100, Remark = "树选择", IsEnabled = true, CreatedTime = DateTimeOffset.Parse("2022-02-10 00:00:00").ToUnixTimeSeconds(), },
            new GlobalDictionaryDataEntity { Id = 1300000000109, DictionaryTypeId = 1300000000101, Value = "开关", Code = "Switch", OrderNo = 100, Remark = "开关", IsEnabled = true, CreatedTime = DateTimeOffset.Parse("2022-02-10 00:00:00").ToUnixTimeSeconds(), },
            new GlobalDictionaryDataEntity { Id = 1300000000110, DictionaryTypeId = 1300000000101, Value = "常量选择器", Code = "ConstSelector", OrderNo = 100, Remark = "常量选择器", IsEnabled = true, CreatedTime = DateTimeOffset.Parse("2022-02-10 00:00:00").ToUnixTimeSeconds(), },
            new GlobalDictionaryDataEntity { Id = 1300000000111, DictionaryTypeId = 1300000000101, Value = "枚举选择器", Code = "EnumSelector", OrderNo = 100, Remark = "枚举选择器", IsEnabled = true, CreatedTime = DateTimeOffset.Parse("2022-02-10 00:00:00").ToUnixTimeSeconds(), },

            new GlobalDictionaryDataEntity { Id = 1300000000201, DictionaryTypeId = 1300000000102, Value = "等于", Code = "==", OrderNo = 1, Remark = "等于", IsEnabled = true, CreatedTime = DateTimeOffset.Parse("2022-02-10 00:00:00").ToUnixTimeSeconds(), },
            new GlobalDictionaryDataEntity { Id = 1300000000202, DictionaryTypeId = 1300000000102, Value = "模糊", Code = "like", OrderNo = 1, Remark = "模糊", IsEnabled = true, CreatedTime = DateTimeOffset.Parse("2022-02-10 00:00:00").ToUnixTimeSeconds(), },
            new GlobalDictionaryDataEntity { Id = 1300000000203, DictionaryTypeId = 1300000000102, Value = "大于", Code = ">", OrderNo = 1, Remark = "大于", IsEnabled = true, CreatedTime = DateTimeOffset.Parse("2022-02-10 00:00:00").ToUnixTimeSeconds(), },
            new GlobalDictionaryDataEntity { Id = 1300000000204, DictionaryTypeId = 1300000000102, Value = "小于", Code = "<", OrderNo = 1, Remark = "小于", IsEnabled = true, CreatedTime = DateTimeOffset.Parse("2022-02-10 00:00:00").ToUnixTimeSeconds(), },
            new GlobalDictionaryDataEntity { Id = 1300000000205, DictionaryTypeId = 1300000000102, Value = "不等于", Code = "!=", OrderNo = 1, Remark = "不等于", IsEnabled = true, CreatedTime = DateTimeOffset.Parse("2022-02-10 00:00:00").ToUnixTimeSeconds(), },
            new GlobalDictionaryDataEntity { Id = 1300000000206, DictionaryTypeId = 1300000000102, Value = "大于等于", Code = ">=", OrderNo = 1, Remark = "大于等于", IsEnabled = true, CreatedTime = DateTimeOffset.Parse("2022-02-10 00:00:00").ToUnixTimeSeconds(), },
            new GlobalDictionaryDataEntity { Id = 1300000000207, DictionaryTypeId = 1300000000102, Value = "小于等于", Code = "<=", OrderNo = 1, Remark = "小于等于", IsEnabled = true, CreatedTime = DateTimeOffset.Parse("2022-02-10 00:00:00").ToUnixTimeSeconds(), },
            new GlobalDictionaryDataEntity { Id = 1300000000208, DictionaryTypeId = 1300000000102, Value = "不为空", Code = "isNotNull", OrderNo = 1, Remark = "不为空", IsEnabled = true, CreatedTime = DateTimeOffset.Parse("2022-02-10 00:00:00").ToUnixTimeSeconds(), },
            new GlobalDictionaryDataEntity { Id = 1300000000209, DictionaryTypeId = 1300000000102, Value = "时间范围", Code = "~", OrderNo = 1, Remark = "时间范围", IsEnabled = true, CreatedTime = DateTimeOffset.Parse("2022-02-10 00:00:00").ToUnixTimeSeconds(), },

            new GlobalDictionaryDataEntity { Id = 1300000000301, DictionaryTypeId = 1300000000103, Value = "long", Code = "long", OrderNo = 1, Remark = "long", IsEnabled = true, CreatedTime = DateTimeOffset.Parse("2022-02-10 00:00:00").ToUnixTimeSeconds(), },
            new GlobalDictionaryDataEntity { Id = 1300000000302, DictionaryTypeId = 1300000000103, Value = "string", Code = "string", OrderNo = 1, Remark = "string", IsEnabled = true, CreatedTime = DateTimeOffset.Parse("2022-02-10 00:00:00").ToUnixTimeSeconds(), },
            new GlobalDictionaryDataEntity { Id = 1300000000303, DictionaryTypeId = 1300000000103, Value = "DateTime", Code = "DateTime", OrderNo = 1, Remark = "DateTime", IsEnabled = true, CreatedTime = DateTimeOffset.Parse("2022-02-10 00:00:00").ToUnixTimeSeconds(), },
            new GlobalDictionaryDataEntity { Id = 1300000000304, DictionaryTypeId = 1300000000103, Value = "bool", Code = "bool", OrderNo = 1, Remark = "bool", IsEnabled = true, CreatedTime = DateTimeOffset.Parse("2022-02-10 00:00:00").ToUnixTimeSeconds(), },
            new GlobalDictionaryDataEntity { Id = 1300000000305, DictionaryTypeId = 1300000000103, Value = "int", Code = "int", OrderNo = 1, Remark = "int", IsEnabled = true, CreatedTime = DateTimeOffset.Parse("2022-02-10 00:00:00").ToUnixTimeSeconds(), },
            new GlobalDictionaryDataEntity { Id = 1300000000306, DictionaryTypeId = 1300000000103, Value = "double", Code = "double", OrderNo = 1, Remark = "double", IsEnabled = true, CreatedTime = DateTimeOffset.Parse("2022-02-10 00:00:00").ToUnixTimeSeconds(), },
            new GlobalDictionaryDataEntity { Id = 1300000000307, DictionaryTypeId = 1300000000103, Value = "float", Code = "float", OrderNo = 1, Remark = "float", IsEnabled = true, CreatedTime = DateTimeOffset.Parse("2022-02-10 00:00:00").ToUnixTimeSeconds(), },
            new GlobalDictionaryDataEntity { Id = 1300000000308, DictionaryTypeId = 1300000000103, Value = "decimal", Code = "decimal", OrderNo = 1, Remark = "decimal", IsEnabled = true, CreatedTime = DateTimeOffset.Parse("2022-02-10 00:00:00").ToUnixTimeSeconds(), },
            new GlobalDictionaryDataEntity { Id = 1300000000309, DictionaryTypeId = 1300000000103, Value = "Guid", Code = "Guid", OrderNo = 1, Remark = "Guid", IsEnabled = true, CreatedTime = DateTimeOffset.Parse("2022-02-10 00:00:00").ToUnixTimeSeconds(), },
            new GlobalDictionaryDataEntity { Id = 1300000000310, DictionaryTypeId = 1300000000103, Value = "DateTimeOffset", Code = "DateTimeOffset", OrderNo = 1, Remark = "DateTimeOffset", IsEnabled = true, CreatedTime = DateTimeOffset.Parse("2022-02-10 00:00:00").ToUnixTimeSeconds(), },

            new GlobalDictionaryDataEntity { Id = 1300000000401, DictionaryTypeId = 1300000000104, Value = "下载压缩包", Code = "100", OrderNo = 1, Remark = "下载压缩包", IsEnabled = true, CreatedTime = DateTimeOffset.Parse("2022-02-10 00:00:00").ToUnixTimeSeconds(), },
            new GlobalDictionaryDataEntity { Id = 1300000000402, DictionaryTypeId = 1300000000104, Value = "下载压缩包(前端)", Code = "111", OrderNo = 2, Remark = "下载压缩包(前端)", IsEnabled = true, CreatedTime = DateTimeOffset.Parse("2022-02-10 00:00:00").ToUnixTimeSeconds(), },
            new GlobalDictionaryDataEntity { Id = 1300000000403, DictionaryTypeId = 1300000000104, Value = "下载压缩包(后端)", Code = "121", OrderNo = 3, Remark = "下载压缩包(后端)", IsEnabled = true, CreatedTime = DateTimeOffset.Parse("2022-02-10 00:00:00").ToUnixTimeSeconds(), },
            new GlobalDictionaryDataEntity { Id = 1300000000404, DictionaryTypeId = 1300000000104, Value = "生成到本项目", Code = "200", OrderNo = 4, Remark = "生成到本项目", IsEnabled = true, CreatedTime = DateTimeOffset.Parse("2022-02-10 00:00:00").ToUnixTimeSeconds(), },
            new GlobalDictionaryDataEntity { Id = 1300000000405, DictionaryTypeId = 1300000000104, Value = "生成到本项目(前端)", Code = "211", OrderNo = 5, Remark = "生成到本项目(前端)", IsEnabled = true, CreatedTime = DateTimeOffset.Parse("2022-02-10 00:00:00").ToUnixTimeSeconds(), },
            new GlobalDictionaryDataEntity { Id = 1300000000406, DictionaryTypeId = 1300000000104, Value = "生成到本项目(后端)", Code = "221", OrderNo = 6, Remark = "生成到本项目(后端)", IsEnabled = true, CreatedTime = DateTimeOffset.Parse("2022-02-10 00:00:00").ToUnixTimeSeconds(), },

            new GlobalDictionaryDataEntity { Id = 1300000000501, DictionaryTypeId = 1300000000105, Value = "EntityBaseId【基础实体Id】", Code = "EntityBaseId", OrderNo = 1, Remark = "【基础实体Id】", IsEnabled = false, CreatedTime = DateTimeOffset.Parse("2022-02-10 00:00:00").ToUnixTimeSeconds(), },
            new GlobalDictionaryDataEntity { Id = 1300000000502, DictionaryTypeId = 1300000000105, Value = "EntityBase【基础实体】", Code = "EntityBase", OrderNo = 1, Remark = "【基础实体】", IsEnabled = false, CreatedTime = DateTimeOffset.Parse("2022-02-10 00:00:00").ToUnixTimeSeconds(), },
            new GlobalDictionaryDataEntity { Id = 1300000000503, DictionaryTypeId = 1300000000105, Value = "EntityTenantId【租户实体Id】", Code = "EntityTenantId", OrderNo = 1, Remark = "【租户实体Id】", IsEnabled = false, CreatedTime = DateTimeOffset.Parse("2022-02-10 00:00:00").ToUnixTimeSeconds(), },
            new GlobalDictionaryDataEntity { Id = 1300000000504, DictionaryTypeId = 1300000000105, Value = "EntityTenant【租户实体】", Code = "EntityTenant", OrderNo = 1, Remark = "【租户实体】", IsEnabled = false, CreatedTime = DateTimeOffset.Parse("2022-02-10 00:00:00").ToUnixTimeSeconds(), },
            new GlobalDictionaryDataEntity { Id = 1300000000505, DictionaryTypeId = 1300000000105, Value = "EntityBaseData【业务实体】", Code = "EntityBaseData", OrderNo = 1, Remark = "【业务实体】", IsEnabled = false, CreatedTime = DateTimeOffset.Parse("2022-02-10 00:00:00").ToUnixTimeSeconds(), },
            new GlobalDictionaryDataEntity { Id = 1300000000506, DictionaryTypeId = 1300000000105, Value = "无基类", Code = "", OrderNo = 1, Remark = "【无基类】", IsEnabled = false, CreatedTime = DateTimeOffset.Parse("2022-02-10 00:00:00").ToUnixTimeSeconds(), },

            new GlobalDictionaryDataEntity { Id = 1300000000601, DictionaryTypeId = 1300000000106, Value = "发行中心", Code = "publisher", OrderNo = 100, Remark = "游戏发行或运营主体", IsEnabled = true, CreatedTime = DateTimeOffset.Parse("2023-02-10 00:00:00").ToUnixTimeSeconds(), },
            new GlobalDictionaryDataEntity { Id = 1300000000602, DictionaryTypeId = 1300000000106, Value = "游戏工作室", Code = "studio", OrderNo = 101, Remark = "游戏研发或项目工作室", IsEnabled = true, CreatedTime = DateTimeOffset.Parse("2023-02-10 00:00:00").ToUnixTimeSeconds(), },
            new GlobalDictionaryDataEntity { Id = 1300000000603, DictionaryTypeId = 1300000000106, Value = "项目组", Code = "project", OrderNo = 102, Remark = "单款游戏或业务项目组", IsEnabled = true, CreatedTime = DateTimeOffset.Parse("2023-02-10 00:00:00").ToUnixTimeSeconds(), },
            new GlobalDictionaryDataEntity { Id = 1300000000604, DictionaryTypeId = 1300000000106, Value = "区服分组", Code = "server_group", OrderNo = 103, Remark = "游戏大区、区服或跨服运营分组", IsEnabled = true, CreatedTime = DateTimeOffset.Parse("2023-02-10 00:00:00").ToUnixTimeSeconds(), },
            new GlobalDictionaryDataEntity { Id = 1300000000605, DictionaryTypeId = 1300000000106, Value = "渠道运营", Code = "channel_ops", OrderNo = 104, Remark = "渠道、平台、投放和联运运营团队", IsEnabled = true, CreatedTime = DateTimeOffset.Parse("2023-02-10 00:00:00").ToUnixTimeSeconds(), },
            new GlobalDictionaryDataEntity { Id = 1300000000606, DictionaryTypeId = 1300000000106, Value = "客服运营", Code = "support_ops", OrderNo = 105, Remark = "客服、社群和玩家服务团队", IsEnabled = true, CreatedTime = DateTimeOffset.Parse("2023-02-10 00:00:00").ToUnixTimeSeconds(), },
        };
    }
}