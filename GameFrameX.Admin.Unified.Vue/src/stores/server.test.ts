import { beforeEach, expect, test, vi } from 'vitest'
import { createPinia, setActivePinia } from 'pinia'
import { gameAreaPagePost } from '../api/gameArea'
import { local } from '../utils/storage'
import { useServerStore } from './server'
import { GameServerStatus } from '../api/gameArea/types'

vi.mock('../api/gameArea', () => ({ gameAreaPagePost: vi.fn() }))
const pageRequest = vi.mocked(gameAreaPagePost)

function pageResponse(items: { id: string; name?: string; serverStatus?: GameServerStatus }[], totalPages = 1) {
    return { data: { data: { items, totalPages } } } as unknown as Awaited<ReturnType<typeof gameAreaPagePost>>
}

beforeEach(() => {
    setActivePinia(createPinia())
    const stored = new Map<string, string>()
    vi.stubGlobal('localStorage', {
        getItem: (key: string) => stored.get(key) ?? null,
        setItem: (key: string, value: string) => stored.set(key, value),
        removeItem: (key: string) => stored.delete(key),
    })
    vi.resetAllMocks()
})

test('逐页加载完整列表、字符串 Id 保真并合并并发请求', async () => {
    pageRequest.mockResolvedValueOnce(pageResponse([{ id: '9007199254740993', name: 'First' }], 2))
        .mockResolvedValueOnce(pageResponse([{ id: '2', name: 'Second' }], 2))
    const store = useServerStore()
    await Promise.all([store.loadServers(), store.loadServers()])
    expect(pageRequest).toHaveBeenCalledTimes(2)
    expect(pageRequest).toHaveBeenNthCalledWith(2, { page: 2, pageSize: 100, orderBy: 'Id' })
    expect(store.servers.map(server => server.id)).toEqual(['9007199254740993', '2'])
    expect(store.selectedServerId).toBe('')
    await store.loadServers()
    expect(pageRequest).toHaveBeenCalledTimes(2)
})

test('恢复有效持久化选择，切换后写回存储', async () => {
    local.set('selectedServerId', '2')
    pageRequest.mockResolvedValue(pageResponse([{ id: '1' }, { id: '2' }]))
    const store = useServerStore()
    await store.loadServers()
    expect(store.selectedServerId).toBe('2')
    store.selectServer('1')
    expect(local.get('selectedServerId')).toBe('1')
})

test('失效 Id 和非法选择被清空，不自动切换到其他区服', async () => {
    local.set('selectedServerId', 'deleted')
    pageRequest.mockResolvedValue(pageResponse([{ id: '1' }]))
    const store = useServerStore()
    await store.loadServers()
    expect(store.selectedServerId).toBe('')
    store.selectServer('unknown')
    expect(local.get('selectedServerId')).toBeNull()
})

test('空列表是成功空态，损坏缓存不影响加载', async () => {
    localStorage.setItem(local.setKey('selectedServerId'), '{broken')
    pageRequest.mockResolvedValue(pageResponse([]))
    const store = useServerStore()
    await store.loadServers()
    expect(store.loaded).toBe(true)
    expect(store.loadFailed).toBe(false)
    expect(store.servers).toEqual([])
    expect(store.selectedServerId).toBe('')
})

test('刷新失败清除旧列表与选择并允许重试', async () => {
    pageRequest.mockResolvedValueOnce(pageResponse([{ id: '1' }]))
        .mockRejectedValueOnce(new Error('network'))
        .mockResolvedValueOnce(pageResponse([{ id: '2' }]))
    const store = useServerStore()
    await store.loadServers()
    store.selectServer('1')
    await expect(store.loadServers(true)).rejects.toThrow('network')
    expect(store.loadFailed).toBe(true)
    expect(store.loading).toBe(false)
    expect(store.servers).toEqual([])
    expect(store.selectedServerId).toBe('')
    await store.loadServers()
    expect(store.servers[0].id).toBe('2')
    expect(store.loadFailed).toBe(false)
})

test('退出后尚未完成的旧请求不能污染新会话', async () => {
    let finish!: (response: ReturnType<typeof pageResponse>) => void
    pageRequest.mockImplementationOnce(() => new Promise(resolve => { finish = resolve }))
    const store = useServerStore()
    const oldRequest = store.loadServers()
    store.resetServers()
    pageRequest.mockResolvedValueOnce(pageResponse([{ id: '2' }]))
    await store.loadServers()
    finish(pageResponse([{ id: '1' }]))
    await oldRequest
    expect(store.servers.map(server => server.id)).toEqual(['2'])
    expect(store.loading).toBe(false)
})


test('区服列表保留真实登记状态，缺省状态为未知', async () => {
    pageRequest.mockResolvedValue(pageResponse([
        { id: '1', serverStatus: GameServerStatus.Online },
        { id: '2', serverStatus: GameServerStatus.Offline },
        { id: '3', serverStatus: GameServerStatus.Maintenance },
        { id: '4', serverStatus: GameServerStatus.Full },
        { id: '5' },
    ]))
    const store = useServerStore()
    await store.loadServers()
    expect(store.servers.map(server => server.serverStatus)).toEqual([
        GameServerStatus.Online, GameServerStatus.Offline, GameServerStatus.Maintenance,
        GameServerStatus.Full, GameServerStatus.Unknown,
    ])
})
