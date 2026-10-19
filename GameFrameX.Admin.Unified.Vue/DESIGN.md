---
name: GameFrameX Admin
description: 登录门面「纸面剧作卡」——制图纸工作台上一张待填写的纸质操作员卡，颜色全部由 PrimeVue --p-* token 派生
colors:
  blueprint-ink: "#03538f"
  blueprint-ink-faint: "#6394ba"
  blueprint-wash: "#93b5cf"
  blueprint-wash-light: "#c3d6e4"
  blueprint-paper-tint: "#f2f6f9"
  error-red: "var(--p-red-400, #f87171)"
  card-paper: "var(--p-surface-0)"
  desk-paper: "color-mix(in srgb, var(--p-surface-50) 82%, var(--p-primary-50))"
  desk-grid: "color-mix(in srgb, var(--p-primary-200) 38%, transparent)"
  desk-grid-major: "color-mix(in srgb, var(--p-primary-200) 62%, transparent)"
  text-strong: "var(--p-text-color)"
  text-muted: "var(--p-text-muted-color)"
  hairline-border: "var(--p-content-border-color)"
typography:
  headline:
    fontFamily: "-apple-system, BlinkMacSystemFont, 'Segoe UI', 'PingFang SC', 'Hiragino Sans GB', 'Microsoft YaHei', sans-serif"
    fontSize: "26px"
    fontWeight: 800
    lineHeight: 1.2
    letterSpacing: "-0.01em"
  body:
    fontSize: "13px"
    lineHeight: 1.5
  label:
    fontSize: "13px"
    fontWeight: 600
    lineHeight: 1.4
  mono-data:
    fontFamily: "ui-monospace, 'SF Mono', SFMono-Regular, Menlo, Consolas, 'Liberation Mono', monospace"
    fontSize: "11px"
    letterSpacing: "0.08em"
rounded:
  paper: "4px"
  control-inner: "6px"
  control: "8px"
  card: "14px"
spacing:
  field-gap: "6px"
  control-gap: "8px"
  form-rhythm: "18px"
  section-rhythm: "28px"
  desk-gutter: "64px"
components:
  operator-card:
    backgroundColor: "{colors.card-paper}"
    rounded: "{rounded.card}"
    padding: "34px 36px 26px"
  desk-sheet:
    backgroundColor: "{colors.card-paper}"
    rounded: "{rounded.paper}"
    padding: "10px"
  submit-button:
    rounded: "{rounded.control}"
  slide-track:
    backgroundColor: "{colors.card-paper}"
    rounded: "{rounded.control}"
    height: "46px"
  slide-thumb:
    backgroundColor: "{colors.card-paper}"
    textColor: "{colors.blueprint-ink}"
    rounded: "{rounded.control-inner}"
    size: "40px"
  verify-stamp:
    backgroundColor: "{colors.card-paper}"
    textColor: "{colors.blueprint-ink}"
    rounded: "{rounded.paper}"
    padding: "3px 12px"
  captcha-image:
    rounded: "{rounded.control-inner}"
    width: "132px"
    height: "44px"
---

# Design System: GameFrameX Admin

## Overview

**Creative North Star: "纸面剧作卡"**

登录是一张纸质「操作员卡」：访客在制图纸工作台上填字段、盖准入章，进门回到 token 化后台。桌面是 GameFrameX 的制图纸工作台（蓝格网 + 三张叠放的品牌蓝墨 schematic 图纸：终端、机架、多端拓扑），右侧一张 420px 竖卡从 mono 卡号行开始，依次是标题、字段、安全验证、油墨蓝提交。这个世界明确拒绝两类类别默认：「插画左 + 玻璃卡右」的分栏套路，以及它的黑底霓虹反面。

一切颜色由 PrimeVue `--p-*` 语义 token 派生（Aura 派生 `GameFrameXPreset`，主色为 logo 采样深海蓝 #03538F 的十阶色板），`.dark` class 切换亮暗双主题时纸面自动换墨。纸感深度的表达手段是「纸叠纸」：柔和单层阴影加小幅旋转角，不用发光、不用硬投影。动效语言极度克制——整个登录页只有一个强调时刻，即滑块通过后的「已验证」盖章压印。

本文件目前记录的是仓库内唯一自设计界面——登录门面（`src/views/login/`，设计契约 seed `0b4f57a4`）。后台其余界面运行在 PrimeVue 默认形态上；为它们做新界面时，token 纪律、对比度纪律、mono 数据纪律与动效纪律同样适用，世界装饰（格网桌面、图纸、注册标记、盖章）则属于登录门面的原生设备，按需沿用而非全局铺开。

