import {mount, flushPromises} from '@vue/test-utils';
import {beforeEach, expect, test, vi} from 'vitest';
import {createRouter, createMemoryHistory} from 'vue-router';
import DashboardHome from './index.vue';
import {gameHomeOverviewPost, gameHomeOnlineTrendPost} from '../../../api/gameHome';

const {userStore} = vi.hoisted(() => ({
  userStore: {isSuperAdmin: true, userInfos: {account: 'superadmin', realName: ''}},
}));
vi.mock('../../../stores/userInfo', () => ({useUserInfoStore: () => userStore}));
vi.mock('../../../i18n/index', () => ({t: (key: string) => key}));
vi.mock('../../../api/gameHome', () => ({gameHomeOverviewPost: vi.fn(), gameHomeOnlineTrendPost: vi.fn()}));

const overviewRequest = vi.mocked(gameHomeOverviewPost);
const trendRequest = vi.mocked(gameHomeOnlineTrendPost);

async function renderDashboard() {
  const router = createRouter({
    history: createMemoryHistory(),
    routes: [
      ...['/dashboard/home', '/system/menu', '/system/config', '/system/dict', '/playerCenter/gameUserEntity'].map(path => ({
        path, component: {template: '<div />'},
      })),
      {path: '/:path(.*)*', name: 'notFound', component: {template: '<div />'}},
    ],
  });
  await router.push('/dashboard/home');
  return mount(DashboardHome, {
    global: {
      plugins: [router],
      stubs: {HomeTrendChart: true, HomeTermIllust: true, NarrowScreenHint: true},
    },
  });
}

beforeEach(() => {
  vi.clearAllMocks();
  userStore.isSuperAdmin = true;
  userStore.userInfos.account = 'superadmin';
  overviewRequest.mockResolvedValue({data: {data: {onlineCount: 123, todayOrderCount: 45, playerTotalCount: 678}}} as Awaited<ReturnType<typeof gameHomeOverviewPost>>);
  trendRequest.mockResolvedValue({data: {data: {points: [{bucketTime: 0, onlineCount: null}]}}} as Awaited<ReturnType<typeof gameHomeOnlineTrendPost>>);
});

test('超管只显示平台入口，业务卡片不挂载也不发送聚合请求', async () => {
  const wrapper = await renderDashboard();
  await flushPromises();
  expect(wrapper.text()).toContain('superadmin');
  expect(wrapper.find('.business-overview').exists()).toBe(false);
  expect(wrapper.find('a[href="/system/menu"]').exists()).toBe(true);
  expect(wrapper.find('a[href="/playerCenter/gameUserEntity"]').exists()).toBe(false);
  expect(overviewRequest).not.toHaveBeenCalled();
  expect(trendRequest).not.toHaveBeenCalled();
  wrapper.unmount();
});

test('普通管理员读取真实全服统计，显示业务入口并排除平台入口', async () => {
  userStore.isSuperAdmin = false;
  userStore.userInfos.account = 'admin';
  const wrapper = await renderDashboard();
  await flushPromises();
  expect(overviewRequest).toHaveBeenCalledTimes(1);
  expect(trendRequest).toHaveBeenCalledTimes(1);
  expect(wrapper.findAll('.kpi-value').map(card => card.text())).toEqual(['123', '45', '678', '0']);
  expect(wrapper.find('a[href="/playerCenter/gameUserEntity"]').exists()).toBe(true);
  expect(wrapper.find('a[href="/system/menu"]').exists()).toBe(false);
  wrapper.unmount();
});

test('聚合接口失败显示未知读数，不把失败伪装成零', async () => {
  userStore.isSuperAdmin = false;
  overviewRequest.mockRejectedValue(new Error('unavailable'));
  const wrapper = await renderDashboard();
  await flushPromises();
  expect(wrapper.findAll('.kpi-value').map(card => card.text())).toEqual(['—', '—', '—', '—']);
  wrapper.unmount();
});
