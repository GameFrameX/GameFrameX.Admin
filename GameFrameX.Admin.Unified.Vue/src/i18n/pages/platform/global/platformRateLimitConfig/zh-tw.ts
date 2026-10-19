export default {
    platformRateLimitConfig: {
        title: '限流配置',
        saveHint: '保存後，新規則會立即用於本節點後續請求。',
        enabled: '啟用限流',
        permitLimit: '窗口請求數',
        windowSeconds: '窗口秒數',
        queueLimit: '排隊數量',
        version: '配置版本',
        currentStatus: '目前狀態',
        on: '已啟用',
        off: '已關閉',
        saveSuccess: '限流配置已保存，新規則已用於本節點後續請求。',
        permitLimitInvalid: '窗口請求數必須在 1 到 100000 之間',
        windowSecondsInvalid: '窗口秒數必須在 1 到 3600 之間',
        queueLimitInvalid: '排隊數量必須在 0 到 100000 之間',
    },
};
