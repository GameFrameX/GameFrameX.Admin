using GameFrameX.Application.Game.Online.Match;

namespace GameFrameX.Core.Tests.Online.Match;

// C341 / GFX-753（vault:C5）：匹配 Ticket 展示映射契约自检。
// 覆盖：Ticket 状态 6 键 / 失败原因 4 键穷举锁定（严格按 A0 声明顺序，键集合变更须回 change 评审）、
// 已知原值映射、未知原值兜底、null/空值兜底、大小写敏感（Online 原值大小写不同即视为未知，不做宽松匹配）。
public sealed class OnlineMatchTicketDisplayMapTests
{
    [Fact]
    public void TicketStatusDisplayKeys_ExactlySixFrozenValues()
    {
        var expected = new[]
        {
            "onlineMatch.ticketStatus.queued",
            "onlineMatch.ticketStatus.matching",
            "onlineMatch.ticketStatus.matched",
            "onlineMatch.ticketStatus.cancelled",
            "onlineMatch.ticketStatus.expired",
            "onlineMatch.ticketStatus.failed",
        };
        Assert.Equal(expected, OnlineMatchTicketDisplayMap.TicketStatusDisplayKeys);
    }

    [Fact]
    public void FailureReasonDisplayKeys_ExactlyFourFrozenValues()
    {
        var expected = new[]
        {
            "onlineMatch.failureReason.timeout",
            "onlineMatch.failureReason.cancelrace",
            "onlineMatch.failureReason.ratelimited",
            "onlineMatch.failureReason.duplicateticket",
        };
        Assert.Equal(expected, OnlineMatchTicketDisplayMap.FailureReasonDisplayKeys);
    }

    [Theory]
    [InlineData("Queued", "onlineMatch.ticketStatus.queued")]
    [InlineData("Matching", "onlineMatch.ticketStatus.matching")]
    [InlineData("Matched", "onlineMatch.ticketStatus.matched")]
    [InlineData("Cancelled", "onlineMatch.ticketStatus.cancelled")]
    [InlineData("Expired", "onlineMatch.ticketStatus.expired")]
    [InlineData("Failed", "onlineMatch.ticketStatus.failed")]
    public void MapTicketStatus_KnownValues_MapToDisplayKey(string status, string expectedKey)
    {
        Assert.Equal(expectedKey, OnlineMatchTicketDisplayMap.MapTicketStatusToDisplayKey(status));
    }

    [Theory]
    [InlineData("Timeout", "onlineMatch.failureReason.timeout")]
    [InlineData("CancelRace", "onlineMatch.failureReason.cancelrace")]
    [InlineData("RateLimited", "onlineMatch.failureReason.ratelimited")]
    [InlineData("DuplicateTicket", "onlineMatch.failureReason.duplicateticket")]
    public void MapFailureReason_KnownValues_MapToDisplayKey(string reasonCode, string expectedKey)
    {
        Assert.Equal(expectedKey, OnlineMatchTicketDisplayMap.MapFailureReasonToDisplayKey(reasonCode));
    }

    [Theory]
    [InlineData("UnknownStatus")]
    [InlineData("")]
    [InlineData(null)]
    public void MapTicketStatus_UnknownOrNull_FallsBackToUnknownKey(string? status)
    {
        Assert.Equal("onlineMatch.ticketStatus.unknown", OnlineMatchTicketDisplayMap.MapTicketStatusToDisplayKey(status));
    }

    [Theory]
    [InlineData("SomeNewReason")]
    [InlineData("")]
    [InlineData(null)]
    public void MapFailureReason_UnknownOrNull_FallsBackToUnknownKey(string? reasonCode)
    {
        Assert.Equal("onlineMatch.failureReason.unknown", OnlineMatchTicketDisplayMap.MapFailureReasonToDisplayKey(reasonCode));
    }

    [Fact]
    public void MapTicketStatus_IsCaseSensitive_LowercaseInputFallsBack()
    {
        Assert.Equal("onlineMatch.ticketStatus.unknown", OnlineMatchTicketDisplayMap.MapTicketStatusToDisplayKey("queued"));
        Assert.Equal("onlineMatch.ticketStatus.unknown", OnlineMatchTicketDisplayMap.MapTicketStatusToDisplayKey("FAILED"));
    }

    [Fact]
    public void MapFailureReason_IsCaseSensitive_LowercaseInputFallsBack()
    {
        Assert.Equal("onlineMatch.failureReason.unknown", OnlineMatchTicketDisplayMap.MapFailureReasonToDisplayKey("timeout"));
        Assert.Equal("onlineMatch.failureReason.unknown", OnlineMatchTicketDisplayMap.MapFailureReasonToDisplayKey("ratelimited"));
    }
}
