export default {
    // C341 displayKey 映射词条：后端 OnlineMatchTicketDisplayMap 下发的展示键
    // （onlineMatch.ticketStatus.<小写原值> / onlineMatch.failureReason.<小写原值>），
    // 键集合与 vault C5 契约锁定穷举一致（change.md §3 A0），未知原值走 unknown 兜底。
    onlineMatch: {
        ticketStatus: {
            queued: '队列中',
            matching: '匹配中',
            matched: '匹配成功',
            cancelled: '已取消',
            expired: '已过期',
            failed: '失败',
            unknown: '未知状态',
        },
        failureReason: {
            timeout: '匹配超时',
            cancelrace: '取消竞态',
            ratelimited: '限流（7xxx）',
            duplicateticket: '重复入队保护',
            unknown: '未知原因',
        },
    },
};