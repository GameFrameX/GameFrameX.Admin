namespace GameFrameX.Application.Client.Client.ClientChannel.Dto;

    /// <summary>
    /// 渠道输出参数
    /// </summary>
    public class ClientChannelDto
    {
        /// <summary>
        /// Id
        /// </summary>
        public long Id { get; set; }
        
        /// <summary>
        /// 名称
        /// </summary>
        public string? Name { get; set; }
        
        /// <summary>
        /// 描述
        /// </summary>
        public string? Description { get; set; }
        
        /// <summary>
        /// CreateUserName
        /// </summary>
        public string? CreateUserName { get; set; }
        
        /// <summary>
        /// UpdateUserName
        /// </summary>
        public string? UpdateUserName { get; set; }
        
    }
