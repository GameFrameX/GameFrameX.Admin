import {createRouter, createWebHashHistory} from 'vue-router';
import NProgress from 'nprogress';
import 'nprogress/nprogress.css';
import pinia from '../stores/index';
import {storeToRefs} from 'pinia';
import {useKeepAliveNamesStore} from '../stores/keepAliveNames';
import {useRoutesListStore} from '../stores/routesList';
import {useThemeConfigStore} from '../stores/themeConfig';
import {session} from '../utils/storage';
import {staticRoutes, NOT_FOUND_AND_NO_POWER} from './route';
import {initFrontEndControlRoutes} from './frontEnd';
import {initBackEndControlRoutes} from './backEnd';
import {clearTokens} from '../utils/http/shared';
import {showWarning} from '../utils/message';
import {t} from '../i18n/index';
import {useUserInfoStore} from '../stores/userInfo';
import {useCurrentApp} from '../composables/useCurrentApp';
import {
    APP_WORKSPACE_PATH,
    decideTenantAppEntry,
    isAppScopedRoute,
    isAppWorkspaceRoutePath,
    normalizeAppId,
} from '../utils/appWorkspace';

/**
 * 1、前端控制路由时：isRequestRoutes 为 false，需要写 roles，需要走 setFilterRoute 方法。
 * 2、后端控制路由时：isRequestRoutes 为 true，不需要写 roles，不需要走 setFilterRoute 方法），
 * 相关方法已拆解到对应的 `backEnd.ts` 与 `frontEnd.ts`（他们互不影响，不需要同时改 2 个文件）。
 * 特别说明：
 * 1、前端控制：路由菜单由前端去写（无菜单管理界面，有角色管理界面），角色管理中有 roles 属性，需返回到 userInfo 中。
 * 2、后端控制：路由菜单由后端返回（有菜单管理界面、有角色管理界面）
 */

// 注意：不能在此处（模块初始化期）useThemeConfigStore——当前模块参与了一条 store 循环依赖，
// 此时 persistedstate 插件尚未注册（实测 _p=0），顶层创建 store 会使其持久化静默失效。
// isRequestRoutes 仅路由守卫（运行期）使用，改为守卫内惰性读取。

const shouldSkipAppWorkspace = (path: string) => {
    return path === '/login' || path === '/401' || path.startsWith('/platform/')
}

const resolveTenantAppRedirect = async (to: any) => {
    if (shouldSkipAppWorkspace(to.path)) {
        return undefined
    }
    const storesUserInfo = useUserInfoStore(pinia)
    if (storesUserInfo.isSuperAdmin) {
        if (to.path.startsWith('/platform/')) {
            return undefined
        }
        if (to.path === '/tenant/dashboard/home') {
            return undefined
        }
        if (to.path.startsWith('/app/') || to.path.startsWith('/appCenter/')) {
            return '/tenant/dashboard/home'
        }
        return undefined
    }

    const { appOptions, loadApps, setCurrentApp, currentAppId } = useCurrentApp()
    let apps: any[] = []
    try {
        apps = await loadApps()
    } catch {
        return undefined
    }
    const queryAppId = normalizeAppId(to.query?.appId)
    if (queryAppId !== undefined) {
        const queryAppVisible = appOptions.value.some((app) => String(app.id) === String(queryAppId) && app.isEnabled !== false)
        if (queryAppVisible) {
            setCurrentApp(queryAppId)
            return undefined
        }
        showWarning(t('gameBusiness.appWorkspace.appUnavailable'))
        if (isAppScopedRoute(to) || to.path === APP_WORKSPACE_PATH) {
            return decideTenantAppEntry(apps)
        }
        return undefined
    }

    if (to.path === '/' || to.path === '/tenant/dashboard/home') {
        // 三层驾驶舱（2026-08-31 拍板）：/tenant/dashboard/home 是超管平台驾驶舱；
        // 租户访问导流——已选有效应用去应用驾驶舱 appHome，未选去应用选择器
        const currentAppVisible = appOptions.value.some(
            (app) => String(app.id) === String(currentAppId.value) && app.isEnabled !== false,
        )
        if (currentAppId.value !== undefined && currentAppVisible) {
            return {path: '/dashboard/appHome'}
        }
        return decideTenantAppEntry(apps)
    }

    if (to.path === '/dashboard/appHome') {
        // 应用驾驶舱要求已选有效应用，否则回到选择器
        const currentAppVisible = appOptions.value.some(
            (app) => String(app.id) === String(currentAppId.value) && app.isEnabled !== false,
        )
        if (currentAppId.value === undefined || !currentAppVisible) {
            return decideTenantAppEntry(apps)
        }
        return undefined
    }

    if (isAppWorkspaceRoutePath(to.path)) {
        return undefined
    }

    if (isAppScopedRoute(to)) {
        showWarning(t('gameBusiness.appWorkspace.appIdRequired'))
        return decideTenantAppEntry(apps)
    }
    return undefined
}

