import {mount, flushPromises} from '@vue/test-utils';
import {beforeEach, expect, test, vi} from 'vitest';
import {createPinia, setActivePinia} from 'pinia';
import PrimeVue from 'primevue/config';
import TopBar from './index.vue';
import {gameAreaPagePost} from '../../../api/gameArea';

const {userStore} = vi.hoisted(() => ({userStore: {isSuperAdmin: true}}));
vi.mock('../../../stores/userInfo', () => ({useUserInfoStore: () => userStore}));
vi.mock('../../../api/gameArea', () => ({gameAreaPagePost: vi.fn()}));
vi.mock('vue-i18n', () => ({useI18n: () => ({t: (key: string) => key})}));
vi.mock('./UserDropdown.vue', () => ({__esModule: true, default: {template: '<div />'}}));
const pageRequest = vi.mocked(gameAreaPagePost);

beforeEach(() => {
  setActivePinia(createPinia());
  vi.stubGlobal('localStorage', {getItem: () => null, setItem: vi.fn(), removeItem: vi.fn()});
  vi.clearAllMocks();
  userStore.isSuperAdmin = true;
  pageRequest.mockResolvedValue({data: {data: {items: []}}} as unknown as Awaited<ReturnType<typeof gameAreaPagePost>>);
});

test('超管顶栏不挂载区服选择器，也不加载区服', async () => {
  const wrapper = mount(TopBar, {global: {plugins: [PrimeVue]}});
  await flushPromises();
  expect(wrapper.find('.server-selector').exists()).toBe(false);
  expect(pageRequest).not.toHaveBeenCalled();
  wrapper.unmount();
});

test('普通管理员顶栏挂载全局区服选择器并显示空态', async () => {
  userStore.isSuperAdmin = false;
  const wrapper = mount(TopBar, {global: {plugins: [PrimeVue]}});
  await flushPromises();
  expect(wrapper.find('.server-selector').exists()).toBe(true);
  expect(pageRequest).toHaveBeenCalledTimes(1);
  expect(wrapper.text()).toContain('noServersAvailable');
  wrapper.unmount();
});
