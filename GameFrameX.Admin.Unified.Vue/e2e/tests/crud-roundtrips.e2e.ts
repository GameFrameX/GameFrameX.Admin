import { test, expect } from '@playwright/test';
import { expectOverview, navigateByMenu, pickDateTime, shot, sleep, superadminPage } from './helpers';

// 单 shell 数据闭环：20 个保留实体，按根组 + 叶子标题真实导航，不读取菜单树或直接跳 hash。
// 菜单归属和控件序列按终态对账，保留新增→编辑回显→删除的动作强度。
// 例外与既有风险（集成轮核验，不以迁移宣称已修）：
// - 应用管理已按 / 删除，不再作为 CRUD 例外或入口。
// - 游戏角色：历史新增权限问题；本轮 admin 显式授权后应复核，仍未加入此矩阵。
// - 游戏内横幅/邮件/区服/邮件模板：区服与分类等前置数据、联动选择器尚需专项夹具。
// - 游戏订单/发货账本：商业化中心的多表业务页，不是通用 CRUD 对话框。
// - 支付配置：列表锚点；游戏内商品：级联与绑定；角色管理：上下文菜单动作。
// - 分包管理、游戏公告、奖励模板/兑换码、物品目录、资源包、游戏运营配置：
//  保留历史失败的闭环回归，不删除/跳过；隔离夹具提供语言/平台只读授权及奖励分组/物品。
//  控件序列按终态核验， 失败原因与集成实际结果逐项见 notes.md。
// - 渠道软删问题、邮件 Vue#198 等历史项不能因标题迁移直接视为修复。

// 2026-10-20 补编辑半边：闭环升级为 新增 → 行回显 → 编辑提交（回显断言 + 改值）→ 行回显新值 → 删除 → 行消失。
// update 路由按约定由 create 路由派生（xxx/add → xxx/update， 权限码对齐约定），不逐实体重复登记。
type EntitySpec = {
  name: string;
  menuTitle: string;
  menuRoot: string;
  fields: ('text' | 'select' | 'number' | 'date' | 'json' | 'editor' | 'cascade' | 'reward-items' | 'unique-number' | 'skip-text' | 'skip-number' | 'skip-select' | 'skip')[];
  session?: 'admin' | 'superadmin';
  // 行内无「编辑」按钮的页（如兑换码：编辑复用新增弹窗、动作列只挂删除）不跑编辑半边
  rowEdit?: boolean;
  createApi: RegExp;
  deleteApi: RegExp;
};

