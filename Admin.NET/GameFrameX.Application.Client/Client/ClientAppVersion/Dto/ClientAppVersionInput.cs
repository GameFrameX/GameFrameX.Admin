namespace GameFrameX.Application.Client.Client.ClientAppVersion.Dto;

    /// <summary>
    /// 游戏版本管理基础输入参数
    /// </summary>
    public class ClientAppVersionBaseInput
    {
        /// <summary>
        /// Channel
        /// </summary>
        public virtual string? Channel { get; set; }
        
        /// <summary>
        /// Announcement
        /// </summary>
        public virtual string? Announcement { get; set; }
        
        /// <summary>
        /// OldVersion
        /// </summary>
        public virtual object? OldVersion { get; set; }
        
        /// <summary>
        /// DownloadUrl
        /// </summary>
        public virtual string? DownloadUrl { get; set; }
        
        /// <summary>
        /// IsForce
        /// </summary>
        public virtual bool? IsForce { get; set; }
        
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
    /// 游戏版本管理分页查询输入参数
    /// </summary>
    public class ClientAppVersionInput : BasePageInput
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
        /// OldVersion
        /// </summary>
        public object? OldVersion { get; set; }
        
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
        /// Description
        /// </summary>
        public string? Description { get; set; }
        
    }

    /// <summary>
    /// 游戏版本管理增加输入参数
    /// </summary>
    public class AddClientAppVersionInput : ClientAppVersionBaseInput
    {
        /// <summary>
        /// Channel
        /// </summary>
        [Required(ErrorMessage = "Channel不能为空")]
        public override string? Channel { get; set; }
        
        /// <summary>
        /// Announcement
        /// </summary>
        [Required(ErrorMessage = "Announcement不能为空")]
        public override string? Announcement { get; set; }
        
        /// <summary>
        /// OldVersion
        /// </summary>
        [Required(ErrorMessage = "OldVersion不能为空")]
        public override object? OldVersion { get; set; }
        
        /// <summary>
        /// DownloadUrl
        /// </summary>
        [Required(ErrorMessage = "DownloadUrl不能为空")]
        public override string? DownloadUrl { get; set; }
        
        /// <summary>
        /// IsForce
        /// </summary>
        [Required(ErrorMessage = "IsForce不能为空")]
        public override bool? IsForce { get; set; }
        
        /// <summary>
        /// Platform
        /// </summary>
        [Required(ErrorMessage = "Platform不能为空")]
        public override string? Platform { get; set; }
        
        /// <summary>
        /// Version
        /// </summary>
        [Required(ErrorMessage = "Version不能为空")]
        public override string? Version { get; set; }
        
        /// <summary>
        /// Package
        /// </summary>
        [Required(ErrorMessage = "Package不能为空")]
        public override string? Package { get; set; }
        
        /// <summary>
        /// Language
        /// </summary>
        [Required(ErrorMessage = "Language不能为空")]
        public override string? Language { get; set; }
        
    }

    /// <summary>
    /// 游戏版本管理删除输入参数
    /// </summary>
    public class DeleteClientAppVersionInput : BaseIdInput
    {
    }

    /// <summary>
    /// 游戏版本管理更新输入参数
    /// </summary>
    public class UpdateClientAppVersionInput : ClientAppVersionBaseInput
    {
        /// <summary>
        /// Id
        /// </summary>
        [Required(ErrorMessage = "Id不能为空")]
        public long Id { get; set; }
        
    }

    /// <summary>
    /// 游戏版本管理主键查询输入参数
    /// </summary>
    public class QueryByIdClientAppVersionInput : DeleteClientAppVersionInput
    {

    }
