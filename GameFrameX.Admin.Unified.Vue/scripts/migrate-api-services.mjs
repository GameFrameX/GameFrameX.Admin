/**
 * api-services 迁移脚本：typescript-axios 风格 → swagger-typescript-api 风格。
 *
 * 规则：
 * 1. import {A, B} from '.../api-services/api' → 每个 Api class 拆到各自文件
 *    import {A} from '.../api-services/A'（去 Api 后缀 = 文件名/class 名）
 * 2. import {...} from '.../api-services/models' → '.../api-services/data-contracts'
 * 3. class 名 XxxApi → Xxx（使用处，如 getAPI 参数、类型标注）
 * 4. 方法调用 .apiXxxYyy( → .xxxYyy(（去 api 前缀，首字母小写 = operationId）
 *
 * 用法：
 *   node scripts/migrate-api-services.mjs          # 实际改写
 *   node scripts/migrate-api-services.mjs --dry-run # 只打印，不改（前 3 个文件打印 diff）
 *
 * 跑完用 pnpm typecheck 暴露剩余错误（主要是 GET 接口 query 参数 positional → 对象），逐个手改。
 */
import { readFileSync, writeFileSync } from 'node:fs';
import { execSync } from 'node:child_process';

const dryRun = process.argv.includes('--dry-run');
const diffLimit = 3; // dry-run 时详细打印 diff 的文件数

const files = execSync(`grep -rl "api-services" src --include="*.ts" --include="*.vue"`, { encoding: 'utf8' })
    .split('\n').filter(Boolean)
    .map(f => f.replace(/\\/g, '/'))
    .filter(f => !f.includes('src/api-services/'));

let changed = 0;
let diffShown = 0;

for (const file of files) {
    const original = readFileSync(file, 'utf8');
    let content = original;

    // 规则 1：import { ... } from '.../api-services/api' → 每个 class 拆到各自文件
    content = content.replace(
        /import\s*\{([^}]+)\}\s*from\s*(['"])([^'"]*\/api-services)\/api\2;?/g,
        (_m, specifiers, _q, base) => {
            const names = specifiers.split(',').map(s => s.trim()).filter(Boolean);
            const lines = names.map(name => {
                if (/Api$/.test(name)) {
                    const newName = name.replace(/Api$/, '');
                    return `import {${newName}} from '${base}/${newName}';`;
                }
                return `import {${name}} from '${base}/data-contracts';`;
            });
            return lines.join('\n');
        }
    );

    // 规则 2：from '.../api-services/models' → data-contracts
    content = content.replace(
        /from\s*(['"])([^'"]*\/api-services)\/models\1/g,
        "from $1$2/data-contracts$1"
    );

    // 规则 3：class 名 XxxApi → Xxx（PascalCase + Api 后缀的标识符）
    content = content.replace(/\b([A-Z][a-zA-Z0-9]*)Api\b/g, '$1');

    // 规则 4：方法调用 .apiXxxYyy( → .xxxYyy(
    content = content.replace(
        /\.api([A-Z][a-zA-Z0-9]*)\(/g,
        (_m, p1) => '.' + p1[0].toLowerCase() + p1.slice(1) + '('
    );

    if (content !== original) {
        changed++;
        if (dryRun) {
            if (diffShown < diffLimit) {
                console.log(`\n=== ${file} ===`);
                const origLines = original.split('\n');
                const newLines = content.split('\n');
                const max = Math.max(origLines.length, newLines.length);
                for (let i = 0; i < max; i++) {
                    if (origLines[i] !== newLines[i]) {
                        console.log(`- ${origLines[i] ?? ''}`);
                        console.log(`+ ${newLines[i] ?? ''}`);
                    }
                }
                diffShown++;
            } else {
                console.log(`changed: ${file}`);
            }
        } else {
            writeFileSync(file, content);
            console.log('migrated:', file);
        }
    }
}

console.log(`\n${dryRun ? '[DRY-RUN] ' : ''}迁移 ${changed} 个文件。`);
