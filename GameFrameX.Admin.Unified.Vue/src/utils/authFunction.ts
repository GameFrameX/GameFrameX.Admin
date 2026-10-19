import {useUserInfoStore} from '../stores/userInfo';

function getAuthBtnList(): string[] {
    const stores = useUserInfoStore();
    return stores.userInfos.authBtnList ?? [];
}

function isSuperAdmin(): boolean {
    return useUserInfoStore().isSuperAdmin;
}

/**
 * 单个权限验证，用于 v-if / prop 绑定等指令不适用的场景
 * @param value 权限值
 * @returns 有权限返回 true
 */
export function auth(value: string): boolean {
    if (isSuperAdmin()) return true;
    return getAuthBtnList().includes(value);
}

/**
 * 多个权限验证（满足任一），用于操作列 v-if 等场景
 * @example v-if="hasAnyAuth('x:update', 'x:delete')"
 * @param permissions 一个或多个权限值
 * @returns 拥有任一权限返回 true，参数为空时返回 false
 */
export function hasAnyAuth(...permissions: string[]): boolean {
    if (permissions.length === 0) return false;
    if (isSuperAdmin()) return true;
    const authBtnList = getAuthBtnList();
    return permissions.some((p) => authBtnList.includes(p));
}
