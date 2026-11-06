using GameFrameX.Core.Tests.ClientApi;
using GameFrameX.Entity.Business.Game.App;
using GameFrameX.Entity.Business.Game.Release;
using GameFrameX.Hub.Api.Controllers;
using GameFrameX.Hub.Api.Dto.PackageBuildReport;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;

namespace GameFrameX.Core.Tests.HubApi;

public sealed class PackageBuildReportSingleMainTests
{
    [Theory]
    [InlineData("asset")]
    [InlineData("release")]
    public async Task Report_WithoutLegacyScope_UpdatesMainDatabaseVersion(string reportType)
    {
        using (var database = new ClientApiTestDb())
        {
            database.SeedPackage(1, "com.example.game");
            var asset = database.SeedAssetPackageVersion(2, "com.example.game");
            var release = database.SeedAppVersion(3, "com.example.game");
            var controller = CreateController(database);
            var request = new PackageBuildReportRequest
            {
                PackageId = 1,
                ReportType = reportType,
                AppVersion = reportType == "asset" ? asset.AppVersion : release.AppVersion,
                AssetPackageName = asset.AssetPackageName,
                Language = reportType == "asset" ? asset.Language : release.Language,
                ResourceVersion = "new-resource",
                RootPath = "https://resources.example.test/new",
                DownloadUrl = "https://release.example.test/new",
            };

            var result = ClientApiResultReader.Read(await controller.Report(request));

            Assert.Equal(0, ClientApiResultReader.Code(result));
            Assert.Equal(1, database.MainDb.Queryable<GameAppAssetPackageVersionEntity>().Count());
            Assert.Equal(1, database.MainDb.Queryable<GameAppReleaseVersionEntity>().Count());
            if (reportType == "asset")
            {
                Assert.Equal("new-resource", database.MainDb.Queryable<GameAppAssetPackageVersionEntity>().First().ResourceVersion);
            }
            else
            {
                Assert.Equal(request.DownloadUrl, database.MainDb.Queryable<GameAppReleaseVersionEntity>().First().DownloadUrl);
            }
        }
    }

    [Theory]
    [InlineData(false)]
    [InlineData(true)]
    public async Task Report_MissingOrDisabledPackage_IsRejected(bool seedDisabledPackage)
    {
        using (var database = new ClientApiTestDb())
        {
            database.InitBusinessTables(typeof(GameAppPackageEntity), typeof(GameAppAssetPackageVersionEntity), typeof(GameAppReleaseVersionEntity));
            if (seedDisabledPackage)
            {
                var package = database.SeedPackage(1, "com.example.game");
                package.IsEnabled = false;
                database.MainDb.Updateable(package).ExecuteCommand();
            }

            var controller = CreateController(database);
            var result = ClientApiResultReader.Read(await controller.Report(new PackageBuildReportRequest { PackageId = 1, ReportType = "asset" }));

            Assert.NotEqual(0, ClientApiResultReader.Code(result));
            Assert.Equal(0, database.MainDb.Queryable<GameAppAssetPackageVersionEntity>().Count());
        }
    }

    private static PackageBuildReportController CreateController(ClientApiTestDb database)
    {
        return new PackageBuildReportController(database.Repository<GameAppPackageEntity>(),
            database.Repository<GameAppAssetPackageVersionEntity>(), database.Repository<GameAppReleaseVersionEntity>())
        {
            ControllerContext = new ControllerContext { HttpContext = new DefaultHttpContext() }
        };
    }
}
