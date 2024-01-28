using GameFrameX.Entity;
using Microsoft.AspNetCore.Mvc;

namespace GameFrameX.Web.Api
{
    /// <summary>
    /// 
    /// </summary>
    [Route("api/[controller]/[action]")]
    [ApiController]
    public class BaseController : Controller
    {
    }

    /// <summary>
    /// 
    /// </summary>
    /// <typeparam name="T"></typeparam>
    public class BaseController<T> : BaseController where T : EntityBase, new()
    {
        /// <summary>
        /// 
        /// </summary>
        protected BaseService<T> PBaseService;

        /// <summary>
        /// 
        /// </summary>
        /// <param name="service"></param>
        public BaseController(BaseService<T> service)
        {
            this.PBaseService = service;
        }
    }
}