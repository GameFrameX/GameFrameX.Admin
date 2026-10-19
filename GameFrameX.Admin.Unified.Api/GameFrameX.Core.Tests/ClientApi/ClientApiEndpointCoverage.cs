namespace GameFrameX.Core.Tests.ClientApi;

internal sealed record ClientApiEndpointCoverage(
    string Key,
    string HttpMethod,
    string Route,
    string Group,
    bool RequiresTenant,
    bool RequiresApp,
    bool RequiresAccessToken,
    bool IsPublicCallback,
    string SuccessCase,
    IReadOnlyCollection<string> FailureCases);