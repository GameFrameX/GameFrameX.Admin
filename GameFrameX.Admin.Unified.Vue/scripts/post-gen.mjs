/**
 * post-gen.mjs —— swagger-typescript-api 生成后处理（split + restructure 二合一）。
 *
 * 输入：.api-gen/（stc 原始扁平输出：data-contracts.ts + http-client.ts + 各 <Tag>.ts）
 * 输出：src/api/<tag-camel>/{index,types}.ts + src/api/_shared/{http-client,data-contracts}.ts
 *
 * 做三件事：
 * 1. split：TS AST 拆 data-contracts —— 单 tag 引用（独占）→ 各 tag/types.ts；多 tag 共享 → _shared/data-contracts.ts
 * 2. restructure：扁平 <Tag>.ts/<Tag>.types.ts → <tag-camel>/{index,types}.ts（class 放 index.ts）
 * 3. 改写 import 路径：./data-contracts → ../_shared/data-contracts 或 ./types；./http-client → ../_shared/http-client
 *
 * 清理：每次删 src/api/_shared/ + 含 stc 生成标记的 tag 目录（精确，不碰手写层其他文件）。
 * 由 gen-api.mjs 在 generateApi 之后调用；也可单独执行。
 *
 * 归属规则：类型被恰好 1 个 tag 的依赖闭包引用 → 独占；≥2 或无人引用 → 共享。
 */
import { readFileSync, writeFileSync, readdirSync, rmSync, mkdirSync, existsSync, statSync } from 'node:fs';
import { resolve, dirname } from 'node:path';
import { fileURLToPath } from 'node:url';
import { createRequire } from 'node:module';

const require = createRequire(import.meta.url);
const ts = require('typescript');

const __dirname = dirname(fileURLToPath(import.meta.url));
const projectRoot = resolve(__dirname, '..');
const genDir = resolve(projectRoot, '.api-gen');
const apiDir = resolve(projectRoot, 'src/api');
const sharedDir = resolve(apiDir, '_shared');
const isDir = (p) => { try { return statSync(p).isDirectory(); } catch { return false; } };
const camelTag = (tag) => tag[0].toLowerCase() + tag.slice(1);
// C201：移除 // @ts-nocheck —— 生成类型已与运行时 camelCase 归一化结果一致，
// 需要 tsc 正常检查生成代码与调用方，消除类型债根因。保留 eslint/tslint disable（生成代码不强求 lint 通过）。
const HEADER = '/* eslint-disable */\n/* tslint:disable */\n';
// stc 给 <Tag>.ts / http-client.ts 自带 // @ts-nocheck 头，post-gen 原样保留时会把它带进 index.ts / _shared/http-client.ts。
// 这里统一剥掉，确保生成层不再出现 @ts-nocheck。
const stripTsNoCheck = (src) => src.replace(/\/\/[^\n]*@ts-nocheck[^\n]*\n?/g, '');
const GEN_MARK = 'GENERATED VIA SWAGGER-TYPESCRIPT-API';

// --- 1. 解析 .api-gen/data-contracts.ts 的所有顶层类型 ---
const dcRaw = readFileSync(resolve(genDir, 'data-contracts.ts'), 'utf8');
const sf = ts.createSourceFile('data-contracts.ts', dcRaw, ts.ScriptTarget.Latest, true);
const allTypes = new Map();
const order = [];
for (const stmt of sf.statements) {
  let name;
  if (ts.isInterfaceDeclaration(stmt)) name = stmt.name.text;
  else if (ts.isTypeAliasDeclaration(stmt)) name = stmt.name.text;
  else if (ts.isEnumDeclaration(stmt)) name = stmt.name.text;
  else continue;
  const refs = new Set();
  const visit = (node) => {
    if (ts.isTypeReferenceNode(node) && ts.isIdentifier(node.typeName)) refs.add(node.typeName.text);
    ts.forEachChild(node, visit);
  };
  visit(stmt);
  allTypes.set(name, { fullText: stmt.getFullText().trim(), refs });
  order.push(name);
}
for (const t of allTypes.values()) for (const r of [...t.refs]) if (!allTypes.has(r)) t.refs.delete(r);

// --- 2. 解析每个 <Tag>.ts 直接 import 的类型 ---
const tagFiles = readdirSync(genDir)
  .filter(f => f.endsWith('.ts') && !f.endsWith('.types.ts') && f !== 'data-contracts.ts' && f !== 'http-client.ts');