**Key Characteristics:**
- 制图纸蓝格网桌面：24px 细格 + 120px 主格，线色为品牌蓝 200 阶的 38%/62% 透明混色
- 右侧 420px 白卡纸「操作员卡」：mono 卡号行 → 标题 → 字段 → 验证 → 油墨蓝提交
- 品牌蓝墨 schematic 插画三张叠放图纸（角标 TERM-01 / RACK-04 / NODE-07）
- 全部颜色经 `var(--p-*)` 或其 color-mix 派生式书写，亮暗双主题自动映射
- 全页只有一个动效时刻：盖章压印；聚焦反馈是四角注册标记而非发光
- 键盘与读屏可完整走完登录（滑块为 role="slider" 档位步进形态）

## Colors

调色板是单色相蓝图墨水系：品牌蓝十阶色板承担全部表现力，纸面与文字交给会随主题翻转的 surface/text token。

### Primary
- **Blueprint Ink**（#03538f，即 `--p-primary-color`，色板 500 阶）：世界的墨线主色。schematic 描边（2px）与实心填充、「已验证」章的边框与文字、提交按钮底色、输入光标（caret-color）。
- **Blueprint Ink Faint**（#6394ba，`--p-primary-300`）：次级墨。schematic 细线（1.5px）、字段聚焦的四角注册标记、滑块 thumb 边框、focus-visible 轮廓。
- **Blueprint Wash**（#93b5cf，`--p-primary-200`）：淡墨填充与网格线来源。桌面格网以 38%（细格）/62%（主格）透明混色取自它，插画次级填充块直接用它。
- **Blueprint Wash Light**（#c3d6e4，`--p-primary-100`）：插画主填充块、滑块通过后的轨道填充、::selection 选中底色。
- **Blueprint Paper Tint**（#f2f6f9，`--p-primary-50`）：滑块进行中填充；也是桌面纸混色（desk-paper）的着色成分。

品牌蓝阶由 `src/theme/brand.ts` 字面量固化并注入 preset 的 `semantic.primary`，亮暗两主题取值相同；会翻转的是 surface/text 阶，因此下面 Neutral 组保持 var() 引用形态，不拆成具体 hex。

### Neutral
- **Card Paper**（`var(--p-surface-0)`）：卡纸、图纸、滑块轨道与 thumb 的底——一切「纸」的表面。
- **Desk Paper**（`color-mix(in srgb, var(--p-surface-50) 82%, var(--p-primary-50))`）：制图纸桌面底色，比卡纸淡染一层蓝。
- **Text Strong**（`var(--p-text-color)`）：卡标题、品牌名。
- **Text Muted**（`var(--p-text-muted-color)`）：标签、提示、mono 版头数据、图纸角标。小字安静感永远用它，不做透明度降级。
- **Hairline Border**（`var(--p-content-border-color)`）：验证码图边框等结构细线（滑块轨道用 `--p-inputtext-border-color` 并以它兜底）。
- **Error Red**（`var(--p-red-400, #f87171)`）：错误文案与轨道错误描边；唯一允许 fallback 字面量的地方。

### Named Rules
**The Token-Only Rule.** 任何颜色必须写作 `var(--p-*)` 或 `color-mix(… var(--p-*) …)` 派生式；品牌 hex 只存在于 brand.ts 色板。样式里不裸写颜色字面量，既有例外仅错误红的 fallback（`var(--p-red-400, #f87171)`）。

**The Blueprint Ink Rule.** 插画与装饰只使用品牌蓝阶（50/100/200/300/500）加 muted 文字色，不引入第二装饰色相；错误红只作表单功能反馈。

## Typography

**Body Font:** 系统栈（-apple-system, BlinkMacSystemFont, 'Segoe UI', 'PingFang SC', 'Hiragino Sans GB', 'Microsoft YaHei', sans-serif；全局基线 14px / 1.5）
**Label/Mono Font:** ui-monospace, 'SF Mono', SFMono-Regular, Menlo, Consolas, 'Liberation Mono', monospace

**Character:** 标题即正文字体栈的 800 重量加 -0.01em 收紧，纸面文件的正式感来自 mono 版头数据与大写、0.08em 字距，而不是另引 display 字体。

### Hierarchy
- **Headline**（800，26px，line-height 1.2，letter-spacing -0.01em）：卡标题「操作员登录」与品牌名（品牌名 line-height 1.15）。全页最大字号，无更大 display 层。
- **Body**（400，12–13px 带，line-height 1.4–1.5）：滑块提示 13px、错误文案 12.5px、忘记凭据 12px。安静小字靠字号与字距压，不靠降对比。
- **Label**（600，13px，line-height 1.4）：表单字段标签，muted 色。
- **Mono Data**（11px，letter-spacing 0.08em，大写）：文件版头数据——卡号行（FORM NO. MMDD-XXXX）、桌面边条（副题大写 / GITHUB:GAMEFRAMEX / 日期）、图纸角标。图纸内部角标（TERM-01 等）为其 9px 微缩变体。

