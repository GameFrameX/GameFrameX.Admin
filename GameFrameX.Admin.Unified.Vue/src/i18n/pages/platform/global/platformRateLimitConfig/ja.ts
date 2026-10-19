export default {
    platformRateLimitConfig: {
        title: 'レート制限設定',
        saveHint: '保存後、新しいルールはこのノードの後続リクエストにすぐ適用されます。',
        enabled: 'レート制限を有効化',
        permitLimit: 'ウィンドウ要求数',
        windowSeconds: 'ウィンドウ秒数',
        queueLimit: 'キュー数',
        version: '設定バージョン',
        currentStatus: '現在の状態',
        on: '有効',
        off: '無効',
        saveSuccess: 'レート制限設定を保存しました。新しいルールはこのノードの後続リクエストに適用されます。',
        permitLimitInvalid: 'ウィンドウ要求数は 1 から 100000 の間で指定してください',
        windowSecondsInvalid: 'ウィンドウ秒数は 1 から 3600 の間で指定してください',
        queueLimitInvalid: 'キュー数は 0 から 100000 の間で指定してください',
    },
};
