export default {
    platformJwtConfig: {
        title: '플랫폼 JWT 설정',
        issuerSigningKey: '서명 키',
        validIssuer: '발급자',
        validAudience: '대상',
        clockSkew: '만료 허용 초',
        algorithm: '서명 알고리즘',
        tokenVersion: '토큰 버전',
        hasSigningKey: '서명 키 설정됨',
        securityBaseline: '보안 검증',
        enabled: '사용',
        disabled: '사용 안 함',
        saveWarning: '저장 후 모든 AccessToken 및 RefreshToken이 즉시 무효화되며 모든 사용자가 다시 로그인해야 합니다.',
        hubSyncWarning: '서명 키를 교체한 후 GameFrameX.Hub.Api의 appsettings.json:JWTSettings도 함께 업데이트해야 합니다. 그렇지 않으면 Admin과 Hub 간 토큰 검증이 실패합니다.',
        confirmSave: '플랫폼 JWT 설정을 저장하시겠습니까? 현재 계정과 다른 모든 계정이 다시 로그인해야 합니다. 서명 키를 변경한 경우 Hub.Api의 JWTSettings도 업데이트하세요.',
        saveSuccess: '플랫폼 JWT 설정이 저장되었습니다. 다시 로그인하세요.',
    },
};
