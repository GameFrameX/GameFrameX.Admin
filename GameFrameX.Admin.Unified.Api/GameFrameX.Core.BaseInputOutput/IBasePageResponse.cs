using GameFrameX.Foundation.Orm.Entity;

namespace GameFrameX.Core.BaseInputOutput;

public interface IBasePageResponse<T> : IBaseResponse where T : EntityBaseId, new()
{
}