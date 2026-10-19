namespace GameFrameX.Advertisement.Abstract;

/// <summary>
/// 广告回调验签后的有效载荷（验签失败用 <see cref="Invalid"/>）。
/// </summary>
public sealed class VerifiedPayload
{
    public bool IsValid { get; set; }
    public string TransactionId { get; set; } = null!;
    public string AdvertisementUnit { get; set; } = null!;
    public string RewardItem { get; set; } = null!;
    public string RewardAmount { get; set; } = null!;
    public bool IsSandbox { get; set; }
    public long? AdvertisementPackageId { get; set; }

    public static VerifiedPayload Invalid()
    {
        return new VerifiedPayload { IsValid = false, };
    }
}