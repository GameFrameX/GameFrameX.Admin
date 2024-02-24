namespace GameFrameX.Application.Client.Client.ClientPackage.Dto;

    /// <summary>
    /// 包管理基础输入参数
    /// </summary>
    public class ClientPackageBaseInput
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
    /// 包管理分页查询输入参数
    /// </summary>
    public class ClientPackageInput : BasePageInput
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
    /// 包管理增加输入参数
    /// </summary>
    public class AddClientPackageInput : ClientPackageBaseInput
    {
        /// <summary>
        /// 名称
        /// </summary>
        [Required(ErrorMessage = "名称不能为空")]
        public override string? Name { get; set; }
        
    }

    /// <summary>
    /// 包管理删除输入参数
    /// </summary>
    public class DeleteClientPackageInput : BaseIdInput
    {
    }

    /// <summary>
    /// 包管理更新输入参数
    /// </summary>
    public class UpdateClientPackageInput : ClientPackageBaseInput
    {
        /// <summary>
        /// Id
        /// </summary>
        [Required(ErrorMessage = "Id不能为空")]
        public long Id { get; set; }
        
    }

    /// <summary>
    /// 包管理主键查询输入参数
    /// </summary>
    public class QueryByIdClientPackageInput : DeleteClientPackageInput
    {

    }
