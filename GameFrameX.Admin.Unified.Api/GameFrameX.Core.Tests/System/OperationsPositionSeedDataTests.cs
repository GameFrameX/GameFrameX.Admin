using GameFrameX.Application.SeedData.System;

namespace GameFrameX.Core.Tests.System;

public class OperationsPositionSeedDataTests
{
    [Fact]
    public void TenantPositionSeedData_UsesGameEnterpriseJobStructure()
    {
        var positions = new TenantPositionSeedData().HasData().ToList();

        Assert.Contains(positions, pos => pos.Name == "总经理" && pos.Code == "general_manager");
        Assert.Contains(positions, pos => pos.Name == "运营负责人" && pos.Code == "ops_director");
        Assert.Contains(positions, pos => pos.Name == "技术负责人" && pos.Code == "tech_director");
        Assert.Contains(positions, pos => pos.Name == "游戏策划" && pos.Code == "game_designer");
        Assert.Contains(positions, pos => pos.Name == "服务端工程师" && pos.Code == "server_engineer");
        Assert.Contains(positions, pos => pos.Name == "客服专员" && pos.Code == "support_specialist");
        Assert.DoesNotContain(positions, pos => pos.Name.Contains("党委") || pos.Name.Contains("局长") || pos.Name.Contains("科长"));
    }
}