namespace GameFrameX.Application.Client.Client.ClientGlobalInfo.Dto;

    /// <summary>
    /// 全局信息管理基础输入参数
    /// </summary>
    public class ClientGlobalInfoBaseInput
    {
        /// <summary>
        /// Channel
        /// </summary>
        public virtual string? Channel { get; set; }
        
        /// <summary>
        /// Content
        /// </summary>
        public virtual string? Content { get; set; }
        
        /// <summary>
        /// CheckResourceUrl
        /// </summary>
        public virtual string? CheckResourceUrl { get; set; }
        
        /// <summary>
        /// CheckAppVersionUrl
        /// </summary>
        public virtual string? CheckAppVersionUrl { get; set; }
        
        /// <summary>
        /// Platform
        /// </summary>
        public virtual string? Platform { get; set; }
        
        /// <summary>
        /// Version
        /// </summary>
        public virtual string? Version { get; set; }
        
        /// <summary>
        /// Package
        /// </summary>
        public virtual string? Package { get; set; }
        
        /// <summary>
        /// Language
        /// </summary>
        public virtual string? Language { get; set; }
        
        /// <summary>
        /// CreateUserName
        /// </summary>
        public virtual string? CreateUserName { get; set; }
        
        /// <summary>
        /// UpdateUserName
        /// </summary>
        public virtual string? UpdateUserName { get; set; }
        
        /// <summary>
        /// Description
        /// </summary>
        public virtual string? Description { get; set; }
        
    }

    /// <summary>
    /// 全局信息管理分页查询输入参数
    /// </summary>
    public class ClientGlobalInfoInput : BasePageInput
    {
        /// <summary>
        /// 关键字查询
        /// </summary>
        public string? SearchKey { get; set; }

        /// <summary>
        /// Channel
        /// </summary>
        public string? Channel { get; set; }
        
        /// <summary>
        /// Content
        /// </summary>
        public string? Content { get; set; }
        
        /// <summary>
        /// CheckResourceUrl
        /// </summary>
        public string? CheckResourceUrl { get; set; }
        
        /// <summary>
        /// CheckAppVersionUrl
        /// </summary>
        public string? CheckAppVersionUrl { get; set; }
        
        /// <summary>
        /// Platform
        /// </summary>
        public string? Platform { get; set; }
        
        /// <summary>
        /// Version
        /// </summary>
        public string? Version { get; set; }
        
        /// <summary>
        /// Package
        /// </summary>
        public string? Package { get; set; }
        
        /// <summary>
        /// Language
        /// </summary>
        public string? Language { get; set; }
        
        /// <summary>
        /// CreateUserName
        /// </summary>
        public string? CreateUserName { get; set; }
        
        /// <summary>
        /// UpdateUserName
        /// </summary>
        public string? UpdateUserName { get; set; }
        
        /// <summary>
        /// Description
        /// </summary>
        public string? Description { get; set; }
        
    }

    /// <summary>
    /// 全局信息管理增加输入参数
    /// </summary>
    public class AddClientGlobalInfoInput : ClientGlobalInfoBaseInput
    {
    }

    /// <summary>
    /// 全局信息管理删除输入参数
    /// </summary>
    public class DeleteClientGlobalInfoInput : BaseIdInput
    {
    }

    /// <summary>
    /// 全局信息管理更新输入参数
    /// </summary>
    public class UpdateClientGlobalInfoInput : ClientGlobalInfoBaseInput
    {
        /// <summary>
        /// Id
        /// </summary>
        [Required(ErrorMessage = "Id不能为空")]
        public long Id { get; set; }
        
    }

    /// <summary>
    /// 全局信息管理主键查询输入参数
    /// </summary>
    public class QueryByIdClientGlobalInfoInput : DeleteClientGlobalInfoInput
    {

    }
