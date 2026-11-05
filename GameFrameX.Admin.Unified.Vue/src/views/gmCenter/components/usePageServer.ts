import { computed, onScopeDispose, ref, toValue, watch, type MaybeRefOrGetter } from 'vue'
import { useServerStore } from '../../../stores/server'

/** 页内覆盖不写全局选择；缓存区服通过列表校验后才可请求。 */
export function usePageServer(inheritedServerId?: MaybeRefOrGetter<string | undefined>) {
    const serverStore = useServerStore()
    const serverIdOverride = ref<string | undefined>(undefined)
    const effectiveServerId = computed(() => serverIdOverride.value ?? toValue(inheritedServerId) ?? serverStore.selectedServerId)
    const canQueryServer = computed(() => serverStore.loaded && !serverStore.loading && !serverStore.loadFailed
        && effectiveServerId.value !== '' && serverStore.servers.some(server => server.id === effectiveServerId.value))
    const serverStateMessage = computed(() => {
        if (serverStore.loadFailed) return 'message.menu.loadServersFailed'
        if (serverStore.loaded && serverStore.servers.length === 0) return 'message.menu.noServersAvailable'
        return 'message.menu.selectServer'
    })
    const serverSelectionVersion = ref(0)
    let requestVersion = 0
    let disposed = false

    watch([effectiveServerId, canQueryServer], () => {
        serverSelectionVersion.value += 1
        requestVersion += 1
    }, { flush: 'sync' })
    onScopeDispose(() => {
        disposed = true
        requestVersion += 1
    })

    /** 同服重查、切服再切回、刷新列表或卸载后，旧请求均不得回写。 */
    function beginServerRequest() {
        if (!canQueryServer.value || disposed) return undefined
        const version = ++requestVersion
        const serverId = effectiveServerId.value
        return {
            serverId,
            isCurrent: () => !disposed && canQueryServer.value && requestVersion === version,
        }
    }

    return { serverIdOverride, effectiveServerId, canQueryServer, serverStateMessage, serverSelectionVersion, beginServerRequest }
}
