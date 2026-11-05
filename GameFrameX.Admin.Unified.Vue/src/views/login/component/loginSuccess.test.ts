import {beforeEach, expect, test, vi} from 'vitest';
import type {Router} from 'vue-router';
import {completeSignIn} from './loginSuccess';

const {calls, resetServers, initializeRoutes, push, setLocal, setSession, clearTokens} = vi.hoisted(() => {
  const calls: string[] = [];
  return {
    calls,
    resetServers: vi.fn(() => { calls.push('reset'); }),
    initializeRoutes: vi.fn(async () => { calls.push('initialize'); return false; }),
    push: vi.fn(async () => { calls.push('navigate'); }),
    setLocal: vi.fn(() => { calls.push('token'); }),
    setSession: vi.fn(),
    clearTokens: vi.fn(),
  };
});
vi.mock('../../../stores/server', () => ({useServerStore: () => ({resetServers})}));
vi.mock('../../../utils/storage', () => ({local: {set: setLocal}, session: {set: setSession}}));
vi.mock('../../../utils/axiosUtils', () => ({ACCESS_TOKEN_KEY: 'access-token', clearTokens}));
vi.mock('../../../router/backEnd', () => ({initBackEndControlRoutes: initializeRoutes}));
vi.mock('../../../utils/message', () => ({showSuccess: vi.fn(), showWarning: vi.fn()}));
vi.mock('../../../utils/formatTime', () => ({formatAxis: () => 'morning'}));
vi.mock('../../../utils/loading', () => ({nextLoading: {start: vi.fn()}}));
vi.mock('../../../i18n/index', () => ({t: (key: string) => key}));

beforeEach(() => {
  vi.clearAllMocks();
  calls.length = 0;
  initializeRoutes.mockImplementation(async () => { calls.push('initialize'); return false; });
});

test('每次登录先废弃旧区服状态，再初始化菜单并固定落总览', async () => {
  await completeSignIn('new-token', {push} as unknown as Router);
  expect(resetServers).toHaveBeenCalledTimes(1);
  expect(calls).toEqual(['reset', 'token', 'initialize', 'navigate']);
  expect(initializeRoutes).toHaveBeenCalledTimes(1);
  expect(push).toHaveBeenCalledWith('/dashboard/home');
});

test('无菜单权限时清凭据，不导航到总览', async () => {
  initializeRoutes.mockResolvedValue(true);
  await completeSignIn('new-token', {push} as unknown as Router);
  expect(clearTokens).toHaveBeenCalledTimes(1);
  expect(push).not.toHaveBeenCalled();
});
