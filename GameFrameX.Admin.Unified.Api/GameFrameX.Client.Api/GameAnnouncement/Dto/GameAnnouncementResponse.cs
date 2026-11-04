using GameFrameX.Core.BaseInputOutput;
using GameFrameX.Entity.Tenant.Game.Announcement;

namespace GameFrameX.Client.Api.GameAnnouncement.Dto;

/// <summary>
/// 游戏公告输出参数
/// </summary>
public class TenantGameAnnouncementEntityResponse : TenantGameAnnouncementEntity, IBasePageResponse<TenantGameAnnouncementEntity>
{
}