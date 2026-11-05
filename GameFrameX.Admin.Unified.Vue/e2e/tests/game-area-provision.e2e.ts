import { spawn, type ChildProcessWithoutNullStreams } from 'node:child_process';
import { resolve } from 'node:path';
import { test, expect } from '@playwright/test';
import { ADMIN_STATE, navigateByMenu, shot, signIn } from './helpers';
import { installApiRecorder, readApiRecords } from './module-coverage';
import { globalServerSelector, selectServerOption } from './server-selection';
import { readField } from './admin-menu-fixture';

// 环境准备见 notes-provision.md：隔离库模板“E2E 开服模板”的分类 HttpManagerUrl 指向本桩。
// 用例只通过真实表单与菜单操作，不主动调用管理 API 或 SQL。
test.use({ storageState: { cookies: [], origins: [] } });
let stub: ChildProcessWithoutNullStreams | undefined;
const port = Number(process.env.E2E_PROVISION_STUB_PORT ?? 22240);

async function startStub(registrationFailures: number): Promise<void> {
  stub = spawn(process.execPath, [resolve(__dirname, '../stubs/server-provision.mjs')], {
    env: { ...process.env, E2E_PROVISION_STUB_PORT: String(port), E2E_PROVISION_REGISTRATION_FAILURES: String(registrationFailures) },
    stdio: 'pipe',
  });
  const child = stub;
  await new Promise<void>((resolve, reject) => {
    const timer = setTimeout(() => reject(new Error('开服桩启动超时')), 10000);
    let output = '';
    let errors = '';
    child.stderr.on('data', chunk => { errors += String(chunk); });
    child.stdout.on('data', chunk => {
      output += String(chunk);
      if (output.includes(`provision-ready:${port}`)) { clearTimeout(timer); resolve(); }
    });
    child.once('error', error => { clearTimeout(timer); reject(error); });
    child.once('exit', code => { clearTimeout(timer); reject(new Error(`开服桩退出 ${code}: ${errors}`)); });
  });
}

test.afterEach(async () => {
  if (!stub || stub.exitCode !== null) return;
  const child = stub;
  await new Promise<void>(resolve => {
    const timer = setTimeout(() => child.kill('SIGKILL'), 5000);
    child.once('exit', () => { clearTimeout(timer); resolve(); });
    child.kill('SIGTERM');
  });
  stub = undefined;
});

for (const registrationFailures of [0, 1]) {
  test(`开服向导真实菜单闭环${registrationFailures ? '：登记失败后重试' : '：成功'}`, async ({ page }) => {
    await startStub(registrationFailures);
    await installApiRecorder(page);
    await signIn(page, process.env.E2E_ACCOUNT ?? 'admin', process.env.E2E_PASSWORD ?? '123456');
    // 同账号真实登录会替换旧会话，更新后续串行用例的状态文件。
    await page.context().storageState({ path: ADMIN_STATE });
    await navigateByMenu(page, '开服向导', '区服');
    const wizard = page.locator('.gameAreaProvision-container');
    // 模板下拉的 label 未做 aria 关联（无 accessible name），按容器内首个 combobox 定位。
    await wizard.getByRole('combobox').first().click();
    const previewResponse = page.waitForResponse(response => /\/gameAreaProvision\/preview(?:\?|$)/.test(response.url()));
    await page.getByRole('option', { name: process.env.E2E_PROVISION_TEMPLATE_NAME ?? 'E2E 开服模板', exact: true }).click();
    expect(readField(await (await previewResponse).json(), 'code')).toBe(0);
    await expect(wizard.getByText('将从模板复制的配置', { exact: true })).toBeVisible();
    const identifier = String(Date.now() * 10 + registrationFailures);
    const name = `开服回归-${identifier}`;
    await wizard.getByLabel('新区服编号', { exact: true }).fill(identifier);
    await wizard.getByLabel('新区服名称', { exact: true }).fill(name);
    await wizard.getByLabel('游戏服 HTTP 管理地址', { exact: true }).fill(`http://127.0.0.1:${port}`);
    await wizard.getByLabel('TCP 连接地址', { exact: true }).fill('tcp://127.0.0.1:22241');
    await wizard.getByLabel('WebSocket 连接地址', { exact: true }).fill('ws://127.0.0.1:22242');
    await wizard.getByLabel('容器环境变量覆盖项（JSON 字符串字典）', { exact: true }).fill(JSON.stringify({ ServerId: identifier }));
    const executeResponse = page.waitForResponse(response => /\/gameAreaProvision\/execute$/.test(response.url()));
    await wizard.getByRole('button', { name: '创建并开服', exact: true }).click();
    const envelope: unknown = await (await executeResponse).json();
    if (registrationFailures) {
      expect(readField(envelope, 'code')).not.toBe(0);
      await expect(wizard.getByRole('alert').first()).toBeVisible();
      await expect(wizard.getByTestId('provision-result')).toContainText('游戏容器: 已完成');
      await expect(wizard.getByTestId('provision-result')).toContainText('启用区服: 待完成');
      await shot(page, 'provision-registration-failed');
      const retryResponse = page.waitForResponse(response => /\/gameAreaProvision\/execute$/.test(response.url()));
      await wizard.getByRole('button', { name: '重试未完成步骤', exact: true }).click();
      expect(readField(await (await retryResponse).json(), 'code')).toBe(0);
    } else { expect(readField(envelope, 'code')).toBe(0); }
    await expect(wizard.getByTestId('provision-result')).toContainText('开服成功，新区服已启用。');
    await expect(wizard.getByTestId('provision-result')).toContainText(name);
    await selectServerOption(page, globalServerSelector(page), name);
    await shot(page, 'provision-enabled-global-selector');
    await navigateByMenu(page, '游戏区服', '区服');
    const listing = page.locator('.gameAreaEntity-container');
    await listing.getByRole('button', { name: '高级', exact: true }).click();
    await listing.getByPlaceholder('请输入名称', { exact: true }).fill(name);
    const listResponse = page.waitForResponse(response => /\/api\/gameArea\/page$/.test(response.url()));
    await listing.getByRole('button', { name: /^(查询|搜索)$/ }).click();
    expect(readField(await (await listResponse).json(), 'code')).toBe(0);
    await expect(listing.locator('tbody tr').filter({ hasText: name })).toContainText(identifier);
    const records = await readApiRecords(page);
    for (const action of ['templates', 'preview', 'create', 'execute']) {
      expect(records.some(record => record.url.includes(`/gameAreaProvision/${action}`)), `页面发出 ${action}`).toBe(true);
    }
    const create = records.find(record => record.url.includes('/gameAreaProvision/create'));
    expect(readField(JSON.parse(create?.body ?? '{}'), 'serverId')).toBe(Number(identifier));
    await shot(page, 'provision-visible-in-area-list');
  });
}
