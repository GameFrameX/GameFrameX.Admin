import {expect, test, vi} from 'vitest';
import {convertRoutesToMenuItems} from './menuConverter';

vi.mock('../../i18n/index', () => ({t: (key: string) => key}));

test('直接保留后端菜单树，不进行旧作用域元数据筛选', () => {
  const routes = [
    {path: '/dashboard', meta: {title: 'Overview'}, children: [
      {path: '/dashboard/home', meta: {title: 'Home'}},
    ]},
    {path: '/serverCenter', meta: {title: 'Servers'}, children: [
      {path: '/serverCenter/gameAreaEntity', meta: {title: 'Areas'}},
      {path: '/serverCenter/hidden', meta: {isHide: true}},
    ]},
  ];
  const items = convertRoutesToMenuItems(routes, key => key);
  expect(items.map(item => item.key)).toEqual(['/dashboard', '/serverCenter']);
  expect(items[1].items?.map(item => item.route)).toEqual(['/serverCenter/gameAreaEntity']);
});
