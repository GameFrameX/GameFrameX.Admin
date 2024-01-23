using Admin.NET.Core;
using System.ComponentModel.DataAnnotations;

namespace Admin.NET.Application;

    /// <summary>
    /// 版本管理基础输入参数
    /// </summary>
    public class ClientVersionBaseInput
    {
        /// <summary>
        /// 名称
        /// </summary>
        public virtual string? Name { get; set; }
        
        /// <summary>
        /// 描述
        /// </summary>
        public virtual string? Description { get; set; }
        
        /// <summary>
        /// CreateUserName
        /// </summary>
        public virtual string? CreateUserName { get; set; }
        
        /// <summary>
        /// UpdateUserName
        /// </summary>
        public virtual string? UpdateUserName { get; set; }
        
    }

    /// <summary>
    /// 版本管理分页查询输入参数
    /// </summary>
    public class ClientVersionInput : BasePageInput
    {
        /// <summary>
        /// 关键字查询
        /// </summary>
        public string? SearchKey { get; set; }

        /// <summary>
        /// 名称
        /// </summary>
        public string? Name { get; set; }
        
    }

    /// <summary>
    /// 版本管理增加输入参数
    /// </summary>
    public class AddClientVersionInput : ClientVersionBaseInput
    {
        /// <summary>
        /// 名称
        /// </summary>
        [Required(ErrorMessage = "名称不能为空")]
        public override string? Name { get; set; }
        
    }

    /// <summary>
    /// 版本管理删除输入参数
    /// </summary>
    public class DeleteClientVersionInput : BaseIdInput
    {
    }

    /// <summary>
    /// 版本管理更新输入参数
    /// </summary>
    public class UpdateClientVersionInput : ClientVersionBaseInput
    {
        /// <summary>
        /// Id
        /// </summary>
        [Required(ErrorMessage = "Id不能为空")]
        public long Id { get; set; }
        
    }

    /// <summary>
    /// 版本管理主键查询输入参数
    /// </summary>
    public class QueryByIdClientVersionInput : DeleteClientVersionInput
    {

    }
