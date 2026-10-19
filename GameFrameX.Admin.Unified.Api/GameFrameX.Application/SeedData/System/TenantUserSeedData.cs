using GameFrameX.Core.Enum;
using GameFrameX.Core.SqlSugar;
using GameFrameX.Core.Utility;
using GameFrameX.Entity.Enum;

namespace GameFrameX.Application.SeedData.System;

/// <summary>
/// 租户用户表种子数据。
/// </summary>
public class TenantUserSeedData : ISqlSugarEntitySeedData<TenantUserEntity>
{
    /// <summary>
    /// 种子数据
    /// </summary>
    /// <returns></returns>
    public IEnumerable<TenantUserEntity> HasData()
    {
        var encryptPasswod = CryptogramUtility.HashPassword("123456");
        return BuildTenantData(encryptPasswod);
    }

    /// <summary>
    /// 构建默认租户用户种子数据。
    /// </summary>
    /// <param name="encryptPasswod"></param>
    /// <returns></returns>
    public static IEnumerable<TenantUserEntity> BuildTenantData(string encryptPasswod)
    {
        return BuildData(encryptPasswod);
    }

    /// <summary>
    /// 构建种子数据
    /// </summary>
    /// <param name="encryptPasswod"></param>
    /// <returns></returns>
    public static IEnumerable<TenantUserEntity> BuildData(string encryptPasswod)
    {
        return new[]
        {
            new TenantUserEntity { Id = 1300000000111, Account = "admin", Password = encryptPasswod, NickName = "系统管理员", RealName = "系统管理员", Avatar = "", Phone = "13884998275", BirthdayTime = new DateTimeOffset(DateTime.Parse("1986-06-28")).ToUnixTimeSeconds(), Sex = GenderEnum.Male, AccountType = AccountTypeEnum.SystemAdmin, Remark = "系统管理员", CreatedTime = DateTimeOffset.Parse("2022-02-10 00:00:00").ToUnixTimeSeconds(), OrganizationId = 1300000000101, PosId = 1300000000103, TenantId = 1300000000001, UpdateCount = 0, Nation = "汉", IdCardNum = "", Email = "", Address = "", PoliticalOutlook = "", College = "", OfficePhone = "", EmergencyContact = "", EmergencyPhone = "", EmergencyAddress = "", Introduction = "", JobNum = "", PosLevel = "", PosTitle = "", Expertise = "", OfficeZone = "", Office = "", LastLoginIp = "", LastLoginAddress = "", LastLoginDevice = "", Signature = "", },
            new TenantUserEntity { Id = 1300000000112, Account = "ops_manager", Password = encryptPasswod, NickName = "运营负责人", RealName = "运营负责人", Avatar = "", Phone = "13884998275", BirthdayTime = new DateTimeOffset(DateTime.Parse("1986-06-28")).ToUnixTimeSeconds(), Sex = GenderEnum.Female, AccountType = AccountTypeEnum.NormalUser, Remark = "运营负责人", CreatedTime = DateTimeOffset.Parse("2022-02-10 00:00:00").ToUnixTimeSeconds(), OrganizationId = 1300000000104, PosId = 1300000000102, TenantId = 1300000000001, UpdateCount = 0, Nation = "汉", IdCardNum = "", Email = "", Address = "", PoliticalOutlook = "", College = "", OfficePhone = "", EmergencyContact = "", EmergencyPhone = "", EmergencyAddress = "", Introduction = "", JobNum = "", PosLevel = "", PosTitle = "", Expertise = "", OfficeZone = "", Office = "", LastLoginIp = "", LastLoginAddress = "", LastLoginDevice = "", Signature = "", },
            new TenantUserEntity { Id = 1300000000113, Account = "gm_support", Password = encryptPasswod, NickName = "客服GM", RealName = "客服GM", Avatar = "", Phone = "13884998275", BirthdayTime = new DateTimeOffset(DateTime.Parse("1986-06-28")).ToUnixTimeSeconds(), Sex = GenderEnum.Female, AccountType = AccountTypeEnum.NormalUser, Remark = "客服GM", CreatedTime = DateTimeOffset.Parse("2022-02-10 00:00:00").ToUnixTimeSeconds(), OrganizationId = 1300000000106, PosId = 1300000000114, TenantId = 1300000000001, UpdateCount = 0, Nation = "汉", IdCardNum = "", Email = "", Address = "", PoliticalOutlook = "", College = "", OfficePhone = "", EmergencyContact = "", EmergencyPhone = "", EmergencyAddress = "", Introduction = "", JobNum = "", PosLevel = "", PosTitle = "", Expertise = "", OfficeZone = "", Office = "", LastLoginIp = "", LastLoginAddress = "", LastLoginDevice = "", Signature = "", },
            new TenantUserEntity { Id = 1300000000114, Account = "content_ops", Password = encryptPasswod, NickName = "内容运营", RealName = "内容运营", Avatar = "", Phone = "13884998275", BirthdayTime = new DateTimeOffset(DateTime.Parse("1986-06-28")).ToUnixTimeSeconds(), Sex = GenderEnum.Female, AccountType = AccountTypeEnum.NormalUser, Remark = "内容运营", CreatedTime = DateTimeOffset.Parse("2022-02-10 00:00:00").ToUnixTimeSeconds(), OrganizationId = 1300000000104, PosId = 1300000000107, TenantId = 1300000000001, UpdateCount = 0, Nation = "汉", IdCardNum = "", Email = "", Address = "", PoliticalOutlook = "", College = "", OfficePhone = "", EmergencyContact = "", EmergencyPhone = "", EmergencyAddress = "", Introduction = "", JobNum = "", PosLevel = "", PosTitle = "", Expertise = "", OfficeZone = "", Office = "", LastLoginIp = "", LastLoginAddress = "", LastLoginDevice = "", Signature = "", },
            new TenantUserEntity { Id = 1300000000115, Account = "server_ops", Password = encryptPasswod, NickName = "区服运维", RealName = "区服运维", Avatar = "", Phone = "13884998275", BirthdayTime = new DateTimeOffset(DateTime.Parse("1986-06-28")).ToUnixTimeSeconds(), Sex = GenderEnum.Female, AccountType = AccountTypeEnum.Member, Remark = "区服运维", CreatedTime = DateTimeOffset.Parse("2022-02-10 00:00:00").ToUnixTimeSeconds(), OrganizationId = 1300000000303, PosId = 1300000000108, TenantId = 1300000000001, UpdateCount = 0, Nation = "汉", IdCardNum = "", Email = "", Address = "", PoliticalOutlook = "", College = "", OfficePhone = "", EmergencyContact = "", EmergencyPhone = "", EmergencyAddress = "", Introduction = "", JobNum = "", PosLevel = "", PosTitle = "", Expertise = "", OfficeZone = "", Office = "", LastLoginIp = "", LastLoginAddress = "", LastLoginDevice = "", Signature = "", },
            new TenantUserEntity { Id = 1300000000116, Account = "payment_ops", Password = encryptPasswod, NickName = "支付运营", RealName = "支付运营", Avatar = "", Phone = "13884998275", BirthdayTime = new DateTimeOffset(DateTime.Parse("1986-06-28")).ToUnixTimeSeconds(), Sex = GenderEnum.Female, AccountType = AccountTypeEnum.NormalUser, Remark = "支付运营", CreatedTime = DateTimeOffset.Parse("2022-02-10 00:00:00").ToUnixTimeSeconds(), OrganizationId = 1300000000101, PosId = 1300000000116, TenantId = 1300000000001, UpdateCount = 0, Nation = "汉", IdCardNum = "", Email = "", Address = "", PoliticalOutlook = "", College = "", OfficePhone = "", EmergencyContact = "", EmergencyPhone = "", EmergencyAddress = "", Introduction = "", JobNum = "", PosLevel = "", PosTitle = "", Expertise = "", OfficeZone = "", Office = "", LastLoginIp = "", LastLoginAddress = "", LastLoginDevice = "", Signature = "", },
            new TenantUserEntity { Id = 1300000000117, Account = "marketing_ops", Password = encryptPasswod, NickName = "市场活动", RealName = "市场活动", Avatar = "", Phone = "13884998275", BirthdayTime = new DateTimeOffset(DateTime.Parse("1986-06-28")).ToUnixTimeSeconds(), Sex = GenderEnum.Female, AccountType = AccountTypeEnum.NormalUser, Remark = "市场活动", CreatedTime = DateTimeOffset.Parse("2022-02-10 00:00:00").ToUnixTimeSeconds(), OrganizationId = 1300000000101, PosId = 1300000000112, TenantId = 1300000000001, UpdateCount = 0, Nation = "汉", IdCardNum = "", Email = "", Address = "", PoliticalOutlook = "", College = "", OfficePhone = "", EmergencyContact = "", EmergencyPhone = "", EmergencyAddress = "", Introduction = "", JobNum = "", PosLevel = "", PosTitle = "", Expertise = "", OfficeZone = "", Office = "", LastLoginIp = "", LastLoginAddress = "", LastLoginDevice = "", Signature = "", },
            new TenantUserEntity { Id = 1300000000118, Account = "data_analyst", Password = encryptPasswod, NickName = "数据分析", RealName = "数据分析", Avatar = "", Phone = "13884998275", BirthdayTime = new DateTimeOffset(DateTime.Parse("1986-06-28")).ToUnixTimeSeconds(), Sex = GenderEnum.Female, AccountType = AccountTypeEnum.NormalUser, Remark = "数据分析", CreatedTime = DateTimeOffset.Parse("2022-02-10 00:00:00").ToUnixTimeSeconds(), OrganizationId = 1300000000101, PosId = 1300000000115, TenantId = 1300000000001, UpdateCount = 0, Nation = "汉", IdCardNum = "", Email = "", Address = "", PoliticalOutlook = "", College = "", OfficePhone = "", EmergencyContact = "", EmergencyPhone = "", EmergencyAddress = "", Introduction = "", JobNum = "", PosLevel = "", PosTitle = "", Expertise = "", OfficeZone = "", Office = "", LastLoginIp = "", LastLoginAddress = "", LastLoginDevice = "", Signature = "", },
            new TenantUserEntity { Id = 1300000000119, Account = "audit_security", Password = encryptPasswod, NickName = "审计安全", RealName = "审计安全", Avatar = "", Phone = "13884998275", BirthdayTime = new DateTimeOffset(DateTime.Parse("1986-06-28")).ToUnixTimeSeconds(), Sex = GenderEnum.Female, AccountType = AccountTypeEnum.NormalUser, Remark = "审计安全", CreatedTime = DateTimeOffset.Parse("2022-02-10 00:00:00").ToUnixTimeSeconds(), OrganizationId = 1300000000101, PosId = 1300000000103, TenantId = 1300000000001, UpdateCount = 0, Nation = "汉", IdCardNum = "", Email = "", Address = "", PoliticalOutlook = "", College = "", OfficePhone = "", EmergencyContact = "", EmergencyPhone = "", EmergencyAddress = "", Introduction = "", JobNum = "", PosLevel = "", PosTitle = "", Expertise = "", OfficeZone = "", Office = "", LastLoginIp = "", LastLoginAddress = "", LastLoginDevice = "", Signature = "", },
            new TenantUserEntity { Id = 1300000000120, Account = "app_config_admin", Password = encryptPasswod, NickName = "应用配置管理员", RealName = "应用配置管理员", Avatar = "", Phone = "13884998275", BirthdayTime = new DateTimeOffset(DateTime.Parse("1986-06-28")).ToUnixTimeSeconds(), Sex = GenderEnum.Female, AccountType = AccountTypeEnum.NormalUser, Remark = "应用配置管理员", CreatedTime = DateTimeOffset.Parse("2022-02-10 00:00:00").ToUnixTimeSeconds(), OrganizationId = 1300000000104, PosId = 1300000000104, TenantId = 1300000000001, UpdateCount = 0, Nation = "汉", IdCardNum = "", Email = "", Address = "", PoliticalOutlook = "", College = "", OfficePhone = "", EmergencyContact = "", EmergencyPhone = "", EmergencyAddress = "", Introduction = "", JobNum = "", PosLevel = "", PosTitle = "", Expertise = "", OfficeZone = "", Office = "", LastLoginIp = "", LastLoginAddress = "", LastLoginDevice = "", Signature = "", },
        };
    }
}