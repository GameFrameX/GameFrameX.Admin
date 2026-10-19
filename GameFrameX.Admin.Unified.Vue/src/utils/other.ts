import {nextTick, defineAsyncComponent} from 'vue';
import type {App} from 'vue';
import router from '../router/index';
import pinia from '../stores/index';
import {storeToRefs} from 'pinia';
import {useThemeConfigStore} from '../stores/themeConfig';
import {local} from './storage';

// 引入组件
// eslint-disable-next-line @typescript-eslint/naming-convention -- Vue 组件占位变量（defineAsyncComponent/defineComponent 绑定），按 Vue 风格指南保持 PascalCase
const SvgIcon = defineAsyncComponent(() => import('/@/components/svg-icon/index.vue'));

/**
 * 导出全局注册 SvgIcon 组件
 * @param app vue 实例
 */
export function elSvg(app: App) {
    app.component('SvgIcon', SvgIcon);
}

/**
 * 设置浏览器标题国际化
 * @method const title = useTitle(); ==> title()
 */
export function useTitle() {
    const stores = useThemeConfigStore(pinia);
    const {themeConfig} = storeToRefs(stores);
    nextTick(() => {
        let webTitle = '';
        let globalTitle: string = themeConfig.value.globalTitle;
        const {path, meta} = router.currentRoute.value;
        if (path === '/login') {
            webTitle = <string>meta.title;
        } else {
            webTitle = setTagsViewNameI18n(router.currentRoute.value);
        }
        document.title = `${webTitle} - ${globalTitle}` || globalTitle;
    });
}

/**
 * 设置 自定义 tagsView 名称、 自定义 tagsView 名称国际化
 * @param params 路由 query、params 中的 tagsViewName
 * @returns 返回当前 tagsViewName 名称
 */
export function setTagsViewNameI18n(item: any) {
    let tagsViewName: string = '';
    const {query, params, meta} = item;
    // 修复tagsViewName匹配到其他含下列单词的路由
    const pattern = /^\{("(zh-cn|en|zh-tw|ja|ko)":"[^,]+",?){1,5}}$/;
    if (query?.tagsViewName || params?.tagsViewName) {
        if (pattern.test(query?.tagsViewName) || pattern.test(params?.tagsViewName)) {
            // 国际化
            const urlTagsParams = (query?.tagsViewName && JSON.parse(query?.tagsViewName)) || (params?.tagsViewName && JSON.parse(params?.tagsViewName));
            tagsViewName = urlTagsParams[locale.value];
        } else {
            // 非国际化
            tagsViewName = query?.tagsViewName || params?.tagsViewName;
        }
    } else {
        // 非自定义 tagsView 名称
        tagsViewName = t(meta.titleCode || meta.title);
    }
    return tagsViewName;
}

/**
 * 图片懒加载
 * @param el dom 目标元素
 * @param arr 列表数据
 * @description data-xxx 属性用于存储页面或应用程序的私有自定义数据
 */
export const lazyImage = (el: string, arr: EmptyArrayType) => {
    const io = new IntersectionObserver((res) => {
        res.forEach((v: any) => {
            if (v.isIntersecting) {
                const {img, key} = v.target.dataset;
                v.target.src = img;
                v.target.onload = () => {
                    io.unobserve(v.target);
                    arr[key]['loading'] = false;
                };
            }
        });
    });
    nextTick(() => {
        document.querySelectorAll(el).forEach((img) => io.observe(img));
    });
};

/**
 * 全局组件大小
 * @returns 返回 `window.localStorage` 中读取的缓存值 `globalComponentSize`
 */
export const globalComponentSize = (): string => {
    const stores = useThemeConfigStore(pinia);
    const {themeConfig} = storeToRefs(stores);
    return local.get('themeConfig')?.globalComponentSize || themeConfig.value?.globalComponentSize;
};

/**
 * 判断是否是移动端
 */
export function isMobile() {
    if (navigator.userAgent.match(/('phone|pad|pod|iPhone|iPod|ios|iPad|Android|Mobile|BlackBerry|IEMobile|MQQBrowser|JUC|Fennec|wOSBrowser|BrowserNG|WebOS|Symbian|Windows Phone')/i)) {
        return true;
    } else {
        return false;
    }
}

/**
 * 统一批量导出
 * @method elSvg 导出全局注册 element plus svg 图标
 * @method useTitle 设置浏览器标题国际化
 * @method setTagsViewNameI18n 设置 自定义 tagsView 名称、 自定义 tagsView 名称国际化
 * @method lazyImage 图片懒加载
 * @method globalComponentSize() element plus 全局组件大小
 * @method isMobile 判断是否是移动端
 */
const other = {
    elSvg: (app: App) => {
        elSvg(app);
    },
    useTitle: () => {
        useTitle();
    },
    setTagsViewNameI18n(route: RouteToFrom) {
        return setTagsViewNameI18n(route);
    },
    lazyImage: (el: string, arr: EmptyArrayType) => {
        lazyImage(el, arr);
    },
    globalComponentSize: () => {
        return globalComponentSize();
    },
    isMobile: () => {
        return isMobile();
    },
};

// 统一批量导出
export default other;
