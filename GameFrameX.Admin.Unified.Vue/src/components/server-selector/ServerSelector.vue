<template>
    <div class="server-selector">
        <Select
            :modelValue="currentServerId"
            :options="serverStore.servers"
            optionLabel="name"
            optionValue="id"
            :loading="serverStore.loading"
            :disabled="disabled || serverStore.loading || serverStore.servers.length === 0"
            :placeholder="t('message.menu.selectServer')"
            :aria-label="t('message.menu.selectServer')"
            :emptyMessage="t('message.menu.noServersAvailable')"
            filter
            showClear
            @update:modelValue="changeServer"
        />
        <small v-if="serverStore.loadFailed" role="alert">{{ t('message.menu.loadServersFailed') }}</small>
        <small v-else-if="serverStore.loaded && serverStore.servers.length === 0" role="status">{{ t('message.menu.noServersAvailable') }}</small>
    </div>
</template>

<script setup lang="ts">
import { computed, onMounted } from 'vue'
import { useI18n } from 'vue-i18n'
import Select from 'primevue/select'
import { useServerStore } from '../../stores/server'

const props = withDefaults(defineProps<{
    mode?: 'global' | 'local'
    modelValue?: string
    disabled?: boolean
}>(), { mode: 'global', modelValue: undefined, disabled: false })
const emit = defineEmits<{ 'update:modelValue': [serverId: string] }>()
const { t } = useI18n()
const serverStore = useServerStore()
const currentServerId = computed(() => props.mode === 'local'
    ? props.modelValue ?? serverStore.selectedServerId
    : serverStore.selectedServerId)

function changeServer(value: string | null): void {
    const serverId = serverStore.servers.some(server => server.id === value) ? value! : ''
    if (props.mode === 'global') serverStore.selectServer(serverId)
    emit('update:modelValue', serverId)
}

onMounted(() => {
    void serverStore.loadServers().catch(() => {
        // 加载失败由可见错误状态反馈，下一次挂载或显式刷新可重试。
    })
})
</script>
