export default {
    platformRateLimitConfig: {
        title: 'Rate Limit Configuration',
        saveHint: 'After saving, new rules apply to subsequent requests on this node immediately.',
        enabled: 'Enable Rate Limit',
        permitLimit: 'Window Request Limit',
        windowSeconds: 'Window Seconds',
        queueLimit: 'Queue Limit',
        version: 'Version',
        currentStatus: 'Current Status',
        on: 'Enabled',
        off: 'Disabled',
        saveSuccess: 'Rate limit configuration saved. New rules now apply to subsequent requests on this node.',
        permitLimitInvalid: 'Window request limit must be between 1 and 100000',
        windowSecondsInvalid: 'Window seconds must be between 1 and 3600',
        queueLimitInvalid: 'Queue limit must be between 0 and 100000',
    },
};