/**
 * 创建一个可以被 Vue 应用程序使用的路由实例
 * @method createRouter(options: RouterOptions): Router
 * @link 参考：https://next.router.vuejs.org/zh/api/#createrouter
 */
export const router = createRouter({
    history: createWebHashHistory(),
    /**
     * 说明：
     * 1、NOT_FOUND_AND_NO_POWER 默认添加 404、401 界面，防止一直提示 No match found for location with path 'xxx'
     * 2、backEnd.ts(后端控制路由)、frontEnd.ts(前端控制路由) 中也需要加 NOT_FOUND_AND_NO_POWER 404、401 界面。
     *    防止 404、401 不在 layout 布局中，不设置的话，404、401 界面将全屏显示
     */
    routes: [...NOT_FOUND_AND_NO_POWER, ...staticRoutes],
});

/**
 * 路由多级嵌套数组处理成一维数组
 * @param arr 传入路由菜单数据数组
 * @returns 返回处理后的一维路由菜单数组
 */
export function formatFlatteningRoutes(arr: any) {
    if (arr.length <= 0) return false;
    for (let i = 0; i < arr.length; i++) {
        if (arr[i].children) {
            arr = arr.slice(0, i + 1).concat(arr[i].children, arr.slice(i + 1));
        }
    }
    return arr;
}

/**
 * 一维数组处理成多级嵌套数组（只保留二级：也就是二级以上全部处理成只有二级，keep-alive 支持二级缓存）
 * @description isKeepAlive 处理 `name` 值，进行缓存。顶级关闭，全部不缓存
 * @link 参考：https://v3.cn.vuejs.org/api/built-in-components.html#keep-alive
 * @param arr 处理后的一维路由菜单数组
 * @returns 返回将一维数组重新处理成 `定义动态路由（dynamicRoutes）` 的格式
 */
export function formatTwoStageRoutes(arr: any) {
    if (arr.length <= 0) return false;
    const newArr: any = [];
    const cacheList: Array<string> = [];
    arr.forEach((v: any) => {
        if (v.path == null || v.path == undefined) return;

        if (v.path === '/') {
            newArr.push({
                component: v.component,
                name: v.name,
                path: v.path,
                redirect: v.redirect,
                meta: v.meta,
                children: []
            });
        } else {
            // 判断是否是动态路由（xx/:id/:name），用于 tagsView 等中使用
            // 修复：https://gitee.com/lyt-top/vue-next-admin/issues/I3YX6G
            if (v.meta && v.path.indexOf('/:') > -1) {
                v.meta['isDynamic'] = true;
                v.meta['isDynamicPath'] = v.path;
            }
            newArr[0].children.push({...v});
            // 存 name 值，keep-alive 中 include 使用，实现路由的缓存
            // 路径：/@/layout/routerView/parent.vue
            if (newArr[0].meta?.isKeepAlive && v.meta?.isKeepAlive) {
                cacheList.push(v.name);
                const stores = useKeepAliveNamesStore(pinia);
                stores.setCacheKeepAlive(cacheList);
            }
        }
    });
    return newArr;
}

// 路由加载前
router.beforeEach(async (to, from) => {
    NProgress.configure({showSpinner: false});
    if (to.meta.title) NProgress.start();
    const token = session.get('token');
    if (to.path === '/login' && !token) {
        NProgress.done();
        return true;
    }
    if (!token) {
        session.clear();
        NProgress.done();
        return `/login?redirect=${to.path}&params=${JSON.stringify(to.query ? to.query : to.params)}`;
    }
    if (to.path === '/login') {
        NProgress.done();
        return '/tenant/dashboard/home';
    }
    const storesRoutesList = useRoutesListStore(pinia);
    const {routesList} = storeToRefs(storesRoutesList);
    if (routesList.value.length === 0) {
        if (useThemeConfigStore(pinia).themeConfig.isRequestRoutes) {
            const noPower = await initBackEndControlRoutes();
            if (noPower) {
                clearTokens();
                showWarning(t('message.login.account.noPermission'));
                return '/login';
            }
            const appRedirect = await resolveTenantAppRedirect(to);
            if (appRedirect) {
                return appRedirect;
            }
            return {path: to.path, query: to.query};
        }
        await initFrontEndControlRoutes();
        const appRedirect = await resolveTenantAppRedirect(to);
        if (appRedirect) {
            return appRedirect;
        }
        return {path: to.path, query: to.query};
    }
    const appRedirect = await resolveTenantAppRedirect(to);
    if (appRedirect) {
        return appRedirect;
    }
    return true;
});

// 路由加载后
router.afterEach(() => {
    NProgress.done();
});

// 导出路由
export default router;
