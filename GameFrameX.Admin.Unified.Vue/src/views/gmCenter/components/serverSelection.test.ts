import { mount, flushPromises } from '@vue/test-utils'
import { beforeEach, expect, test, vi } from 'vitest'
import { createPinia, setActivePinia } from 'pinia'
import { createI18n } from 'vue-i18n'
import PrimeVue from 'primevue/config'
import Select from 'primevue/select'
import OnlineDashboard from '../onlineDashboard/index.vue'
import AssetAudit from '../assetAudit/index.vue'
import OnlineCursorList from '../../../components/OnlineCursorList/index.vue'
import { useServerStore } from '../../../stores/server'
import { gameAreaPagePost } from '../../../api/gameArea'
import { onlineOverviewQuerySnapshotPost } from '../../../api/onlineOverviewQuery'
import { onlineAssetAuditQueryPagePost } from '../../../api/onlineAssetAuditQuery'

vi.mock('../../../api/gameArea', () => ({ gameAreaPagePost: vi.fn() }))
vi.mock('../../../api/onlineOverviewQuery', () => ({ onlineOverviewQuerySnapshotPost: vi.fn() }))
vi.mock('../../../api/onlineAssetAuditQuery', () => ({ onlineAssetAuditQueryPagePost: vi.fn() }))
vi.mock('../../../i18n/index', () => ({ t: (key: string) => key }))

function response(data: unknown): any {
    return { data: { data } }
}

function deferredResponse() {
    let resolve!: (value: any) => void
    const promise = new Promise<any>(complete => { resolve = complete })
    return { promise, resolve }
}

function renderPage(component: any) {
    return mount(component, {
        global: {
            plugins: [PrimeVue, createI18n({ legacy: false, locale: 'en', missingWarn: false, fallbackWarn: false })],
            stubs: { SearchForm: true },
            directives: { blockLoading: () => undefined },
        },
    })
}

beforeEach(() => {
    vi.resetAllMocks()
    vi.stubGlobal('localStorage', { getItem: () => null, setItem: () => undefined, removeItem: () => undefined })
    setActivePinia(createPinia())
    vi.mocked(gameAreaPagePost).mockResolvedValue(response({ items: [{ id: '9007199254740993', name: 'First' }, { id: '2', name: 'Second' }] }))
})

test('快照页等待区服校验、响应全局选择且切服后忽略迟到响应', async () => {
    const servers = deferredResponse()
    vi.mocked(gameAreaPagePost).mockReturnValueOnce(servers.promise)
    const store = useServerStore()
    store.selectedServerId = '9007199254740993'
    const first = deferredResponse()
    vi.mocked(onlineOverviewQuerySnapshotPost).mockReturnValueOnce(first.promise)
        .mockResolvedValue(response({ onlinePlayerCount: 22 }))
    const wrapper = renderPage(OnlineDashboard)
    expect(onlineOverviewQuerySnapshotPost).not.toHaveBeenCalled()
    servers.resolve(response({ items: [{ id: '9007199254740993' }, { id: '2' }] }))
    await flushPromises()
    expect(onlineOverviewQuerySnapshotPost).toHaveBeenCalledWith({ serverId: '9007199254740993' })
    store.selectServer('2')
    await flushPromises()
    expect(wrapper.get('[data-metric="onlinePlayerCount"]').text()).toBe('22')
    first.resolve(response({ onlinePlayerCount: 11 }))
    await flushPromises()
    expect(wrapper.get('[data-metric="onlinePlayerCount"]').text()).toBe('22')
    wrapper.getComponent(Select).vm.$emit('update:modelValue', '')
    await flushPromises()
    expect(store.selectedServerId).toBe('2')
    expect(wrapper.get('[data-metric="onlinePlayerCount"]').text()).toBe('-')
    expect(onlineOverviewQuerySnapshotPost).toHaveBeenCalledTimes(2)
    await wrapper.get('button.p-button-text').trigger('click')
    await flushPromises()
    expect(onlineOverviewQuerySnapshotPost).toHaveBeenCalledTimes(3)
    wrapper.unmount()
})

test('游标页切服重建首页，旧服加载更多不会写入新服列表', async () => {
    const store = useServerStore()
    await store.loadServers()
    store.selectServer('9007199254740993')
    const oldPage = deferredResponse()
    vi.mocked(onlineAssetAuditQueryPagePost)
        .mockResolvedValueOnce(response({ items: [{ commandType: 'first-server' }], nextCursor: 'previous-server-cursor', hasMore: true }))
        .mockReturnValueOnce(oldPage.promise)
        .mockResolvedValueOnce(response({ items: [{ commandType: 'second-server' }], hasMore: false }))
    const wrapper = renderPage(AssetAudit)
    await flushPromises()
    expect(wrapper.text()).toContain('first-server')
    await wrapper.get('button.online-cursor-list-load-more').trigger('click')
    expect(vi.mocked(onlineAssetAuditQueryPagePost).mock.calls[1][0]).toMatchObject({ cursor: 'previous-server-cursor' })
    store.selectServer('2')
    await flushPromises()
    const lastRequest = vi.mocked(onlineAssetAuditQueryPagePost).mock.calls[2][0]
    expect(lastRequest).toMatchObject({ serverId: '2' })
    expect(lastRequest.cursor).toBeUndefined()
    expect(wrapper.text()).toContain('second-server')
    oldPage.resolve(response({ items: [{ commandType: 'late-first-server' }], hasMore: false }))
    await flushPromises()
    expect(wrapper.text()).not.toContain('late-first-server')
    store.selectServer('')
    await flushPromises()
    expect(wrapper.findComponent(OnlineCursorList).exists()).toBe(false)
    expect(onlineAssetAuditQueryPagePost).toHaveBeenCalledTimes(3)
    wrapper.unmount()
})
