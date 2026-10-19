#!/usr/bin/env node

/**
 * 前端权限码（按钮 Key）与后端权威清单对账脚本
 *
 * 权威清单 scripts/permission-keys.txt 来自主库 global_menu_entity 按钮 Key
 * （type=3 且 is_enabled，排除 permission:auto:* 隐藏自动按钮）。
 * 检测两类失配：
 *   1. 字面量码不在清单（error）：v-auth="'x'" / auth('x') / hasAnyAuth('a', 'b')
 *      —— 后端无此按钮 Key，运行时按钮必然隐藏，视为硬错误。
 *   2. 组件拼码不在清单（warning，--composed-fail 收紧）：
 *      CrudDataTable / SearchForm 的静态 authPrefix × 动作集
 *      （CrudDataTable: detail/update/delete/export + 同文件 customActions.auth；
 *        SearchForm: page/add）
 *      —— 失配可能是真失配（prefix 写错），也可能是该页面未启用该动作按钮
 *      （后端由 permission:auto:* 隐藏按钮兜底，仅前端不显示），需人工拍板。
 * 另输出清单中未被前端消费的孤儿码（仅提示，不算失败）。
 *
 * 已确认的合法失配登记在 scripts/permission-keys-ignored.json（按文件列 code）。
 *
 * 清单刷新（本机 docker postgres，菜单种子/按钮变更后执行）：
 *   docker exec gameframex-postgres psql -U gameframex -d gameframex_admin -At -c \
 *     "SELECT key FROM global_menu_entity WHERE type=3 AND is_enabled AND key IS NOT NULL AND key NOT LIKE 'permission:auto:%' ORDER BY key" \
 *     > scripts/permission-keys.txt
 *
 * 用法: node scripts/check-permission-keys.mjs [--composed-fail]
 */

import { readdirSync, readFileSync, existsSync } from 'node:fs';
import { join } from 'node:path';
import { fileURLToPath } from 'node:url';

const __dirname = fileURLToPath(new URL('.', import.meta.url));
const SRC_ROOT = join(__dirname, '..', 'src');
const KEYS_FILE = join(__dirname, 'permission-keys.txt');
const IGNORED_FILE = join(__dirname, 'permission-keys-ignored.json');
const COMPOSED_FAIL = process.argv.includes('--composed-fail');

// 组件自身文件跳过：内部含 authPrefix="authPrefix" 自引用与模板串，不是消费方
const SKIP_FILES = [
  join(SRC_ROOT, 'components', 'crud-data-table', 'index.vue'),
  join(SRC_ROOT, 'components', 'search-form', 'index.vue'),
];

// CrudDataTable 模板固定 + builtinAuthKeys 动作（detail/edit→update/delete/export）
const CRUD_ACTIONS = ['detail', 'update', 'delete', 'export'];
// SearchForm: queryAuth → prefix:page / addAuth → prefix:add
const SEARCH_FORM_ACTIONS = ['page', 'add'];

// v-auth="'xxx'" 静态字面量
const V_AUTH_RE = /v-auth="+'([^']+)'"+/g;
// auth('x') / hasAnyAuth('a', 'b') 字面量参数；前导非 word 防 format( / it( 误匹配
const AUTH_CALL_RE = /(?:^|[^a-zA-Z0-9_$.])(?:auth|hasAnyAuth)\(\s*['"]([^'"]+)['"]/g;
// <CrudDataTable ... authPrefix="xxx"> / <SearchForm ... authPrefix="xxx">（属性可跨行）
const TAG_PREFIX_RE = /<(CrudDataTable|SearchForm|crud-data-table|search-form)\b[^>]*?\sauthPrefix="([^"]+)"/g;
// <CrudDataTable ... :authPrefix="expr"> 动态绑定 → 无法静态展开，显式报告
const TAG_DYNAMIC_PREFIX_RE = /<(?:CrudDataTable|SearchForm|crud-data-table|search-form)\b[^>]*?\s:authPrefix="/g;
// customActions 数组内的 auth: 'xxx' 字面量
const CUSTOM_ACTION_AUTH_RE = /(?:^|[^a-zA-Z0-9_$])auth:\s*['"]([^'"]+)['"]/g;

// ---------------------------------------------------------------------------
// 工具函数
// ---------------------------------------------------------------------------

/** 递归收集 src 下所有 .vue/.ts/.tsx 文件 */
function collectFiles(dir) {
  const results = [];
  for (const entry of readdirSync(dir, { withFileTypes: true })) {
    const fullPath = join(dir, entry.name);
    if (entry.isDirectory()) {
      results.push(...collectFiles(fullPath));
    } else if (entry.isFile() && /\.(vue|tsx?)$/.test(entry.name)) {
      results.push(fullPath);
    }
  }
  return results;
}

function rel(p) {
  return p.replace(SRC_ROOT + '/', '');
}

// ---------------------------------------------------------------------------
// 主流程
// ---------------------------------------------------------------------------

