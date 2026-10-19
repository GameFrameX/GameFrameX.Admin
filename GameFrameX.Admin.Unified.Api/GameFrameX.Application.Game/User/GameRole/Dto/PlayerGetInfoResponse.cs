namespace GameFrameX.Application.Game.User.GameRole.Dto;

/// <summary>
/// 获取玩家信息响应
/// </summary>
public sealed class PlayerGetInfoResponse
{
    /// <summary>
    /// 角色信息
    /// </summary>
    public PlayerDataResponse PlayerInfo { get; set; } = new();

    /// <summary>
    /// 在线时长秒
    /// </summary>
    public long OnlineTime { get; set; }

    /// <summary>
    /// 角色数据
    /// </summary>
    public sealed class PlayerDataResponse
    {
        /// <summary>
        /// 角色ID
        /// </summary>
        public long Id { get; set; }

        /// <summary>
        /// 角色名
        /// </summary>
        public string Name { get; set; } = string.Empty;

        /// <summary>
        /// 角色等级
        /// </summary>
        public uint Level { get; set; }

        /// <summary>
        /// 角色状态
        /// </summary>
        public int State { get; set; }

        /// <summary>
        /// 角色头像
        /// </summary>
        public uint Avatar { get; set; }

        /// <summary>
        /// 角色当前经验
        /// </summary>
        public ulong CurrentExp { get; set; }

        /// <summary>
        /// 当前关卡配置id
        /// </summary>
        public int CurrentLevelConfigId { get; set; }

        /// <summary>
        /// 是否是沙盒角色
        /// </summary>
        public bool IsSandbox { get; set; }

        /// <summary>
        /// 性别
        /// </summary>
        public bool Gender { get; set; }

        /// <summary>
        /// 角色头像框
        /// </summary>
        public uint AvatarFrame { get; set; }
    }
}