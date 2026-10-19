using GameFrameX.Application.Game.Goods.GameCommodity;
using GameFrameX.Core.Enum.Errors;
using GameFrameX.Core.Exceptions;

namespace GameFrameX.Core.Tests.GameBusiness;

/// <summary>
/// C391（GFX-923 / Api#291）：CRUD 基类复杂入参 null 兜底。
/// body 为字面 null 等场景绑定不产生 ModelState 错误、参数静默为 null——
/// 基类统一抛 ParameterValidationFailed（400 语义），替代首次解引用的 500 NRE。
/// </summary>
public class GenericCrudServiceNullInputGuardTests
{
    [Fact]
    public async Task PageAsync_NullInput_ThrowsParameterValidationFailed()
    {
        var service = new TenantGameCommodityEntityService(null!, null!);

        var exception = await Assert.ThrowsAsync<BusinessException>(() => service.PageAsync(null!));

        Assert.Equal((int)CommonErrorCode.ParameterValidationFailed, exception.Code);
    }

    [Fact]
    public async Task AddAsync_NullInput_ThrowsParameterValidationFailed()
    {
        var service = new TenantGameCommodityEntityService(null!, null!);

        var exception = await Assert.ThrowsAsync<BusinessException>(() => service.AddAsync(null!));

        Assert.Equal((int)CommonErrorCode.ParameterValidationFailed, exception.Code);
    }

    [Fact]
    public async Task UpdateAsync_NullInput_ThrowsParameterValidationFailed()
    {
        var service = new TenantGameCommodityEntityService(null!, null!);

        var exception = await Assert.ThrowsAsync<BusinessException>(() => service.UpdateAsync(null!));

        Assert.Equal((int)CommonErrorCode.ParameterValidationFailed, exception.Code);
    }
}
