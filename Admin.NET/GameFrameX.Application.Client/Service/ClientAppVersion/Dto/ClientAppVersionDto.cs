namespace GameFrameX.Application.Client.Client.ClientAppVersion.Dto;

    /// <summary>
    /// 游戏版本管理输出参数
    /// </summary>
    public class ClientAppVersionDto
    {
        /// <summary>
        /// Id
        /// </summary>
        public long Id { get; set; }
        
        /// <summary>
        /// Channel
        /// </summary>
        public string? Channel { get; set; }
        
        /// <summary>
        /// Announcement
        /// </summary>
        public string? Announcement { get; set; }
        
        /// <summary>
        /// OldVersion
        /// </summary>
        public object? OldVersion { get; set; }
        
        /// <summary>
        /// DownloadUrl
        /// </summary>
        public string? DownloadUrl { get; set; }
        
        /// <summary>
        /// IsForce
        /// </summary>
        public bool? IsForce { get; set; }
        
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
