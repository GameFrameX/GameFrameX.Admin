/**
 * 匹配队列观测轮询（C341 §3：退避封装；7xxx 镜像码即停 + Toast）。
 *
 * 自 onlineMatch 手写 API 层收敛迁出——HTTP 调用已改走生成层
 *（src/api/onlineMatchQueueQuery），本文件只承载纯前端轮询/错误码判定逻辑。
 */
import {showError} from '../../../../utils/message';
import {t} from '../../../../i18n/index';

// 轮询间隔常量（C341 §3：默认 10000，前端可配；遵循 Online 侧限流，遇 7xxx 即退避）
export const ONLINE_MATCH_POLL_INTERVAL_MS = 10000;

// Online 7xxx 段（限流与风控）经 C339 八段镜像为 Admin 326xxxxx 段（OnlineErrorCode.RateLimited=32600001 起）。
const ONLINE_RATE_LIMIT_MIRROR_MIN = 32600000;
const ONLINE_RATE_LIMIT_MIRROR_MAX = 32700000;

/** 判断统一数字业务码是否为 Online 7xxx 段镜像（限流/风控）——按码判断，禁止按文案判断。 */
export function isOnlineRateLimitMirrorCode(code: unknown): boolean {
    return typeof code === 'number' && code >= ONLINE_RATE_LIMIT_MIRROR_MIN && code < ONLINE_RATE_LIMIT_MIRROR_MAX;
}

/**
 * 从拦截器抛出的业务错误中提取统一数字业务码。覆盖两条失败路径：
 * HTTP < 400 信封错误（业务码挂在 Error.code，见 utils/http/shared.ts）与
 * HTTP >= 400 错误（AxiosError.response.data 信封，键已 normalize 为小驼峰）。
 */
export function extractBusinessCode(error: unknown): unknown {
    if (!error || typeof error !== 'object') {
        return undefined;
    }
    const record = error as Record<string, any>;
    if (typeof record.code === 'number') {
        return record.code;
    }
    const envelope = record.response?.data as Record<string, any> | undefined;
    return envelope?.code ?? envelope?.Code;
}

export interface OnlineMatchPollingHandle {
    /** 停止轮询（幂等；页面卸载时必须调用）。 */
    stop: () => void;
}

/**
 * 启动匹配观测轮询：立即执行一次后按 ONLINE_MATCH_POLL_INTERVAL_MS 定时重查。
 * 任务抛错即停止轮询；错误码命中 Online 7xxx 镜像段（限流/风控）时额外 Toast 说明
 * （码表文案本身已由响应拦截器经 resolveResponseMessage 透出）。
 */
export function startOnlineMatchPolling(task: () => Promise<void>): OnlineMatchPollingHandle {
    let stopped = false;
    let timer: number | null = null;

    const clearTimer = () => {
        if (timer !== null) {
            window.clearTimeout(timer);
            timer = null;
        }
    };

    const stop = () => {
        stopped = true;
        clearTimer();
    };

    const run = async () => {
        if (stopped) {
            return;
        }
        try {
            await task();
        } catch (error) {
            stop();
            if (isOnlineRateLimitMirrorCode(extractBusinessCode(error))) {
                showError(t('message.onlineCenter.matchQueueOverview.pollStoppedByRateLimit'));
            }
            return;
        }
        if (!stopped) {
            timer = window.setTimeout(() => {
                void run();
            }, ONLINE_MATCH_POLL_INTERVAL_MS);
        }
    };

    void run();
    return {stop};
}
