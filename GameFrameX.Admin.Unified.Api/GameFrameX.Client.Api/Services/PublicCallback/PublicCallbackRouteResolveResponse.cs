namespace GameFrameX.Client.Api.Services.PublicCallback;

public sealed record PublicCallbackRouteResolveResponse(bool IsPublicCallback, bool IsSuccess, PublicCallbackRouteContext? Context, string? ErrorMessage)
{
    public static PublicCallbackRouteResolveResponse NotPublicCallback()
    {
        return new PublicCallbackRouteResolveResponse(false, false, null, null);
    }

    public static PublicCallbackRouteResolveResponse Success(PublicCallbackRouteContext context)
    {
        return new PublicCallbackRouteResolveResponse(true, true, context, null);
    }

    public static PublicCallbackRouteResolveResponse Fail(string errorMessage)
    {
        return new PublicCallbackRouteResolveResponse(true, false, null, errorMessage);
    }
}