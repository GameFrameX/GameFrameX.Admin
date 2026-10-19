export default {
    platformJwtConfig: {
        title: '平台 JWT 配置',
        issuerSigningKey: '签名密钥',
        validIssuer: '签发方',
        validAudience: '签收方',
        clockSkew: '过期容错秒数',
        algorithm: '签名算法',
        tokenVersion: '令牌版本',
        hasSigningKey: '已配置密钥',
        securityBaseline: '安全校验',
        enabled: '已启用',
        disabled: '未启用',
        saveWarning: '保存后所有用户的 AccessToken 和 RefreshToken 都会立即失效，必须重新登录。',
        hubSyncWarning: '轮换签名密钥后，必须同步更新 GameFrameX.Hub.Api 的 appsettings.json:JWTSettings，否则 Admin 与 Hub 之间的令牌校验会失败。',
        confirmSave: '确定保存平台 JWT 配置吗？保存后当前账号和其他所有账号都需要重新登录。若修改了签名密钥，请一并更新 Hub.Api 的 JWTSettings。',
        saveSuccess: '平台 JWT 配置已保存，请重新登录。',
    },
};