### Named Rules
**The Mono Margin Rule.** 等宽字体只用于「纸面文件版头数据」：卡号行、桌面边条、图纸角标。它是世界原生的表格数据设备（FIRST VIEWPORT 契约明列），不是 kicker/eyebrow 眉题；按钮与正文不用等宽，也不得再发明其他 mono 装饰用途。

**The Full-Token Contrast Rule.** 需要安静的小字直接用完整 `--p-text-muted-color`，不用透明度混色压对比（72% 透明混色在浅色桌面纸上仅约 2.9:1）；安静感交给字号与字距解决。

## Layout

两区制图桌：桌面区 `flex: 1 1 0`（含品牌头、三张叠放图纸、mono 边条，列间距 28px），右侧操作员卡固定 420px（`flex: 0 0 420px`，z-index 2 压在格网上）。容器 `min(1120px, 100%)` 居中，内距 48px 40px，两区间距 64px。

桌面格网是背景而非容器：`background-image` 四层 1px 线性渐变，`background-size` 24px（细格）与 120px（主格）两档。三张图纸绝对定位叠放，宽度 56%/44%/40%，各带小幅旋转（-2.4deg / 1.8deg / -1.2deg）与 z-index 1–3，全部 `aria-hidden`。桌面区最小高 560px、图纸区最小高 320px，保证小屏不塌。

响应式（断点 640px / 768px / 1024px，`@include bp-down(sm|md)` mixin）：≤767px 收起整个桌面侧，卡区 `margin-top: calc(50dvh - 260px)` 垂直居中，桌内距降为 24px 16px、卡左右内距 24px；≤639px 验证码行纵排（输入在上、图片在下）。页面滚动容器是 `100dvh` 的 login-container 自身。

## Elevation & Depth

深度来自「纸叠在桌上」：两层柔和单层阴影 token（--sheet-shadow / --card-shadow，均为 surface-900 的低比例透明混色），叠加小幅旋转角完成纸感；没有发光、没有硬 offset 投影、没有多层叠影。焦点与状态从不借助阴影表达。

### Shadow Vocabulary
- **Sheet Shadow**（`box-shadow: 0 8px 22px color-mix(in srgb, var(--p-surface-900) 10%, transparent)`）：桌面上的单张图纸。
- **Card Shadow**（`box-shadow: 0 22px 60px color-mix(in srgb, var(--p-surface-900) 13%, transparent)`）：操作员卡——离桌面更「高」的纸，阴影更大更淡。阴影与圆角只在纸容器上声明一次，内部元素不再叠影。

### Named Rules
**The Paper Stack Rule.** 阴影只表达物理纸叠：柔和、单层、token 化、随主题自动换墨；禁止硬 offset 阴影、多层叠影、发光与阴影做交互反馈。

## Shapes

纸张近直角，控件微圆，只有操作员卡最圆：图纸与盖章 4px、控件内件（滑块 thumb、验证码图）6px、标准交互控件（滑块轨道、提交按钮）8px、操作员卡 14px。盖章带 -4deg 旋转压在轨道上；字段聚焦的四角注册标记是 8×2px 线对贴在字段四角。schematic 插画是纯线稿几何：2px 主线 + 1.5px 细线，填充块用 100/200 阶淡墨，元素圆角克制（rx 3–8），直线为主。

## Components

### Buttons
- **Shape:** 标准控件圆角（8px）
- **Primary（提交）：** PrimeVue `<Button>` primary + `size="large"`，页面只覆写圆角（8px）、字重（600）、字距（0.08em）与全宽；hover/按压色由 preset（品牌蓝 600 阶）接管
- **Focus:** preset focus ring；页面内自定义控件的 focus-visible 统一为 2px primary-300 外轮廓 + 2px offset
- **Ghost（验证码刷新）：** 无边框透明按钮包住验证码图片，hover 只亮边框（border-color 0.15s ease 过渡到 `--p-inputtext-hover-border-color`）

### Inputs / Fields
- **Style:** PrimeVue InputText / Password，`size="large"`，invalid 经组件 `:invalid` prop 驱动；字段结构为 label（13px/600 muted）→ 输入 → 可选错误文案，label 与输入间距 6px，字段间距 18px
- **Focus:** 四角注册标记浮现（见 Named Rules），不用发光
- **Error:** 12.5px 错误红文案，`role="alert"`；滑块轨道错误态为红色描边
- **长占位文案：** 省略号截断必须落在 input 本体（`overflow: hidden; text-overflow: ellipsis`），Chromium 在 placeholder 伪元素上设置无效

