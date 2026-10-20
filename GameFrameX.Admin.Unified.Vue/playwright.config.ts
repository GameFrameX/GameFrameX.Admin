import { defineConfig } from '@playwright/test';

// 浏览器：channel msedge 直接启动系统 Edge（Chromium 内核），免下载 Playwright 自带 Chromium，
// 也免旧套件的手动 Edge CDP 9222 前置步骤。
// 会话模型：setup 项目真实表单登录并落 storageState（e2e/.e2e/state/*.json），
// admin 项目默认注入 admin 态；superadmin/匿名用例经 tests/helpers.ts 开显式 context。
const APP_URL = process.env.E2E_APP_URL ?? 'http://127.0.0.1:20800';
const STATE_DIR = 'e2e/.e2e/state';
// setup 项目必须全新上下文，不能被 admin 态污染（否则跳过登录直接落地）
const NO_STORAGE_STATE = { cookies: [], origins: [] };

export default defineConfig({
  testDir: './e2e/tests',
  testMatch: '**/*.e2e.ts',
  outputDir: './e2e/.e2e/results',
  // 同账号多 worker 并行会触发后端单会话互踢，套件必须串行
  workers: 1,
  fullyParallel: false,
  retries: 0,
  // 巡查/闭环用例单测内含全菜单遍历或多步表单编排
  timeout: 180_000,
  // 断言超时（原套件 assertionTimeout 15s；个别用例自带显式 20s 覆盖）
  expect: { timeout: 15_000 },
  reporter: [
    ['list'],
    ['json', { outputFile: 'e2e/.e2e/report.json' }],
    ['html', { outputFolder: 'e2e/.e2e/report', open: 'never' }],
  ],
  use: {
    baseURL: APP_URL,
    channel: 'msedge',
    headless: process.env.E2E_HEADED !== '1',
    locale: 'zh-CN',
    actionTimeout: 15_000,
    trace: 'retain-on-failure',
    screenshot: 'off',
  },
  projects: [
    {
      name: 'setup-admin',
      testMatch: /auth\.setup\.e2e\.ts/,
      use: { storageState: NO_STORAGE_STATE },
    },
    {
      name: 'setup-superadmin',
      testMatch: /superadmin\.setup\.e2e\.ts/,
      use: { storageState: NO_STORAGE_STATE },
    },
    {
      name: 'admin',
      testIgnore: /.*\.setup\.e2e\.ts/,
      use: { storageState: `${STATE_DIR}/admin.json` },
      dependencies: ['setup-admin', 'setup-superadmin'],
    },
  ],
});
