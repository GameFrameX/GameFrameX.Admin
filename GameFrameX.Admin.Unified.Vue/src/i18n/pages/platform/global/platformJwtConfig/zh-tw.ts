export default {
    platformJwtConfig: {
        title: '平台 JWT 配置',
        issuerSigningKey: '簽名密鑰',
        validIssuer: '簽發方',
        validAudience: '簽收方',
        clockSkew: '過期容錯秒數',
        algorithm: '簽名演算法',
        tokenVersion: '令牌版本',
        hasSigningKey: '已配置密鑰',
        securityBaseline: '安全校驗',
        enabled: '已啟用',
        disabled: '未啟用',
        saveWarning: '儲存後所有使用者的 AccessToken 和 RefreshToken 都會立即失效，必須重新登入。',
        hubSyncWarning: '輪換簽名密鑰後，必須同步更新 GameFrameX.Hub.Api 的 appsettings.json:JWTSettings，否則 Admin 與 Hub 之間的令牌校驗會失敗。',
        confirmSave: '確定儲存平台 JWT 配置嗎？儲存後目前帳號和其他所有帳號都需要重新登入。若修改了簽名密鑰，請一併更新 Hub.Api 的 JWTSettings。',
        saveSuccess: '平台 JWT 配置已儲存，請重新登入。',
    },
};
