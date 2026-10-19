/**
 * 数据可视化 / 装饰分类色板（C254 / GFX-660）。
 *
 * echarts 等 canvas 渲染场景无法读取 CSS 变量，页面层禁止散落 hex 色值——
 * 图表系列色、装饰渐变、图标分类色统一从本模块取值（品牌蓝打头）。
 * 样式层（SCSS/Vue style）继续用 `--p-*` 设计令牌，不用本模块。
 */
import {brandPrimaryPalette} from './brand';

// 品牌蓝（与主色一致，图表第一系列优先使用）
export const BRAND_BLUE = brandPrimaryPalette['500'];

// echarts 分类系列色（多系列折线/柱状/饼图，品牌蓝打头 + 可区分度优先）
export const CHART_SERIES_COLORS: string[] = [
  BRAND_BLUE,      // 品牌蓝
  '#968AF5',      // 淡紫
  '#FEC279',      // 橙黄
  '#36C78B',      // 绿
  '#E790E8',      // 粉紫
  '#F97316',      // 橙
];

// 图表轴 / 文本色（echarts canvas 无法用 CSS 变量，按亮暗主题二值切换）
export const CHART_AXIS_COLORS = {
  light: {
    label: '#303133',
    splitLine: '#f5f5f5',
  },
  dark: {
    label: '#dadada',
    splitLine: '#3f3f46',
  },
} as const;

// 首页/图表页装饰渐变端点色（系列色 + 透明度后缀拼 rgba 由使用处处理）
export const DECORATIVE_GRADIENTS = {
  // 折线图面积渐变：珊瑚 → 透明
  coral: '#fe9a8b',
  coralSoft: '#fe9a8bb3',
  coralFaint: '#fe9a8b03',
  // 折线图面积渐变：紫 → 透明
  violet: '#9E87FF',
  violetSoft: '#9E87FFb3',
  violetFaint: '#9E87FF03',
  // 柱状图渐变（橙/青蓝/紫）
  orange: '#FF8000',
  orangeSoft: 'rgba(250, 180, 101, 0.3)',
  orangeShadow: 'rgba(250, 180, 101, 0.2)',
  orangeTransparent: 'rgba(250, 180, 101, 0)',
  teal: '#3bbc86',
  tealSoft: 'rgba(199, 237, 250, 0.5)',
  tealFaint: 'rgba(199, 237, 250, 0.2)',
  purple: 'rgba(108, 80, 243, 0.3)',
  // personal 页海报卡装饰对色（彩色底 + 白字豁免，色对集中管理）
  posterGreenBg: '#48D18D',
  posterGreenIcon: '#64d89d',
  posterRedBg: '#F95959',
  posterRedIcon: '#F86C6B',
  posterIndigoBg: '#8595F4',
  posterIndigoIcon: '#92A1F4',
  posterAmberBg: '#FEBB50',
  posterAmberIcon: '#FDC566',
  purpleFaint: 'rgba(108, 80, 243, 0)',
} as const;

// 快捷导航 / 分类图标色（多分类徽标配色，用于图标前景）
export const CATEGORY_ICON_COLORS = {
  green: '#22C55E',
  amber: '#F59E0B',
  red: '#EF4444',
  blue: '#3B82F6',
  violet: '#8B5CF6',
  teal: '#14B8A6',
  orange: '#F97316',
  slate: '#64748B',
  pink: '#EC4899',
} as const;
