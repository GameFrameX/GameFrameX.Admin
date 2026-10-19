/**
 * 当前版本：v2.0.0（swagger-typescript-api 适配）
 *
 * 基于 swagger-typescript-api 生成的 HttpClient，注入全局拦截器（JWT、token 刷新、
 * 响应解包、时间戳）。所有 Api class 共享同一个 HttpClient 实例，复用拦截器与 baseURL。
 */

import type {AxiosInstance} from 'axios';
import {HttpClient} from '../api/_shared/http-client';

import {
    createRequestInterceptor,
    createResponseInterceptor,
    createResponseErrorHandler,
    createRequestErrorHandler,
} from './http/shared';
import {local} from './storage';

// 重导出共享常量和工具函数（保持消费者 import 兼容）
export {
    ACCESS_TOKEN_KEY,
    REFRESH_ACCESS_TOKEN_KEY,
    getToken,
    clearTokens,
    clearAccessTokens,
} from './http/shared';
export {decryptJWT, getJWTDate, convertTimestamps} from './http/shared';

// stc 共享 HttpClient：内部 axios 实例承载 baseURL，全部拦截器挂在它上面。
export const sharedHttpClient = new HttpClient({
    baseURL: window.__env__.VITE_API_URL,
});

// axios 实例（兼容既有 axiosInstance 引用）
export const axiosInstance: AxiosInstance = sharedHttpClient.instance;

// ── 注册拦截器 ──────────────────────────────────────────────

// 请求拦截器：JWT 注入 + Accept-Language
axiosInstance.interceptors.request.use(
    createRequestInterceptor({
        onAfterToken: (config) => {
            const globalI18n = local.get('themeConfig')?.globalI18n;
            if (globalI18n) {
                config.headers['Accept-Language'] = globalI18n;
            }
            return config;
        },
    }),
    createRequestErrorHandler()
);

// 响应拦截器：401 / token 刷新 / 响应解包 / 时间戳转换
axiosInstance.interceptors.response.use(
    createResponseInterceptor(),
    createResponseErrorHandler()
);

/**
 * 包装 Promise 并返回 [Error, any]
 * @param promise Promise 方法
 * @param errorExt 自定义错误信息（拓展）
 * @returns [Error, any]
 */
export function feature<T, U = Error>(promise: Promise<T>, errorExt?: object): Promise<[U, undefined] | [null, T]> {
    return promise
        .then<[null, T]>((data: T) => [null, data])
        .catch<[U, undefined]>((err: U) => {
            if (errorExt) {
                const parsedError = Object.assign({}, err, errorExt);
                return [parsedError, undefined];
            }

            return [err, undefined];
        });
}

const _apiCache = new Map<new (http: HttpClient) => any, any>();
/**
 * 获取/创建服务 API 实例（swagger-typescript-api 适配版，按 class 缓存单例）。
 * @param apiType 接受 HttpClient 构造的 Api class（如 GlobalMenu）
 * @returns 服务API 实例（首次创建后缓存，共享 sharedHttpClient 含拦截器与 baseURL）
 */
export function getAPI<T>(apiType: new (http: HttpClient) => T): T {
    if (!_apiCache.has(apiType)) {
        _apiCache.set(apiType, new apiType(sharedHttpClient));
    }
    return _apiCache.get(apiType) as T;
}
