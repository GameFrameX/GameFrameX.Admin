/**
 * 配置灰度发布枚举选项（C343 / GFX-759 前端半边）。
 * 值域对齐后端 OnlineConfigRolloutKind（RemoteConfig=1/Announcement=2/DeviceGroup=3/ScheduledTask=4；
 * 邮件不参与灰度编排——后端枚举即无此值，前端不造第五项）。
 */

// 配置族选项（ConfigKey = 对应域实体 Id 字符串）
export const ONLINE_CONFIG_ROLLOUT_KIND_OPTIONS = [
  { value: 1, labelKey: 'message.onlineCenter.configRollout.kindRemoteConfig' },
  { value: 2, labelKey: 'message.onlineCenter.configRollout.kindAnnouncement' },
  { value: 3, labelKey: 'message.onlineCenter.configRollout.kindDeviceGroup' },
  { value: 4, labelKey: 'message.onlineCenter.configRollout.kindScheduledTask' },
];
