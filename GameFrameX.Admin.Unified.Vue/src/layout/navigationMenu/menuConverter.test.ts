import {expect, test, vi} from 'vitest';
import {convertRoutesToMenuItems} from './menuConverter';

vi.mock('../../i18n/index', () => ({t: (key: string) => key}));

test('直接保留后端菜单树，不再根据旧应用作用域筛选菜单', () => {
  const routes = [
    {path: '/dashboard', meta: {title: 'Overview', menuContext: 'tenantShell'}, children: [
      {path: '/dashboard/home', meta: {title: 'Home', appScope: 'console'}},
    ]},
    {path: '/serverCenter', meta: {title: 'Servers', menuContext: 'appWorkspace'}, children: [
      {path: '/serverCenter/gameAreaEntity', meta: {title: 'Areas', appScope: 'app'}},
      {path: '/serverCenter/hidden', meta: {isHide: true}},
    ]},
  ];
  const items = convertRoutesToMenuItems(routes, key => key);
  expect(items.map(item => item.key)).toEqual(['/dashboard', '/serverCenter']);
  expect(items[1].items?.map(item => item.route)).toEqual(['/serverCenter/gameAreaEntity']);
});
