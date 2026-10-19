import {onUnmounted, ref} from 'vue'

/**
 * 媒体查询响应式判定（change C252）。
 *
 * 返回的 ref 随视口变化自动更新，供组件按视口切换渲染逻辑
 * （如 CrudDataTable 的 responsivePriority 列过滤、组织树侧栏折叠）。
 * 阈值语义与 `src/theme/breakpoints.scss` 的 bp-* mixin 保持一致。
 *
 * @param query 标准 media query 字符串，如 '(max-width: 767px)'
 */
export function useMediaQueryMatch(query: string) {
    const matches = ref(false)

    // SSR / 测试环境无 matchMedia 时保持 false，不抛错
    if (typeof window !== 'undefined' && typeof window.matchMedia === 'function') {
        const mql = window.matchMedia(query)
        matches.value = mql.matches

        const onChange = (e: MediaQueryListEvent) => {
            matches.value = e.matches
        }
        // 兼容旧 Safari（Safari < 14 无 addEventListener）
        if (typeof mql.addEventListener === 'function') {
            mql.addEventListener('change', onChange)
            onUnmounted(() => mql.removeEventListener('change', onChange))
        } else {
            mql.addListener(onChange)
            onUnmounted(() => mql.removeListener(onChange))
        }
    }

    return matches
}
