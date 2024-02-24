namespace GameFrameX.Application.Client.Client.ClientChannel.Dto;

    /// <summary>
    /// 渠道基础输入参数
    /// </summary>
    public class ClientChannelBaseInput
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
    /// 渠道分页查询输入参数
    /// </summary>
    public class ClientChannelInput : BasePageInput
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
    /// 渠道增加输入参数
    /// </summary>
    public class AddClientChannelInput : ClientChannelBaseInput
    {
        /// <summary>
        /// 名称
        /// </summary>
        [Required(ErrorMessage = "名称不能为空")]
        public override string? Name { get; set; }
        
    }

    /// <summary>
    /// 渠道删除输入参数
    /// </summary>
    public class DeleteClientChannelInput : BaseIdInput
    {
    }

    /// <summary>
    /// 渠道更新输入参数
    /// </summary>
    public class UpdateClientChannelInput : ClientChannelBaseInput
    {
        /// <summary>
        /// Id
        /// </summary>
        [Required(ErrorMessage = "Id不能为空")]
        public long Id { get; set; }
        
    }

    /// <summary>
    /// 渠道主键查询输入参数
    /// </summary>
    public class QueryByIdClientChannelInput : DeleteClientChannelInput
    {

    }
