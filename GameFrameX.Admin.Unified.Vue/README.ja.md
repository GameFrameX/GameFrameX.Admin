<div align="center">

<img src="https://download.alianblank.com/gameframex/gameframex_logo_320.png" alt="GameFrameX Logo" width="160" />

# GameFrameX Admin Web

[![License](https://img.shields.io/badge/license-blue.svg)](./LICENSE)
[![Version](https://img.shields.io/github/v/release/GameFrameX/Admin)](https://github.com/GameFrameX/Admin/releases)
[![Documentation](https://img.shields.io/badge/docs-gameframex.doc.alianblank.com-blue)](https://gameframex.doc.alianblank.com)

[![Discord](https://img.shields.io/badge/-5865F2?logo=discord&logoColor=white)](https://discord.gg/VDWUjWMDw9)
[![GitHub](https://img.shields.io/badge/-181717?logo=github&logoColor=white)](https://github.com/GameFrameX/gameframex)
[![Bilibili](https://img.shields.io/badge/-00A1D6?logo=bilibili&logoColor=white)](https://www.bilibili.com/video/BV1yrpeepEn7)
[![Gitee](https://img.shields.io/badge/-C71D23?logo=gitee&logoColor=white)](https://gitee.com/GameFrameX/gameframex)

インディゲーム開発者向けオールインワンソリューション · インディ開発者の夢を支援

<br />

[ドキュメント](https://gameframex.doc.alianblank.com) · [クイックスタート](#クイックスタート) · QQグループ: 467608841 / 233840761
<br />

[English](README.md) | [简体中文](README.zh-CN.md) | [繁體中文](README.zh-TW.md) | **日本語** | [한국어](README.ko.md)

</div>

---

GameFrameX ゲーム管理プラットフォームのフロントエンドプロジェクト。Vue 3 + TypeScript + Vite + PrimeVue で構築されています。

> 本プロジェクトは [vue-next-admin](https://gitee.com/lyt-top/vue-next-admin) を fork し、GameFrameX のマルチテナント / アプリケーションスコープモデルに合わせて改造したものです。現在のバージョンは ![version](https://img.shields.io/github/package-json/v/GameFrameX/Admin?filename=GameFrameX.Admin.Unified.Vue/package.json)（package.json 由来、自動同期）です。

## 目次

- [プロジェクト概要](#プロジェクト概要)
- [技術スタック](#技術スタック)
- [リポジトリディレクトリ構成](#リポジトリディレクトリ構成)
- [クイックスタート](#クイックスタート)
- [仕組み](#仕組み)
- [API とバックエンドの取り決め](#api-とバックエンドの取り決め)
- [新しいページを追加する](#新しいページを追加する)
- [開発規約](#開発規約)
- [FAQ とハマりポイント](#faq-とハマりポイント)
- [デプロイ](#デプロイ)
- [コマンド一覧](#コマンド一覧)
- [バックエンドから OpenAPI をエクスポート](#バックエンドから-openapi-をエクスポート)
- [おわりに](#おわりに)
- [ドキュメントとリソース](#ドキュメントとリソース)
- [変更履歴](#変更履歴)

---

## プロジェクト概要

**GameFrameX ゲーム管理プラットフォーム**向けのフロントエンドです。運営・GM・カスタマーサポート・インフラ担当が毎日ブラウザでメニューを変更し、ユーザーを追加し、注文を確認し、バージョンをリリースし、ログを閲覧する——**それがこのプロジェクトの仕事です**。

バックエンドは .NET で書かれており、リポジトリは [`GameFrameX.Admin.Unified.Api`](../GameFrameX.Admin.Unified.Api) にあります。データベースはデフォルトで PostgreSQL。フロントエンドは Vue 3 + Vite + PrimeVue + Pinia + Vue Router + vue-i18n + Axios といった定番の技術スタックで、奇抜なトリックはありません。

**知っておくべき最も重要な 3 つのこと**：

1. **メニュー・権限・文言はすべてバックエンドが制御**。フロントエンドにメニューは書かず、メニューの追加はバックエンドのメニュー管理で行います。フロントエンドはバックエンドが返すメニューツリーを描画するだけです。
2. **バックエンド動的ルーティング**。ログイン後、フロントエンドは `/api/globalMenu/loginMenuTree` を呼んでメニューを取得し、`component` フィールドを使って `src/views/` から対応する `.vue` ファイルを探し、最後に `router.addRoute()` で登録します。なのでページファイルを追加するだけでよく、ルーティングファイルを触る必要はありません。
3. **JWT 自動更新**。ログインで取得した token は `localStorage` に保存され、期限切れ前にフロントエンドがリクエストヘッダーに `X-Authorization` を付けてバックエンドに黙って再発行させるので、一度ログインすれば長く使えます。

---

## 技術スタック

| 項目 | 技術選定 | バージョン |
|---|---|---|
| フレームワーク | Vue 3（Composition API + `<script setup lang="ts">`） | 3.5.x |
| ビルド | Vite | 6.x |
| UI コンポーネントライブラリ | PrimeVue（Aura テーマ）+ PrimeFlex + PrimeIcons | 4.5.x |
| 状態管理 | Pinia + pinia-plugin-persistedstate | 3.x |
| ルーティング | Vue Router（hash モード、バックエンド動的制御） | 4.x |
| 言語 | TypeScript（strict） | 5.3.x |
| 国際化 | vue-i18n（5 言語） | 11.x |
| チャート | ECharts + echarts-gl + echarts-wordcloud | 6.x |
| リアルタイム通信 | @microsoft/signalr | 10.x |
| リッチテキスト | wangEditor + editor-for-vue | 5.x |
| コードエディター | Monaco Editor | 0.44.x |
| HTTP | Axios + 自作インターセプター | 1.x |
| ユニットテスト | Vitest（jsdom + @vue/test-utils） | 3.x |
| E2E テスト | Playwright（システムの Edge） | 1.64.x |
| アイコン | FontAwesome 7（vue-fontawesome + 自動オンデマンド） | 7.x |
| フォーム | @primevue/forms + 自作 formResolver | 4.5.x |
| エディター補助 | code-inspector-plugin（Shift 押しながらクリックでソースを特定） | — |
| パッケージ管理 | pnpm（Corepack で自動有効化） | ≥ 11.5.2 |
| Node | — | ≥ 16（推奨 20） |

> Vue Router は **hash モード**（`createWebHashHistory`）なので、デプロイ時にサーバー側の rewrite は不要ですが、URL に `#` が付きます。

---

## リポジトリディレクトリ構成

### 最上位

```
GameFrameX.Admin.Unified.Vue/
├── src/                        # 業務ソースコード（時間の 99% はここ）
├── public/                     # 静的リソース
│   └── config/config.js        # Vite 起動時に生成されるランタイム設定
├── scripts/                    # エンジニアリングスクリプト（gen-api、check-frontend など）
├── openapi.json                # バックエンド Swagger エクスポート、pnpm gen:api の入力
├── build/                      # Docker / Nginx / デプロイスクリプト
├── Dockerfile                  # 多段階ビルドイメージ
├── .cnb.yml                    # CNB クラウドネイティブビルド / デプロイパイプライン
├── .env / .env.development / .env.production   # 環境変数
├── eslint.config.mjs           # ESLint 9 flat config
├── .prettierrc.json
├── tsconfig.json / tsconfig.typecheck.json
├── vite.config.ts              # Vite 設定
├── index.html                  # SPA エントリー（public/config/config.js を注入）
├── package.json                # scripts / dependencies
├── pnpm-lock.yaml
├── pnpm-workspace.yaml
├── CHANGELOG.md
├── LICENSE     # ライセンスファイル
└── README.md                   # 本ファイル
```

### src/ 詳解

```
src/
├── api/                          # バックエンド呼び出し（※）
│   ├── _shared/                  # 自動生成される共有層（HttpClient + data-contracts）
│   ├── <module-camel>/           # バックエンド tag ごとに 1 ディレクトリ、index.ts（Api class）+ types.ts を含む
│   │   ├── index.ts              # 自動生成 + 末尾でトップレベル関数をエクスポート
│   │   └── types.ts              # このモジュールだけで使う data-contracts
│   └── <業務手書き>.ts             # ごく少数の手書き拡張、生成物と衝突しない命名
│
│   現在あるモジュール（抜粋）：
│   systemAuth / systemUser / systemRole / systemMenu / systemConfig /
│   globalMenu / globalDictType / globalLanguage / globalPlatform /
│   gameAppReleaseVersion / gameAppAssetPackage / gameHomeEntity /
│   gameOrderEntity / gameMailEntity / gameRedeemCodeEntity /
│   platformCaptchaConfig / platformJwtConfig / platformPasswordPolicyConfig …
│
├── api-services/                 # 過去の遺物ディレクトリ、空になったので無視
│
├── composables/                    # composables（※）
│   ├── useCrudTable.ts             テーブルページング / ソート / loading ラップ
│   ├── useEditDialog.ts            追加 / 編集ダイアログ（監査フィールドの除去、isEnabled デフォルト）
│   ├── useTableColumnPersistence.ts  列幅 / 列順 / 列表示のローカル永続化
│   ├── useCurrentApp.ts            現在のアプリケーション（AppId）状態 + 読み込み / 切り替え
│   ├── useConfigAppScope.ts        アプリケーションスコープ変更
│   ├── useRequiredFields.ts        必須フィールドの集合
│   ├── usePrimeTablePage.ts        テーブルページング hook
│   ├── useRouteAppIdQuerySync.ts   AppId query とテーブルの同期
│   ├── formResolver.ts             フォームバリデーションルールの適合
│   └── types/                      composables の TS 型
│
├── components/                   # 汎用コンポーネント
│   ├── crud-data-table/            PrimeVue DataTable の CRUD ラップ
│   ├── crud-form-fields/           フォームフィールドの統一描画
│   ├── base-edit-dialog/           追加 / 編集ダイアログの基礎
│   ├── search-form/                検索バー、authPrefix で自動的に権限ボタンを追加
│   ├── time-column/                テーブル列：long 秒を自動フォーマット
│   ├── form-label / form-help-text フォームフィールドのラベル / ヘルプテキスト
│   ├── app-confirm-dialog          グローバル確認ダイアログ
│   ├── config-app-scope-picker     アプリケーションスコープセレクター
│   ├── narrow-screen-hint          狭画面ヒント
│   ├── notice-bar                  通知バー
│   ├── cropper / drag-verify / editor / icon-selector / reward-items-editor / svg-icon / table
│
├── directive/                    # カスタムディレクティブ
│   ├── authDirective.ts            v-auth — 権限による非表示
│   ├── customDirective.ts          v-waves / v-drag / v-reclick（連打防止）
│   ├── blockLoading.ts             v-block-loading — ブロック全体の loading オーバーレイ
│   └── index.ts                    登録エントリー
│
├── i18n/                         # 国際化
│   ├── lang/                       フレームワークレベル（zh-cn / en / zh-tw / ja / ko）
│   └── pages/                      業務レベル、モジュールごとのサブディレクトリ
│       ├── system/ / client/ / game/ / home/ / login/ / menu/
│       ├── personal/ / layout/ / errors/ / formI18n/ / chart/
│       └── platform* / global*Config/
│
├── layout/                       # レイアウト
│   ├── splitter/                   メインレイアウトコンテナー
│   ├── navMenu/                    左サイド / 横メニュー
│   ├── navBars/                    トップバー（ユーザー / 通知 / 全画面 / テーマ）
│   ├── routerView/                 keep-alive コンテナー
│   ├── footer / logo / upgrade     その他のレイアウト要素
│   └── index.vue                   レイアウトエントリー
│
├── router/                       # ルーティング（※）
│   ├── index.ts                    ルーターインスタンス + ガード
│   ├── frontEnd.ts                 フロントエンド制御ルート（フォールバック、現在未使用）
│   ├── backEnd.ts                  バックエンド動的ルーティングの核
│   ├── appWorkspaceRoutes.ts       アプリケーションワークスペースルート
│   ├── globalConfigRoutes.ts       グローバル設定ルート
│   └── route.ts                    静的 / フォールバックルート（/login / 404 / 401）
│
├── stores/                       # Pinia 状態
│   ├── themeConfig.ts              テーマ / レイアウト / ダーク / 透かし / globalI18n / isRequestRoutes
│   ├── userInfo.ts                 ユーザー情報 + authBtnList + constList + dictList
│   ├── routesList.ts               メニューツリー
│   ├── tagsViewRoutes.ts           Tabs バー
│   ├── keepAliveNames.ts           keep-alive でキャッシュされる name リスト
│   └── index.ts                    Pinia インスタンス + 永続化アダプター
│
├── theme/                        # SCSS テーマ
│   ├── z-index.scss                統一 z-index スケール
│   ├── breakpoints.scss            ブレークポイント（--bp-lg = 1024）
│   ├── primevue-overrides.scss     PrimeVue token の上書き
│   └── common/transition.scss      汎用トランジション
│
├── types/                        # TS 型宣言
│
├── utils/                        # ユーティリティ関数（純関数を維持）
│   ├── request.ts                  手書き API 用 axios インスタンス
│   ├── axios-utils.ts              生成 API 用 HttpClient + getAPI シングルトンキャッシュ
│   ├── http/shared.ts              ※ 共有インターセプター（JWT / refresh / エラーコード i18n）
│   ├── storage.ts                  Local / Session（キーに __NEXT_NAME__: プレフィックス）
│   ├── authFunction.ts             auth() / hasAnyAuth() 関数型権限
│   ├── message.ts                  showSuccess / showError / showConfirm（PrimeVue Toast）
│   ├── watermark.ts                透かし
│   ├── isEnabled.ts                IsEnabled の正規化と追加時デフォルト値
│   ├── rowVersion.ts               行バージョン読み取り（rowVersion / RowVersion 互換）
│   ├── targetTenant.ts             テナント横断 header（X-Target-Tenant-Id）
│   ├── primevue-locale.ts          PrimeVue コンポーネントの言語切替
│   ├── formatTime.ts               日付 / 時刻フォーマット
│   ├── download.ts / exportExcel.ts  エクスポート
│   ├── sanitize.ts                 DOMPurify ラップ
│   ├── fontawesome.ts              FA 自動オンデマンド
│   ├── icons.ts                    FA 名前マッピング
│   ├── build.ts                    CDN 設定
│   ├── appWorkspace.ts             ※ アプリケーションスコープユーティリティ（resolveRouteScope など）
│   ├── arrayOperation.ts / commonFunction.ts / base64Conver.ts / crudConfig.ts / getStyleSheets.ts / loading.ts / mitt.ts / other.ts / template-utils.ts
│   └── http/                       インターセプター分割
│
├── views/                        # 業務ページ
│   ├── system/                     システム管理（ユーザー / ロール / メニュー / 組織 / テナント / 辞書 / ログ / 設定 / 通知 / プラグイン / 印刷 / 地域 / オンラインユーザー ……）
│   ├── game/                       ゲーム管理（区服 / 商品 / 注文 / メール / 引換コード / ショップ / お知らせ / アイテム ……）
│   ├── client/                     クライアント管理（プラットフォーム / バージョン / チャネル / デバイス / 言語 / リソースパッケージ / オープン API ……）
│   ├── appCenter/                  アプリケーションセンター（appEntry / appSelector / appWorkspace / gameAppEntity など）
│   ├── contentCenter / chart / chartCenter / chartCenter / opsCenter / trustCenter / automationCenter / goodsCenter / redeemCenter / webShopCenter / playerCenter
│   ├── home/                       ホームダッシュボード
│   ├── login/                      ログイン
│   ├── personal/                   個人センター
│   ├── error/                      404 / 401
│   └── global/                     グローバルルートコンテナー
│
├── App.vue                       # ルートコンポーネント
├── main.ts                       # ※ アプリケーションエントリー
├── auto-imports.d.ts             # unplugin-auto-import 生成
└── components.d.ts               # unplugin-vue-components 生成
```

### scripts/ 詳解

```
scripts/
├── gen-api.mjs                   # pnpm gen:api — swagger-typescript-api でプログラム的に生成
├── post-gen.mjs                  # 生成物の分割 + src/api/<module>/ へ再構成
├── migrate-api-services.mjs      # 過去の移行スクリプト
├── migrate-to-api-dir.mjs        # api-services/ を api/ へ統合
├── check-i18n-keys.mjs           # pnpm run lint:i18n — 5 言語キー完全性
├── check-frontend-standards.mjs  # pnpm run check:frontend — クロスプラットフォーム規約ゲート
│   ├─ modes: all / routes / permissions / api / api-query-get
│   │         / text / sys-abbreviation / text-baseline / self-test
│   ├─ routes         バックエンドメニュー component ↔ src/views/<file>
│   ├─ permissions    フロント v-auth 宣言 ↔ バックエンドシード Btn
│   ├─ api            手書き vs 生成 API の重複 review
│   ├─ api-query-get  page/list/query 系インターフェースで GET の params 送信を禁止
│   ├─ text           changed files のハードコード中国語スキャン
│   └─ sys-abbreviation C179 回帰：業務での Sys/sys 略記を禁止
├── smoke-test.mjs                # スモークテスト
├── fix-collisions.mjs            # 衝突修正
├── frontend-standards-allowlist.json   # 規約免除リスト
└── i18n-text-baseline.json       # ハードコード中国語免除ベースライン（C151 過去の負債）
```

---

## クイックスタート

### 環境

- Node.js ≥ 16（推奨 20）
- pnpm ≥ 11.5.2（`corepack enable` で自動有効化）

### 起動

```bash
git clone https://github.com/GameFrameX/Admin.git
cd Admin/GameFrameX.Admin.Unified.Vue
corepack enable
pnpm install
pnpm run dev    # デフォルト http://localhost:20800
```

ブラウザを開く → ログインページが表示される → バックエンド担当者にアカウントをもらう。

### つまずいたら？

| 症状 | 最も可能性が高い原因 |
|---|---|
| `pnpm: command not found` | 未インストール：`npm i -g pnpm@11.5.2` |
| ポート 20800 が使用中 | `.env` の `VITE_PORT=9000` を変更し、dev を再起動 |
| ブラウザが白画面 | ターミナルの最後の数行を確認、Vite がコンパイルエラーを吐いています |
| ログイン失敗 | バックエンドがない / バックエンドが起動していない / `.env.development` の `VITE_API_URL` が違う |

### 環境変数

| 変数 | 説明 | 開発時デフォルト値 |
|---|---|---|
| `VITE_API_URL` | バックエンド API ベース URL | `http://127.0.0.1:20802` |
| `VITE_PORT` | dev ポート | `20800` |
| `VITE_OPEN` | ブラウザを自動で開くか | `false` |
| `VITE_OPEN_CDN` | CDN 外部化を有効にするか | `false` |
| `VITE_PUBLIC_PATH` | 本番の公開パス | 空 |

Vite 起動時にこれらの値が `public/config/config.js` の `window.__env__` に書き込まれます。Docker デプロイ時には `entrypoint.sh` がコンテナーの環境変数からこのファイルを再生成するので、**同じイメージを環境変数で別環境に切り替えられます**。

---

## 仕組み

### 起動フロー（`src/main.ts`）

`index.html` が `public/config/config.js` を読み込み（`window.__env__` に書き込み）→ `src/main.ts` を読み込み → 順番にマウント：

1. Pinia（永続化プラグイン込み）
2. Router（hash モード）
3. i18n（`src/i18n/**` から自動 glob）
5. PrimeVue + Aura テーマ + ToastService + ConfirmationService
6. `@vue-js-cron/light`、`vue-grid-layout`、`vue-signature-pad`、`vue3-tree-org`
7. カスタムディレクティブ（v-auth / v-waves / v-reclick / v-block-loading）

`App.vue` はさらに 4 つのこと：ルート変更で title を更新、ダークテーマの初期化、透かしの注入、PrimeVue Locale のバインド、SignalR メッセージサービスの初期化。

### ログインとメニュー登録

ブラウザでログインページを開く → アカウントを入力 → フロントエンドが `/api/systemAuth/login` を送信 → バックエンドが検証 + JWT 発行 → フロントエンドが token を `localStorage` に保存 → `/dashboard/home` へ遷移。

ルートガード `router.beforeEach`（`src/router/index.ts`）が「まだメニューがない」と判定すると、2 つの API を並行で取得します：

- `/api/systemAuth/userInfo` → ユーザー情報 + `buttons` 権限配列
- `/api/globalMenu/loginMenuTree` → メニューツリー

メニューは概ねこんな形です（DevTools Network で確認できます）：

```json
[
  { "path": "/dashboard",    "name": "dashboard",  "component": "/home/index" },
  { "path": "/system/user",  "name": "systemUser", "component": "/system/user/index" }
]
```

`backEnd.ts` は `import.meta.glob('../views/**/*.{vue,tsx}')` で全ビューファイルを 1 つのテーブルにプリロードし、`dynamicImport()` で `component` 文字列をコンポーネント関数にマッピングし、最後に `router.addRoute()` で登録します。

**なのでメニューを変更したらバックエンドの再起動 + フロントエンドの再ログインが必要です**。フロントエンド自体にメニューリストはありません。

ボタン権限も同様：`<Button v-auth="'systemUser:add'">` のカスタムディレクティブが `buttons` 配列を検索し、なければ `display: none` で隠します。

### HTTP インターセプター（`src/utils/http/shared.ts`）

2 つの axios インスタンスが同じインターセプターファクトリーを共有：

- `src/utils/request.ts` の `service` —— 手書き API 用、GET params のシリアライズに対応
- `src/utils/axios-utils.ts` の `sharedHttpClient` —— swagger 生成 API 用、`Accept-Language` を自動付与

**リクエストインターセプター**（4 ステップ）：

1. `Authorization: Bearer <access-token>` を自動付与；JWT が期限切れ間近なら `X-Authorization: Bearer <x-refresh-token>` も同時に付与し、バックエンドに黙って再発行させる
2. `X-Target-Tenant-Id` を自動付与（テナント横断切替時は `utils/targetTenant.ts` が制御）
3. `request.ts` は追加で GET リクエストの `data` を query string にシリアライズ（`tansParams`）
4. `axios-utils.ts` は追加で `Accept-Language: <globalI18n>` を付与

**レスポンスインターセプター**（4 ステップ）：

1. HTTP 401 → token を消去して reload（`clearAccessTokens`）
2. レスポンスヘッダーの `access-token` / `x-access-token` → 自動で LocalStorage に書き込み（token の黙示的再発行）
3. 業務コード `code === 200` はそのまま通過；その他のエラーコード：
   - `errors.codes.<ERROR_CODE>` が i18n にヒット → ローカライズされた文言を表示
   - `VALIDATION.FAILED` にヒット → バックエンドの元の message を表示（フィールド単位のエラーを保持）
   - その他 → バックエンドの `message` を表示
4. キャメルケース化（先頭大文字 → 小文字）+ エラー通知は PrimeVue Toast 経由

### アプリケーションスコープ（マルチテナント）

バックエンドメニューの各項目は `app_scope`（`platform` / `tenant` / `app`）と `menu_context` を持ちます。フロントエンドは `meta.appScope` + `meta.menuContext` でマークします：

| Scope | 意味 | ルート例 |
|---|---|---|
| `platform` | プラットフォームレベル（超管理者のみ、テナント横断） | `/platform/*` |
| `tenant` | テナントレベル（全アプリ共通、appId query なし） | `/dashboard/*`、`/system/*` |
| `app` | アプリケーションワークスペース内（`appId` query 必須） | `/gameCenter/*` |

遷移ロジックは `src/utils/appWorkspace.ts` の `decideTenantAppEntry()` / `resolveRouteScope()` にあり、`router/index.ts` の `resolveTenantAppRedirect()` がガード内で発火します。

### Pinia 永続化

| Store | ストレージ | 主要フィールド |
|---|---|---|
| `themeConfig` | localStorage | テーマ / レイアウト / ダーク / 透かし / globalI18n / `isRequestRoutes` |
| `userInfo` | sessionStorage（`pick: ['constList', 'dictList']` のみ） | システム定数 + 辞書（起動が遅いのでキャッシュ） |
| `routesList` | 永続化しない | 現在のメニューツリー |
| `tagsViewRoutes` | 永続化しない | Tabs バー |
| `keepAliveNames` | 永続化しない | keep-alive include |

すべてのキーに `__NEXT_NAME__:` プレフィックスが自動で付きます（`Local.setKey` 経由）、アプリ横断の衝突を回避します。

---

## API とバックエンドの取り決め

### 2 層の API

| 層 | 場所 | 由来 | 呼び出し |
|---|---|---|---|
| **生成 API** | `src/api/<module>/{index.ts,types.ts}` | swagger-typescript-api でプログラム生成 | `getAPI(SomeApi).someMethod()` |
| **手書き API** | `src/api/<業務>.ts` | 業務に必要な拡張 / ラップ | `request({ url, method, data })` |

生成 API の呼び出しパターン：

```ts
import { getAPI } from '/@/utils/axios-utils'
import { SystemUser } from '/@/api/systemUser'

const api = getAPI(SystemUser)                       // シングルトンキャッシュ、複数回呼んでもオーバーヘッドなし
const res = await api.systemUserPagePost({ page: 1, pageSize: 20 })
const user = res.data.result.items[0]                 // 自動正規化された { code, result, ... }
```

手書き API の呼び出しパターン：

```ts
import request from '/@/utils/request'

export const pageX = (params: PageParams) =>
  request<Result<PageResult<X>>>({
    url: '/api/game/x/page',
    method: 'post',
    data: params,
  })
```

### URL 命名（動的 API ルーティングルール）

バックエンドは自作の `IDynamicApiController`（`Web.Core/ApiRouting/AdminApiRouteRules` で実装）で service クラス名から URL を自動生成します：**`Service` / `AppService` / `Controller` サフィックスを除去 → 先頭を小文字に → `api/` プレフィックスを付与**。

| バックエンドクラス名 | フロントエンド URL |
|---|---|
| `SystemAuthService` | `/api/systemAuth/...` |
| `GameAppService` | `/api/gameApp/...` |
| `SystemUserService` | `/api/systemUser/...` |

**フロントエンド URL に `Service` サフィックスを残すことは厳禁**。新人が 100% 踏む穴です。

### エラーコード → i18n

バックエンドの安定エラーコードは `src/i18n/pages/errors/<lang>.ts` の `errors.codes.<CODE_WITH_UNDERSCORE>` に自動マッピングされます：

- `AUTH.FORBIDDEN` → `errors.codes.AUTH_FORBIDDEN`
- `D0008` → `errors.codes.D0008`
- `VALIDATION.FAILED` → バックエンドの message をそのまま透過（フィールド単位のエラーを保持）

新しいエラーコードは `errors/zh-cn.ts` に対応する key を追加するだけです。

### バックエンド契約変更フロー

1. バックエンドが OpenAPI / Swagger を変更
2. 新しい `openapi.json` をエクスポート → リポジトリルートを置換
3. `pnpm gen:api` → `git diff src/api/` を確認
4. フィールドのリネーム / 削除 → フロントエンドの呼び出し箇所を修正（TS コンパイルでエラーになります）
5. `pnpm run check:frontend api` を実行（手書きと生成の重複を確認）
6. `pnpm run quality` を必ず通す

---

## 新しいページを追加する

手順通りにやれば、日常業務の 80% が身につきます。「お知らせ」リストページを追加してみましょう。

### 1. ファイルを作成

```
src/views/system/demo/
├── index.vue          # リストページ
└── component/
    └── editDemo.vue   # 追加/編集ダイアログ
```

### 2. リストページを書く

`src/views/system/demo/index.vue` の核心構造（完全なスタイルは省略）：

```vue
<template>
  <div class="p-3">
    <!-- 上部：検索 + 追加 -->
    <Card>
      <template #content>
        <div class="flex flex-wrap gap-3 align-items-end">
          <InputText v-model="state.queryParams.title" />
          <Button v-auth="'demo:page'" @click="handleQuery">
            {{ t('common.button.search') }}
          </Button>
          <Button v-auth="'demo:add'" @click="editRef.openDialog()">
            {{ t('common.button.add') }}
          </Button>
        </div>
      </template>
    </Card>

    <!-- テーブル -->
    <Card class="mt-2">
      <template #content>
        <CrudDataTable
          :value="state.data"
          :totalRecords="state.tableParams.total"
          :rows="state.tableParams.pageSize"
          :first="(state.tableParams.page - 1) * state.tableParams.pageSize"
          @page="onPageChange"
          v-block-loading="state.loading"
        >
          <Column field="title" :header="t('common.label.title')" />
          <Column :header="t('common.table.action')">
            <template #body="{ data }">
              <Button link v-auth="'demo:update'" @click="editRef.openDialog(data)">
                {{ t('common.button.edit') }}
              </Button>
            </template>
          </Column>
        </CrudDataTable>
      </template>
    </Card>

    <EditDemo ref="editRef" @handle-query="handleQuery" />
  </div>
</template>

<script setup lang="ts" name="systemDemo">
import { onMounted, reactive, ref } from 'vue'
import { useI18n } from 'vue-i18n'
import { getAPI } from '/@/utils/axios-utils'
import { Demo as DemoApi } from '/@/api/demo'
import CrudDataTable from '/@/components/crud-data-table/index.vue'

const { t } = useI18n()
const editRef = ref()
const state = reactive({
  loading: false,
  data: [] as any[],
  queryParams: { title: '' },
  tableParams: { page: 1, pageSize: 10, total: 0 },
})
const handleQuery = async () => {
  state.loading = true
  try {
    const res = await getAPI(DemoApi).demoPagePost({ ...state.queryParams, ...state.tableParams })
    state.data = res.data.result?.items ?? []
    state.tableParams.total = res.data.result?.total ?? 0
  } finally { state.loading = false }
}
const onPageChange = (e: any) => {
  state.tableParams.page = e.page + 1
  state.tableParams.pageSize = e.rows
  handleQuery()
}
onMounted(handleQuery)
</script>
```

### 3. ダイアログを書く

`src/views/system/demo/component/editDemo.vue`：

```vue
<template>
  <BaseEditDialog
    v-model:visible="state.isShowDialog"
    :default-form="() => ({ title: '', content: '' })"
    :add-api="(data) => getAPI(DemoApi).demoAddPost(data)"
    :update-api="(data) => getAPI(DemoApi).demoUpdatePut(data)"
    :rules="{ title: { required: true, message: 'タイトルを入力してください' } }"
    @handle-query="emit('handleQuery')"
  >
    <InputText v-model="state.ruleForm.title" />
    <InputText v-model="state.ruleForm.content" />
  </BaseEditDialog>
</template>

<script setup lang="ts">
import { reactive } from 'vue'
import { getAPI } from '/@/utils/axios-utils'
import { Demo as DemoApi } from '/@/api/demo'

const emit = defineEmits<{ (e: 'handleQuery'): void }>()
const state = reactive({ isShowDialog: false, ruleForm: {} as any })
</script>
```

`CrudDataTable` と `BaseEditDialog` はプロジェクト組み込みの部品（`src/components/`）で、ページング / loading / 操作列 / 追加編集の判定 / 送信 loading をラップしています。**車輪の再発明はしないでください**。

### 4. i18n を追加（5 言語すべて必須）

`src/i18n/pages/system/demo/zh-cn.ts`：

```ts
export default {
  title: '通知公告',
  form: { title: '标题', content: '内容' },
}
```

残り 4 つの言語ファイルも**必ず作成**します（機械的コピーでも可）：

```
src/i18n/pages/system/demo/
├── zh-cn.ts
├── en.ts
├── zh-tw.ts
├── ja.ts
└── ko.ts
```

1 つでも欠けると `pnpm run lint:i18n` が fail します。

### 5. バックエンドにメニューを登録してもらう

コードは書き終わってもまだページは見えません——メニューはバックエンドが管理しています。バックエンド担当者に伝えること：

```
path:        /system/demo
name:        systemDemo
component:   /system/demo/index       ← src/views/ のパスと一致させること
title:       通知公告
titleCode:   system.demo.title        ← zh-cn.ts に存在させること
parent:      /system
```

バックエンドの commit + デプロイ後、フロントエンドで再ログインするとメニューが見えます。

### 6. コミット

```bash
pnpm run quality      # すべてのゲートを一括実行
git add .
git commit -m "feat(system.demo): お知らせリスト + ダイアログを追加"
git push
```

`pnpm run quality` が落ちたらエラーに従って修正：i18n の key 不足は翻訳を追加、権限がメニューシードにないならバックエンドに追加を依頼、ハードコード中国語は i18n に移行。

---

## 開発規約

> このセクションは**制約**です——CI でチェックされ、違反すると build fail になります。

### 命名規約

| 分類 | スタイル | 例 |
|---|---|---|
| ビューディレクトリ / ファイル | kebab-case | `src/views/system/user/index.vue` |
| API モジュールディレクトリ | camelCase（バックエンド swagger tag） | `src/api/systemUser/` |
| Pinia store ファイル | camelCase | `themeConfig.ts` |
| Composables ファイル | `useXxx` | `useEditDialog.ts` |
| ユーティリティ関数ファイル | camelCase | `authFunction.ts` |
| 権限文字列 | `<resource>:<action>` | `systemUser:page`、`gameHomeEntity:query` |
| ルート name | camelCase | `systemUser` |
| ルート path | kebab-case | `/system/user` |
| i18n key | ドット区切りの階層 | `system.user.title`、`common.button.search` |
| エラーコード key | 大文字アンダースコア | `AUTH_FORBIDDEN` |

### ディレクトリ規約

- ビューの `<module>` はバックエンドメニュー `component` の最初のセグメントと一致させ、人間 ↔ ルート ↔ ソースの三者間定位を容易にする
- ダイアログは `views/<module>/component/` に統一
- 単一ページ専用コンポーネントは `views/<module>/component/` に置き、`src/components/`（再利用プール）には**入れない**
- API モジュールは生成物と同じ名前にしない（`check:api` が検出して human review に回します）

### 権限規約

- テンプレートでは `v-auth="'xxx'"` / `v-auth="['x','y']"` / `v-auth-all` を使用
- `v-if` / props では `auth('xxx')` または `hasAnyAuth('x','y')`（`src/utils/authFunction.ts`）
- すべての権限文字列はバックエンドメニューシードの `MenuTypeEnum.Btn` エントリーに存在しなければならず、さもないと `pnpm run check:permissions` が fail
- 新しい権限の追加フロー：バックエンドでシードエントリー追加 → フロントで `v-auth` 参照 → `check:permissions` を実行

### i18n 規約

- **すべての i18n キーは 5 言語すべてに存在必須**（`zh-cn / en / zh-tw / ja / ko`）、`pnpm run lint:i18n` が自動チェック
- テンプレートと `<script setup>` 内での**ハードコード中国語は厳禁**、`pnpm run check:frontend text` が changed files をスキャン
- 過去の負債は `scripts/i18n-text-baseline.json` のフィンガープリントで免除；移行期間中は `pnpm run check:frontend text-baseline` で増分ベースライン化
- 新しいエラーコードは `errors/zh-cn.ts` に `errors.codes.<CODE_WITH_UNDERSCORE>` を追加

### HTTP 規約

- **page / list / query / search / filter 系インターフェースで GET による params / data 送信は禁止**——`pnpm run check:frontend api-query-get` がブロックします
- バックエンド生成 API は `xxxPagePost` / `xxxListPost` / `xxxQueryPost` を使用
- 手書き API は `request({ method: 'post', data: params })` を使用
- 業務コードで `Authorization` / `Accept-Language` / `X-Tenant-Id` を手動で付与**しない**——インターセプターが自動処理
- レスポンス内で手動 `showError` **しない**——インターセプターが業務コードを自動処理

### コミットフロー

```bash
pnpm run quality      # 必須：lint + typecheck + test:unit + build + check:frontend
git add .
git commit -m "<scope>(<module>): <subject>"   # 中国語 / 英語どちらでも可、≤ 60 字
git push
```

> `pnpm run quality` が束ねる総合ゲートには、2026-10-20 から Vitest のユニット / コンポーネントテストが lint と build の間に組み込まれています。
> ユニット層の回帰は build の前に拦み、統合コストを削減します。新しい純ロジックのユーティリティには `src/<dir>/<name>.test.ts` に同位で単体テストを追加してください。
> 新しい Vue コンポーネントは `src/components/form-label/index.test.ts`（`mount` + provider 注入 + slot テキスト assert）を参照してください。
> 開発中は `pnpm run test:unit:watch` で watch UI を利用できます。

- scope: `feat / fix / refactor / docs / style / test / chore / perf`
- module: `system.user` / `api` / `router` / `docs` / `i18n`…
- body には動機 + 主要な変更を書く
- ブランチ：`feat/<module>-<desc>` / `fix/<module>-<desc>`

### 違反例（このように書かない）

```ts
// ❌ axios.get('/api/xxx', { params }) — query 系インターフェースで GET は禁止
const res = await axios.get('/api/user/page', { params })

// ✅
const res = await request({ url: '/api/user/page', method: 'post', data })

// ❌ localStorage.setItem('themeConfig', ...) — プレフィックスが自動で付かない
window.localStorage.setItem('themeConfig', JSON.stringify({...}))

// ✅
import { Local } from '/@/utils/storage'
Local.set('themeConfig', {...})

// ❌ URL に Service サフィックス
const url = '/api/systemUserService/page'

// ✅
const url = '/api/systemUser/page'

// ❌ テンプレートに中国語をハードコード
<span>検索</span>

// ✅
<span>{{ t('common.button.search') }}</span>

// ❌ isShowDialog / openDialog / submit の三種セットを手書き
defineExpose({ isShowDialog, openDialog, submit })

// ✅ useEditDialog composable を使う
const { isShowDialog, openDialog, submit } = useEditDialog({ ... })
```

---

## FAQ とハマりポイント

### FAQ

**メニュー / 権限 / 文言はなぜバックエンドから？**
フロントエンドが「誰が何を見られるか」を知るべきではないからです。バックエンドがロールに基づいて計算して返し、フロントは描画するだけ。バックエンドが変更してリリースしても、フロントは何もしなくて済みます。

**JWT って何？ 自動更新はどう仕組み？**
映画のチケットです。サーバーが暗号化したチケットを渡し、リクエストヘッダーに `Authorization: Bearer xxx` を付けて提示します。期限切れ前にフロントエンドが自動で `X-Authorization` を付け、バックエンドに黙って再発行させます。

**なぜ i18n は 5 ファイル必要？**
多言語製品なので、CI が `zh-cn / en / zh-tw / ja / ko` の存在を強制します。1 つ欠ければ fail。新しく書く key はまず zh-cn を機械的にコピーし、翻訳は後で補っても構いません。

**`getAPI(...)` と `request(...)` はどちらを使う？**
- `getAPI(SomeApi).someMethod()` —— 自動生成 API（`src/api/<module>/`）
- `request({ url, method })` —— 手書き API（`src/api/login.ts` など）
両者は同じ HTTP インターセプターを共有、自分で axios を作らないでください。

**`v-auth` / `v-block-loading` / `v-reclick` って何？**
- `v-auth="'xxx'"` —— 権限なしで `display: none`
- `v-block-loading="loading"` —— ブロック全体に loading オーバーレイ
- `v-reclick` —— ボタン連打防止
定義は `src/directive/`、テンプレートでそのまま attribute として使えます。

**`src/api-services/` は何のため？**
過去の遺物ディレクトリで、廃止済み。新しく生成される API はすべて `src/api/<module>/` にあります。

### ハマりポイント

**ログイン後に白画面 / メニューが出ない**
DevTools → Network で `/api/systemAuth/login` が 200 か確認。違えば → `VITE_API_URL` とバックエンドを確認；200 なら → `/userInfo` と `/loginMenuTree` を確認、どちらかが 200 でないと `/login` に飛ばされます。両方 200 でも白画面なら → Console の JS エラーを確認、大抵は component が存在しない .vue を指しています。

**メニューには入れるのにページが 404**
`pnpm run check:routes` を実行。大抵はバックエンドメニューの `component` パスが `src/views/` と一致していません。

**ボタンが全く見えない**
Network で `/userInfo` レスポンスの `buttons` フィールドに期待の文字列があるか確認；あるのに見えない → `v-auth` の文字列が**完全に一致**しているか（大文字小文字を区別）；そもそもない → バックエンド担当者へ、あなたのロールにその権限がありません。

**i18n で missing key**
`pnpm run lint:i18n` を実行し、エラーに従って 5 言語ファイルの不足 key を補完。

**`check:frontend text` が全滅**
ハードコード中国語を追加していないか確認 → i18n に移行；そうでなければ移行期間中の過去の負債 → `pnpm run check:frontend text-baseline` で増分ベースライン化（慎重に）。

**Docker デプロイ後も API アドレスが古いまま**
1. `docker exec -it <container> cat /usr/share/nginx/html/config/config.js` で `window.__env__.VITE_API_URL` を確認
2. 古い → 起動時に `-e VITE_API_URL=...` を渡していない
3. 新しい値なのにブラウザが古い → Ctrl+F5 / Cmd+Shift+R で強制リロード
4. それでもダメ → コンテナーが古いイメージで起動しているので `docker pull` で最新を取得

**ビルドが OOM**
Docker Desktop → Preferences → Resources → Memory を 8G+、Swap を 4G+ に調整。ビルドコマンドには `NODE_OPTIONS=--max-old-space-size=4096` を追加済みです。

**401 後に自動でログインへ飛ばない**
`localStorage.access-token` が期限切れでないか；JWT payload の `exp`；バックエンド `/api/systemAuth/refresh-token` が新しい token を返しているかを確認。

---

## デプロイ

### Docker（推奨）

> 単一リポジトリ統合（2026-10-19）：Vue Dockerfile の build context はリポジトリルートに移動済み、
> `build/default.conf` / `build/entrypoint.sh` もリポジトリルートの `build/` へ移出されました。リポジトリルートでビルドを実行してください。

```bash
docker build -t gameframex-admin-web:dev -f GameFrameX.Admin.Unified.Vue/Dockerfile .
docker run -d -p 80:80 gameframex-admin-web:dev                                          # デフォルト API アドレス
docker run -d -p 80:80 -e VITE_API_URL=https://api.example.com gameframex-admin-web:dev   # カスタム API
```

コンテナーには小さな魔法があります：`build/entrypoint.sh` が起動時に環境変数を読んで `config.js` を書き換えるので、**同じイメージを環境変数で別環境に切り替えられます**。

### docker-build-push.sh の完全なパラメーター

```bash
./build/docker-build-push.sh --tag v2.4.33
./build/docker-build-push.sh \
  --registry registry.cn-hangzhou.aliyuncs.com \
  --namespace myns \
  --tag v2.4.33
./build/docker-build-push.sh \
  --platforms linux/amd64,linux/arm64 \
  --tag v2.4.33
./build/docker-build-push.sh --no-push                # ビルドのみで push しない
./build/docker-build-push.sh \
  --registry registry.cn-hangzhou.aliyuncs.com \
  --namespace myns \
  --username your_user \
  --password your_pass \
  --tag v2.4.33                                       # プライベートリポジトリ認証
```

| パラメーター | 説明 | デフォルト値 |
|---|---|---|
| `--registry URL` | コンテナーレジストリ URL | 空（Docker Hub） |
| `--namespace NAME` | 名前空間 / ユーザー | `alianblank` |
| `--name NAME` | イメージ名 | `gameframex-admin-web` |
| `--tag TAG` | イメージタグ | git tag または `ブランチ-日付-SHA` |
| `--platforms LIST` | 対象プラットフォーム | `linux/amd64` |
| `--no-push` | ビルドのみで push しない | — |
| `--username USER` | レジストリのユーザー名 | — |
| `--password PASS` | レジストリのパスワード | — |

イメージタグの自動生成ルール：現在の commit に git tag があれば → tag を使用；なければ → `ブランチ名-日付-SHA`；同時に `latest` も付与。

### CNB パイプライン

`main` / `develop` への push で自動ビルド + テスト環境デプロイ；tag を打つと `latest` の付与 + 自動デプロイが同期します。詳細は `.cnb.yml` を参照。

### 純静的

`pnpm run build` の成果物は `GameFrameX.Admin.Unified.Vue/build/dist/` にあります（`vite.config.ts` の `outDir` は引き続き cwd 相対の `build/dist`、相対サブディレクトリで有効）。SPA fallback（`try_files $uri /index.html`）をサポートする HTTP サーバーなら何でもホストできます。Nginx テンプレートはリポジトリルートの `build/default.conf`（2026-10-19 の単一リポジトリ統合で、上に移動した Vue の `build/` から移出済み）。

### Docker Compose 例

```yaml
services:
  admin-web:
    image: gameframex-admin-web
    ports: ["80:80"]
    environment:
      - VITE_API_URL=https://api.example.com
    restart: unless-stopped
```

---

## コマンド一覧

| コマンド | 用途 |
|---|---|
| `pnpm run dev` | 開発サーバー起動 |
| `pnpm run build` | 本番ビルド（成果物は `GameFrameX.Admin.Unified.Vue/build/dist/`、vite `outDir` は cwd 相対） |
| `pnpm run lint` / `lint-fix` | ESLint チェック / 自動修正 |
| `pnpm run format` / `format-fix` | Prettier チェック / 自動修正 |
| `pnpm run typecheck` | tsc 型チェック |
| `pnpm run test:unit` / `test:unit:watch` / `test:unit:coverage` | Vitest ユニット / コンポーネントテスト（jsdom + @vue/test-utils、v8 カバレッジ付き） |
| `pnpm run test:e2e` / `test:e2e:headed` / `test:e2e:list` / `test:e2e:report` | Playwright E2E（システムの Edge、スイートは直列） |
| `pnpm gen:api` | `openapi.json` からフロントエンド API クライアントを再生成 |
| `pnpm run lint:i18n` | 5 言語 i18n キーの完全性チェック |
| `pnpm run check:routes` | バックエンドメニュー component とフロントビューの整合チェック |
| `pnpm run check:permissions` | フロントの権限宣言とバックエンドシードの整合チェック |
| `pnpm run check:api` | 手書きと生成 API の重複 review |
| `pnpm run check:text` | ハードコード中国語スキャン（changed files で fail） |
| `pnpm run check:frontend` | 上記すべて + Sys 略記回帰 |
| `pnpm run quality` | **すべてのゲートを一括実行**、コミット前に必須 |

`pnpm gen:api` の前提：バックエンドから `openapi.json` をエクスポート——次のセクションを参照。

---

## バックエンドから OpenAPI をエクスポート

`pnpm gen:api` はリポジトリルートの `openapi.json` しか認識しませんが、**フロントエンドは自分でこれを生成しません**。バックエンドの Swagger エンドポイントから取得します。

バックエンドは .NET + Swashbuckle で、Swagger JSON は以下に公開されます：

```
http://localhost:20802/swagger/web.api/swagger.json
```

> Swagger は**デフォルトで dev 環境のみ有効**です。本番 / Docker では `Swagger__Enabled=true`（または `Swagger:Enabled=true`）を設定しないとこのエンドポイントは公開されません——バックエンド `Startup.cs` のスイッチ動作で、本番でデフォルトはドキュメントを露出しないためです。

```bash
# 1. バックエンドを起動（どちらかで Swagger にアクセス可能に）
ASPNETCORE_ENVIRONMENT=Development dotnet run --project ../GameFrameX.Admin.Unified.Api/GameFrameX.Web.Api &
# または
Swagger__Enabled=true dotnet run --project ../GameFrameX.Admin.Unified.Api/GameFrameX.Web.Api &

# 2. swagger.json を取得
curl -sSL http://localhost:20802/swagger/web.api/swagger.json -o openapi.json
ls -lh openapi.json   # 約 3 MB のはず

# 3. フロントエンド生成を実行
pnpm gen:api
git diff src/api/     # 破壊的変更がないか確認
```

Client / Hub API も同じパターン：`/swagger/client.api/swagger.json`（ポート 20808）、`/swagger/hub.api/swagger.json`。フロントの Admin プロジェクトは Admin API だけ使うので、この 2 つは通常取得不要です。

### 自動化スクリプト（任意）

チームで頻繁にやるならスクリプトを書きましょう（自分で書いてください、代わりには書きません）。最小構成：

```bash
#!/usr/bin/env bash
set -euo pipefail

BACKEND_HOST="${BACKEND_HOST:-http://localhost:20802}"
FRONTEND_REPO="${FRONTEND_REPO:-$(cd "$(dirname "$0")/../../GameFrameX.Admin.Unified.Vue" && pwd)}"

ASPNETCORE_ENVIRONMENT=Development \
  dotnet run --project ../GameFrameX.Admin.Unified.Api/GameFrameX.Web.Api &
BACKEND_PID=$!
trap "kill $BACKEND_PID 2>/dev/null || true" EXIT

for i in $(seq 1 30); do
  curl -fsSL "${BACKEND_HOST}/swagger/web.api/swagger.json" -o /dev/null && break
  sleep 1
done

curl -sSL "${BACKEND_HOST}/swagger/web.api/swagger.json" -o "${FRONTEND_REPO}/openapi.json"
echo "wrote ${FRONTEND_REPO}/openapi.json"
```

---

## おわりに

口伝：

> **メニューはバックエンドから、権限はバックエンドから、国際化は 5 言語必須、コミット前に `pnpm run quality`。**

詰まっても死にもの狂いで悩まない：まずハマりポイントを見て、解決しなければ git log を見て、それでもだめなら同僚に聞く。

具体的なコードを見たければここへ：

- `src/router/index.ts` — ルートガード
- `src/router/backEnd.ts` — バックエンドメニュー → フロントルート
- `src/utils/http/shared.ts` — HTTP インターセプター
- `src/stores/userInfo.ts` — ユーザー情報 / ボタン権限
- `src/composables/useEditDialog.ts` — ダイアログの核
- `src/utils/appWorkspace.ts` — マルチテナントアプリケーションスコープ
- `scripts/check-frontend-standards.mjs` — クロスプラットフォーム規約ゲート

## ドキュメントとリソース

- プロジェクトドキュメントサイト：<https://gameframex.doc.alianblank.com>
- プロジェクト概要：[`../README.ja.md`](../README.ja.md)
- バックエンド運用マニュアル：[`../GameFrameX.Admin.Unified.Api/README.ja.md`](../GameFrameX.Admin.Unified.Api/README.ja.md)
- Issue 報告：<https://github.com/GameFrameX/Admin/issues>

## コミュニティとサポート

[![GitHub](https://img.shields.io/badge/GitHub-181717?style=for-the-badge&logo=github&logoColor=white)](https://github.com/GameFrameX/gameframex)
[![Discord](https://img.shields.io/badge/Discord-5865F2?style=for-the-badge&logo=discord&logoColor=white)](https://discord.gg/VDWUjWMDw9)
[<img src="https://cdn.jsdelivr.net/npm/devicon@2/icons/linkedin/linkedin-original.svg" height="28" alt="LinkedIn" />](https://www.linkedin.com/in/alianblank)
[![Reddit](https://img.shields.io/badge/Reddit-FF4500?style=for-the-badge&logo=reddit&logoColor=white)](https://www.reddit.com/r/GameFrameX/)
[![X](https://img.shields.io/badge/X-000000?style=for-the-badge&logo=x&logoColor=white)](https://x.com/alian_blank)
[![YouTube](https://img.shields.io/badge/YouTube-FF0000?style=for-the-badge&logo=youtube&logoColor=white)](https://www.youtube.com/channel/UCD9QhSFJ5xZkn5NTSV-DVAw)
[![Bluesky](https://img.shields.io/badge/Bluesky-0285FF?style=for-the-badge&logo=bluesky&logoColor=white)](https://bsky.app/profile/alianblank.bsky.social)
[![Bilibili](https://img.shields.io/badge/Bilibili-00A1D6?style=for-the-badge&logo=bilibili&logoColor=white)](https://www.bilibili.com/video/BV1yrpeepEn7)
[![Gitee](https://img.shields.io/badge/Gitee-C71D23?style=for-the-badge&logo=gitee&logoColor=white)](https://gitee.com/GameFrameX/gameframex)
![QQ](https://img.shields.io/badge/QQ-467608841%2F233840761-EB1923?style=for-the-badge&logo=qq&logoColor=white)

## 変更履歴

バージョン変更履歴は [`CHANGELOG.md`](./CHANGELOG.md) を参照してください。

## ライセンス

詳しくは [LICENSE](./LICENSE) をご参照ください。

<!--
EN: See [LICENSE](./LICENSE) for license information.
zh-CN: 详见 [LICENSE](./LICENSE) 文件。
zh-TW: 詳見 [LICENSE](./LICENSE) 檔案。
ja: 詳しくは [LICENSE](./LICENSE) をご参照ください。
ko: 자세한 내용은 [LICENSE](./LICENSE) 파일을 참조하세요.
-->
