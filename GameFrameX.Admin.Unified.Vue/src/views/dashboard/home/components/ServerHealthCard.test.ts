import {mount, flushPromises} from '@vue/test-utils';
import {beforeEach, expect, test, vi} from 'vitest';
import {createPinia, setActivePinia} from 'pinia';
import PrimeVue from 'primevue/config';
import Button from 'primevue/button';
import ServerHealthCard from './ServerHealthCard.vue';
import {useServerStore} from '../../../../stores/server';
import {gameAreaPagePost} from '../../../../api/gameArea';
import {systemEnumEnumDataListGet} from '../../../../api/systemEnum';

vi.mock('../../../../api/gameArea', () => ({gameAreaPagePost: vi.fn()}));
vi.mock('../../../../api/systemEnum', () => ({systemEnumEnumDataListGet: vi.fn()}));
vi.mock('../../../../i18n/index', () => ({t: (key: string) => key}));
vi.mock('../../../../utils/message', () => ({showSuccess: vi.fn(), showError: vi.fn()}));
vi.mock('vue-clipboard3', () => ({default: () => ({toClipboard: vi.fn()})}));
const pageRequest = vi.mocked(gameAreaPagePost);
const enumRequest = vi.mocked(systemEnumEnumDataListGet);

function renderCard() {
  return mount(ServerHealthCard, {global: {plugins: [PrimeVue]}});
}

beforeEach(() => {
  setActivePinia(createPinia());
  const stored = new Map<string, string>();
  vi.stubGlobal('localStorage', {
    getItem: (key: string) => stored.get(key) ?? null,
    setItem: (key: string, value: string) => stored.set(key, value),
    removeItem: (key: string) => stored.delete(key),
  });
  vi.resetAllMocks();
  enumRequest.mockResolvedValue({data: {data: [
    {value: 0, describe: '未知'}, {value: 1, describe: '在线'}, {value: 2, describe: '离线'},
    {value: 3, describe: '维护中'}, {value: 4, describe: '满员'},
  ]}} as Awaited<ReturnType<typeof systemEnumEnumDataListGet>>);
});

test('按真实登记状态统计全部区服，复用共享枚举描述并响应选择变化', async () => {
  pageRequest.mockResolvedValue({data: {data: {items: [
    {id: '1', name: 'First', serverStatus: 1}, {id: '2', name: 'Second', serverStatus: 1},
    {id: '3', serverStatus: 2}, {id: '4', serverStatus: 3}, {id: '5', serverStatus: 4}, {id: '6'},
  ]}}} as unknown as Awaited<ReturnType<typeof gameAreaPagePost>>);
  const store = useServerStore();
  await store.loadServers();
  const wrapper = renderCard();
  await flushPromises();
  expect(enumRequest).toHaveBeenCalledWith({enumName: 'GameServerStatus'});
  expect(wrapper.get('[data-server-status="1"] dt').text()).toBe('在线');
  expect(wrapper.findAll('dd').map(item => item.text())).toEqual(['2', '1', '1', '1', '1']);
  expect(wrapper.text()).toContain('message.home.serverHealthDescription');
  store.selectServer('2');
  await flushPromises();
  expect(wrapper.get('.selected-server').text()).toContain('Second');
  expect(pageRequest).toHaveBeenCalledTimes(1);
  wrapper.unmount();
});

test('失败显示错误、隐藏旧计数，并可从卡片刷新恢复为空态', async () => {
  pageRequest.mockRejectedValueOnce(new Error('offline'));
  const store = useServerStore();
  await store.loadServers().catch(() => {});
  const wrapper = renderCard();
  await flushPromises();
  expect(wrapper.get('[role="alert"]').text()).toContain('loadServersFailed');
  expect(wrapper.findAll('dd')).toHaveLength(0);
  pageRequest.mockResolvedValueOnce({data: {data: {items: []}}} as unknown as Awaited<ReturnType<typeof gameAreaPagePost>>);
  await wrapper.getComponent(Button).trigger('click');
  await flushPromises();
  expect(wrapper.find('[role="alert"]').exists()).toBe(false);
  expect(wrapper.get('[role="status"]').text()).toContain('noServersAvailable');
  wrapper.unmount();
});
