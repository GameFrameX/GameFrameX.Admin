/**
 * Smoke 测试：登录后遍历侧边栏路由，抓 console error / pageerror / 请求失败。
 * 重点抓全量回迁后的运行时隐患（enum/interface import 丢失 → ReferenceError: X is not defined）。
 * 用系统 Chrome（puppeteer-core，不下载 Chromium）。
 */
import puppeteer from 'puppeteer-core';

const CHROME = '/Applications/Google Chrome.app/Contents/MacOS/Google Chrome';
const BASE = 'http://127.0.0.1:8888';

const browser = await puppeteer.launch({
  executablePath: CHROME,
  headless: 'new',
  args: ['--no-sandbox', '--window-size=1400,900'],
});
const page = await browser.newPage();
const errors = [];
const visited = [];
let curUrl = '';
const ignore = (s) => /WebSocket|SignalR|onlineUser|hubs\/|ERR_CONNECTION|favicon|\.map$/.test(s);
page.on('console', (m) => {
  const t = m.text();
  if (m.type() === 'error' && !ignore(t)) errors.push(`[console] ${curUrl}\n    ${t}`);
});
page.on('pageerror', (e) => {
  if (!ignore(e.message)) errors.push(`[pageerror] ${curUrl}\n    ${e.message}`);
});
page.on('requestfailed', (r) => {
  const u = r.url();
  if (!u.includes('favicon') && !u.includes('.map')) errors.push(`[reqfail] ${curUrl}\n    ${u} ${r.failure()?.errorText}`);
});
page.on('response', (r) => {
  if (r.status() >= 500) errors.push(`[http${r.status()}] ${curUrl}\n    ${r.url()}`);
});
let loginBody = '';
let loginResp = '';
page.on('request', (req) => {
  if (req.url().includes('/systemAuth/login') && req.method() === 'POST') loginBody = req.postData() || '';
});
page.on('response', async (res) => {
  if (res.url().includes('/systemAuth/login') && res.request().method() === 'POST') {
    loginResp = `${res.status()} ${(await res.text().catch(() => '')).slice(0, 200)}`;
  }
});

const goto = async (hash) => {
  curUrl = hash;
  const before = errors.length;
  try {
    await page.goto(BASE + hash, { waitUntil: 'networkidle2', timeout: 20000 });
  } catch (e) {
    errors.push(`[goto-timeout] ${hash}\n    ${e.message}`);
  }
  await new Promise((rr) => setTimeout(rr, 1500));
  visited.push(hash);
  const delta = errors.length - before;
  console.log(`${delta ? '✗' : '✓'} ${hash}${delta ? ` (${delta} 错误)` : ''}`);
};

// 1. 登录
console.log('=== 登录 ===');
await page.goto(BASE + '/#/login', { waitUntil: 'networkidle2' });
await page.waitForSelector('#login-account', { timeout: 10000 });
await page.evaluate(() => {
  const set = (id, v) => { const e = document.getElementById(id); e.value = v; e.dispatchEvent(new Event('input', { bubbles: true })); };
  set('login-account', 'superadmin');
  set('login-password', '123456');
});
await page.click('.login-content-submit');
await new Promise((r) => setTimeout(r, 6000));
await page.screenshot({ path: '/tmp/login-after.png' });
console.log('登录后 url:', page.url());
if (page.url().includes('login')) {
  console.log('登录失败\nloginBody:', loginBody, '\nloginResp:', loginResp, '\nerrors:');
  console.log(errors.join('\n') || '(无)');
  await browser.close();
  process.exit(1);
}

// 2. 抓路由：vue-router getRoutes（登录后动态路由已注册）
let routes = await page.evaluate(() => {
  const app = document.querySelector('#app')?.__vue_app__;
  const router = app?.config?.globalProperties?.$router;
  if (!router) return [];
  return router.getRoutes()
    .filter((r) => r.path && !r.path.includes(':') && !['/', '/login', '/dashboard/home'].includes(r.path))
    .map((r) => '#' + r.path);
});
if (!routes.length) console.log('router 0，localStorage keys:', await page.evaluate(() => Object.keys(localStorage)));
routes = [...new Set(routes)];
routes = [...new Set(routes)].filter((r) => r && r !== '#/login');
console.log(`\n=== 遍历 ${routes.length} 个路由 ===`);
console.log('路由列表:', routes.slice(0, 30).join(', '));

// 3. 遍历
for (const r of routes) await goto(r);

// 4. 汇总
console.log(`\n========== 测试结果 ==========`);
console.log(`访问 ${visited.length} 路由，错误 ${errors.length} 条`);
if (errors.length) {
  // 去重 + 分类
  const seen = new Set();
  const unique = errors.filter((e) => {
    const key = e.split('\n')[1]?.slice(0, 80);
    if (seen.has(key)) return false;
    seen.add(key);
    return true;
  });
  console.log(`\n--- 错误（去重 ${unique.length}）---`);
  console.log(unique.join('\n\n'));
}
await browser.close();
