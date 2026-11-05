import { expect, type Locator, type Page } from '@playwright/test';

export function globalServerSelector(page: Page): Locator {
  // 顶栏在文档顺序上先于页面内选择器；两个实例使用同一个共享组件。
  return page.locator('.server-selector').first();
}

export async function selectServerOption(page: Page, selector: Locator, name?: string): Promise<string> {
  const control = selector.getByRole('combobox', { name: '选择区服', exact: true });
  await expect(control).toBeEnabled();
  await control.click();
  const option = name ? page.getByRole('option', { name, exact: true }) : page.getByRole('option').first();
  await expect(option).toBeVisible();
  const selectedName = (await option.innerText()).trim();
  await option.click();
  await expect(control).toContainText(selectedName);
  return selectedName;
}

export async function selectGlobalServer(page: Page): Promise<string> {
  return selectServerOption(page, globalServerSelector(page));
}
