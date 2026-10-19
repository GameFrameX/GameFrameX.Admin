export default {
    // C341 displayKey 매핑 사전: 백엔드 OnlineMatchTicketDisplayMap이 발급하는 표시 키
    // (onlineMatch.ticketStatus.<소문자 원값> / onlineMatch.failureReason.<소문자 원값>),
    // 키 집합은 vault C5 계약의 열거 목록과 잠금 일치(change.md §3 A0), 알 수 없는 원값은 unknown 폴백.
    onlineMatch: {
        ticketStatus: {
            queued: '대기 중',
            matching: '매칭 중',
            matched: '매칭 성공',
            cancelled: '취소됨',
            expired: '만료됨',
            failed: '실패',
            unknown: '알 수 없는 상태',
        },
        failureReason: {
            timeout: '매치 시간 초과',
            cancelrace: '취소 경합',
            ratelimited: '속도 제한(7xxx)',
            duplicateticket: '중복 입대 보호',
            unknown: '알 수 없는 사유',
        },
    },
};