const tagDirectImports = new Map();
for (const f of tagFiles) {
  const tag = f.slice(0, -3);
  const src = readFileSync(resolve(genDir, f), 'utf8');
  const s = ts.createSourceFile(f, src, ts.ScriptTarget.Latest, true);
  const names = [];
  const seen = new Set();
  for (const stmt of s.statements) {
    if (!ts.isImportDeclaration(stmt)) continue;
    if ((stmt.moduleSpecifier && stmt.moduleSpecifier.text) !== './data-contracts') continue;
    const clause = stmt.importClause;
    if (!clause || !clause.namedBindings || !ts.isNamedImports(clause.namedBindings)) continue;
    for (const el of clause.namedBindings.elements) {
      const n = el.name.text;
      if (!seen.has(n)) { seen.add(n); names.push(n); }
    }
  }
  tagDirectImports.set(tag, names);
}

// --- 3. 闭包 + 归属 ---
const tagClosure = new Map();
for (const [tag, direct] of tagDirectImports) {
  const closure = new Set(direct);
  const stack = [...direct];
  while (stack.length) {
    const t = allTypes.get(stack.pop());
    if (!t) continue;
    for (const r of t.refs) if (!closure.has(r)) { closure.add(r); stack.push(r); }
  }
  tagClosure.set(tag, closure);
}
const typeTags = new Map();
for (const name of allTypes.keys()) typeTags.set(name, new Set());
for (const [tag, closure] of tagClosure) for (const name of closure) if (typeTags.has(name)) typeTags.get(name).add(tag);

const tagOwned = new Map();
const shared = [];
const sharedSet = new Set();
for (const name of order) {
  const tags = typeTags.get(name);
  if (tags.size === 1) {
    const tag = [...tags][0];
    if (!tagOwned.has(tag)) tagOwned.set(tag, []);
    tagOwned.get(tag).push(name);
  } else { shared.push(name); sharedSet.add(name); }
}

// --- 4. 清理旧生成产物（_shared/ + 含 stc 标记的 tag 目录），不碰手写层 ---
if (existsSync(sharedDir)) rmSync(sharedDir, { recursive: true, force: true });
for (const entry of readdirSync(apiDir)) {
  const d = resolve(apiDir, entry);
  if (!isDir(d)) continue;
  const idx = resolve(d, 'index.ts');
  if (!existsSync(idx)) continue;
  if (readFileSync(idx, 'utf8').includes(GEN_MARK)) rmSync(d, { recursive: true, force: true });
}
mkdirSync(sharedDir, { recursive: true });

// --- 5. _shared/http-client.ts（自包含，原样复制，剥掉 stc 自带的 @ts-nocheck）---
writeFileSync(resolve(sharedDir, 'http-client.ts'), stripTsNoCheck(readFileSync(resolve(genDir, 'http-client.ts'), 'utf8')));

// --- 6. _shared/data-contracts.ts（共享类型 + barrel re-export 各 tag/types）---
{
  const sharedBlock = stripTsNoCheck(shared.map(n => allTypes.get(n).fullText).join('\n\n'));
  const barrel = [...tagOwned.keys()].sort().map(t => `export * from "../${camelTag(t)}/types";`).join('\n');
  writeFileSync(resolve(sharedDir, 'data-contracts.ts'),
    `${HEADER}\n// 多 tag 共享类型 + barrel（re-export 各 tag 独占类型，保持统一入口；业务可按需从 <tag>/types 精确 import）\n\n${sharedBlock}\n\n${barrel}\n`);
}

// --- 7. 各 <tag-camel>/{index,types}.ts ---
const fmtImport = (names, from) => names.length ? `import {\n  ${names.join(',\n  ')},\n} from "${from}";` : '';

