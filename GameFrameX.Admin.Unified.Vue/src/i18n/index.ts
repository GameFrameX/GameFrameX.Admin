import {createI18n} from 'vue-i18n';
import {useThemeConfigStore} from '../stores/themeConfig';
import {local} from '../utils/storage';

// 定义语言国际化内容

/**
 * 说明：
 * 须在 pages 下新建文件夹（建议 `要国际化界面目录` 与 `i18n 目录` 相同，方便查找），
 * 注意国际化定义的字段，不要与原有的定义字段相同。
 * 1、/src/i18n/lang 下的 ts 为框架的国际化内容
 * 2、/src/i18n/pages 下的 ts 为各界面的国际化内容
 */

// 定义变量内容
const messages: Record<string, EmptyObjectType> = {};
const itemize: Record<string, EmptyObjectType[]> = {en: [], 'zh-cn': [], 'zh-tw': [], ja: [], ko: []};
const modules: Record<string, any> = import.meta.glob('./**/*.ts', {eager: true});

// 对自动引入的 modules 进行分类 en、zh-cn、zh-tw、ja、ko
// https://vitejs.cn/vite3-cn/guide/features.html#glob-import
for (const path in modules) {
    const key = path.match(/(\S+)\/(\S+).ts/);
    const localeKey = key?.[2];
    if (!localeKey) continue;
    if (itemize[localeKey]) itemize[localeKey].push(modules[path].default);
    else itemize[localeKey] = [modules[path].default ?? modules[path]];
}

// 深度合并对象
function deepMerge(target: EmptyObjectType, source: EmptyObjectType): EmptyObjectType {
    const result = {...target};
    for (const key of Object.keys(source)) {
        if (source[key] && typeof source[key] === 'object' && !Array.isArray(source[key]) &&
            result[key] && typeof result[key] === 'object' && !Array.isArray(result[key])) {
            result[key] = deepMerge(result[key] as EmptyObjectType, source[key] as EmptyObjectType);
        } else {
            result[key] = source[key];
        }
    }
    return result;
}

// 合并数组对象（非标准数组对象，数组中对象的每项 key、value 都不同）
function mergeArrObj(list: Record<string, EmptyObjectType[]>, key: string) {
    let obj: EmptyObjectType = {};
    list[key].forEach((i: EmptyObjectType) => {
        obj = deepMerge(obj, i);
    });
    return obj;
}

// 处理最终格式
for (const key in itemize) {
    const merged = mergeArrObj(itemize, key);
    messages[key] = {
        name: key,
        ...merged,
        message: merged,
    };
}

// 注意：本模块不得在初始化期 useThemeConfigStore——模块加载序里 pinia 插件（persistedstate）可能尚未注册，
// 提前创建 store 会导致其持久化静默失效（2026-08-30 实测 _p=0）。初始语言直读 localStorage，store 同步全部推迟到运行期。

// 支持的语言清单（与 itemize 分类键一致）
export const SUPPORTED_LOCALES = ['zh-cn', 'zh-tw', 'en', 'ja', 'ko'];

// 浏览器语言 → 支持语言映射：按 navigator.languages 顺序取第一个可识别的（繁简/日/韩/英前缀区分），全部识别不出回退产品默认
function detectBrowserLocale(): string {
    const candidates = (navigator.languages && navigator.languages.length > 0) ? navigator.languages : [navigator.language];
    for (const tag of candidates) {
        const lower = (tag || '').toLowerCase();
        if (!lower) {
            continue;
        }
        if (lower === 'zh-tw' || lower === 'zh-hk' || lower === 'zh-mo' || lower.startsWith('zh-hant')) {
            return 'zh-tw';
        }
        if (lower.startsWith('zh')) {
            return 'zh-cn';
        }
        if (lower.startsWith('ja')) {
            return 'ja';
        }
        if (lower.startsWith('ko')) {
            return 'ko';
        }
        if (lower.startsWith('en')) {
            return 'en';
        }
    }
    return 'zh-cn';
}

// 初始语言：已有持久化偏好（用户选过或此前检测固化）则沿用；首次访问按浏览器语言识别。
// 只解析不写回——固化由运行期（App 挂载后 store 可用时）执行，避免模块期写 store 触发上述时序问题。
function resolveInitialLocale(): string {
    const stored = local.get('themeConfig')?.globalI18n;
    if (typeof stored === 'string' && (SUPPORTED_LOCALES as string[]).includes(stored)) {
        return stored;
    }
    return detectBrowserLocale();
}

// 导出语言国际化
// https://vue-i18n.intlify.dev/guide/essentials/fallback.html#explicit-fallback-with-one-locale
export const i18n = createI18n({
    legacy: false,
    silentTranslationWarn: true,
    missingWarn: false,
    silentFallbackWarn: true,
    fallbackWarn: false,
    locale: resolveInitialLocale(),
    fallbackLocale: 'zh-cn',
    messages,
});

export const {t, te, locale} = i18n.global;

// 运行期调用（App 挂载后）：把当前 locale 同步回 themeConfig store。
// 首访检测的固化、以及登录前切换语言的持久化都经此路径（运行期 store 的 persist 已就绪）。
export function syncLocaleToStore(): void {
    const stores = useThemeConfigStore();
    if (stores.themeConfig.globalI18n !== locale.value) {
        stores.themeConfig.globalI18n = locale.value;
    }
}
