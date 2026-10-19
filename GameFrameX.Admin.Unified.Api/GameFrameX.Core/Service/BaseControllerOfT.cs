using GameFrameX.Foundation.Orm.Entity;

namespace GameFrameX.Core.Service;

/// <summary>
///
/// </summary>
/// <typeparam name="T"></typeparam>
public class BaseController<T> : BaseController where T : EntityBase, new()
{
    /// <summary>
    ///
    /// </summary>
    protected readonly SqlSugarRepository<T> PRepository;

    /// <summary>
    ///
    /// </summary>
    /// <param name="service"></param>
    public BaseController(SqlSugarRepository<T> service)
    {
        PRepository = service;
    }
}