for (const [tag, names] of tagOwned) {
  const dir = resolve(apiDir, camelTag(tag));
  mkdirSync(dir, { recursive: true });

  // types.ts：独占类型 + import type 它们引用的共享类型
  const neededShared = new Set();
  for (const name of names) for (const r of allTypes.get(name).refs) if (sharedSet.has(r)) neededShared.add(r);
  const tImports = neededShared.size
    ? `import type { ${[...neededShared].sort().join(', ')} } from "../_shared/data-contracts";\n\n`
    : '';
  // 独占类型里的首个类型可能携带 stc 文件头 banner（含 @ts-nocheck）作为前导注释，剥掉它
  const tDefs = stripTsNoCheck(names.map(n => allTypes.get(n).fullText).join('\n\n'));
  writeFileSync(resolve(dir, 'types.ts'), `${HEADER}\n${tImports}${tDefs}\n`);

  // index.ts：原始 <Tag>.ts 改写 import 路径（并剥掉 stc 自带的 @ts-nocheck）
  let content = stripTsNoCheck(readFileSync(resolve(genDir, `${tag}.ts`), 'utf8'));
  const owned = new Set(names);
  const allNames = tagDirectImports.get(tag);
  const fromTypes = allNames.filter(n => owned.has(n));
  const fromShared = allNames.filter(n => !owned.has(n));
  // 先确定类名（stc modular 输出里类名 == tag），用于检测「共享类型与类名同名」冲突
  let className = tag;
  {
    const sf0 = ts.createSourceFile(`${tag}.ts`, content, ts.ScriptTarget.Latest, true);
    for (const cst of sf0.statements) {
      if (ts.isClassDeclaration(cst) && cst.name) { className = cst.name.text; break; }
    }
  }
  // C201: 共享类型与 tag 类名同名时，stc 生成的 import { <Name> } 会与 export class <Name> 冲突
  // （此前靠 @ts-nocheck 掩盖）。给同名 import 加别名 <Name>Model，并改写方法体里作为类型引用的裸 <Name>。
  // 类声明 export class <Name>、getAPI(<Name>)、typeof <Name> 保持不变（值语义，指类本身）。
  const collideSet = new Set(fromShared.filter(n => n === className));
  const blocks = [];
  if (fromTypes.length) blocks.push(fmtImport(fromTypes, './types'));
  if (fromShared.length) {
    const specs = fromShared.map(n => collideSet.has(n) ? `${n} as ${n}Model` : n);
    blocks.push(`import {\n  ${specs.join(',\n  ')},\n} from "../_shared/data-contracts";`);
  }
  const replacement = blocks.join('\n') + (blocks.length ? '\n' : '');
  const importRe = /import(?:\s+type)?\s*\{[\s\S]*?\}\s*from\s*["']\.\/data-contracts["'];?\s*\n?/g;
  let first = true;
  content = content.replace(importRe, () => { if (first) { first = false; return replacement; } return ''; });
  content = content.replace(/from\s*["']\.\/http-client["']/g, 'from "../_shared/http-client"');
  // 改写方法体里裸类型引用 <Name> -> <Name>Model（仅请求体形参类型位置，\b 边界避免误伤 <Name>AddPostData 等）
  for (const name of collideSet) {
    content = content.replace(new RegExp(`(\\bdata:\\s*)${name}\\b`, 'g'), `$1${name}Model`);
  }
  // B: 解析 class 方法，生成函数包装 —— 业务可直接 import 函数，省 getAPI(Class). 前缀
  const classSf = ts.createSourceFile(`${tag}.ts`, content, ts.ScriptTarget.Latest, true);
  const methods = [];
  for (const cst of classSf.statements) {
    if (ts.isClassDeclaration(cst) && cst.name) {
      for (const m of cst.members) {
        if (ts.isPropertyDeclaration(m) && m.name && ts.isIdentifier(m.name) && m.initializer && ts.isArrowFunction(m.initializer)) methods.push(m.name.text);
      }
    }
  }
  content = content.replace(/(from\s*["']\.\.\/_shared\/http-client["'];?[^\n]*\n)/, `$1import {getAPI} from "../../utils/axiosUtils";\n`);
  const wrappers = methods.map(m => `export const ${m} = (...args: Parameters<InstanceType<typeof ${className}>['${m}']>) => getAPI(${className}).${m}(...args);`).join('\n');
  content += `\n${wrappers}\n`;
  writeFileSync(resolve(dir, 'index.ts'), content);
}

const ownedCount = [...tagOwned.values()].reduce((a, b) => a + b.length, 0);
console.log(`[post-gen] ${tagOwned.size} 个 tag 目录 + _shared/；独占 ${ownedCount} 类型，共享 ${shared.length} 类型`);
