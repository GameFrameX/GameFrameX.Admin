// Vite 构建期 define 注入的全局变量类型声明（见 vite.config.ts 的 define 块）。
// 这些常量在构建时被静态替换，运行时不存在模块导入，因此以全局 ambient 声明提供类型，
// 消除业务代码中的 @ts-ignore。
//
// 注：window.__env__ 由 src/types/global.d.ts 的 Window 接口声明，此处不重复。

declare const __NEXT_VERSION__: string;
declare const __NEXT_NAME__: string;