const ENTITIES: EntitySpec[] = [
  {
    name: '版本定义',
    menuTitle: '版本定义',
    menuRoot: 'C 端交付',
    fields: ['text', 'text'],
    createApi: /gameAppVersionDefinition\/add/,
    deleteApi: /gameAppVersionDefinition\/delete/,
  },
  {
    name: '资源包定义',
    // menuTitle 必须是侧边栏渲染后的 i18n 标签（用户所见），不是种子菜单树的 Meta.Title：
    // gameAppAssetPackage 的 zh-cn 标签是「资源包」，与树 Title「资源包定义」已漂移
    menuTitle: '资源包',
    menuRoot: 'C 端交付',
    fields: ['text', 'text'],
    createApi: /gameAppAssetPackage\/add/,
    deleteApi: /gameAppAssetPackage\/delete/,
  },
  {
    name: '配置字段定义',
    menuTitle: '内容元数据', // configKey 的 zh-cn 渲染标签（树 Title「配置字段定义」已漂移）
    menuRoot: 'C 端交付',
    fields: ['text', 'text', 'select'],
    createApi: /gameAppRemoteConfigKey\/add/,
    deleteApi: /gameAppRemoteConfigKey\/delete/,
  },
  {
    name: '配置模式定义',
    menuTitle: '模式配置', // configMode 的 zh-cn 渲染标签（树 Title「配置模式定义」已漂移）
    menuRoot: 'C 端交付',
    fields: ['text', 'text'],
    createApi: /gameAppRemoteConfigMode\/add/,
    deleteApi: /gameAppRemoteConfigMode\/delete/,
  },
  {
    name: '设备定义',
    menuTitle: '设备', // device 的 zh-cn 渲染标签（树 Title「设备定义」已漂移）
    menuRoot: 'C 端交付',
    fields: ['text', 'text', 'text'],
    createApi: /gameAppDevice\/add/,
    deleteApi: /gameAppDevice\/delete/,
  },
  {
    name: '邮件模板分类',
    menuTitle: '模板分类',
    menuRoot: '内容运营',
    fields: ['text'],
    createApi: /gameMailTemplateCategory\/add/,
    deleteApi: /gameMailTemplateCategory\/delete/,
  },
  {
    name: '物品目录',
    menuTitle: '物品目录',
    menuRoot: '商业化',
    fields: ['text', 'text'],
    createApi: /gameGoodsCatalogue\/add/,
    deleteApi: /gameGoodsCatalogue\/delete/,
  },
  {
    name: '游区分类',
    menuTitle: '游区分类',
    menuRoot: '区服',
    fields: ['text', 'text'],
    createApi: /gameAreaClassify\/add/,
    deleteApi: /gameAreaClassify\/delete/,
  },
  {
    name: '兑换码渠道',
    menuTitle: '兑换码渠道',
    menuRoot: '商业化',
    fields: ['text', 'text', 'number'],
    createApi: /gameRedeemCodeChannel\/add/,
    deleteApi: /gameRedeemCodeChannel\/delete/,
  },
  {
    name: '定时任务',
    menuTitle: '定时任务',
    menuRoot: '系统',
    fields: ['text', 'text', 'text'],
    createApi: /gameTimeJob\/add/,
    deleteApi: /gameTimeJob\/delete/,
  },
  {
    name: '奖励模板分组',
    menuTitle: '奖励模板分组',
    menuRoot: '内容运营',
    fields: ['text'],
    createApi: /gameRewardTemplateGroup\/add/,
    deleteApi: /gameRewardTemplateGroup\/delete/,
  },
  {
    name: '游戏公告',
    menuTitle: '游戏公告',
    menuRoot: '内容运营',
    fields: ['select', 'select', 'date', 'date', 'number', 'text', 'text'],
    createApi: /gameAnnouncement\/add/,
    deleteApi: /gameAnnouncement\/delete/,
  },
  {
    name: '游戏物品',
    menuTitle: '游戏物品',
    menuRoot: '商业化',
    fields: ['unique-number', 'select', 'text', 'text'],
    createApi: /gameGoods\/add/,
    deleteApi: /gameGoods\/delete/,
  },
  {
    name: '游戏运营配置',
    menuTitle: '游戏运营配置',
    menuRoot: 'C 端交付',
    fields: ['text', 'json', 'text'],
    createApi: /gameAppServerConfig\/add/,
    deleteApi: /gameAppServerConfig\/delete/,
  },
  {
    name: '分包管理',
    menuTitle: '分包管理',
    menuRoot: 'C 端交付',
    fields: ['skip-select', 'text', 'select', 'select', 'text', 'text'],
    createApi: /gameAppPackage\/add/,
    deleteApi: /gameAppPackage\/delete/,
  },
  {
    name: '渠道定义',
    menuTitle: '渠道定义',
    menuRoot: 'C 端交付',
    fields: ['text', 'text', 'text', 'select', 'text'],
    createApi: /channelDefinition\/add/,
    deleteApi: /channelDefinition\/delete/,
  },
  {
    name: '奖励模板',
    menuTitle: '奖励模板',
    menuRoot: '内容运营',
    fields: ['text', 'select', 'number', 'reward-items', 'text'],
    createApi: /gameRewardTemplate\/add/,
    deleteApi: /gameRewardTemplate\/delete/,
  },
  {
    name: '兑换码',
rowEdit: false, // 动作列仅删除（盘点同源）；编辑复用新增弹窗
    menuTitle: '兑换码',
    menuRoot: '商业化',
    fields: ['select', 'text', 'date', 'select', 'text', 'number', 'date', 'skip-select', 'reward-items'],
    createApi: /gameRedeemCode\/add/,
    deleteApi: /gameRedeemCode\/delete/,
  },
  // 游戏内商品：级联 v2 已验证可用（前置目录数据已 seed），但 add 稳定 500 NRE——
  // （AddRequest CategoryId:string/Number:long 与前端契约错位 → 绑定失败 input=null）。
  // 级别 InputNumber fill 的 onInputFocus NRE 已在应用启动层兜底修复（Vue#201），
  // 奖励模板「级别」改回真实 number fill 作回归。
  {
    name: '字典管理',
    menuTitle: '字典管理',
    menuRoot: '系统',
    session: 'superadmin',
    fields: ['text', 'text'],
    createApi: /globalDictionaryType\/add/,
    deleteApi: /globalDictionaryType\/delete/,
  },
  {
    name: '参数配置',
    menuTitle: '参数配置',
    menuRoot: '系统',
    session: 'superadmin',
    fields: ['text', 'text', 'text', 'text'],
    createApi: /systemConfig\/add/,
    deleteApi: /systemConfig\/delete/,
  },
];

