import { test, expect, type Route } from '@playwright/test';
import { expectOverview, navigateByMenu, shot } from './helpers';
import { installApiRecorder, readApiRecords, apiAnomalies } from './module-coverage';
import { globalServerSelector, selectServerOption } from './server-selection';
import { readField } from './admin-stack';

// 区服选项来自真实页面发出的 gameArea/page；只 mock 在线业务响应，避免依赖游戏服在线状态。
// mock 不发请求也不改变 serverId，记录器与 DOM 指标共同验证页面实际使用的上下文。
test('区服全局切换、页内覆盖隔离与刷新持久化', async ({ page }) => {
  const servers = new Map<string, string>();
  page.on('response', response => {
    if (!/\/api\/gameArea\/page(?:\?|$)/.test(response.url()) || response.request().method() !== 'POST') return;
    void response.json().then((payload: unknown) => {
      if (readField(payload, 'code') !== 0) return;
      const items = readField(readField(payload, 'data'), 'items');
      if (!Array.isArray(items)) return;
      for (const item of items) {
        const id = readField(item, 'id');
        const name = readField(item, 'name');
        if (id != null && typeof name === 'string' && name) servers.set(String(id), name);
      }
    }).catch(() => undefined);
  });
  let delayFirstServer = false;
  let delayedRoute: Route | undefined;
  await installApiRecorder(page);
  await page.route('**/api/onlineOverviewQuery/snapshot', async route => {
    const serverId: unknown = route.request().postDataJSON()?.serverId;
    const index = [...servers.keys()].indexOf(String(serverId));
    if (delayFirstServer && index === 0) {
      delayedRoute = route;
      return;
    }
    await route.fulfill({ json: { code: 0, data: { onlinePlayerCount: index + 101, sessionCount: 0,
      matchCount: 0, reconnectRate: 0, queueSummaries: [], serverTime: 1793808000 } } });
  });
  await page.route('**/api/onlineAssetLedgerQuery/page', route => route.fulfill({
    json: { code: 0, data: { items: [], hasNextPage: false, nextCursor: null } },
  }));
  await page.goto('/');
  await expectOverview(page);
  await expect.poll(() => servers.size, '隔离库至少准备两个名称不同的可用区服').toBeGreaterThanOrEqual(2);
  const [first, second] = [...servers.entries()];
  expect(first[1]).not.toBe(second[1]);
  const globalSelector = globalServerSelector(page);
  // 先选择第二服再切回第一服，证明这是实际切换而非默认值巧合。
  await selectServerOption(page, globalSelector, second[1]);
  await selectServerOption(page, globalSelector, first[1]);
  await navigateByMenu(page, '在线总览', '在线运维');

  const expectServerPayload = async (endpoint: string, serverId: string, after = 0): Promise<void> => {
    await expect.poll(async () => {
      const records = (await readApiRecords(page)).slice(after).filter(record => record.url.includes(endpoint));
      const last = records.at(-1);
      return last?.body ? readField(JSON.parse(last.body), 'serverId') : undefined;
    }).toBe(serverId);
    const matching = (await readApiRecords(page)).slice(after).filter(record => record.url.includes(endpoint));
    expect(matching.every(record => record.body && readField(JSON.parse(record.body), 'serverId') === serverId),
      '本阶段不能发出空区服或串服请求').toBe(true);
  };
  await expectServerPayload('/onlineOverviewQuery/snapshot', first[0]);
  await expect(page.locator('[data-metric="onlinePlayerCount"]')).toHaveText('101');
  await shot(page, 'server-global-payload');

  const recordCount = (await readApiRecords(page)).length;
  const localSelector = page.locator('.onlineDashboard-container .server-selector');
  await selectServerOption(page, localSelector, second[1]);
  await expectServerPayload('/onlineOverviewQuery/snapshot', second[0], recordCount);
  await expect(page.locator('[data-metric="onlinePlayerCount"]')).toHaveText('102');
  await expect(globalSelector.getByRole('combobox')).toContainText(first[1]);
  await shot(page, 'server-local-override');

  const beforeOtherPage = (await readApiRecords(page)).length;
  await navigateByMenu(page, '资产流水', '在线运维');
  await expectServerPayload('/onlineAssetLedgerQuery/page', first[0], beforeOtherPage);
  await expect(globalSelector.getByRole('combobox')).toContainText(first[1]);
  expect(apiAnomalies(await readApiRecords(page))).toEqual([]);
  await shot(page, 'server-other-page-inherits-global');

  await page.reload();
  await expect(globalServerSelector(page).getByRole('combobox')).toContainText(first[1]);
  await expectServerPayload('/onlineAssetLedgerQuery/page', first[0]);
  expect(apiAnomalies(await readApiRecords(page))).toEqual([]);
  // 新文档内的局部覆盖退场；返回在线总览也继承持久化的全局服。
  await navigateByMenu(page, '在线总览', '在线运维');
  await expectServerPayload('/onlineOverviewQuery/snapshot', first[0]);
  await expect(page.locator('[data-metric="onlinePlayerCount"]')).toHaveText('101');
  await shot(page, 'server-persisted-after-reload');

  // 先挂住第一服请求，再切至第二服；旧结果晚到也不得覆盖第二服的 DOM。
  const refreshedGlobalSelector = globalServerSelector(page);
  await selectServerOption(page, refreshedGlobalSelector, second[1]);
  await expect(page.locator('[data-metric="onlinePlayerCount"]')).toHaveText('102');
  delayFirstServer = true;
  await selectServerOption(page, refreshedGlobalSelector, first[1]);
  await expect.poll(() => Boolean(delayedRoute)).toBe(true);
  await selectServerOption(page, refreshedGlobalSelector, second[1]);
  await expect(page.locator('[data-metric="onlinePlayerCount"]')).toHaveText('102');
  const oldResponse = page.waitForResponse(response => response.url().includes('/onlineOverviewQuery/snapshot')
    && String(response.request().postDataJSON()?.serverId) === first[0]);
  await delayedRoute!.fulfill({ json: { code: 0, data: { onlinePlayerCount: 999, sessionCount: 0,
    matchCount: 0, reconnectRate: 0, queueSummaries: [], serverTime: 1793808000 } } });
  await (await oldResponse).finished();
  await page.evaluate(() => new Promise<void>(resolve => requestAnimationFrame(() => requestAnimationFrame(() => resolve()))));
  await expect(page.locator('[data-metric="onlinePlayerCount"]')).toHaveText('102');
  await shot(page, 'server-delayed-response-does-not-overwrite');
});
