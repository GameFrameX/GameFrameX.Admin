import { mount, flushPromises } from '@vue/test-utils'
import { beforeEach, expect, test, vi } from 'vitest'
import { createPinia, setActivePinia } from 'pinia'
import { createI18n } from 'vue-i18n'
import PrimeVue from 'primevue/config'
import Select from 'primevue/select'
import ServerSelector from './ServerSelector.vue'
import { useServerStore } from '../../stores/server'
import { gameAreaPagePost } from '../../api/gameArea'

vi.mock('../../api/gameArea', () => ({ gameAreaPagePost: vi.fn() }))
const pageRequest = vi.mocked(gameAreaPagePost)
const translations = { message: { menu: { selectServer: 'Select server', noServersAvailable: 'No servers available', loadServersFailed: 'Load failed' } } }

function renderSelector(props: { mode?: 'global' | 'local'; modelValue?: string } = {}) {
    return mount(ServerSelector, {
        props,
        global: { plugins: [PrimeVue, createI18n({ legacy: false, locale: 'en', messages: { en: translations } })] },
    })
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
    pageRequest.mockResolvedValue({ data: { data: { items: [{ id: '1', name: 'First' }, { id: '2', name: 'Second' }] } } } as unknown as Awaited<ReturnType<typeof gameAreaPagePost>>)
})

test('全局选择写 store 并显示已选区服', async () => {
    const wrapper = renderSelector()
    await flushPromises()
    wrapper.getComponent(Select).vm.$emit('update:modelValue', '2')
    await flushPromises()
    expect(useServerStore().selectedServerId).toBe('2')
    expect(wrapper.text()).toContain('Second')
    expect(wrapper.emitted('update:modelValue')).toEqual([['2']])
    wrapper.unmount()
})

test('页内模式继承全局值，覆盖只更新 v-model', async () => {
    const store = useServerStore()
    await store.loadServers()
    store.selectServer('1')
    const wrapper = renderSelector({ mode: 'local' })
    await flushPromises()
    expect(wrapper.text()).toContain('First')
    wrapper.getComponent(Select).vm.$emit('update:modelValue', '2')
    expect(store.selectedServerId).toBe('1')
    expect(wrapper.emitted('update:modelValue')).toEqual([['2']])
    await wrapper.setProps({ modelValue: '2' })
    store.selectServer('')
    await flushPromises()
    expect(wrapper.text()).toContain('Second')
    wrapper.unmount()
})

test('空区服列表显示空态且禁用选择', async () => {
    pageRequest.mockResolvedValue({ data: { data: { items: [] } } } as unknown as Awaited<ReturnType<typeof gameAreaPagePost>>)
    const wrapper = renderSelector()
    await flushPromises()
    expect(wrapper.get('[role="status"]').text()).toBe('No servers available')
    expect(wrapper.getComponent(Select).props('disabled')).toBe(true)
    wrapper.unmount()
})

test('加载失败显示错误而非成功空态', async () => {
    pageRequest.mockRejectedValue(new Error('network'))
    const wrapper = renderSelector()
    await flushPromises()
    expect(wrapper.get('[role="alert"]').text()).toBe('Load failed')
    expect(wrapper.find('[role="status"]').exists()).toBe(false)
    wrapper.unmount()
})
