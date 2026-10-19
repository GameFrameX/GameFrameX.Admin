using GameFrameX.Application.SeedData.System;
using GameFrameX.Core.Const.MenuSeed;

namespace GameFrameX.Core.Tests.System;

[Collection("MenuSeedCatalog")]
public class OperationsOrganizationSeedDataTests
{
    [Fact]
    public void TenantOrganizationSeedData_UsesGameOperationsOrganizationStructure()
    {
        var orgs = new TenantOrganizationSeedData().HasData().ToList();
        var rootOrg = orgs.Single(org => org.Code == "GFX-PUBLISHER");

        Assert.Equal("发行运营中心", rootOrg.Name);
        Assert.Equal("publisher", rootOrg.Type);
        Assert.Contains(orgs, org => org.Name == "雷霆工作室" && org.Pid == rootOrg.Id && org.Type == "studio");
        Assert.Contains(orgs, org => org.Name == "国服运营大区" && org.Type == "server_group");
        Assert.Contains(orgs, org => org.Name == "渠道运营组" && org.Type == "channel_ops");
        Assert.Contains(orgs, org => org.Name == "玩家服务组" && org.Type == "support_ops");
    }

    [Fact]
    public void GlobalDictionarySeedData_UsesGameOperationsOrganizationTypes()
    {
        var dictionaryType = new GlobalDictionaryTypeSeedData().HasData()
                                                   .Single(type => type.Code == "org_type");
        var dictionaryData = new GlobalDictionaryDataSeedData().HasData()
                                                   .Where(data => data.DictionaryTypeId == dictionaryType.Id)
                                                   .ToDictionary(data => data.Code, data => data.Value, StringComparer.Ordinal);

        Assert.Equal("运营组织类型", dictionaryType.Name);
        Assert.Equal("发行中心", dictionaryData["publisher"]);
        Assert.Equal("游戏工作室", dictionaryData["studio"]);
        Assert.Equal("项目组", dictionaryData["project"]);
        Assert.Equal("区服分组", dictionaryData["server_group"]);
        Assert.Equal("渠道运营", dictionaryData["channel_ops"]);
        Assert.Equal("客服运营", dictionaryData["support_ops"]);
    }

    [Fact]
    public void DbMenuSeedSource_RenamesTenantOrganizationMenuForGameOperations()
    {
        MenuSeedCatalogTestData.LoadSampleMenus();
        var systemOrganizationMenu = MenuSeedCatalog.Menus
                                           .Single(menu => string.Equals(menu.Name, "systemOrg", StringComparison.Ordinal));

        Assert.Equal("组织管理", systemOrganizationMenu.Title);
    }
}