#!/usr/bin/env node

/**
 * i18n 跨语言键完整性验证脚本
 *
 * 以 zh-cn 为基准，检测 en/zh-tw/ja/ko 是否存在缺失或多余的翻译 key。
 * 存在差异时 exit(1)，CI 友好。
 *
 * 用法: node scripts/check-i18n-keys.mjs
 */

import { readdirSync, readFileSync } from 'node:fs';
import { join, basename } from 'node:path';
import { fileURLToPath } from 'node:url';

const __dirname = fileURLToPath(new URL('.', import.meta.url));
const I18N_ROOT = join(__dirname, '..', 'src', 'i18n');
const SRC_ROOT = join(__dirname, '..', 'src');
const LOCALES = ['zh-cn', 'en', 'zh-tw', 'ja', 'ko'];
const BASE_LOCALE = 'zh-cn';

// 匹配 t('key') / t("key") / $t('key') / $t("key") 的字面量 key 引用。
// 前导字符必须非 word（避免匹配 format( / get( / it( 等）。
const KEY_REF_RE = /(?:^|[^a-zA-Z0-9_$])(?:\$t|t)\(\s*['"]([a-zA-Z0-9_.]+)['"]/g;

// ---------------------------------------------------------------------------
// 工具函数
// ---------------------------------------------------------------------------

/** 递归收集目录下所有 .ts 文件（相对 I18N_ROOT 的路径） */
function collectTsFiles(dir) {
  const results = [];
  for (const entry of readdirSync(dir, { withFileTypes: true })) {
    const fullPath = join(dir, entry.name);
    if (entry.isDirectory()) {
      results.push(...collectTsFiles(fullPath));
    } else if (entry.isFile() && entry.name.endsWith('.ts')) {
      results.push(fullPath);
    }
  }
  return results;
}

/** 从 .ts 文件内容中提取导出的对象 */
function parseTsObject(filePath) {
  let content = readFileSync(filePath, 'utf-8');

  // 去除单行注释
  content = content.replace(/\/\/.*$/gm, '');
  // 去除多行注释
  content = content.replace(/\/\*[\s\S]*?\*\//g, '');
  // 去除 export default
  content = content.replace(/export\s+default\s*/, '');

  const trimmed = content.trim().replace(/;$/, '');
  if (!trimmed || trimmed === '{}') return {};

  try {
    // 用括号包裹防止 ASI 将 `return` + 换行 + `{` 误解析为 return undefined
    return new Function(`return (${trimmed})`)();
  } catch (e) {
    console.error(`\n💥 解析失败: ${filePath}\n    ${e.message}\n    翻译文件格式可能已变化，脚本需更新。\n`);
    process.exit(1);
  }
}

/** 递归提取对象的所有叶子 key 路径（dot-notation） */
function extractKeyPaths(obj, prefix = '') {
  const paths = [];
  for (const key of Object.keys(obj)) {
    const fullPath = prefix ? `${prefix}.${key}` : key;
    if (obj[key] !== null && typeof obj[key] === 'object' && !Array.isArray(obj[key])) {
      paths.push(...extractKeyPaths(obj[key], fullPath));
    } else {
      paths.push(fullPath);
    }
  }
  return paths;
}

/** 按文件名推断所属语言 */
function localeFromFileName(fileName) {
  const name = basename(fileName, '.ts');
  return LOCALES.includes(name) ? name : null;
}

/** 递归收集目录下所有匹配扩展名的源码文件（排除 node_modules 与 i18n 定义目录） */
function collectSourceFiles(dir, exts) {
  const results = [];
  for (const entry of readdirSync(dir, { withFileTypes: true })) {
    if (entry.name === 'node_modules') continue;
    const fullPath = join(dir, entry.name);
    if (entry.isDirectory()) {
      results.push(...collectSourceFiles(fullPath, exts));
    } else if (entry.isFile() && exts.some((ext) => entry.name.endsWith(ext))) {
      results.push(fullPath);
    }
  }
  return results;
}

/** 扫描 src/ 下 .vue/.ts/.js 中的字面量 i18n key 引用，返回 key -> 引用文件集合 */
function scanUsageRefs() {
  const files = collectSourceFiles(SRC_ROOT, ['.vue', '.ts', '.js']);
  const refs = new Map();
  for (const filePath of files) {
    // 跳过 i18n 定义文件本身
    if (filePath.startsWith(I18N_ROOT + '/')) continue;
    const content = readFileSync(filePath, 'utf-8');
    KEY_REF_RE.lastIndex = 0;
    let match;
    while ((match = KEY_REF_RE.exec(content)) !== null) {
      if (!refs.has(match[1])) refs.set(match[1], new Set());
      refs.get(match[1]).add(filePath);
    }
  }
  return refs;
}

/** 递归提取对象的所有路径（含中间节点），t() 可引用中间节点用于动态拼接 */
function extractAllPaths(obj, prefix = '') {
  const paths = [];
  for (const key of Object.keys(obj)) {
    const fullPath = prefix ? `${prefix}.${key}` : key;
    paths.push(fullPath);
    if (obj[key] !== null && typeof obj[key] === 'object' && !Array.isArray(obj[key])) {
      paths.push(...extractAllPaths(obj[key], fullPath));
    }
  }
  return paths;
}

// ---------------------------------------------------------------------------
// 主逻辑
// ---------------------------------------------------------------------------

function main() {
  const unusedFail = process.argv.includes('--unused-fail');

  const files = collectTsFiles(I18N_ROOT);

  // 按语言收集合并后的 key 集合
  const localeKeys = {};
  for (const locale of LOCALES) {
    localeKeys[locale] = new Set();
  }

  // 按命名空间分组收集（用于差异报告中的定位）
  // namespace -> { locale -> Set<keyPath> }
  const namespaceKeys = {};

  // 基准语言的全路径集合（含中间节点），用于 missing 检测
  const baseFullPaths = new Set();

  for (const filePath of files) {
    const fileName = basename(filePath);
    const locale = localeFromFileName(fileName);
    if (!locale) continue;

    const relativePath = filePath.replace(I18N_ROOT + '/', '');
    const ns = relativePath.includes('/')
      ? relativePath.split('/').slice(0, -1).join('/')
      : 'lang';

    if (!namespaceKeys[ns]) {
      namespaceKeys[ns] = {};
      for (const l of LOCALES) namespaceKeys[ns][l] = new Set();
    }

    const obj = parseTsObject(filePath);
    const keyPaths = extractKeyPaths(obj);
    if (locale === BASE_LOCALE) {
      for (const p of extractAllPaths(obj)) baseFullPaths.add(p);
    }

    for (const kp of keyPaths) {
      localeKeys[locale].add(kp);
      namespaceKeys[ns][locale].add(kp);
    }
  }

  // 统计
  const baseCount = localeKeys[BASE_LOCALE].size;
  console.log(`\n🔑 Key 统计 (基准: ${BASE_LOCALE}, ${baseCount} keys)\n`);

  let hasDiff = false;
  const sortedLocales = LOCALES.filter((l) => l !== BASE_LOCALE);

  for (const locale of sortedLocales) {
    const targetKeys = localeKeys[locale];
    const count = targetKeys.size;

    // 缺失: zh-cn 有但目标语言没有
    const missing = [...localeKeys[BASE_LOCALE]].filter((k) => !targetKeys.has(k));
    // 多余: 目标语言有但 zh-cn 没有
    const extra = [...targetKeys].filter((k) => !localeKeys[BASE_LOCALE].has(k));

    if (missing.length === 0 && extra.length === 0) {
      console.log(`  ✅ ${locale}: ${count} keys — 与基准一致`);
      continue;
    }

    hasDiff = true;
    console.log(`  ❌ ${locale}: ${count} keys — 存在差异`);

    if (missing.length > 0) {
      console.log(`     缺失 (${missing.length}):`);
      for (const ns of Object.keys(namespaceKeys).sort()) {
        const nsMissing = missing.filter((k) => namespaceKeys[ns][BASE_LOCALE].has(k));
        if (nsMissing.length > 0) {
          console.log(`       [${ns}]`);
          for (const k of nsMissing) {
            console.log(`         - ${k}`);
          }
        }
      }
    }

    if (extra.length > 0) {
      console.log(`     多余 (${extra.length}):`);
      for (const ns of Object.keys(namespaceKeys).sort()) {
        const nsExtra = extra.filter((k) => namespaceKeys[ns][locale]?.has(k));
        if (nsExtra.length > 0) {
          console.log(`       [${ns}]`);
          for (const k of nsExtra) {
            console.log(`         - ${k}`);
          }
        }
      }
    }
  }

  if (!hasDiff) {
    console.log(`\n✨ All ${LOCALES.length} locales have identical keys (${baseCount} keys each)\n`);
  } else {
    console.log(`\n💥 Key differences detected. Please sync translation files.\n`);
    process.exit(1);
  }

  // -------------------------------------------------------------------------
  // Missing key 检测：代码引用了但翻译文件未定义的 key（运行时页面显示原始 key）
  // -------------------------------------------------------------------------
  // 运行时 messages 结构为 { name, ...merged, message: merged }（见 src/i18n/index.ts），
  // 顶级 key 同时暴露在顶级与 message 之下，因此 t() 引用可带或不带 `message.` 前缀。
  const stripMessagePrefix = (k) => (k.startsWith('message.') ? k.slice('message.'.length) : k);
  const refs = scanUsageRefs();
  const missing = [...refs.keys()].filter((k) => !baseFullPaths.has(stripMessagePrefix(k)));

  console.log(`🔍 Missing key 检测 (字面量引用 vs ${BASE_LOCALE} 定义)\n`);

  if (missing.length === 0) {
    console.log(`  ✅ 未发现 missing key — ${refs.size} 个引用 key 全部有定义。\n`);
  } else {
    const byFile = {};
    for (const k of missing) {
      for (const f of refs.get(k)) {
        (byFile[f] ??= []).push(k);
      }
    }
    console.log(`  ❌ 发现 ${missing.length} 个 missing key（分布在 ${Object.keys(byFile).length} 个文件）\n`);
    for (const f of Object.keys(byFile).sort()) {
      console.log(`  ${f}  (${byFile[f].length} keys)`);
      for (const k of byFile[f].sort().slice(0, 5)) {
        console.log(`    - ${k}`);
      }
      if (byFile[f].length > 5) {
        console.log(`    ... 还有 ${byFile[f].length - 5} 个`);
      }
    }
    console.log(`\n💥 Missing keys detected. 请补齐对应语言文件的翻译，或修正引用 key。\n`);
    process.exit(1);
  }

  // -------------------------------------------------------------------------
  // Unused key 检测：定义了但代码中无字面量引用的 key
  // -------------------------------------------------------------------------

  const usedKeys = new Set([...refs.keys()].map(stripMessagePrefix));
  const unused = [...localeKeys[BASE_LOCALE]].filter((k) => !usedKeys.has(k));

  // 按 namespace 分组
  const unusedByNs = {};
  for (const ns of Object.keys(namespaceKeys)) {
    const nsUnused = unused.filter((k) => namespaceKeys[ns][BASE_LOCALE].has(k));
    if (nsUnused.length > 0) {
      unusedByNs[ns] = nsUnused.sort();
    }
  }

  console.log(`🔍 Unused key 检测 (字面量引用扫描)\n`);

  if (unused.length === 0) {
    console.log(`  ✅ 未发现 unused key — 全部 ${baseCount} 个 key 均有字面量引用。\n`);
    return;
  }

  const pct = ((unused.length / baseCount) * 100).toFixed(1);
  console.log(`  ⚠️  发现 ${unused.length} / ${baseCount} (${pct}%) 个 unused key（定义了但无字面量引用）\n`);

  for (const ns of Object.keys(unusedByNs).sort()) {
    const nsKeys = unusedByNs[ns];
    console.log(`  [${ns}] (${nsKeys.length} unused)`);
    for (const k of nsKeys.slice(0, 10)) {
      console.log(`    - ${k}`);
    }
    if (nsKeys.length > 10) {
      console.log(`    ... 还有 ${nsKeys.length - 10} 个`);
    }
  }

  if (unusedFail) {
    console.log(`\n💥 Unused keys detected (--unused-fail enabled).\n`);
    process.exit(1);
  } else {
    console.log(`\n  ℹ️  Unused key 不算硬失败（未传 --unused-fail），仅警告。\n`);
  }
}

main();
