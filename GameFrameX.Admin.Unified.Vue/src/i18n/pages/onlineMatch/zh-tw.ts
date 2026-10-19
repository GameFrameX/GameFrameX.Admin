export default {
    // C341 displayKey 映射詞條：後端 OnlineMatchTicketDisplayMap 下發的展示鍵
    // （onlineMatch.ticketStatus.<小寫原值> / onlineMatch.failureReason.<小寫原值>），
    // 鍵集合與 vault C5 契約鎖定窮舉一致（change.md §3 A0），未知原值走 unknown 兜底。
    onlineMatch: {
        ticketStatus: {
            queued: '佇列中',
            matching: '匹配中',
            matched: '匹配成功',
            cancelled: '已取消',
            expired: '已過期',
            failed: '失敗',
            unknown: '未知狀態',
        },
        failureReason: {
            timeout: '匹配逾時',
            cancelrace: '取消競態',
            ratelimited: '限流（7xxx）',
            duplicateticket: '重複入隊保護',
            unknown: '未知原因',
        },
    },
};