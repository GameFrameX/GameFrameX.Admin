using System.Reflection;
using GameFrameX.Application.Game.Mail.GameMailCampaign;
using GameFrameX.Application.Game.Mail.GameMailCampaign.Dto;
using GameFrameX.Entity.Game.Mail;

namespace GameFrameX.Core.Tests.MailCampaign;

/// <summary>
/// 运营邮件 Campaign Admin API 契约自检（C134 / GFX-141）。
/// 编译期即 failing-without-implementation（服务/DTO/枚举类型不存在则编译失败）；运行期反射断言路由方法、
/// 过滤字段、枚举值域不漂移。跨仓库字段对齐铁律由 <see cref="MailCampaignServerClientTests"/> 通过
/// 序列化 JSON 键名二次锚定。
/// </summary>
public class MailCampaignEntityServiceContractTests
{
    [Fact]
    public void Service_Exposes_Required_Crud_And_Campaign_Actions()
    {
        var methodNames = typeof(TenantGameMailCampaignEntityService)
                          .GetMethods(BindingFlags.Public | BindingFlags.Instance)
                          .Select(m => m.Name)
                          .ToHashSet();

        // 通用 CRUD
        Assert.Contains("PageAsync", methodNames);
        Assert.Contains("AddAsync", methodNames);
        Assert.Contains("UpdateAsync", methodNames);
        Assert.Contains("DeleteAsync", methodNames);
        // Campaign 专属动作
        Assert.Contains("PreviewAsync", methodNames);
        Assert.Contains("PublishAsync", methodNames);
        Assert.Contains("RevokeAsync", methodNames);
    }

    [Fact]
    public void PageInput_Carries_Required_Filter_Fields()
    {
        var props = PropNames<PageTenantGameMailCampaignEntityRequest>();

        Assert.Contains("AppId", props);
        Assert.Contains("AreaId", props);
        Assert.Contains("Name", props);
        Assert.Contains("MailType", props);
        Assert.Contains("PublishStatus", props);
        Assert.Contains("TemplateId", props);
        Assert.Contains("ServerCampaignId", props);
    }

    [Fact]
    public void AddInput_Carries_Server_Contract_Fields()
    {
        var props = PropNames<AddTenantGameMailCampaignEntityRequest>();

        Assert.Contains("AppId", props);
        Assert.Contains("AreaId", props);
        Assert.Contains("Name", props);
        Assert.Contains("TemplateId", props);
        Assert.Contains("MailType", props);
        Assert.Contains("TitlesJson", props);
        Assert.Contains("ContentsJson", props);
        Assert.Contains("SenderNamesJson", props);
        Assert.Contains("AttachmentsJson", props);
        Assert.Contains("ServerIdsJson", props);
        Assert.Contains("ChannelIdsJson", props);
        Assert.Contains("MinLevel", props);
        Assert.Contains("MaxLevel", props);
        Assert.Contains("ExpireAttachmentPolicy", props);
        Assert.Contains("ExpireTime", props);
        Assert.Contains("Remark", props);
    }

    [Fact]
    public void Output_Carries_Server_Writeback_Fields()
    {
        var props = PropNames<TenantGameMailCampaignEntityResponse>();

        Assert.Contains("PublishStatus", props);
        Assert.Contains("ServerCampaignId", props);
        Assert.Contains("PublishVersion", props);
        Assert.Contains("PublishedTime", props);
        Assert.Contains("RevokedTime", props);
        Assert.Contains("PublishOperator", props);
        Assert.Contains("RevokeOperator", props);
        Assert.Contains("LastServerCode", props);
        Assert.Contains("LastServerMessage", props);
        Assert.Equal(typeof(long?), typeof(TenantGameMailCampaignEntityResponse).GetProperty("RowVersion")?.PropertyType);
    }

    [Fact]
    public void MailType_Enum_Aligns_Server_MailType_Values()
    {
        Assert.Equal(0, (int)MailCampaignMailType.System);
        Assert.Equal(1, (int)MailCampaignMailType.Operation);
        Assert.Equal(2, (int)MailCampaignMailType.Compensation);
    }

    [Fact]
    public void PublishStatus_Enum_Covers_Admin_Lifecycle()
    {
        Assert.Equal(0, (int)MailCampaignPublishStatus.Draft);
        Assert.Equal(1, (int)MailCampaignPublishStatus.Published);
        Assert.Equal(2, (int)MailCampaignPublishStatus.Revoked);
        Assert.Equal(3, (int)MailCampaignPublishStatus.Failed);
    }

    [Fact]
    public void ServerCode_Enum_Covers_Server_Response_Codes()
    {
        Assert.Equal(0, (int)MailCampaignServerCode.Ok);
        Assert.Equal(1, (int)MailCampaignServerCode.HasExist);
        Assert.Equal(2, (int)MailCampaignServerCode.InvalidCampaignParameter);
        Assert.Equal(3, (int)MailCampaignServerCode.CampaignNotFound);
        Assert.Equal(4, (int)MailCampaignServerCode.CampaignAlreadyRevoked);
        Assert.Equal(5, (int)MailCampaignServerCode.InvalidReward);
        Assert.Equal(6, (int)MailCampaignServerCode.Unsupported);
    }

    [Fact]
    public void ExpirePolicy_Enum_Covers_B4_Strategies()
    {
        Assert.Equal(0, (int)MailCampaignExpirePolicy.DiscardUnclaimed);
        Assert.Equal(1, (int)MailCampaignExpirePolicy.KeepUnclaimed);
        Assert.Equal(2, (int)MailCampaignExpirePolicy.AutoClaim);
    }

    private static HashSet<string> PropNames<T>()
    {
        return typeof(T).GetProperties().Select(p => p.Name).ToHashSet();
    }
}