### Cards / Containers
- **Corner Style:** 操作员卡 14px；图纸 4px
- **Background:** Card Paper（`var(--p-surface-0)`）
- **Shadow Strategy:** 见 Elevation & Depth；阴影与圆角只在容器声明一次
- **Internal Padding:** 卡 34px 36px 26px（≤767px 左右收 24px）；图纸 10px
- **卡头结构：** mono 卡号行（副题 + `FORM NO. MMDD-XXXX`）与表单同在一张卡纸上，是文件版头而非眉题

### 安全验证滑块（签名组件）
标准滑块形态替代拖拽弹窗：轨道 46px 高、8px 圆角、1px inputtext 边框；thumb 40px 方块（6px 圆角、primary-300 边框、内嵌方向箭头）。4 档棘轮步进（键盘 ←→/↑↓/Home/End 可达，`role="slider"` 且 aria 关联落在滑轨元素上），松手保留进度不清零；填充条不做宽度过渡（拖拽瞬时跟手），thumb 键盘步进落位 0.12s ease-out。到达末档即通过：填充升为 primary-100，thumb 反白为品牌蓝，轨道中央盖「已验证」章。

### 「已验证」章（签名组件）
1.5px 品牌蓝描边框（4px 圆角）、品牌蓝文字（13px/700/0.08em）、白纸底、-4deg 旋转、内距 3px 12px；入场为 0.22s `cubic-bezier(0.2, 0.9, 0.3, 1)` 的压印动画（opacity 0→1、scale 1.45→1）。它是全页唯一的强调动效时刻。

### 制图纸插画（签名组件）
三张 SVG（viewBox 300×220）线稿：终端、服务器机架、多端设备拓扑，各带 Actor 拓扑角点。样式只用五个类：主墨线（primary-color 2px stroke）、细线（primary-300 1.5px）、实心（primary-color fill）、淡填充（primary-100/200 fill）、mono 角标（9px，命名格式 `XX-NN`：TERM-01 / RACK-04 / NODE-07）。不引入第二色相，不使用位图或第三方插画素材。

### Named Rules
**The One Moment Rule.** 每个界面最多一个强调动效时刻（登录页是盖章 0.22s `cubic-bezier(0.2, 0.9, 0.3, 1)`）；拖拽等连续操作必须瞬时跟手、不做宽度/位置过渡；`prefers-reduced-motion: reduce` 下全部过渡与动画归零（全局 GFX-659/C253 已声明，页面不得绕过）。

**The Registration Mark Rule.** 字段聚焦反馈是四角注册标记：8×2px 品牌蓝 300 阶线对贴字段四角，0.12s ease-out 淡入；不是发光、描边环绕或阴影抬升。

## Do's and Don'ts

### Do:
- **Do** 将一切颜色写作 `var(--p-*)` 或 `color-mix(… var(--p-*) …)` 派生式，让 `.dark` 主题自动换墨
- **Do** 小字的安静感用完整 `--p-text-muted-color` + 小字号 + 0.08em 字距表达，不用透明混色压对比
- **Do** mono 字体只用于文件版头数据（FORM NO.、桌面边条、图纸角标 XX-NN）
- **Do** 聚焦反馈用四角注册标记（8×2px 线对、0.12s ease-out）
- **Do** 动效只保留状态确认时刻（盖章 0.22s `cubic-bezier(0.2, 0.9, 0.3, 1)`），并保持 reduced-motion 全灭
- **Do** 自定义交互控件的 aria 关联属性落在带 role 的元素上（如滑轨），不透传到包装层
- **Do** 插画用品牌蓝 schematic 线稿重绘产品叙事（终端/机架/多端），不用第三方通用素材

### Don't:
- **Don't** 使用「插画左 + 玻璃卡右」分栏套路、玻璃拟态或黑底霓虹——这是本世界明确拒绝的类别默认
- **Don't** 添加 kicker/eyebrow 眉题装饰；mono 卡号行是文件版头数据（世界原生设备），不是 kicker，也不得据此再发明眉题
- **Don't** 使用硬 offset 阴影、多层叠影或聚焦发光
- **Don't** 为标题另引 display 字体（web font 或系统 display 变体）；标题就是正文字体栈的 800 重量加 -0.01em
- **Don't** 引入第二装饰色相或裸写品牌 hex；错误红只作功能反馈且保留 fallback 形态
- **Don't** 新增图标字体或 glyph 图标做装饰；既有 PrimeIcons 仅限控件功能图标（滑块箭头、对勾）
- **Don't** 为连续操作（拖拽、滑块填充）加过渡动画；动效预算只给确认时刻
