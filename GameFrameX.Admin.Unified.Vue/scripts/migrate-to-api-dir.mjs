/**
 * 一次性迁移脚本（阶段 2）：业务代码的 api-services import → src/api/ 新结构。
 *
 * 规则：
 *   api-services/data-contracts → api/_shared/data-contracts
 *   api-services/http-client    → api/_shared/http-client
 *   api-services/<Tag>（PascalCase）→ api/<tag-camel>      （class 放 index.ts）
 *
 * 跑完用 grep 确认无残留（排除 api-services 自身），typecheck 过，再删 src/api-services/。
 */
import { readFileSync, writeFileSync } from 'node:fs';
import { execSync } from 'node:child_process';

const files = execSync(`grep -rl "api-services" src --include="*.ts" --include="*.vue" --exclude-dir=api-services`, { encoding: 'utf8' })
  .split('\n').filter(Boolean).map(f => f.replace(/\\/g, '/'));

let changed = 0;
for (const f of files) {
  const orig = readFileSync(f, 'utf8');
  let c = orig;
  c = c.replace(/api-services\/data-contracts/g, 'api/_shared/data-contracts');
  c = c.replace(/api-services\/http-client/g, 'api/_shared/http-client');
  c = c.replace(/api-services\/([A-Z][A-Za-z0-9]*)/g, (_, tag) => `api/${tag[0].toLowerCase() + tag.slice(1)}`);
  if (c !== orig) { writeFileSync(f, c); changed++; }
}
console.log(`[migrate] 迁移 ${changed} 文件：api-services → api`);
