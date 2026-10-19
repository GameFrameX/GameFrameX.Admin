import type {AxiosResponse, AxiosError, InternalAxiosRequestConfig} from 'axios';
import {local, session} from '../storage';
import {formatDate} from '../formatTime';
import {showError} from '../message';
import {t, te} from '../../i18n/index';
import {clearTargetTenantId, getTargetTenantId, TARGET_TENANT_ID_HEADER} from '../targetTenant';

// ── Token 常量 ──────────────────────────────────────────────

export const ACCESS_TOKEN_KEY = 'access-token';
export const REFRESH_ACCESS_TOKEN_KEY = `x-${ACCESS_TOKEN_KEY}`;

// ── Token 工具 ──────────────────────────────────────────────

export const getToken = (): string | null => {
    return local.get(ACCESS_TOKEN_KEY);
};

export const clearTokens = (): void => {
    local.remove(ACCESS_TOKEN_KEY);
    local.remove(REFRESH_ACCESS_TOKEN_KEY);
    clearTargetTenantId();
    session.clear();
};

export const clearAccessTokens = (): void => {
    clearTokens();
    window.location.reload();
};

// ── JWT 工具 ────────────────────────────────────────────────

interface JwtPayload {
    exp?: number;
    iat?: number;
    nbf?: number;
    [key: string]: unknown;
}

const DEFAULT_JWT_PAYLOAD: JwtPayload = {exp: 0};

export function decryptJWT(token: string): JwtPayload {
    try {
        token = token.replace(/_/g, '/').replace(/-/g, '+');
        const json = decodeURIComponent(escape(window.atob(token.split('.')[1])));
        return JSON.parse(json) as JwtPayload;
    } catch (e) {
        console.warn('[decryptJWT] Failed to decode JWT:', e);
        return DEFAULT_JWT_PAYLOAD;
    }
}

export function getJWTDate(timestamp: number): Date {
    return new Date(timestamp * 1000);
}

// ── 时间戳转换（C145：后端时间字段统一 long 秒 + *Time 后缀，按后缀匹配）──

function formatTimestampValue(value: number): string {
    if (value > 0 && value < 4102444800) {
        return formatDate(new Date(value * 1000), 'YYYY-mm-dd HH:MM:SS');
    }
    return String(value);
}

export function convertTimestamps(obj: unknown): unknown {
    if (!obj || typeof obj !== 'object') return obj;
    if (Array.isArray(obj)) {
        for (let i = 0; i < obj.length; i++) {
            if (typeof obj[i] === 'object' && obj[i] !== null) {
                convertTimestamps(obj[i]);
            }
        }
    } else {
        for (const key of Object.keys(obj as Record<string, unknown>)) {
            const record = obj as Record<string, unknown>;
            if (/Time$/i.test(key) && typeof record[key] === 'number') {
                record[key] = formatTimestampValue(record[key] as number);
            } else if (typeof record[key] === 'object' && record[key] !== null) {
                convertTimestamps(record[key]);
            }
        }
    }
    return obj;
}

// ── 响应字段归一化 ──────────────────────────────────────────

function toCamelCaseKey(key: string): string {
    if (!key || key[0] !== key[0].toUpperCase()) return key;
    return key[0].toLowerCase() + key.slice(1);
}

export function normalizeResponseKeys(obj: unknown): unknown {
    if (!obj || typeof obj !== 'object') return obj;
    if (Array.isArray(obj)) {
        for (let i = 0; i < obj.length; i++) {
            obj[i] = normalizeResponseKeys(obj[i]);
        }
        return obj;
    }

    const record = obj as Record<string, unknown>;
    for (const key of Object.keys(record)) {
        const normalizedKey = toCamelCaseKey(key);
        const normalizedValue = normalizeResponseKeys(record[key]);

        if (normalizedKey !== key && !Object.prototype.hasOwnProperty.call(record, normalizedKey)) {
            record[normalizedKey] = normalizedValue;
            delete record[key];
        } else {
            record[key] = normalizedValue;
        }
    }

    return record;
}

// ── 业务错误文案解析 ──────────────────────────────────────

const ERROR_CODE_I18N_PREFIX = 'errors.codes.';
/** CommonErrorCode.ParameterValidationFailed（统一数字业务错误码）：message 承载校验明细，直接透出 */
const VALIDATION_FAILED_ERROR_CODE = 20000002;

function normalizeBackendMessage(message: unknown): string {
    if (!message) return '';

    if (typeof message === 'object') {
        return Object.values(message as Record<string, unknown>)
            .flatMap((value) => Array.isArray(value) ? value : [value])
            .filter((value): value is string => typeof value === 'string' && value.length > 0)
            .join('；') || JSON.stringify(message);
    }

    if (typeof message !== 'string') {
        return String(message);
    }

    const trimmed = message.trim();
    if (!trimmed) return '';

    try {
        return normalizeBackendMessage(JSON.parse(trimmed));
    } catch {
        return trimmed.replace(/^\[[A-Za-z0-9_.-]+\]\s*/, '');
    }
}

/**
 * 解析业务错误展示文案：优先用失败信封 code（统一数字业务错误码，C262/C266）查当前
 * locale 的 i18n 错误码表，命中则返回本地化文案；未命中（如 code 回退 HTTP 状态语义值）
 * 时回退到后端 message（服务端 ErrorLocalizer 已按请求语言解析）。
 * message 为对象时沿用既有 JSON.stringify 兜底语义。永不抛二次错误。
 */
// 窄化 vue-i18n 的 t/te 签名，避免在严格 tsc 下触发 TS2589（类型组合爆炸）。
type I18nKeyChecker = (key: string) => boolean;
type I18nTranslator = (key: string) => string;

