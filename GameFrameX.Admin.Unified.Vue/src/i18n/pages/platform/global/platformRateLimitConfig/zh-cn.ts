export default {
    platformRateLimitConfig: {
        title: '限流配置',
        saveHint: '保存后，新规则立即用于本节点后续请求。',
        enabled: '启用限流',
        permitLimit: '窗口请求数',
        windowSeconds: '窗口秒数',
        queueLimit: '排队数量',
        version: '配置版本',
        currentStatus: '当前状态',
        on: '已启用',
        off: '已关闭',
        saveSuccess: '限流配置已保存，新规则已用于本节点后续请求。',
        permitLimitInvalid: '窗口请求数必须在 1 到 100000 之间',
        windowSecondsInvalid: '窗口秒数必须在 1 到 3600 之间',
        queueLimitInvalid: '排队数量必须在 0 到 100000 之间',
    },
};
