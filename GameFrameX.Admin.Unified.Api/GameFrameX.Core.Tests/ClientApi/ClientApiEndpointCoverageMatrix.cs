using System.Collections.ObjectModel;

namespace GameFrameX.Core.Tests.ClientApi;

internal static class ClientApiEndpointCoverageMatrix
{
    private static readonly ClientApiEndpointCoverage[] Endpoints =
    {
        Endpoint("GameAdWebHook.TopOnNotify", "GET", "api/GameAdWebHook/TopOnNotify/{tenantId:long}/{appId:long}/{packageId:long?}", "ad-callback", true, true, false, true, "valid TopOn callback grants reward once", "invalid-route", "signature-failed", "duplicate-callback", "cross-package"),
        Endpoint("GameAdWebHook.PangleGlobalNotify", "GET", "api/GameAdWebHook/PangleGlobalNotify/{tenantId:long}/{appId:long}/{packageId:long?}", "ad-callback", true, true, false, true, "valid Pangle callback grants reward once", "invalid-route", "signature-failed", "duplicate-callback", "cross-app"),
        Endpoint("GameAdWebHook.AdMobNotify", "GET", "api/GameAdWebHook/AdMobNotify/{tenantId:long}/{appId:long}/{packageId:long?}", "ad-callback", true, true, false, true, "valid AdMob callback grants reward once", "invalid-route", "signature-failed", "duplicate-callback", "cross-tenant"),
        Endpoint("GameAdWebHook.UnityAdsNotify", "GET", "api/GameAdWebHook/UnityAdsNotify/{tenantId:long}/{appId:long}/{packageId:long?}", "ad-callback", true, true, false, true, "valid UnityAds callback grants reward once", "invalid-route", "signature-failed", "duplicate-callback"),

        Endpoint("GameAnnouncement.List", "GET", "api/TenantGameAnnouncementEntityService/List", "announcement", true, true, false, false, "lists enabled announcements by language", "missing-tenant", "disabled-record-hidden"),
        Endpoint("GameAnnouncement.ListByAreaId", "POST", "api/TenantGameAnnouncementEntityService/ListByAreaId", "announcement", true, true, false, false, "lists enabled area announcements", "missing-tenant", "area-not-found"),

        Endpoint("GameAppVersion.GetInfo", "POST", "api/GameAppVersion/GetInfo", "client-config", true, true, false, false, "matches release version by app/package/platform/channel", "missing-app", "not-found", "disabled-record-hidden", "fallback-boundary"),
        Endpoint("GameArea.GetAreaList", "POST", "api/GameArea/GetAreaList", "area", true, true, false, false, "returns visible area tree", "missing-app", "closed-area-filter", "package-fallback"),
        Endpoint("GameAssetPackageVersion.GetInfo", "POST", "api/GameAssetPackageVersion/GetInfo", "client-config", true, true, false, false, "matches asset package version", "missing-app", "not-found", "disabled-record-hidden", "package-boundary"),
        Endpoint("GameAssetPackageVersion.RegisterDevice", "POST", "api/GameAssetPackageVersion/RegisterDevice", "client-config", true, true, false, false, "registers device idempotently by app and identifier", "missing-app", "empty-identifier"),
        Endpoint("GameFeatureSwitch.GetInfo", "POST", "api/GameFeatureSwitch/GetInfo", "client-config", true, true, false, false, "returns versioned feature switches with unchanged short-circuit", "missing-tenant", "missing-app", "audience-server-hidden", "out-of-window-hidden", "rollout-miss", "pascal-case"),
        Endpoint("GameRemoteConfig.GetInfo", "POST", "api/GameRemoteConfig/GetInfo", "client-config", true, true, false, false, "returns rollout-matched global info", "missing-app", "rollout-miss", "whitelist-boundary", "pascal-case"),
        Endpoint("GameLoginTicketKey.PublicKeys", "GET", "api/GameLoginTicketKey/PublicKeys", "ticket", true, true, false, false, "returns tenant-app public keys only", "missing-tenant", "missing-app", "private-key-not-returned"),

        Endpoint("GameOrder.Create", "POST", "api/GameOrder/Create", "payment", true, true, false, false, "creates order and payment payload", "missing-app", "area-not-found", "role-not-found", "unsupported-channel", "amount-boundary"),
        Endpoint("GameOrder.NotifyAliPay", "POST", "api/GameOrder/NotifyAliPay/{tenantId:long}/{appId:long}/{packageId:long?}", "payment-callback", true, true, false, true, "valid AliPay notify ships once", "invalid-route", "signature-failed", "order-not-found", "amount-mismatch", "duplicate-callback"),
        Endpoint("GameOrder.NotifyApple", "POST", "api/GameOrder/NotifyApple/{tenantId:long}/{appId:long}/{packageId:long?}", "payment-callback", true, true, false, true, "valid Apple notification ships once", "invalid-route", "signature-failed", "order-not-found", "duplicate-callback", "refund-boundary"),
        Endpoint("GameOrder.NotifyGoogle", "POST", "api/GameOrder/NotifyGoogle/{tenantId:long}/{appId:long}/{packageId:long?}", "payment-callback", true, true, false, true, "valid Google Pub/Sub ships once", "invalid-route", "jwt-failed", "purchase-invalid", "order-not-found", "duplicate-callback"),
        Endpoint("GameOrder.NotifyWeChatPay", "POST", "api/GameOrder/NotifyWeChatPay/{tenantId:long}/{appId:long}/{packageId:long?}", "payment-callback", true, true, false, true, "valid WeChat Pay callback ships once", "invalid-route", "signature-failed", "amount-mismatch", "duplicate-callback"),
        Endpoint("GameOrder.NotifyXsolla", "POST", "api/GameOrder/NotifyXsolla/{tenantId:long}/{appId:long}/{packageId:long?}", "payment-callback", true, true, false, true, "valid Xsolla webhook ships once", "invalid-route", "signature-failed", "notification-unsupported", "order-not-found", "duplicate-callback"),

        Endpoint("GameRedeemCode.Consume", "POST", "api/GameRedeemCode/Consume", "redeem-code", true, true, false, false, "consumes active code once and sends item", "missing-app", "code-not-found", "not-active", "expired", "limit-exhausted", "already-used", "area-not-found"),

        Endpoint("GameRole.ChangePlayerName", "POST", "api/GameRole/ChangePlayerName", "role", true, true, false, false, "changes owned role name", "role-not-found", "duplicate-name", "invalid-name"),
        Endpoint("GameRole.Add", "POST", "api/GameRole/Add", "role", true, true, false, false, "creates role in open area", "area-not-found", "duplicate-role", "area-closed", "area-full"),
        Endpoint("GameRole.Delete", "POST", "api/GameRole/Delete", "role", true, true, false, false, "deletes owned role", "role-not-found", "cross-user-role"),
        Endpoint("GameRole.Update", "POST", "api/GameRole/Update", "role", true, true, false, false, "updates owned role snapshot", "role-not-found", "rowversion-conflict"),
        Endpoint("GameRole.GetList", "POST", "api/GameRole/GetList", "role", true, true, false, false, "lists roles for user", "missing-user", "cross-app-filter"),
        Endpoint("GameRole.Login", "POST", "api/GameRole/Login", "ticket", true, true, true, false, "issues short-lived game login ticket", "forbidden-context-field", "invalid-access-token", "role-not-found", "cross-user-role", "area-closed", "ticket-ttl"),
        Endpoint("GameRole.Get", "POST", "api/GameRole/Get", "role", true, true, false, false, "gets role by filter", "role-not-found", "cross-app-filter"),
        Endpoint("GameRole.GetById", "POST", "api/GameRole/GetById", "role", true, true, false, false, "gets role by id", "role-not-found", "cross-user-role"),

        Endpoint("GameUser.DeviceLogin", "POST", "api/GameUser/DeviceLogin", "account", true, true, false, false, "logs in or registers device account and returns AccessToken", "account-deleting", "account-disabled", "pascal-case"),
        Endpoint("GameUser.WeChatLogin", "POST", "api/GameUser/WeChatLogin", "account", true, true, false, false, "logs in WeChat account and returns AccessToken", "account-deleting", "account-disabled", "third-party-invalid", "pascal-case"),
        Endpoint("GameUser.BindWeChat", "POST", "api/GameUser/BindWeChat", "account", true, true, false, false, "binds WeChat account", "account-not-found", "already-bound-other-user"),
        Endpoint("GameUser.UnbindWeChatGameUser", "POST", "api/GameUser/UnbindWeChatGameUser", "account", true, true, false, false, "unbinds WeChat account", "account-not-found", "not-bound", "password-error"),
        Endpoint("GameUser.QQLogin", "POST", "api/GameUser/QQLogin", "account", true, true, false, false, "logs in QQ account and returns AccessToken", "account-deleting", "account-disabled", "third-party-invalid", "pascal-case"),
        Endpoint("GameUser.BindQQ", "POST", "api/GameUser/BindQQ", "account", true, true, false, false, "binds QQ account", "account-not-found", "already-bound-other-user"),
        Endpoint("GameUser.UnbindQQGameUser", "POST", "api/GameUser/UnbindQQGameUser", "account", true, true, false, false, "unbinds QQ account", "account-not-found", "not-bound", "password-error"),
        Endpoint("GameUser.FacebookLogin", "POST", "api/GameUser/FacebookLogin", "account", true, true, false, false, "logs in Facebook account and returns AccessToken", "account-deleting", "account-disabled", "third-party-invalid", "pascal-case"),
        Endpoint("GameUser.BindFacebook", "POST", "api/GameUser/BindFacebook", "account", true, true, false, false, "binds Facebook account", "account-not-found", "already-bound-other-user"),
        Endpoint("GameUser.UnbindFacebookGameUser", "POST", "api/GameUser/UnbindFacebookGameUser", "account", true, true, false, false, "unbinds Facebook account", "account-not-found", "not-bound", "password-error"),
        Endpoint("GameUser.EmailReset", "POST", "api/GameUser/EmailReset", "account", true, true, false, false, "resets email password with valid code", "email-code-invalid", "account-not-found"),
        Endpoint("GameUser.EmailRegister", "POST", "api/GameUser/EmailRegister", "account", true, true, false, false, "registers email account and returns AccessToken", "email-exists", "email-code-invalid", "pascal-case"),
        Endpoint("GameUser.EmailLogin", "POST", "api/GameUser/EmailLogin", "account", true, true, false, false, "logs in email account and returns AccessToken", "password-error", "account-disabled", "account-deleting", "pascal-case"),
        Endpoint("GameUser.BindEmail", "POST", "api/GameUser/BindEmail", "account", true, true, false, false, "binds email account", "account-not-found", "email-bound-other-user", "email-code-invalid"),
        Endpoint("GameUser.UnbindEmailGameUser", "POST", "api/GameUser/UnbindEmailGameUser", "account", true, true, false, false, "unbinds email account", "account-not-found", "not-bound", "password-error"),
        Endpoint("GameUser.Register", "POST", "api/GameUser/Register", "account", true, true, false, false, "registers guest account and returns AccessToken", "account-exists", "pascal-case"),
        Endpoint("GameUser.Login", "POST", "api/GameUser/Login", "account", true, true, false, false, "logs in password account and returns AccessToken", "password-error", "account-disabled", "account-deleting", "pascal-case"),
        Endpoint("GameUser.Get", "POST", "api/GameUser/Get", "account", true, true, true, false, "gets account by valid AccessToken subject", "invalid-access-token", "account-not-found", "cross-user-token"),
        Endpoint("GameUser.BindAppleGameUser", "POST", "api/GameUser/BindAppleGameUser", "account", true, true, false, false, "binds Apple account", "account-not-found", "apple-bound-other-user"),
        Endpoint("GameUser.AppleLogin", "POST", "api/GameUser/AppleLogin", "account", true, true, false, false, "logs in Apple account and returns AccessToken", "account-disabled", "apple-token-invalid", "pascal-case"),
        Endpoint("GameUser.UnbindAppleGameUser", "POST", "api/GameUser/UnbindAppleGameUser", "account", true, true, false, false, "unbinds Apple account", "account-not-found", "not-bound", "password-error"),
        Endpoint("GameUser.BindGoogleGameUser", "POST", "api/GameUser/BindGoogleGameUser", "account", true, true, false, false, "binds Google account", "account-not-found", "google-bound-other-user"),
        Endpoint("GameUser.GoogleLogin", "POST", "api/GameUser/GoogleLogin", "account", true, true, false, false, "logs in Google account and returns AccessToken", "account-disabled", "google-token-invalid", "pascal-case"),
        Endpoint("GameUser.UnbindGoogleGameUser", "POST", "api/GameUser/UnbindGoogleGameUser", "account", true, true, false, false, "unbinds Google account", "account-not-found", "not-bound", "password-error"),
        Endpoint("GameUser.Unbind", "POST", "api/GameUser/Unbind", "account", true, true, true, false, "unbinds provider from AccessToken subject", "invalid-access-token", "provider-not-bound", "cross-user-token"),
        Endpoint("GameUser.LogOff", "POST", "api/GameUser/LogOff", "account", true, true, true, false, "marks account deleting by AccessToken subject", "invalid-access-token", "account-not-found", "already-deleting"),
        Endpoint("GameUser.ReActive", "POST", "api/GameUser/ReActive", "account", true, true, false, false, "reactivates deleting account", "account-not-found", "not-deleting", "password-error"),

        Endpoint("GameUserArchive.Upload", "POST", "api/GameUserArchive/Upload", "archive", true, true, false, false, "upserts owned archive snapshot", "archive-sync-conflict"),
        Endpoint("GameUserArchive.Download", "POST", "api/GameUserArchive/Download", "archive", true, true, false, false, "downloads owned archive snapshot", "archive-not-found"),
        Endpoint("GameUserArchive.GetOtherProgress", "POST", "api/GameUserArchive/GetOtherProgress", "archive", true, true, false, false, "returns other players progress summary", "progress-not-found"),

        Endpoint("GameUserEmail.SendRegister", "POST", "api/GameUserEmail/SendRegister", "email-code", true, true, false, false, "sends register code through tenant email", "missing-app", "rate-limited", "daily-limit", "tenant-email-missing"),
        Endpoint("GameUserEmail.VerifyCode", "POST", "api/GameUserEmail/VerifyCode", "email-code", true, true, false, false, "verifies active code", "code-not-found", "code-expired"),
        Endpoint("GameUserEmail.SendReset", "POST", "api/GameUserEmail/SendReset", "email-code", true, true, false, false, "sends reset code for existing account", "missing-app", "account-not-found", "rate-limited", "tenant-email-missing"),

        Endpoint("GameUserMisc.MarkLogout", "POST", "api/GameUserMisc/MarkLogout", "misc", true, true, false, false, "marks user logout state", "account-not-found"),
        Endpoint("GameUserMisc.RandomRobots", "POST", "api/GameUserMisc/RandomRobots", "misc", true, true, false, false, "returns random robot users excluding caller", "empty-robot-pool"),

        Endpoint("GameUserPay.Create", "POST", "api/GameUserPay/Create", "pay", true, true, false, false, "creates pay order and returns payload", "order-exists", "sandbox-direct-ship"),
        Endpoint("GameUserPay.Verify", "POST", "api/GameUserPay/Verify", "pay", true, true, false, false, "verifies receipt and ships order once", "receipt-invalid", "transaction-bound-other-order", "duplicate-callback"),
        Endpoint("GameUserPay.Query", "POST", "api/GameUserPay/Query", "pay", true, true, false, false, "queries order pay state", "order-not-found"),
        Endpoint("GameUserPay.Report", "POST", "api/GameUserPay/Report", "pay", true, true, false, false, "reports payment flow as ledger record", "order-not-found"),

        Endpoint("GameUserRace.Entry", "POST", "api/GameUserRace/Entry", "race", true, true, false, false, "enters user into race and returns entry state", "round-not-found", "already-joined"),

        Endpoint("GameUserRank.JoinRank", "POST", "api/GameUserRank/JoinRank", "rank", true, true, false, false, "joins rank group with entry tokens", "round-not-open", "already-joined"),
        Endpoint("GameUserRank.Query", "POST", "api/GameUserRank/Query", "rank", true, true, false, false, "queries rank board snapshot", "round-not-found"),
        Endpoint("GameUserRank.HeartBeat", "POST", "api/GameUserRank/HeartBeat", "rank", true, true, false, false, "refreshes rank heartbeat scores", "token-invalid"),

        Endpoint("GameUserReward.GetUserRewards", "POST", "api/GameUserReward/GetUserRewards", "reward", true, true, false, false, "lists user rewards by keys", "reward-key-not-found"),
        Endpoint("GameUserReward.ReceiveRewards", "POST", "api/GameUserReward/ReceiveRewards", "reward", true, true, false, false, "receives pending reward once", "reward-not-found", "already-received"),

        Endpoint("GameUserSession.Login", "POST", "api/GameUserSession/Login", "session", true, true, false, false, "establishes login session and returns heartbeat intervals", "client-version-blocked", "account-deleting", "account-not-found"),

        Endpoint("GameWebShop.Get", "GET", "api/GameWebShop/Get", "web-shop", true, true, false, false, "lists enabled webshop goods", "missing-tenant", "disabled-goods-hidden"),
        Endpoint("GameWebShop.Buy", "POST", "api/GameWebShop/Buy", "web-shop", true, true, false, false, "creates webshop buy request", "role-not-found", "area-not-found", "area-cannot-buy", "goods-not-found", "notify-url-context-missing"),
        Endpoint("GameWebShop.NotifyWebShop", "GET", "api/GameWebShop/NotifyWebShop/{tenantId:long}/{appId:long}/{packageId:long?}", "web-shop-callback", true, true, false, true, "valid webshop notify sends mail once", "invalid-route", "signature-failed", "role-not-found", "goods-not-found", "duplicate-callback"),
    };

    public static IReadOnlyDictionary<string, ClientApiEndpointCoverage> ByKey { get; } =
        new ReadOnlyDictionary<string, ClientApiEndpointCoverage>(
            Endpoints.ToDictionary(endpoint => endpoint.Key, StringComparer.Ordinal));

    private static ClientApiEndpointCoverage Endpoint(
        string key,
        string httpMethod,
        string route,
        string group,
        bool requiresTenant,
        bool requiresApp,
        bool requiresAccessToken,
        bool isPublicCallback,
        string successCase,
        params string[] failureCases)
    {
        return new ClientApiEndpointCoverage(
            key,
            httpMethod,
            route,
            group,
            requiresTenant,
            requiresApp,
            requiresAccessToken,
            isPublicCallback,
            successCase,
            failureCases);
    }
}