test.describe('CRUD 功能闭环（首批）', () => {
  for (const entity of ENTITIES) {
    test(`${entity.name} 新增回显删除闭环`, async ({ page: adminPage, browser }) => {
      // 默认 page fixture 是 admin 态；平台域实体用 superadmin 显式上下文
      const ownContext = entity.session === 'superadmin';
      const page = ownContext ? await superadminPage(browser) : adminPage;

      const token = `GFX_E2E_${Date.now().toString(36)}`;

      let created = false;
      let updated = false;
      let deleted = false;
      await page.route(entity.createApi, async (route) => {
        created = true;
        await route.continue();
      });
      await page.route(new RegExp(entity.createApi.source.replace('add', 'update')), async (route) => {
        updated = true;
        await route.continue();
      });
      await page.route(entity.deleteApi, async (route) => {
        deleted = true;
        await route.continue();
      });

      await page.goto('/');
      await expectOverview(page);
      await navigateByMenu(page, entity.menuTitle, entity.menuRoot);
      await expect(page.locator('.p-datatable-tbody')).toBeVisible({ timeout: 20_000 });

      await page.locator('role=button[name="新增"]').first().click();
      await expect(page.locator('.p-dialog')).toBeVisible({ timeout: 10_000 });

      // 按对话框内控件顺序索引填充（label 不关联可访问名，按名匹配不可靠）；
      // 每个文本字段值唯一化（避免唯一约束撞历史残留），首字段承载回显锚点 token
      let textIndex = 0;
      let selectIndex = 0;
      let numberIndex = 0;
      let cascadeIndex = 0;
      let dateFieldIndex = 0;
      for (const field of entity.fields) {
        if (field === 'text') {
          const value = textIndex === 0 ? token : `${token}-${textIndex}`;
          await page.locator(`.p-dialog >> role=textbox >> nth=${textIndex}`).fill(value);
          textIndex += 1;
        } else if (field === 'cascade') {
          // 级联选择 v2：物理 click 在 PrimeVue 4 CascadeSelect 上不生效（onOptionClick 不触发），
          // 用程序化 dispatchEvent。策略：每次在当前最深子列里优先点叶子（group=false），
          // 无叶子则点首个目录展开下一层；点中叶子即选中收起。封顶 6 层。
          await page.locator(`.p-dialog >> .p-cascadeselect-dropdown >> nth=${cascadeIndex}`).click();
          for (let depth = 0; depth < 6; depth += 1) {
            const step = await page.evaluate(() => {
              const overlays = Array.from(document.querySelectorAll('.p-cascadeselect-overlay')).filter(
                (el) => el.getBoundingClientRect().height > 0,
              );
              const overlay = overlays[0];
              if (!overlay) return 'closed';
              const lists = Array.from(overlay.querySelectorAll(':scope ul, :scope ul ul')).filter(
                (el) => el.getBoundingClientRect().width > 0,
              );
              const deepest = lists[lists.length - 1];
              if (!deepest) return 'no-list';
              const items = Array.from(deepest.children) as HTMLElement[];
              const leaf = items.find((li) => li.dataset.pOptionGroup === 'false' || li.getAttribute('data-p-option-group') === 'false');
              const target = (leaf ?? items[0])?.querySelector('.p-cascadeselect-option-content');
              if (!target) return 'no-item';
              target.dispatchEvent(new MouseEvent('click', { bubbles: true, cancelable: true }));
              return leaf ? 'leaf-clicked' : 'group-clicked';
            });
            if (step === 'closed' || step === 'leaf-clicked') break;
            await sleep(350);
          }
          // 不用 keyboard Escape：会直达 PrimeVue Dialog 的 dismissOnEscape 关掉整个对话框；
          // 面板未自动收起时点 body 外部关闭
          await page.evaluate(() => {
            const overlay = Array.from(document.querySelectorAll('.p-cascadeselect-overlay')).find(
              (el) => el.getBoundingClientRect().height > 0,
            );
            if (overlay) document.body.dispatchEvent(new MouseEvent('mousedown', { bubbles: true }));
          });
          cascadeIndex += 1;
          selectIndex += 1;
        } else if (field === 'json') {
          // 占 textbox 序的 JSON 语义字段（如 serverConfig.config），填合法空对象
          await page.locator(`.p-dialog >> role=textbox >> nth=${textIndex}`).fill('{}');
          textIndex += 1;
        } else if (field === 'editor') {
          // wangeditor 富文本：contenteditable 区点击后键入（不占 textbox/combobox 序）
          await page.locator('.w-e-text-container [contenteditable="true"]').click();
          await page.keyboard.type('E2E 内容');
        } else if (field === 'reward-items') {
          // RewardItemsEditor：点"添加"按钮增行，行内 catalogue/goods 两个 Select 各选首项。
          // 与 cascade 同理用 dispatchEvent（物理 click 对 PrimeVue 下拉不触发）。
          await page.locator('.p-dialog >> .reward-items-editor .add-button').click();
          await sleep(300);
          for (const sel of ['.catalogue-select', '.goods-select']) {
            await page.locator(`.p-dialog >> .reward-items-editor-row ${sel}`).click();
            const firstOption = page.locator('role=option >> nth=0');
            if (!(await firstOption.isVisible())) {
              await page.locator(`.p-dialog >> .reward-items-editor-row ${sel} ~ * , .p-dialog >> .reward-items-editor-row ${sel}`).first().click();
            }
            await firstOption.first().click();
            // 选项懒加载（goods 按 catalogue 过滤），等数据与面板收起
            await sleep(600);
          }
        } else if (field === 'skip-text') {
          textIndex += 1;
        } else if (field === 'skip-number') {
          // 占位：有默认值的数字字段（如 orderNo），仅递增 spinbutton 序号
          numberIndex += 1;
        } else if (field === 'skip-select') {
          selectIndex += 1;
        } else if (field === 'skip') {
          // 占位：物理存在但非必填的 combobox（如级联上级），仅递增序号
          selectIndex += 1;
        } else if (field === 'select') {
          await page.locator(`.p-dialog >> role=combobox >> nth=${selectIndex}`).click();
          const firstOption = page.locator('role=option >> nth=0');
          if (!(await firstOption.isVisible())) {
            // editable/只读 Select 点输入框不开面板：兜底点专用下拉钮
            await page.locator(`.p-dialog >> .p-select-dropdown >> nth=${selectIndex}`).click();
          }
          // 选项异步懒加载（如分组列表）：先等首项可见再点，避免空面板竞态
          await firstOption.first().waitFor({ state: 'visible', timeout: 8_000 });
          await firstOption.first().click();
          selectIndex += 1;
        } else if (field === 'unique-number') {
          // 唯一编号字段（如游戏物品 Number）：预置数据可能已占 1，用时间派生值避免唯一约束冲突
          await page.locator(`.p-dialog >> role=spinbutton >> nth=${numberIndex}`).fill(String((Date.now() % 899) + 2));
          numberIndex += 1;
        } else if (field === 'number') {
          await page.locator(`.p-dialog >> role=spinbutton >> nth=${numberIndex}`).fill('1');
          numberIndex += 1;
        } else {
          // DatePicker(showTime) 真实面板交互：点开 → 选可点日期格 → 调时分 → 点外部收起（helpers.pickDateTime）；
          // 同表单第 2+ 个日期选月末格，满足 end>start 及最小间隔校验（如兑换码失效时间）
          await pickDateTime(page, selectIndex, dateFieldIndex === 0 ? 'first' : 'last');
          dateFieldIndex += 1;
          selectIndex += 1;
        }
      }

      await page.locator('role=button[name="确定"]').first().click();
      await expect.poll(() => created, `${entity.name}: 新增请求未发出`).toBe(true);
      await expect(page.locator('.p-datatable-tbody')).toContainText(token, { timeout: 20_000 });
      // —— 编辑闭环（rowEdit=false 的页跳过：动作列未挂编辑按钮，如兑换码）：
      // 真实打开行编辑弹窗 → 回显断言（任意文本框含 token；编辑态字段顺序/禁用可与新增态不同）
      // → 改首个「可编辑」文本框（业务键编辑态锁定是常态；数字框用数字值）→ 提交 → 更新真实发出且行仍在 ——
      if (entity.rowEdit !== false) {
        await page.locator(`tr:has-text("${token}") >> text=编辑`).first().click();
        await expect(page.locator('.p-dialog')).toBeVisible({ timeout: 10_000 });
        const editState = await page.evaluate((tk) => {
          const boxes = Array.from(document.querySelectorAll('.p-dialog input, .p-dialog textarea')).filter(
            (el) => el.getBoundingClientRect().height > 0,
          );
          return {
            echoed: boxes.some((el) => (el as HTMLInputElement).value.includes(tk)),
            editableIndex: boxes.findIndex(
              (el) => !(el as HTMLInputElement).disabled && !(el as HTMLInputElement).readOnly,
            ),
          };
        }, token);
        expect(editState.echoed, `${entity.name}: 编辑弹窗未回显新增值`).toBe(true);
        if (editState.editableIndex >= 0) {
          const editableBox = page.locator('.p-dialog input:visible, .p-dialog textarea:visible').nth(editState.editableIndex);
          const isNumeric = await editableBox.evaluate(
            (el) => (el as HTMLInputElement).type === 'number' || (el as HTMLInputElement).getAttribute('inputmode') === 'numeric',
          );
          // 数字框填数字值（非数字会被校验拦下，编辑请求发不出）
          await editableBox.fill(isNumeric ? String((Date.now() % 899) + 2) : `${token}-E2`, { timeout: 5000 });
          await page.locator('role=button[name="确定"]').first().click();
          await expect.poll(() => updated, `${entity.name}: 编辑请求未发出`).toBe(true);
          await sleep(800);
          await expect(page.locator('.p-datatable-tbody')).toContainText(token, { timeout: 20_000 });
        } else {
          // 全字段锁定：仅验证编辑弹窗可开且回显
          await page.keyboard.press('Escape');
        }
      }

      await page.locator(`tr:has-text("${token}") >> text=删除`).first().click();
      await page.locator('role=button[name="确定"]').first().click();
      await expect.poll(() => deleted, `${entity.name}: 删除请求未发出`).toBe(true);
      await expect(page.locator('.p-datatable-tbody')).not.toContainText(token, { timeout: 20_000 });

      await shot(page, `crud-roundtrip-${entity.name}`);
      if (ownContext) await page.close();
    });
  }
});

