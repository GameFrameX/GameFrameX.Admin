// 租户渠道定义下拉映射：available 接口返回的渠道列表 → { label, value } 选项。
// 025cf914 手写 API 层回迁 gen:api 时，原 src/api/tenantChannelDefinition.ts 内的该工具被删而引用未迁，此处恢复。
export const mapAvailableChannelDefinitions = (channels: any[] = [], valueKey: 'id' | 'code' = 'code') =>
    channels.map((channel: any) => ({
        ...channel,
        label: channel.displayName || channel.name,
        value: valueKey === 'id' ? channel.id : channel.code,
    }));
