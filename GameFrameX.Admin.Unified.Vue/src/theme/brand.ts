/**
 * 品牌主色色板（C254 / GFX-660）。
 *
 * 品牌色取自项目 logo（src/assets/logo-mini.svg）采样最高频主色——深海蓝 #03538F，
 * 经 @primeuix/themes 的 palette() 生成 10 阶语义色阶（字面量固化，避免运行时依赖）。
 * 重新生成方式：用 @primeuix/themes 的 palette('#03538F')（dist/index.mjs 导出）
 */
export const brandPrimaryPalette: Record<string, string> = {
  '50': '#f2f6f9',
  '100': '#c3d6e4',
  '200': '#93b5cf',
  '300': '#6394ba',
  '400': '#3374a4',
  '500': '#03538f',
  '600': '#03477a',
  '700': '#023a64',
  '800': '#022e4f',
  '900': '#012139',
  '950': '#011524',
};
