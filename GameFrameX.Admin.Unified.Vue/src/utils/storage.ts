import Cookies from 'js-cookie';

/**
 * window.localStorage 浏览器永久缓存
 * @method set 设置永久缓存
 * @method get 获取永久缓存
 * @method remove 移除永久缓存
 * @method clear 移除全部永久缓存
 */
export const local = {
    // 查看 v2.4.3版本更新日志
    setKey(key: string) {
        return `${__NEXT_NAME__}:${key}`;
    },
    // 设置永久缓存
    set<T>(key: string, val: T) {
        window.localStorage.setItem(local.setKey(key), JSON.stringify(val));
    },
    // 获取永久缓存
    get(key: string) {
        let json = <string>window.localStorage.getItem(local.setKey(key));
        return JSON.parse(json);
    },
    // 移除永久缓存
    remove(key: string) {
        window.localStorage.removeItem(local.setKey(key));
    },
    // 移除全部永久缓存
    clear() {
        window.localStorage.clear();
    },
};

/**
 * window.sessionStorage 浏览器临时缓存
 * @method set 设置临时缓存
 * @method get 获取临时缓存
 * @method remove 移除临时缓存
 * @method clear 移除全部临时缓存
 */
export const session = {
    // 设置临时缓存
    set<T>(key: string, val: T) {
        if (key === 'token') return Cookies.set(key, val);
        window.sessionStorage.setItem(local.setKey(key), JSON.stringify(val));
    },
    // 获取临时缓存
    get(key: string) {
        if (key === 'token') return Cookies.get(key);
        let json = <string>window.sessionStorage.getItem(local.setKey(key));
        return JSON.parse(json);
    },
    // 移除临时缓存
    remove(key: string) {
        if (key === 'token') return Cookies.remove(key);
        window.sessionStorage.removeItem(local.setKey(key));
    },
    // 移除全部临时缓存
    clear() {
        Cookies.remove('token');
        window.sessionStorage.clear();
    },
};