export function resolveResponseMessage(serve: Record<string, any> | null | undefined): string {
    if (serve) {
        const code = serve.code;
        const message = normalizeBackendMessage(serve.message);
        if (code === VALIDATION_FAILED_ERROR_CODE && message) {
            return message;
        }

        if (typeof code === 'number' && code !== 0) {
            const key = `${ERROR_CODE_I18N_PREFIX}${code}`;
            try {
                if ((te as I18nKeyChecker)(key)) {
                    const translated = (t as I18nTranslator)(key);
                    if (translated) {
                        return translated;
                    }
                }
            } catch (e) {
                console.warn('[resolveResponseMessage] i18n lookup failed:', e);
            }
        }

        return message;
    }

    return '';
}

// ── 拦截器工厂 ──────────────────────────────────────────────

interface RequestInterceptorOptions {
    /** 在 token 注入之后、返回 config 之前的扩展点 */
    onAfterToken?: (config: InternalAxiosRequestConfig) => InternalAxiosRequestConfig;
}

interface ResponseInterceptorOptions {
    /** 响应错误时的自定义处理（覆盖默认的 i18n 提示） */
    onError?: (error: AxiosError) => void;
}

/**
 * 创建请求拦截器：JWT 注入 + 过期检测 + 刷新令牌
 */
export function createRequestInterceptor(options: RequestInterceptorOptions = {}) {
    return (config: InternalAxiosRequestConfig): InternalAxiosRequestConfig => {
        const accessToken = local.get(ACCESS_TOKEN_KEY);
        if (accessToken) {
            config.headers['Authorization'] = `Bearer ${accessToken}`;

            const jwt = decryptJWT(accessToken);
            if (jwt.exp != null) {
                const exp = getJWTDate(jwt.exp);
                if (new Date() >= exp) {
                    const refreshAccessToken = local.get(REFRESH_ACCESS_TOKEN_KEY);
                    if (refreshAccessToken) {
                        config.headers['X-Authorization'] = `Bearer ${refreshAccessToken}`;
                    }
                }
            }
        }

        const targetTenantId = getTargetTenantId();
        if (targetTenantId) {
            config.headers[TARGET_TENANT_ID_HEADER] = targetTenantId;
        } else {
            delete config.headers[TARGET_TENANT_ID_HEADER];
        }

        if (options.onAfterToken) {
            config = options.onAfterToken(config);
        }

        return config;
    };
}

/**
 * 创建响应拦截器：401 / token 刷新 / 响应解包 / 时间戳转换
 */
export function createResponseInterceptor() {
    return (res: AxiosResponse): AxiosResponse => {
        const status = res.status;
        const serve = normalizeResponseKeys(res.data) as Record<string, any>;
        res.data = serve;

        // HTTP 401
        if (status === 401) {
            clearAccessTokens();
        }

        // 未规范化处理
        if (status >= 400) {
            throw new Error(res.statusText || 'Request Error.');
        }

        // 规范化结果错误
        if (serve && Object.prototype.hasOwnProperty.call(serve, 'errors') && serve.errors) {
            throw new Error(JSON.stringify(serve.errors || 'Request Error.'));
        }

        // 读取响应头 token
        const newAccessToken = res.headers[ACCESS_TOKEN_KEY];
        const newRefreshToken = res.headers[REFRESH_ACCESS_TOKEN_KEY];

        if (newAccessToken === 'invalid_token') {
            clearAccessTokens();
        } else if (newRefreshToken && newAccessToken && newAccessToken !== 'invalid_token') {
            local.set(ACCESS_TOKEN_KEY, newAccessToken);
            local.set(REFRESH_ACCESS_TOKEN_KEY, newRefreshToken);
        }

        // 业务码处理（C226：信封切换 HttpJsonResultData，成功 code=0；失败时 code 承载 HTTP 状态语义，401 仍触发清 token）
        if (serve.code === 401) {
            clearAccessTokens();
        } else if (serve.code === undefined) {
            return res;
        } else if (serve.code !== 0) {
            const message = resolveResponseMessage(serve);
            showError(message);
            // 业务码随异常透出（C341：调用方按码判断而非文案——如 onlineMatch 轮询遇 7xxx 镜像段即退避）
            const error = new Error(message) as Error & { code?: unknown };
            error.code = serve.code;
            throw error;
        }

        // C145：不再自动覆写时间戳（后端 *Time 字段统一 long 秒，显示由 formatUnix 显式处理，
        // 避免覆写导致编辑回显 beforeOpen 与过期/轮换计算拿不到原始 long）。
        return res;
    };
}

/**
 * 创建响应错误拦截器（处理网络错误、超时等）
 */
export function createResponseErrorHandler(options: ResponseInterceptorOptions = {}) {
    return (error: AxiosError): Promise<never> => {
        if (error.response) {
            if (error.response.status === 401) {
                clearAccessTokens();
            }
        }

        if (options.onError) {
            options.onError(error);
        } else {
            // 默认 i18n 错误提示
            if (error.message.indexOf('timeout') !== -1) {
                showError(t('message.common.message.networkTimeout'));
            } else if (error.message === 'Network Error') {
                showError(t('message.common.message.networkError'));
            } else {
                const data = normalizeResponseKeys(error.response?.data) as Record<string, unknown> | undefined;
                if (data) {
                    const message = resolveResponseMessage(data) || error.response?.statusText || t('message.common.message.apiNotFound');
                    showError(message);
                } else {
                    showError(t('message.common.message.apiNotFound'));
                }
            }
        }

        return Promise.reject(error);
    };
}

/**
 * 创建请求错误拦截器
 */
export function createRequestErrorHandler() {
    return (error: Error): Promise<never> => {
        showError(error.message);
        return Promise.reject(error);
    };
}
