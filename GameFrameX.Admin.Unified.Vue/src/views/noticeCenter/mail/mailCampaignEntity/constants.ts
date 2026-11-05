/**
 * 运营邮件 Campaign 枚举与契约类型（前端语义名，对齐后端 C134 / GFX-141）。
 * 生成 types.ts 的同 enum 是 ValueN（数字无名），这里保留语义名供业务可读；
 * 值域（0/1/2...）与后端 C# 枚举一致，运行时传数字给后端。
 */

// 邮件类型（对齐 Server MailType：System=0 / Operation=1 / Compensation=2）
export enum MailCampaignMailType {
  System = 0,
  Operation = 1,
  Compensation = 2,
}

// 未领取附件过期策略（对齐 Server ExpireAttachmentPolicy）
export enum MailCampaignExpirePolicy {
  DiscardUnclaimed = 0,
  KeepUnclaimed = 1,
  AutoClaim = 2,
}

// Admin 侧发布生命周期状态（对齐 C134 MailCampaignPublishStatus）
export enum MailCampaignPublishStatus {
  Draft = 0,
  Published = 1,
  Revoked = 2,
  Failed = 3,
}

// 多语言内容（对齐 Server MailLocalizedContent：Language + Text）
export interface MailLocalizedContent {
  language: string;
  text: string;
}

// 附件（对齐 Server MailAttachmentState：SlotId / RewardType / ItemId / Amount / IconId）
export interface MailCampaignAttachment {
  slotId: number;
  rewardType: number;
  itemId: number;
  amount: number;
  iconId?: number;
}

// Campaign 行（对齐 C134 TenantGameMailCampaignEntityOutput，camelCase）
export interface MailCampaignRow {
  id?: number;
  appId: number;
  areaId: number;
  name: string;
  mailType: number;
  templateId?: number;
  titlesJson?: string;
  contentsJson?: string;
  senderNamesJson?: string;
  attachmentsJson?: string;
  serverIdsJson?: string;
  channelIdsJson?: string;
  minLevel?: number;
  maxLevel?: number;
  expireAttachmentPolicy: number;
  expireTime?: number;
  publishStatus: number;
  serverCampaignId?: number;
  publishVersion?: number;
  publishedTime?: number;
  revokedTime?: number;
  publishOperator?: string;
  revokeOperator?: string;
  lastServerCode?: number;
  lastServerMessage?: string;
  remark?: string;
  createdTime?: string;
  rowVersion?: string;
  isDelete?: boolean;
}

// 邮件类型选项（值域对齐 Server MailType）
export const MAIL_CAMPAIGN_MAIL_TYPE_OPTIONS = [
  { value: MailCampaignMailType.System, labelKey: 'message.gameBusiness.mailCampaign.mailTypeSystem' },
  { value: MailCampaignMailType.Operation, labelKey: 'message.gameBusiness.mailCampaign.mailTypeOperation' },
  { value: MailCampaignMailType.Compensation, labelKey: 'message.gameBusiness.mailCampaign.mailTypeCompensation' },
];

// 未领取附件过期策略选项（值域对齐 Server ExpireAttachmentPolicy）
export const MAIL_CAMPAIGN_EXPIRE_POLICY_OPTIONS = [
  { value: MailCampaignExpirePolicy.DiscardUnclaimed, labelKey: 'message.gameBusiness.mailCampaign.expirePolicyDiscard' },
  { value: MailCampaignExpirePolicy.KeepUnclaimed, labelKey: 'message.gameBusiness.mailCampaign.expirePolicyKeep' },
  { value: MailCampaignExpirePolicy.AutoClaim, labelKey: 'message.gameBusiness.mailCampaign.expirePolicyAutoClaim' },
];

// 发布状态选项（Admin 侧生命周期）
export const MAIL_CAMPAIGN_PUBLISH_STATUS_OPTIONS = [
  { value: MailCampaignPublishStatus.Draft, labelKey: 'message.gameBusiness.mailCampaign.statusDraft' },
  { value: MailCampaignPublishStatus.Published, labelKey: 'message.gameBusiness.mailCampaign.statusPublished' },
  { value: MailCampaignPublishStatus.Revoked, labelKey: 'message.gameBusiness.mailCampaign.statusRevoked' },
  { value: MailCampaignPublishStatus.Failed, labelKey: 'message.gameBusiness.mailCampaign.statusFailed' },
];

// Tag severity map
export const MAIL_CAMPAIGN_PUBLISH_STATUS_SEVERITY: Record<number, 'info' | 'success' | 'warn' | 'danger'> = {
  [MailCampaignPublishStatus.Draft]: 'info',
  [MailCampaignPublishStatus.Published]: 'success',
  [MailCampaignPublishStatus.Revoked]: 'warn',
  [MailCampaignPublishStatus.Failed]: 'danger',
};

// 多语言内容支持的语言（对齐 i18n 已支持语言）
export const MAIL_CAMPAIGN_LANGUAGE_OPTIONS = [
  { value: 'zh-cn', labelKey: 'message.gameBusiness.mailCampaign.languageZhCn' },
  { value: 'en', labelKey: 'message.gameBusiness.mailCampaign.languageEn' },
  { value: 'zh-tw', labelKey: 'message.gameBusiness.mailCampaign.languageZhTw' },
  { value: 'ja', labelKey: 'message.gameBusiness.mailCampaign.languageJa' },
  { value: 'ko', labelKey: 'message.gameBusiness.mailCampaign.languageKo' },
];

// 附件 RewardType 选项（一期仅 NormalItem 可发放，对齐 C134 范围外声明 / Server 约束）
export const MAIL_CAMPAIGN_REWARD_TYPE_OPTIONS = [
  { value: 0, labelKey: 'message.gameBusiness.mailCampaign.rewardTypeNormalItem' },
];

// 通过 i18n key + t() 拿本地化标签的辅助
export const buildOptionLabel = (options: { value: number; labelKey: string }[], t: (key: string) => string) =>
  options.map((o) => ({ value: o.value, label: t(o.labelKey) }));
