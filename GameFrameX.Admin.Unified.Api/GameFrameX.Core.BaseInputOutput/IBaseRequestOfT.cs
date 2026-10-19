using GameFrameX.Foundation.Orm.Entity;

namespace GameFrameX.Core.BaseInputOutput;

public interface IBaseRequest<T> : IBaseRequest where T : EntityBaseId, new()
{
}