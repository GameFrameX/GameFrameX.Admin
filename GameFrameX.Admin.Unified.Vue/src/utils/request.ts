import axios, {AxiosInstance} from 'axios';
import {
    createRequestInterceptor,
    createResponseInterceptor,
    createResponseErrorHandler,
} from './http/shared';

// 重导出共享常量和工具函数（保持消费者 import 兼容）
export {
    ACCESS_TOKEN_KEY,
    REFRESH_ACCESS_TOKEN_KEY,
    getToken,
    clearTokens,
    clearAccessTokens,
} from './http/shared';
export {decryptJWT, getJWTDate, convertTimestamps} from './http/shared';

// 配置新建一个 axios 实例
export const service = axios.create({
    baseURL: window.__env__.VITE_API_URL as any,
    timeout: 50000,
    headers: {'Content-Type': 'application/json'},
});

// axios 实例（保持向后兼容）
export const axiosInstance: AxiosInstance = axios;

// ── 注册拦截器 ──────────────────────────────────────────────

// 请求拦截器：JWT 注入 + GET params 序列化
service.interceptors.request.use(
    createRequestInterceptor({
        onAfterToken: (config) => {
            // GET 请求映射 params 参数
            if (config.method?.toLowerCase() === 'get' && config.data) {
                let url = config.url + '?' + tansParams(config.data);
                url = url.slice(0, -1);
                config.data = {};
                config.url = url;
            }
            return config;
        },
    }),
    (error: Error) => Promise.reject(error)
);

// 响应拦截器：401 / token 刷新 / 响应解包 / 时间戳转换
service.interceptors.response.use(
    createResponseInterceptor(),
    createResponseErrorHandler()
);

/**
 *  参数处理
 * @param {*} params  参数
 */
export function tansParams(params: any) {
    let result = '';
    for (const propName of Object.keys(params)) {
        const value = params[propName];
        var part = encodeURIComponent(propName) + '=';
        if (value !== null && value !== '' && typeof value !== 'undefined') {
            if (typeof value === 'object') {
                for (const key of Object.keys(value)) {
                    if (value[key] !== null && value[key] !== '' && typeof value[key] !== 'undefined') {
                        let params = propName + '[' + key + ']';
                        var subPart = encodeURIComponent(params) + '=';
                        result += subPart + encodeURIComponent(value[key]) + '&';
                    }
                }
            } else {
                result += part + encodeURIComponent(value) + '&';
            }
        }
    }
    return result;
}

// 导出 axios 实例
export default service;
