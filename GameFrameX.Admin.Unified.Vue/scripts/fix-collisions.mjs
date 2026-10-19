/**
 * 修正脚本：处理迁移后的两个遗留问题。
 *
 * 1. class/interface 同名冲突：stc 的 class 名（GlobalMenu）与 data-contracts 的
 *    interface 同名，同文件 import 会 duplicate。对冲突文件把 class import 加 alias
 *    （GlobalMenu as GlobalMenuApi），getAPI(GlobalMenu) → getAPI(GlobalMenuApi)，
 *    interface 保持原名。
 * 2. upload 接口：旧 PostForm → 新 Post，参数包成 {file: ...}（Payload 结构）。
 */
import { readFileSync, writeFileSync } from 'node:fs';
import { execSync } from 'node:child_process';

const files = execSync(`grep -rl --exclude-dir=api-services "api-services" src --include="*.ts" --include="*.vue"`, { encoding: 'utf8' })
    .split('\n').filter(Boolean).map(f => f.replace(/\\/g, '/'));

let collFixed = 0, uploadFixed = 0;

for (const f of files) {
    const orig = readFileSync(f, 'utf8');
    let c = orig;

    // 1. class/interface 同名冲突
    const classNames = new Set(), ifaceNames = new Set();
    for (const m of c.matchAll(/import(?:\s+type)?\s*\{([^}]+)\}\s*from\s*['"]([^'"]*\/api-services)\/([A-Z][a-zA-Z]*)['"]/g)) {
        m[1].split(',').map(s => s.trim()).filter(Boolean).forEach(n => classNames.add(n));
    }
    for (const m of c.matchAll(/import(?:\s+type)?\s*\{([^}]+)\}\s*from\s*['"]([^'"]*\/api-services)\/data-contracts['"]/g)) {
        m[1].split(',').map(s => s.trim()).filter(Boolean).forEach(n => ifaceNames.add(n));
    }
    const conflicts = [...classNames].filter(n => ifaceNames.has(n));
    let touched = false;
    for (const N of conflicts) {
        const re = new RegExp(`import\\s*\\{\\s*${N}\\s*\\}\\s*from\\s*(['"][^'"]*\\/api-services\\/${N}\\1)`, 'g');
        const before = c;
        c = c.replace(re, `import {${N} as ${N}Api} from $1`);
        c = c.replace(new RegExp(`getAPI\\(${N}\\)`, 'g'), `getAPI(${N}Api)`);
        if (c !== before) touched = true;
    }
    if (touched) collFixed++;

    // 2. upload PostForm → Post + {file: ...}
    const beforeUpload = c;
    c = c.replace(/tenantFileUpload(\w+)PostForm\((.+)\)/g, (_m, name, args) => `tenantFileUpload${name}Post({file: ${args}})`);
    if (c !== beforeUpload) uploadFixed++;

    if (c !== orig) writeFileSync(f, c);
}
console.log(`class/interface 冲突修正: ${collFixed} 文件; upload 修正: ${uploadFixed} 文件`);
