using GameFrameX.Core.BaseInputOutput;
using GameFrameX.Entity.Business.Game.Announcement;

namespace GameFrameX.Client.Api.GameAnnouncement.Dto;

/// <summary>
/// 游戏公告输出参数
/// </summary>
public class GameAnnouncementResponse : GameAnnouncementEntity, IBasePageResponse<GameAnnouncementEntity>
{
}