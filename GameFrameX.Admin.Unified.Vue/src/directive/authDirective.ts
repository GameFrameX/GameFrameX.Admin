import type {App, DirectiveBinding} from 'vue';
import {useUserInfoStore} from '../stores/userInfo';

function checkAuth(binding: DirectiveBinding): boolean {
    const stores = useUserInfoStore();
    // 超管短路：accountType=999 拥有全部按钮权限（与后端 SuperAdmin 语义一致）
    if (stores.isSuperAdmin) {
        return true;
    }
    const authBtnList: string[] = stores.userInfos.authBtnList ?? [];
    const value = binding.value;

    if (Array.isArray(value)) {
        return value.some((v: string) => authBtnList.includes(v));
    }
    return authBtnList.includes(value);
}

/**
 * 用户权限指令
 * @directive v-auth="'xxx:update'" — 单个权限
 * @directive v-auth="['xxx:update', 'xxx:delete']" — 满足任一权限
 * 无权限则隐藏元素（display:none），支持权限动态变化
 */
export function authDirective(app: App) {
    app.directive('auth', {
        mounted(el: HTMLElement, binding: DirectiveBinding) {
            if (!checkAuth(binding)) {
                el.dataset.authOriginalDisplay = el.style.display;
                el.style.display = 'none';
            }
        },
        updated(el: HTMLElement, binding: DirectiveBinding) {
            const hasPermission = checkAuth(binding);
            const isHidden = el.dataset.authOriginalDisplay !== undefined;

            if (hasPermission && isHidden) {
                el.style.display = el.dataset.authOriginalDisplay ?? '';
                delete el.dataset.authOriginalDisplay;
            } else if (!hasPermission && !isHidden) {
                el.dataset.authOriginalDisplay = el.style.display;
                el.style.display = 'none';
            }
        },
        unmounted(el: HTMLElement) {
            delete el.dataset.authOriginalDisplay;
        },
    });
}
