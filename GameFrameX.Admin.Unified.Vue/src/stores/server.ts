import { defineStore } from 'pinia'
import { ref } from 'vue'
import { gameAreaPagePost } from '../api/gameArea'
import { local } from '../utils/storage'
import { GameServerStatus } from '../api/gameArea/types'

export interface ServerOption {
    id: string
    name: string
    serverStatus: GameServerStatus
}

const selectionStorageKey = 'selectedServerId'

function readSelection(): string {
    try {
        const selected = local.get(selectionStorageKey)
        return typeof selected === 'string' ? selected : ''
    } catch {
        return ''
    }
}

export const useServerStore = defineStore('server', () => {
    const servers = ref<ServerOption[]>([])
    const selectedServerId = ref(readSelection())
    const loading = ref(false)
    const loaded = ref(false)
    const loadFailed = ref(false)
    let pending: Promise<ServerOption[]> | undefined
    let generation = 0

    function saveSelection(serverId: string): void {
        selectedServerId.value = serverId
        try {
            if (serverId) local.set(selectionStorageKey, serverId)
            else local.remove(selectionStorageKey)
        } catch {
            // 浏览器禁用存储时保留本次会话内的选择。
        }
    }

    function selectServer(serverId: string): void {
        saveSelection(servers.value.some(server => server.id === serverId) ? serverId : '')
    }

    function loadServers(force = false): Promise<ServerOption[]> {
        if (pending) return pending
        if (loaded.value && !force) return Promise.resolve(servers.value)
        const requestGeneration = generation
        loading.value = true
        loadFailed.value = false
        pending = (async () => {
            try {
                const options = new Map<string, ServerOption>()
                let page = 1
                while (true) {
                    const response = await gameAreaPagePost({ page, pageSize: 100, orderBy: 'Id' })
                    if (requestGeneration !== generation) return []
                    const result = response.data.data
                    for (const server of result.items ?? []) {
                        const identifier = server.id == null ? '' : String(server.id)
                        if (identifier) options.set(identifier, { id: identifier, name: server.name || identifier, serverStatus: server.serverStatus ?? GameServerStatus.Unknown })
                    }
                    if (!(result.hasNextPage || page < (result.totalPages ?? 0))) break
                    page += 1
                }
                servers.value = [...options.values()]
                loaded.value = true
                selectServer(selectedServerId.value)
                return servers.value
            } catch (error) {
                if (requestGeneration !== generation) return []
                servers.value = []
                loaded.value = false
                loadFailed.value = true
                saveSelection('')
                throw error
            } finally {
                if (requestGeneration === generation) {
                    loading.value = false
                    pending = undefined
                }
            }
        })()
        return pending
    }

    function resetServers(): void {
        generation += 1
        pending = undefined
        servers.value = []
        loaded.value = false
        loading.value = false
        loadFailed.value = false
        saveSelection('')
    }

    return { servers, selectedServerId, loading, loaded, loadFailed, loadServers, selectServer, resetServers }
})
