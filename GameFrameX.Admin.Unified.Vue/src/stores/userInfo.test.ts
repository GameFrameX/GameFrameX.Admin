import {beforeEach, expect, test, vi} from 'vitest';
import {createPinia, setActivePinia} from 'pinia';
import {useUserInfoStore} from './userInfo';
import {useServerStore} from './server';
import {gameAreaPagePost} from '../api/gameArea';
import {local} from '../utils/storage';

vi.mock('../api/gameArea', () => ({gameAreaPagePost: vi.fn()}));
vi.mock('../utils/axiosUtils', () => ({getAPI: vi.fn()}));
vi.mock('../utils/watermark', () => ({default: {set: vi.fn()}}));
const pageRequest = vi.mocked(gameAreaPagePost);

beforeEach(() => {
  setActivePinia(createPinia());
  const stored = new Map<string, string>();
  vi.stubGlobal('localStorage', {
    getItem: (key: string) => stored.get(key) ?? null,
    setItem: (key: string, value: string) => stored.set(key, value),
    removeItem: (key: string) => stored.delete(key),
  });
  vi.resetAllMocks();
});

test('恢复超管身份只清除旧区服上下文，不请求业务区服列表', async () => {
  local.set('selectedServerId', '2');
  const userStore = useUserInfoStore();
  vi.spyOn(userStore, 'getApiUserInfo').mockResolvedValue({id: '1', accountType: 999} as never);
  await userStore.setUserInfos();
  expect(userStore.isSuperAdmin).toBe(true);
  expect(pageRequest).not.toHaveBeenCalled();
  expect(useServerStore().selectedServerId).toBe('');
  expect(local.get('selectedServerId')).toBeNull();
});

test('刷新普通管理员会话加载区服并保留有效持久化选择', async () => {
  local.set('selectedServerId', '2');
  pageRequest.mockResolvedValue({data: {data: {items: [{id: '2', name: 'Second'}]}}} as Awaited<ReturnType<typeof gameAreaPagePost>>);
  const userStore = useUserInfoStore();
  vi.spyOn(userStore, 'getApiUserInfo').mockResolvedValue({id: '2', accountType: 100} as never);
  await userStore.setUserInfos();
  expect(pageRequest).toHaveBeenCalledTimes(1);
  expect(useServerStore().selectedServerId).toBe('2');
});

test('区服列表加载失败不阻断身份初始化，且清除旧选择', async () => {
  local.set('selectedServerId', '2');
  pageRequest.mockRejectedValue(new Error('unavailable'));
  const userStore = useUserInfoStore();
  vi.spyOn(userStore, 'getApiUserInfo').mockResolvedValue({id: '2', accountType: 100} as never);
  await expect(userStore.setUserInfos()).resolves.toBeUndefined();
  expect(userStore.userInfos.id).toBe('2');
  expect(useServerStore().loadFailed).toBe(true);
  expect(useServerStore().selectedServerId).toBe('');
});
