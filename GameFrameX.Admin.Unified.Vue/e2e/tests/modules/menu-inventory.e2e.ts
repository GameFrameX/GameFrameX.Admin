import { test, expect } from '@playwright/test';
import { enterWorkspace, gotoAppSelector } from '../helpers';
import { renderedMenuGroups } from '../module-coverage';

// 租户最高权限（admin）双表面模块清单守护：应用选择页全局侧边栏 + 应用工作台侧边栏
// 的根组集合必须与菜单种子一致——模块增删立即暴露，防止分模块覆盖出现静默空洞。

test('全局侧边栏模块清单一致（四组）', async ({ page }) => {
  await page.goto('/');
  await page.locator('.p-panelmenu').waitFor({ timeout: 15_000 });
  await gotoAppSelector(page);
  const groups = (await renderedMenuGroups(page)).map((entry) => entry.group);
  expect(new Set(groups), '全局侧边栏根组').toEqual(new Set(['工作台', '应用中心', '系统管理', '日志管理']));
});

test('应用工作台模块清单一致（十组）', async ({ page }) => {
  await page.goto('/');
  await page.locator('.p-panelmenu').waitFor({ timeout: 15_000 });
  await enterWorkspace(page);
  const groups = (await renderedMenuGroups(page)).map((entry) => entry.group);
  expect(new Set(groups), '工作台侧边栏根组').toEqual(
    new Set([
      // 渲染标签取侧边栏 i18n 文案，与菜单树 Title 存在既定漂移：
      // 信任与安全（树：信任与安全中心）、在线业务中心（树：在线平台）
      '应用中心',
      '玩家中心',
      '信任与安全',
      '商品与道具',
      '区服运维',
      '在线业务中心',
      '兑换码',
      'Web 商城',
      '任务与自动化',
      '内容运营',
    ]),
  );
});
