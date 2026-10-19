export default {
    platformJwtConfig: {
        title: 'プラットフォーム JWT 設定',
        issuerSigningKey: '署名キー',
        validIssuer: '発行者',
        validAudience: '対象者',
        clockSkew: '許容秒数',
        algorithm: '署名アルゴリズム',
        tokenVersion: 'トークンバージョン',
        hasSigningKey: '署名キー設定済み',
        securityBaseline: 'セキュリティ検証',
        enabled: '有効',
        disabled: '無効',
        saveWarning: '保存後、すべての AccessToken と RefreshToken は直ちに無効になり、全ユーザーの再ログインが必要です。',
        hubSyncWarning: '署名キーをローテーションした後、GameFrameX.Hub.Api の appsettings.json:JWTSettings を同期更新してください。そうしないと Admin と Hub 間のトークン検証が失敗します。',
        confirmSave: 'プラットフォーム JWT 設定を保存しますか？現在のアカウントを含むすべてのアカウントで再ログインが必要です。署名キーを変更した場合は Hub.Api の JWTSettings も更新してください。',
        saveSuccess: 'プラットフォーム JWT 設定を保存しました。再ログインしてください。',
    },
};
