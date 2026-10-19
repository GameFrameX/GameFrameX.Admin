export default {
    platformJwtConfig: {
        title: 'Platform JWT Configuration',
        issuerSigningKey: 'Signing Key',
        validIssuer: 'Issuer',
        validAudience: 'Audience',
        clockSkew: 'Clock Skew Seconds',
        algorithm: 'Algorithm',
        tokenVersion: 'Token Version',
        hasSigningKey: 'Signing Key Set',
        securityBaseline: 'Security Validation',
        enabled: 'Enabled',
        disabled: 'Disabled',
        saveWarning: 'After saving, every AccessToken and RefreshToken becomes invalid immediately and all users must sign in again.',
        hubSyncWarning: 'After rotating the signing key you must also update JWTSettings in GameFrameX.Hub.Api/appsettings.json, otherwise tokens between Admin and Hub will fail validation.',
        confirmSave: 'Save the platform JWT configuration? The current account and all other accounts must sign in again. If you changed the signing key, update Hub.Api JWTSettings too.',
        saveSuccess: 'Platform JWT configuration saved. Please sign in again.',
    },
};
