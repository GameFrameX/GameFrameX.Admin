export default {
    // C341 displayKey map entries: display keys issued by backend OnlineMatchTicketDisplayMap
    // (onlineMatch.ticketStatus.<lower raw value> / onlineMatch.failureReason.<lower raw value>),
    // key set locked to the vault C5 contract exhaustive list (change.md §3 A0); unknown falls back to `unknown`.
    onlineMatch: {
        ticketStatus: {
            queued: 'Queued',
            matching: 'Matching',
            matched: 'Matched',
            cancelled: 'Cancelled',
            expired: 'Expired',
            failed: 'Failed',
            unknown: 'Unknown Status',
        },
        failureReason: {
            timeout: 'Match Timeout',
            cancelrace: 'Cancel Race',
            ratelimited: 'Rate Limited (7xxx)',
            duplicateticket: 'Duplicate Ticket Guard',
            unknown: 'Unknown Reason',
        },
    },
};