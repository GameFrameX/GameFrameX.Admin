using GameFrameX.Core.Authorization;

namespace GameFrameX.Core.Tests.Authorization;

public sealed class EndpointAuthorizeTests
{
    [Fact]
    public void HasAnyBoundMenu_NullInputs_ReturnsFalse()
    {
        Assert.False(EndpointAuthorize.HasAnyBoundMenu(null, new[] { 1L, }));
        Assert.False(EndpointAuthorize.HasAnyBoundMenu(new[] { 1L, }, null));
    }

    [Fact]
    public void HasAnyBoundMenu_EmptyBound_ReturnsFalse()
    {
        Assert.False(EndpointAuthorize.HasAnyBoundMenu(new List<long>(), new[] { 1L, 2L, }));
    }

    [Fact]
    public void HasAnyBoundMenu_EmptyOwn_ReturnsFalse()
    {
        Assert.False(EndpointAuthorize.HasAnyBoundMenu(new[] { 1L, 2L, }, new List<long>()));
    }

    [Fact]
    public void HasAnyBoundMenu_Disjoint_ReturnsFalse()
    {
        Assert.False(EndpointAuthorize.HasAnyBoundMenu(new[] { 1L, 2L, }, new[] { 3L, 4L, }));
    }

    [Fact]
    public void HasAnyBoundMenu_Intersection_ReturnsTrue()
    {
        Assert.True(EndpointAuthorize.HasAnyBoundMenu(new[] { 1L, 2L, 3L, }, new[] { 3L, }));
    }

    [Fact]
    public void HasAnyBoundMenu_AnyBoundMatchSuffices()
    {
        Assert.True(EndpointAuthorize.HasAnyBoundMenu(new[] { 5L, 6L, }, new[] { 6L, }));
    }

    [Fact]
    public void HasAnyBoundMenu_HashSetInputPerformsWell()
    {
        var bound = new HashSet<long> { 10L, 20L, };
        var own = new HashSet<long> { 20L, };
        Assert.True(EndpointAuthorize.HasAnyBoundMenu(bound, own));
    }
}