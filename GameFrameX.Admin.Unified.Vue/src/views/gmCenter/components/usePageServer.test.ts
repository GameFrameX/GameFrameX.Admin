import { beforeEach, expect, test, vi } from 'vitest'
import { createPinia, setActivePinia } from 'pinia'
import { effectScope, ref } from 'vue'
import { useServerStore } from '../../../stores/server'
import { usePageServer } from './usePageServer'

vi.mock('../../../api/gameArea', () => ({ gameAreaPagePost: vi.fn() }))

beforeEach(() => {
    setActivePinia(createPinia())
})

function createSelection() {
    const store = useServerStore()
    store.servers = [{ id: '9007199254740993', name: 'First' }, { id: '2', name: 'Second' }]
    store.selectedServerId = '9007199254740993'
    const scope = effectScope()
    const selection = scope.run(() => usePageServer())!
    return { store, scope, selection }
}

test('缓存选择在列表验证完成前不可请求；完整 string Id 原样使用', () => {
    const { store, scope, selection } = createSelection()
    expect(selection.beginServerRequest()).toBeUndefined()
    store.loaded = true
    expect(selection.beginServerRequest()?.serverId).toBe('9007199254740993')
    store.loading = true
    expect(selection.beginServerRequest()).toBeUndefined()
    scope.stop()
})

test('继承全局、页内覆盖、显式清空和恢复继承互不混淆', () => {
    const { store, scope, selection } = createSelection()
    store.loaded = true
    expect(selection.serverIdOverride.value).toBeUndefined()
    store.selectedServerId = '2'
    expect(selection.effectiveServerId.value).toBe('2')
    selection.serverIdOverride.value = '9007199254740993'
    expect(store.selectedServerId).toBe('2')
    expect(selection.effectiveServerId.value).toBe('9007199254740993')
    selection.serverIdOverride.value = ''
    expect(selection.beginServerRequest()).toBeUndefined()
    selection.serverIdOverride.value = undefined
    expect(selection.beginServerRequest()?.serverId).toBe('2')
    scope.stop()
})

test('无区服、失败和失效覆盖都拒绝请求并区分反馈', () => {
    const { store, scope, selection } = createSelection()
    store.loaded = true
    selection.serverIdOverride.value = 'removed'
    expect(selection.beginServerRequest()).toBeUndefined()
    store.servers = []
    expect(selection.serverStateMessage.value).toBe('message.menu.noServersAvailable')
    store.loadFailed = true
    expect(selection.serverStateMessage.value).toBe('message.menu.loadServersFailed')
    expect(selection.beginServerRequest()).toBeUndefined()
    scope.stop()
})

test('同服重查、切服后切回与卸载都使旧响应失效', () => {
    const { store, scope, selection } = createSelection()
    store.loaded = true
    const first = selection.beginServerRequest()!
    const second = selection.beginServerRequest()!
    expect(first.isCurrent()).toBe(false)
    expect(second.isCurrent()).toBe(true)
    store.selectedServerId = '2'
    store.selectedServerId = first.serverId
    expect(second.isCurrent()).toBe(false)
    const third = selection.beginServerRequest()!
    scope.stop()
    expect(third.isCurrent()).toBe(false)
    expect(selection.beginServerRequest()).toBeUndefined()
})

test('刷新列表及注销使进行中的旧服请求失效', () => {
    const { store, scope, selection } = createSelection()
    store.loaded = true
    const previous = selection.beginServerRequest()!
    store.loading = true
    store.loading = false
    expect(previous.isCurrent()).toBe(false)
    const current = selection.beginServerRequest()!
    store.resetServers()
    expect(current.isCurrent()).toBe(false)
    scope.stop()
})

test('弹窗继承所属页面的目标，页面目标失效后不能提交', () => {
    const { store, scope } = createSelection()
    store.loaded = true
    const pageServerId = ref('2')
    const dialogScope = effectScope()
    const selection = dialogScope.run(() => usePageServer(pageServerId))!
    const operation = selection.beginServerRequest()!
    expect(operation.serverId).toBe('2')
    pageServerId.value = ''
    expect(operation.isCurrent()).toBe(false)
    expect(selection.beginServerRequest()).toBeUndefined()
    scope.stop()
    dialogScope.stop()
})
