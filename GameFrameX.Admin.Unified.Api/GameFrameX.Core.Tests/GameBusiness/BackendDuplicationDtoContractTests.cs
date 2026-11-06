using System.ComponentModel.DataAnnotations;
using System.Globalization;
using System.Reflection;
using GameFrameX.Application.ClientDelivery.GameAppRemoteConfig.Dto;
using GameFrameX.Application.ClientDelivery.GameAppReleaseVersion.Dto;
using GameFrameX.Application.NoticeCenter.GameAnnouncement.Dto;
using GameFrameX.Application.NoticeCenter.GameMailTemplate.Dto;
using GameFrameX.Core.BaseInputOutput;

namespace GameFrameX.Core.Tests.GameBusiness;

public class BackendDuplicationDtoContractTests
{
    private static readonly string[] RemoteConfigEditFields =
    {
        "ActiveModeCode",
        "AotCodeList",
        "AppVersion",
        "Channel",
        "CheckAppVersionUrl",
        "CheckResourceUrl",
        "Content",
        "Description",
        "DeviceWhitelistJson",
        "Language",
        "ModeContents",
        "Package",
        "Platform",
        "Priority",
        "RolloutPercent",
    };

    private static readonly string[] ReleaseVersionEditFields =
    {
        "Announcement",
        "AppVersion",
        "Channel",
        "Description",
        "DownloadUrl",
        "IsForce",
        "Language",
        "OldVersion",
        "Package",
        "Platform",
        "Title",
    };


    private static readonly string[] MailTemplateSharedFields =
    {
        "BodyTemplate",
        "CategoryId",
        "DefaultAttachmentsJson",
        "DefaultExpirationDays",
        "DefaultMailType",
        "Description",
        "IsEnabled",
        "Name",
        "Sort",
        "SubTitleTemplate",
        "TitleTemplate",
    };

    private static readonly string[] AnnouncementSharedFields =
    {
        "AreaId",
        "Content",
        "ContentLocalizationKey",
        "EndTime",
        "IsActive",
        "Language",
        "Priority",
        "StartTime",
        "Title",
        "TitleLocalizationKey",
    };

    [Fact]
    public void RemoteConfig_UpdateKeepsExistingFieldSetWithoutAuditNames()
    {
        AssertContainsExactly<AddGameAppRemoteConfigRequest>(RemoteConfigEditFields.Concat(new[] { "CreatedName", "UpdatedName", }));
        AssertContainsExactly<UpdateGameAppRemoteConfigRequest>(RemoteConfigEditFields.Concat(UpdateContractFields));
        Assert.DoesNotContain("CreatedName", PublicPropertyNames<UpdateGameAppRemoteConfigRequest>());
        Assert.DoesNotContain("UpdatedName", PublicPropertyNames<UpdateGameAppRemoteConfigRequest>());
        AssertUpdateContract<UpdateGameAppRemoteConfigRequest>();
    }

    [Fact]
    public void ReleaseVersion_UpdateKeepsExistingFieldSetWithoutAuditNames()
    {
        AssertContainsExactly<AddGameAppReleaseVersionRequest>(ReleaseVersionEditFields.Concat(new[] { "CreatedName", "UpdatedName", }));
        AssertContainsExactly<UpdateGameAppReleaseVersionRequest>(ReleaseVersionEditFields.Concat(UpdateContractFields));
        Assert.DoesNotContain("CreatedName", PublicPropertyNames<UpdateGameAppReleaseVersionRequest>());
        Assert.DoesNotContain("UpdatedName", PublicPropertyNames<UpdateGameAppReleaseVersionRequest>());
        AssertUpdateContract<UpdateGameAppReleaseVersionRequest>();
    }


    [Fact]
    public void GameBusinessDtos_ShareEditFieldsAndKeepUpdateContract()
    {
        AssertContainsExactly<AddGameMailTemplateEntityRequest>(MailTemplateSharedFields.Concat(new[] { "Id", }));
        AssertContainsExactly<UpdateGameMailTemplateEntityRequest>(MailTemplateSharedFields.Concat(UpdateContractFields));
        AssertUpdateContract<UpdateGameMailTemplateEntityRequest>();

        AssertContainsExactly<AddGameAnnouncementEntityRequest>(AnnouncementSharedFields);
        AssertContainsExactly<UpdateGameAnnouncementEntityRequest>(AnnouncementSharedFields.Concat(UpdateContractFields));
        AssertUpdateContract<UpdateGameAnnouncementEntityRequest>();
    }

    [Fact]
    public void SharedDtoFields_AreDeclaredOnceInTheirCommonBase()
    {
        AssertDeclaredOn<GameAppRemoteConfigEditRequest, UpdateGameAppRemoteConfigRequest>(RemoteConfigEditFields);
        AssertDeclaredOn<GameAppReleaseVersionEditRequest, UpdateGameAppReleaseVersionRequest>(ReleaseVersionEditFields);
        AssertDeclaredOn<GameMailTemplateBaseRequest, UpdateGameMailTemplateEntityRequest>(MailTemplateSharedFields);
        AssertDeclaredOn<GameAnnouncementBaseRequest, UpdateGameAnnouncementEntityRequest>(AnnouncementSharedFields);
    }

    private static readonly string[] UpdateContractFields = { "Id", "RowVersion", };

    private static void AssertContainsExactly<T>(IEnumerable<string> expected)
    {
        Assert.Equal(
            expected.OrderBy(name => name, StringComparer.Ordinal).ToArray(),
            PublicPropertyNames<T>());
    }

    private static string[] PublicPropertyNames<T>()
    {
        return typeof(T)
               .GetProperties(BindingFlags.Instance | BindingFlags.Public)
               .Select(property => property.Name)
               .OrderBy(name => name, StringComparer.Ordinal)
               .ToArray();
    }

    private static void AssertDeclaredOn<TBase, TDerived>(IEnumerable<string> propertyNames)
    {
        foreach (var propertyName in propertyNames)
        {
            var property = typeof(TDerived).GetProperty(propertyName);
            Assert.NotNull(property);
            Assert.Equal(typeof(TBase), property!.DeclaringType);
        }
    }

    private static void AssertUpdateContract<T>() where T : ICrudUpdateRequest
    {
        AssertRequired<T>("Id");
        AssertRange<T>("Id", 1, long.MaxValue);
        AssertRequired<T>("RowVersion");
        AssertRange<T>("RowVersion", 0, long.MaxValue);
    }

    private static void AssertRequired<T>(string propertyName)
    {
        Assert.NotNull(GetProperty<T>(propertyName).GetCustomAttribute<RequiredAttribute>(true));
    }

    private static void AssertNotRequired<T>(string propertyName)
    {
        Assert.Null(GetProperty<T>(propertyName).GetCustomAttribute<RequiredAttribute>(true));
    }

    private static void AssertRange<T>(string propertyName, long minimum, long maximum)
    {
        var attribute = GetProperty<T>(propertyName).GetCustomAttribute<RangeAttribute>(true);
        Assert.NotNull(attribute);
        Assert.Equal((double)minimum, Convert.ToDouble(attribute!.Minimum, CultureInfo.InvariantCulture));
        Assert.Equal((double)maximum, Convert.ToDouble(attribute.Maximum, CultureInfo.InvariantCulture));
    }

    private static PropertyInfo GetProperty<T>(string propertyName)
    {
        var property = typeof(T).GetProperty(propertyName);
        Assert.NotNull(property);
        return property!;
    }
}