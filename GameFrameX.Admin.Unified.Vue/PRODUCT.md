# Product

<!-- impeccable:product-schema 1 -->

## Platform

web

## Users

使用 GameFrameX 框架的独立游戏开发者与小团队的运营/开发人员（推断自仓库 README「独立游戏开发者的圆梦大使」与多中心后台结构）。登录用户是被管理员建号的内部用户（多租户、应用作用域），桌面为主、每天高频「开门」的操作者。

## Product Purpose

GameFrameX Admin：游戏前后端一站式框架的管理后台（玩家中心、商品、兑换、Web 商城、运营中心、风控、自动化、系统管理等）。登录页是整个后台的门面入口。成功 = 用户几秒内可信、无摩擦地进入后台开始工作。

## Positioning

独立游戏前后端一体化解决方案（Unity/Godot/Laya 多端 + .NET Actor 架构服务器 + 热更新），多租户/应用作用域管理模型——不是通用 admin 模板。

## Operating Context

多租户登录（账号@租户ID 格式）；登录摩擦由服务端配置驱动（图片验证码、二次校验开关）；`?token=` 免登路径；5 语言（zh-cn/zh-tw/en/ja/ko）；亮暗双主题（`.dark` class）；PrimeVue Aura 派生 preset + 品牌蓝 #03538F 十阶色板。

## Capabilities and Constraints

- 二次校验与验证码**严格跟随服务端配置**，前端不兜底默认开启（2026-08-30 用户确认）
- 二次验证换为**无障碍友好的标准形态**（键盘/读屏可完成）（2026-08-30 用户确认）
- 演示凭据预填（superadmin/123456）移除，demo 文案收敛到 DEV 构建
- 视觉基底锁定 PrimeVue token 体系（`--p-*` 语义变量），品牌蓝十阶色板
- 全部文案走 vue-i18n（含副标题，修复 store 硬编码中文）
- fork 自 vue-next-admin，存量体系按证据对待

## Brand Commitments

GameFrameX 品牌：logo-mini.svg、品牌蓝采样色板（C254）。登录页按「新世界」处理——品牌 token 为基底、个性放足（2026-08-30 用户确认 Q1=B）。插画方向：品牌蓝重绘游戏开发主题（Q3=A），替换现有 unDraw 公共素材。

## Evidence on Hand

logo-mini.svg；brandPrimaryPalette（src/theme/brand.ts）；产品名与文案（themeConfig store）。无商业宣传素材；登录页不得虚构任何能力宣称。

## Product Principles

1. 每天开门的门面：可信与效率优先（Operate 调性），表达不挡任务
2. 摩擦按需递增：服务端说了算，前端不擅自加锁
3. 无障碍是底线：键盘与读屏用户必须能完整走完登录
4. 产品自己的叙事（游戏开发/多端/Actor 架构）优先于通用素材
5. token 纪律：一切颜色过 PrimeVue 语义变量

## Accessibility & Inclusion

键盘与读屏用户必须能完成登录（评审 P0；2026-08-30 用户选定无障碍友好验证形态）；暗色模式下门面文字对比度需达标（≥4.5:1 正文 / ≥3:1 大字）。
