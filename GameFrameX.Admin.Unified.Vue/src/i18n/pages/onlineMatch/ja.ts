export default {
    // C341 displayKey マッピング辞書：バックエンド OnlineMatchTicketDisplayMap が発行する表示キー
    // （onlineMatch.ticketStatus.<小文字の生値> / onlineMatch.failureReason.<小文字の生値>）、
    // キーセットは vault C5 契約の網羅リストと一致（change.md §3 A0）、未知の生値は unknown にフォールバック。
    onlineMatch: {
        ticketStatus: {
            queued: 'キュー中',
            matching: 'マッチ中',
            matched: 'マッチ成功',
            cancelled: 'キャンセル済み',
            expired: '期限切れ',
            failed: '失敗',
            unknown: '不明なステータス',
        },
        failureReason: {
            timeout: 'マッチタイムアウト',
            cancelrace: 'キャンセル競合',
            ratelimited: 'レート制限（7xxx）',
            duplicateticket: '重複入隊ガード',
            unknown: '不明な理由',
        },
    },
};