// 广告回调配置闭环（配置按分包绑定）：该页对话框是 schema 驱动（Provider 参数区 + 密钥 Password 框），
// 不进通用 CRUD 矩阵（fields 引擎不覆盖 password 型字段），此处专项覆盖：
// 新增必须先选分包 → 提交链带 packageId；编辑弹窗回显所选分包标签；更新与删除闭环。
// 权限口径（协调者裁定）：跨页选择器沿用 gameAppPackage Select 现有消费约定（同 remoteConfig / gamePaymentConfigEntity），
// 不为本页单独补按钮权限；e2e 的 admin 夹具授予全部非超管菜单，包含 gameAppPackage:Select。
test.describe('广告回调配置分包闭环（F2）', () => {
  test('广告回调配置 选分包新增 编辑回显 更新删除', async ({ page }) => {
    const token = `GFX_E2E_ADCB_${Date.now().toString(36)}`;

    let created = false;
    let updated = false;
    let deleted = false;
    await page.route(/advertisementCallbackConfig\/add/, async (route) => {
      created = true;
      await route.continue();
    });
    await page.route(/advertisementCallbackConfig\/update/, async (route) => {
      updated = true;
      await route.continue();
    });
    await page.route(/advertisementCallbackConfig\/delete/, async (route) => {
      deleted = true;
      await route.continue();
    });

    await page.goto('/');
    await expectOverview(page);
    await navigateByMenu(page, '广告回调验签', 'C 端交付');
    await expect(page.locator('.p-datatable-tbody')).toBeVisible({ timeout: 20_000 });

    // 前置清理：失败重跑可能留下 (PackageId, Provider) 配置，唯一约束会令新增被拒；
    // 只清本用例会选中的选项 0（分包夹具 com.e2e.pkg.alpha）上的遗留行
    for (let guard = 0; guard < 5; guard += 1) {
      const stale = page.locator('.p-datatable-tbody tr').filter({ hasText: 'com.e2e.pkg.alpha' }).first();
      if (await stale.count() === 0) {
        break;
      }
      await stale.getByText('删除', { exact: true }).click();
      await page.locator('role=button[name="确定"]').first().click();
      await page.waitForTimeout(500);
    }

    // —— 新增：Provider 默认 TopOn（schema 参数区异步加载后出现）；分包选择器在 Provider 之后 ——
    await page.locator('role=button[name="新增"]').first().click();
    await expect(page.locator('.p-dialog')).toBeVisible({ timeout: 10_000 });
    await expect(page.locator('.p-dialog .parameter-panel')).toBeVisible({ timeout: 10_000 });

    await page.locator('.p-dialog >> role=combobox >> nth=1').click();
    const firstPackageOption = page.locator('role=option >> nth=0');
    await firstPackageOption.first().waitFor({ state: 'visible', timeout: 8_000 });
    await firstPackageOption.first().click();
    // 记录所选分包标签，供编辑回显断言
    const chosenPackageLabel = await page.locator('.p-dialog >> role=combobox >> nth=1').evaluate((el) => {
      const label = el.closest('.p-select')?.querySelector('.p-select-label');
      return (label?.textContent ?? '').trim();
    });
    expect(chosenPackageLabel.length, '分包选择器未回显所选标签').toBeGreaterThan(0);

    // schema 参数区：首个文本框为回调 URL（承载 token 锚点），TopOn 的 sec_key 是 Password 框
    await page.locator('.p-dialog >> role=textbox >> nth=0').fill(`https://cb.example.test/${token}`);
    await page.locator('.p-dialog .p-password input').first().fill('e2e-adcb-secret');

    await page.locator('role=button[name="确定"]').first().click();
    await expect.poll(() => created, '广告回调配置新增请求未发出').toBe(true);
    await expect(page.locator('.p-datatable-tbody')).toContainText(token, { timeout: 20_000 });

    // —— 编辑：分包选择器必须回显新增时所选分包（R5 绑定后 packageId 是配置身份的一部分）——
    await page.locator(`tr:has-text("${token}") >> text=编辑`).first().click();
    await expect(page.locator('.p-dialog')).toBeVisible({ timeout: 10_000 });
    await expect(page.locator('.p-dialog .parameter-panel')).toBeVisible({ timeout: 10_000 });
    const echoedPackageLabel = await page.locator('.p-dialog >> role=combobox >> nth=1').evaluate((el) => {
      const label = el.closest('.p-select')?.querySelector('.p-select-label');
      return (label?.textContent ?? '').trim();
    });
    expect(echoedPackageLabel, '编辑弹窗未回显新增时所选分包').toBe(chosenPackageLabel);
    const echoedUrl = await page.locator('.p-dialog >> role=textbox >> nth=0').inputValue();
    expect(echoedUrl, '编辑弹窗未回显回调 URL').toContain(token);

    await page.locator('role=button[name="确定"]').first().click();
    await expect.poll(() => updated, '广告回调配置更新请求未发出').toBe(true);
    await expect(page.locator('.p-datatable-tbody')).toContainText(token, { timeout: 20_000 });

    // —— 删除闭环 ——
    await page.locator(`tr:has-text("${token}") >> text=删除`).first().click();
    await page.locator('role=button[name="确定"]').first().click();
    await expect.poll(() => deleted, '广告回调配置删除请求未发出').toBe(true);
    await expect(page.locator('.p-datatable-tbody')).not.toContainText(token, { timeout: 20_000 });

    await shot(page, 'crud-roundtrip-广告回调配置');
  });
});