function main() {
  if (!existsSync(KEYS_FILE)) {
    console.error(`❌ 权威清单缺失: scripts/permission-keys.txt（生成命令见本脚本头部注释）`);
    process.exit(1);
  }
  const knownKeys = new Set(
    readFileSync(KEYS_FILE, 'utf-8').split('\n').map(s => s.trim()).filter(Boolean)
  );

  const ignoredByFile = existsSync(IGNORED_FILE)
    ? JSON.parse(readFileSync(IGNORED_FILE, 'utf-8'))
    : {};
  // key 为相对 src 的文件路径，value 为 code 数组
  const isIgnored = (file, code) => (ignoredByFile[rel(file)] ?? []).includes(code);

  const files = collectFiles(SRC_ROOT).filter(f => !SKIP_FILES.includes(f));

  // 字面量码失配：[file, code]
  const literalMissing = [];
  // 拼码失配：[file, code]
  const composedMissing = [];
  // 动态绑定 authPrefix（无法静态展开）
  const dynamicPrefixes = [];
  // 已消费的码（供孤儿码统计）
  const consumed = new Set();

  let literalCount = 0;

  for (const file of files) {
    const content = readFileSync(file, 'utf-8');

    // 1. 字面量码
    for (const re of [V_AUTH_RE, AUTH_CALL_RE]) {
      re.lastIndex = 0;
      let m;
      while ((m = re.exec(content)) !== null) {
        literalCount++;
        consumed.add(m[1]);
        if (!knownKeys.has(m[1]) && !isIgnored(file, m[1])) {
          literalMissing.push([rel(file), m[1]]);
        }
      }
    }

    // 2. 组件拼码：文件内静态 prefix × 动作集
    const crudPrefixes = new Set();
    const searchPrefixes = new Set();
    TAG_PREFIX_RE.lastIndex = 0;
    let tm;
    while ((tm = TAG_PREFIX_RE.exec(content)) !== null) {
      const isSearch = /^(SearchForm|search-form)$/.test(tm[1]);
      (isSearch ? searchPrefixes : crudPrefixes).add(tm[2]);
    }
    TAG_DYNAMIC_PREFIX_RE.lastIndex = 0;
    if (TAG_DYNAMIC_PREFIX_RE.test(content)) {
      dynamicPrefixes.push(rel(file));
    }

    const customActions = new Set();
    CUSTOM_ACTION_AUTH_RE.lastIndex = 0;
    let cm;
    while ((cm = CUSTOM_ACTION_AUTH_RE.exec(content)) !== null) {
      customActions.add(cm[1]);
    }

    const crudActionSet = [...new Set([...CRUD_ACTIONS, ...customActions])];
    for (const prefix of crudPrefixes) {
      for (const action of crudActionSet) {
        const code = `${prefix}:${action}`;
        consumed.add(code);
        if (!knownKeys.has(code)) {
          composedMissing.push([rel(file), code]);
        }
      }
    }
    for (const prefix of searchPrefixes) {
      for (const action of SEARCH_FORM_ACTIONS) {
        const code = `${prefix}:${action}`;
        consumed.add(code);
        if (!knownKeys.has(code)) {
          composedMissing.push([rel(file), code]);
        }
      }
    }
  }

  // ---- 报告 ----
  console.log(`🔍 前端权限码对账（权威清单 ${knownKeys.size} 个按钮 Key，扫描 ${files.length} 个文件）\n`);

  if (dynamicPrefixes.length > 0) {
    console.log(`  ⚠️  动态绑定 :authPrefix 无法静态展开（需人工确认）：`);
    for (const f of dynamicPrefixes) {
      console.log(`    - ${f}`);
    }
    console.log('');
  }

  if (literalMissing.length === 0) {
    console.log(`  ✅ 字面量码（${literalCount} 处）全部命中清单。`);
  } else {
    console.log(`  ❌ 字面量码失配 ${literalMissing.length} 处（后端无此按钮 Key，运行时按钮必然隐藏）：`);
    const byFile = new Map();
    for (const [f, code] of literalMissing) {
      if (!byFile.has(f)) byFile.set(f, new Set());
      byFile.get(f).add(code);
    }
    for (const [f, codes] of byFile) {
      console.log(`    [${f}]`);
      for (const c of codes) console.log(`      - ${c}`);
    }
  }
  console.log('');

  if (composedMissing.length === 0) {
    console.log(`  ✅ 组件拼码全部命中清单。`);
  } else {
    console.log(`  ⚠️  组件拼码失配 ${composedMissing.length} 处（真失配 or 未启用动作，需人工确认${COMPOSED_FAIL ? '' : '，默认不失败'}）：`);
    const byFile = new Map();
    for (const [f, code] of composedMissing) {
      if (!byFile.has(f)) byFile.set(f, []);
      byFile.get(f).push(code);
    }
    for (const [f, codes] of byFile) {
      console.log(`    [${f}]`);
      for (const c of codes) console.log(`      - ${c}`);
    }
  }
  console.log('');

  const orphans = [...knownKeys].filter(k => !consumed.has(k));
  console.log(`  ℹ️  清单中未被前端消费的按钮 Key ${orphans.length} 个（含按动作粒度未用到的，仅提示不算失败）。`);

  if (literalMissing.length > 0 || (COMPOSED_FAIL && composedMissing.length > 0)) {
    console.log(`\n💥 权限码失配 detected.\n`);
    process.exit(1);
  }
  console.log(`\n  ✅ 权限码对账通过。\n`);
}

main();
