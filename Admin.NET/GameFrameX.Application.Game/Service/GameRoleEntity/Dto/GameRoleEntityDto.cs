namespace GameFrameX.Application.Game;

    /// <summary>
    /// 游戏角色输出参数
    /// </summary>
    public class GameRoleEntityDto
    {
        /// <summary>
        /// 主键Id
        /// </summary>
        public long Id { get; set; }
        
        /// <summary>
        /// 用户ID
        /// </summary>
        public long UserId { get; set; }
        
        /// <summary>
        /// 区服Id
        /// </summary>
        public long AreaId { get; set; }
        
        /// <summary>
        /// 名称
        /// </summary>
        public string Name { get; set; }
        
        /// <summary>
        /// 详细描述
        /// </summary>
        public string Description { get; set; }
        
        /// <summary>
        /// 角色当前的等级
        /// </summary>
        public long? Level { get; set; }
        
        /// <summary>
        /// 角色最后一次登录时间
        /// </summary>
        public DateTime? LastLoginTime { get; set; }
        
    }
