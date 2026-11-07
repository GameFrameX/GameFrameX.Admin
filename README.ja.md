<div align="center">

<img src="https://download.alianblank.com/gameframex/gameframex_logo_320.png" alt="GameFrameX Logo" width="160" />

# GameFrameX Admin

[![License](https://img.shields.io/badge/license-blue.svg)](LICENSE.md)
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

## プロジェクト概要

> 一言でいうと：**インディゲーム開発者向けの運営 + 共同運営管理プラットフォーム**。バックエンドは `.NET 10 + SqlSugar` の 3 ホスト（Admin / Client / Hub）、フロントエンドは `Vue 3 + PrimeVue`。管理者ログインからプレイヤーの決済コールバックまで一本の流れがフルスタックで通っており、すぐに使えます。

**GameFrameX Admin** は**ゲーム運営向け**の管理システムです。管理画面は運営 / GM / カスタマーサポートによるメニュー編集、注文照会、バージョン配信、ログ閲覧を提供します。クライアント API はゲーム側のログイン / 決済 / 設定取得を、Hub API は外部システム（CI ビルド、コンテナ制御、機能スイッチ同期）を受け持ちます。1 つのリポジトリで 3 つのホストを構成し、DB 分離デプロイで互いに影響しません。

このシステムは汎用 OA / CRM / モールシステムでは**ありません**。メニュー、権限、辞書、フォーム、クライアント設定などは、すべて「ゲーム管理画面」という実際の業務シナリオに沿って設計されています。

本プロジェクトは [Admin.NET](https://gitee.com/zuohuaijun/Admin.NET) をベースに開発しており、Furion フレームワークの残存コードと旧 Admin.NET の業務命名は、すべて置換とベースライン再設定（U5 / U8 / C37–C41）が完了しています。原作者 [zuohuaijun](https://gitee.com/zuohuaijun) とコントリビューターの皆さんに感謝します。

### リポジトリ構成

単一リポジトリ（monorepo）：フロントエンドとバックエンドを同一コードベースで運用します。2026-10-19 より、旧サブリポジトリ `GameFrameX.Admin.Unified.Api` / `GameFrameX.Admin.Unified.Vue` は読み取り専用で凍結され、スナップショットとして本リポジトリへ統合済みです。新規コードはすべて本リポジトリに反映します。

```
Admin/
├── GameFrameX.Admin.Unified.Api/    # .NET バックエンド：Admin / Client / Hub の 3 ホスト + Application / Entity / Core レイヤー
├── GameFrameX.Admin.Unified.Vue/    # Vue フロントエンド：PrimeVue + Pinia + vue-i18n（5 言語）
├── build/                           # Nginx 設定 + デプロイスクリプト（deploy-*.sh / docker-build-push.sh / entrypoint.sh）
├── doc/                             # README 用画像
├── .github/workflows/               # CI：3 API イメージとフロントエンドイメージのビルド、フロントエンドテスト、外部イメージ同期
├── docker-compose.yml               # コマンド 1 つのフルスタック編成（PostgreSQL + 3 API + フロントエンド）
├── docker-compose.admin.yml          # Admin 独立スタック：専用 PostgreSQL + 3 API ホスト、フロントエンドコンテナなし
├── .env.example                     # compose 環境変数テンプレート（.env にコピーして必要に応じて上書き）
├── AGENTS.md / CLAUDE.md            # AI エージェント連携入口（ルート指示 + readme_i18n 設定）
└── LICENSE.md                       # ライセンス
```

## クイックスタート

> **新しいゲーム**に運営管理画面を導入するシーンを想定しています。このセクションを読み終えれば、0 からログインまで進み、TeamS admin でメニューツリーを確認できるようになります。

### 準備

- .NET 10 SDK
- Node.js ≥ 20（推奨 20）+ pnpm ≥ 11.5.2（`corepack enable`）
- データベースがなくても OK。ローカルでソースを直接実行すればデフォルトで SQLite のゼロ設定。本番では PostgreSQL / MySQL など 34 種の DbType に対応

### 手順

```bash
# 1. コードを取得（単一リポジトリ、フロントエンドとバックエンドを同梱）
git clone https://github.com/GameFrameX/Admin.git
cd Admin

# 2. バックエンドを起動（リポジトリルートで実行！ 接続文字列はワーキングディレクトリ相対で解決されます）
dotnet run --project GameFrameX.Admin.Unified.Api/GameFrameX.Web.Api     # Admin API → http://localhost:20802
dotnet run --project GameFrameX.Admin.Unified.Api/GameFrameX.Client.Api  # Client API → http://localhost:20808
dotnet run --project GameFrameX.Admin.Unified.Api/GameFrameX.Hub.Api     # Hub API → http://localhost:20806

# 3. フロントエンドを起動
cd GameFrameX.Admin.Unified.Vue
corepack enable
pnpm install
pnpm run dev    # デフォルト http://localhost:20800（hash ルーティング）
```

ブラウザを開く → ログインページが表示される → アカウント `admin` / パスワード `123456`（初回起動時に SqlSugar Code-First が自動でテーブル作成 + シードデータ投入を行います）。

### コマンド 1 つでフルスタック（リポジトリルートの統一 compose）

```bash
docker compose up -d --build
```

Compose には PostgreSQL + 3 つのホスト（20802/20806/20808）+ Admin フロントエンド（20800、`pnpm run dev` と排他。コンテナを起動する前に先に dev server を停止してください）が組み込まれており、起動後 healthcheck が緑になればアクセスできます。

環境変数の統一エントリ：ルートの [`.env.example`](.env.example) を `.env` にコピーして必要に応じて上書きしてください（未設定の変数は compose のデフォルト値を使用。`.env` は git 管理対象外です）。

### Admin 独立スタック（docker-compose.admin.yml）

本番や隔離環境では Admin 独立 compose を利用できます。専用 PostgreSQL + 3 つの API ホストのみで、フロントエンドコンテナは含みません。管理画面の静的サイトは統一 compose の `web-vue` または独自の静的ホスティングで公開してください。`WEB_VUE_API_URL` は統一 compose の `web-vue` コンテナのみを設定するもので、自己ホストの静的サイトには作用しません。自己ホスト時はビルド・公開した `config/config.js` の `window.__env__.VITE_API_URL` をブラウザから到達可能な Web API アドレスに設定してください。

起動前にルートの `.env`（または実行環境）で空でない `POSTGRES_PASSWORD` を設定してください。未設定の場合、Compose は起動前にエラーで中断します。

```bash
docker compose -f docker-compose.admin.yml up -d --build
```

起動順序は healthcheck で固定されます：`pg` → `admin-web`（メイン DB を初期化）→ `admin-client` / `admin-hub`。ゲームサーバーも Docker で動かす場合は、先に本スタックを起動し 4 つの healthcheck がすべて緑になってからゲームサーバー側 compose を起動してください。2 つの compose は相互に `depends_on` せず、Admin の PostgreSQL データボリュームも共有しません。隔離用オーバーライド（`ADMIN_WEB_PORT` / `ADMIN_CLIENT_PORT` / `ADMIN_HUB_PORT` / `ADMIN_POSTGRES_DATA_DIRECTORY`）も同じルートの `.env` に記述します。

より細かいコマンドと落とし穴については [`GameFrameX.Admin.Unified.Api/README.ja.md`](GameFrameX.Admin.Unified.Api/README.ja.md)（バックエンドマニュアル）と [`GameFrameX.Admin.Unified.Vue/README.ja.md`](GameFrameX.Admin.Unified.Vue/README.ja.md)（フロントエンドマニュアル）を参照してください。

## 向いているケース / 向いていないケース

| 向いている | あまり向いていない |
|---|---|
| インディ / 小規模チームのゲームで、動く運営管理画面が必要 | すでに完全な管理画面があり、その中の一部のモジュールだけ欲しい |
| 複数の接続先（管理 / クライアント / 外部システム）で明確な境界が必要 | 単一用途のシンプルな管理画面（Admin.NET / vue-next-admin を直接使う方が軽量） |
| 単一ゲームの自社運営（サーバー区 / プレイヤー / 課金 / クライアント配信の一体型） | 複数ゲームの一元運営や SaaS 型マルチテナント化 |
| .NET + SqlSugar の技術スタックが必須 | チームが Java / Go / Node しか経験していない |
| 多言語（5 言語 i18n）+ AI Agent フレンドリーなリポジトリが必要 | — |
| マイクロサービスへ進化できるモノリスとして始めたい | — |

## 技術スタック

- **バックエンド**：.NET 10 + ASP.NET Core ネイティブホスティング + SqlSugar（Code-First、34 種の DbType）+ JWT/HMAC + SignalR + Serilog。
- **フロントエンド**：Vue 3.5 + Vite 6 + TypeScript strict + PrimeVue 4.5 + Pinia 3 + vue-i18n（5 言語）+ Axios。

選定リストの全容とバージョンマトリクスは [バックエンド README · 技術スタック](GameFrameX.Admin.Unified.Api/README.ja.md#技術スタック) と [フロントエンド README · 技術スタック](GameFrameX.Admin.Unified.Vue/README.ja.md#技術スタック) を参照してください。

## 3 つのホスト：Admin / Client / Hub

1 つのリポジトリで 3 つのホスト、DB 分離デプロイで互いに干渉しません：**Admin API**（20802、管理画面）、**Client API**（20808、ゲームクライアントのログイン/決済/設定）、**Hub API**（20806、外部システム / CI / WebHook）。三者は Entity と SqlSugar リポジトリを共有し、依存の向きは下方向のみです（`Application.*` / `Core.*` からホストへの逆参照は厳禁）。

### 3 ホストの最大の違い

| 観点 | Admin API | Client API | Hub API |
|---|---|---|---|
| ルーティング | 動的 API（Service を書くと自動的に API が生える） | 動的 API | 従来型 Controller（`[Route("api/[controller]/[action]")]`） |
| データアクセス | Application Service 経由 → リポジトリ | Application Service 経由 → リポジトリ | Controller に `Repository<T>` を直接注入 |
| Application 層の再利用 | する | する | しない（自前でバリデーション） |
| 新規 API の追加 | Service を書く | Service を書く | Controller を作成 + リポジトリを注入 |
| URL 規則 | `Service` 接尾辞を除去 + camelCase | `Service` 接尾辞を除去 + camelCase | `Controller` 名 + `Action` 名 |
| SuperAdmin を拒否するか | しない（SuperAdmin は全フィルタをバイパス） | しない | **する**。Hub は SuperAdmin のアクセスを拒否 |

> **URL に `Service` 接尾辞を含めることは厳禁**。新人が 100% 踏む落とし穴です。フロントエンドはこの規約に基づいて呼び出しコードを生成するため、クラス名が変わるとフロントが全面的に壊れます。

## アイデンティティとスコープ

単一ゲームの管理バックエンドで、2 系統のアイデンティティ（ユーザーテーブルは別々、ログイン検索順序は固定）：

| アイデンティティ | 説明 |
|---|---|
| SuperAdmin（プラットフォーム管理者） | システムレベルのアイデンティティ。グローバル設定とシステム管理を担当し、初回インストールウィザードで作成 |
| 管理者 | 日常運用のアイデンティティ。ロール権限が機能を制御し、サーバー区スコープが業務データ範囲を制御 |

- `ServerId` が唯一の運用スコープです。バックエンドとゲームサーバーの通信・照会・書き込み・コールバック契約はテナントやアプリのアイデンティティに依存しません。
- DB は単一メイン DB：プラットフォームテーブルは `system_` プレフィックス、グローバル定義テーブルは `global_`、業務テーブルはプレフィックスなし。テナント DB やテナント / アプリ単位のルーティングは存在しません。
- 公開コールバック（決済 / 広告 / WebShop）の URL は `{packageId}` 形式のみ。テナント / アプリ区分を含む旧 URL は 404 になり、互換解析も提供しません。

## 30 の機能モジュール

「実際に使うシーン」別のグループ分けであり、ディレクトリ構造順ではありません。

**認証と権限**

| モジュール | 内容 |
|---|---|
| ユーザー管理 | 管理者アカウントの管理。ロール・データ権限をバインド可能 |
| メニュー管理 | ディレクトリ / メニュー / ボタンの 3 段階権限 |
| ロール管理 | ロールへのメニュー + データ権限範囲のバインド |
| 辞書管理 | グローバル辞書 |
| オープン認証 | AccessKey / Secret + HMAC 署名認証（外部 API） |

**プラットフォーム設定（SuperAdmin のみ表示）**

| モジュール | 内容 |
|---|---|
| グローバルプラットフォーム / 言語 / チャネル定義 | プラットフォーム級の列挙型と多言語ベースライン |
| システム設定 | キー・バリュー。高頻度の設定はメイン DB の `SystemConfig` に移管（JWT / パスワードポリシー / 画像 CAPTCHA） |
| データベース管理 | テーブル / カラム CRUD、DB スキーマのオンライン管理 |

**システム機能**

| モジュール | 内容 |
|---|---|
| オンラインユーザー | リアルタイム一覧 + 強制ログオフ、SignalR ベース |
| お知らせ管理 | システム通知 + SignalR リアルタイム配信 |
| ファイル管理 | ローカルストレージ + アリババクラウド OSS + Tencent COS（マルチプロバイダ） |
| 定時タスク | `ProjectScheduler` プロジェクト独自スケジューラ。内蔵 / スクリプトコンパイル / HTTP の 3 種 |
| メール | `GameFrameX.Email.Abstract` マルチプロバイダ |
| システム API | Swashbuckle Swagger。本番ではデフォルトで無効（`Swagger__Enabled=true` が必要） |
| 4 種ログ | 操作 / 例外 / アクセス / データ変更（フィールド単位の Diff 監査）、Excel エクスポート対応 |
| キャッシュ管理 | Memory / Redis の統一抽象 |

**ゲーム運営機能（76 の Application.Game サービス）**

| モジュール | 内容 |
|---|---|
| ゲームアカウント / キャラクター / ゲームサーバ | プレイヤーアカウント、キャラクター管理、Docker コンテナのライフサイクル |
| 注文 / アイテム配布 | 多軸フィルタリング。HTTP でゲームサーバ API を呼び出してアイテムを付与 |
| メール / 引換コード | 全サーバ / 単一サーバメール。引換コードのチャネルと投入 |
| Web ショップ | 商品 + 注文 + カテゴリ |
| アイテム / 商品 | アイテムカタログ、消耗品 / 永久品の管理 |
| お知らせ / バナー | ゲーム内 + ログイン後のお知らせ |
| アプリバージョン / リソースパック | アプリバージョン + 強制アップデート + リソースパックのバージョンと履歴ロールバック |
| アプリ設定 | リモートパラメータ、アプリ検証、クライアント KV |
| メール運用 | テンプレート + Campaign + 多言語 |
| 広告コールバック | TopOn / AdMob / UnityAds のマルチプロバイダ |
| ゲームサーバ連携 | プレイヤー登録 / ログイン / 注文コールバック |

**外部連携**

| 連携 | 説明 |
|---|---|
| WeChat Pay | JSAPI 注文（直結 / サービスプロバイダ）+ 決済コールバック（`Payment.Wechat`） |
| 決済 | Alipay / Apple IAP（App Store Server API v2 含む）/ Google Play / Xsolla。抽象レイヤー + マルチプロバイダ |
| オブジェクトストレージ | Local / Tencent COS（マルチプロバイダ） |
| メール | Tencent SES / SMTP（マルチプロバイダ） |

**フロントエンドフレームワークの特徴**

- **バックエンド駆動メニュー**：メニュー、権限、文言はすべてバックエンドで制御します。フロント側にメニューは書かず、追加はバックエンドのメニュー管理で行います。フロントはバックエンドが返すメニューツリーの描画だけを担当します。
- **JWT 自動更新**：ログインで受け取った token の期限切れ前に自動で `X-Authorization` を付与し、バックエンドが黙って再発行します。一度ログインすれば長期間そのまま使えます。
- **i18n 完全性ゲート**：`pnpm run lint:i18n` が 5 言語のキー完全性をチェックします。1 つでも欠ければ即 fail です。

## セキュリティベースライン

「推奨」ではなく、**デフォルトで有効になる**強い制約です。

| 脅威 | 対策 |
|---|---|
| 弱いパスワード | プロジェクト全体で **Argon2id** に統一（C214 システムユーザー / C215 プレイヤー）。旧 PBKDF2 は `AppSecretHasher` のみ保持（高エントロピー乱数キーのユースケースで、Argon2id は不要） |
| SQL インジェクション | SqlSugar の完全パラメータ化。文字列連結なし |
| 権限超過アクセス | ソフトデリートフィルタ + ロール権限 / サーバー区スコープ。SuperAdmin は権限チェックをバイパスするが Hub API は SuperAdmin を拒否 |
| リプレイ攻撃 | HMAC 署名 + nonce キャッシュ + 5 分の時刻ずれ許容 |
| 情報漏えい | Response DTO には必ず Id/RowVersion/CreatedTime を含める。機密フィールド（パスワードハッシュ / AccessSecret）のあらゆる Response への出現は**禁止**。契約テストで担保 |
| API 変更の回帰 | `ThreeHostApiRoutingContractTests` + `AdminEndpointReturnContractTests` + `AdminActionStyleContractTests` + `NamingAbbreviationGuardTests` で回帰を防止 |

## データベース

**デフォルトはゼロ設定**：ローカルでソースを直接実行する場合はデフォルトで SQLite（リポジトリルートの `db/` 配下）。本番では PostgreSQL（MySQL / Oracle など合計 34 種の DbType もサポート）。`GameFrameX.Application/Configuration/Database.json` が接続文字列の唯一の情報源です（3 ホストで共有、1 箇所変更すれば 3 ホストに反映）。

- 単一メイン DB モデル：3 ホストが 1 つのメイン DB を共有し、エンティティの帰属は境界ルールで決まります（前述の「アイデンティティとスコープ」を参照）。
- 2.0 は完全新規導入のみ想定：旧バージョンのデータは存在せず、旧 DB のアップグレード、移行スクリプト、互換レイヤーも提供しません。新規 DB の初期化のみサポートします。
- DB 切り替え時の接続文字列の書き方（34 種 DbType のチートシート、バックエンド README のデータベース・セクション内で折りたたみ）とペアの環境変数オーバーライド（`DBTYPE` + `CONNECTIONSTRING`）などの詳細は [バックエンド README · データベース](GameFrameX.Admin.Unified.Api/README.ja.md#データベースデフォルトはゼロ設定本番は-postgresql) を参照してください。

> SQLite はローカル開発専用です。本番での使用は禁止。並行書き込みが弱く、独立したサービスプロセスがなく、バックアップ・復旧や運用監視の能力が不足しているためです。

## デモと始め方

| リソース | リンク |
|---|---|
| デモサイト（**自分の本番環境には使わないでください**） | <https://game.admin.web.vue.alianblank.com> |
| ドキュメントサイト | <https://gameframex.doc.alianblank.com> |
| バックエンド運用マニュアル | [`GameFrameX.Admin.Unified.Api/README.ja.md`](GameFrameX.Admin.Unified.Api/README.ja.md) |
| フロントエンド運用マニュアル | [`GameFrameX.Admin.Unified.Vue/README.ja.md`](GameFrameX.Admin.Unified.Vue/README.ja.md) |

## プロジェクトリポジトリ

| プロジェクト | 用途 | リポジトリ |
|---|---|---|
| 本リポジトリ | Admin / Client / Hub の 3 ホスト単一リポジトリ（Cron 化） | [GameFrameX/Admin](https://github.com/GameFrameX/Admin) |
| クライアント API（独立リポジトリ） | 早期の分割。現在は本リポジトリに統合 | [GameFrameX.Admin.Client.Api](https://github.com/GameFrameX/GameFrameX.Admin.Client.Api) |
| 管理画面 API（有償ソースコード） | 旧命名 | [GameFrameX.Admin.Web.Api](https://github.com/GameFrameX/GameFrameX.Admin.Web.Api) |
| 管理画面 Web（有償ソースコード） | 旧命名 | [GameFrameX.Admin.Web.Vue](https://github.com/AlianBlank/GameFrameX.Admin.Web.Vue) |

## リリースとデプロイ

```bash
docker compose up -d --build
# PostgreSQL + Admin(20802) + Client(20808) + Hub(20806) + フロントエンド(20800) を内蔵。起動後 healthcheck が緑になるまで待つ（フロントコンテナと dev server が 20800 を取り合うため排他）
```

本番では Admin 独立スタック（[`docker-compose.admin.yml`](docker-compose.admin.yml)）も利用できます。専用 PostgreSQL + 3 API ホスト、healthcheck による起動順序固定。詳しくは [Admin 独立スタック](#admin-独立スタックdocker-composeadminyml) を参照してください。

単一コンテナの `docker run`、環境変数のペアオーバーライド（片側だけのオーバーライドは起動時に即エラー）などの詳細は [バックエンド README · デプロイ](GameFrameX.Admin.Unified.Api/README.ja.md#デプロイ) を参照してください。

## 開発とコントリビューション

新しい CRUD サービスを追加したいですか？[`GameFrameX.Admin.Unified.Api/README.ja.md` → 新しい CRUD サービスを追加](GameFrameX.Admin.Unified.Api/README.ja.md) を参照してください。Admin API は `IDynamicApiController` を実装するだけで REST エンドポイントが自動で揃います（Page / Add / Update / Get / List / Delete の 6 つ）。Controller を手書きする必要はありません。

CI ゲート（回帰防止）：

- `ThreeHostApiRoutingContractTests` — 3 ホストのルーティング / HTTP 動詞 / パラメータバインディング / エラーコードの意味領域
- `AdminEndpointReturnContractTests` — Response DTO 契約
- `AdminActionStyleContractTests` — Admin action の記法規約
- `NamingAbbreviationGuardTests` + `NamingAbbreviationBaseline.baseline.txt` — システム全体で業務略語を禁止

## 機能プレビュー

### プラットフォーム管理（SuperAdmin）

![ログインページ](doc/img/1.png)
![メインコンソール](doc/img/2.png)
![メニュー管理](doc/img/3.png)
![パラメータ設定](doc/img/4.png)
![辞書管理](doc/img/5.png)
![タスクスケジューラ](doc/img/6.png)
![キャッシュ管理](doc/img/8.png)
![オープン API](doc/img/11.png)
![CAPTCHA 設定](doc/img/12.png)
![テーブル管理](doc/img/14.png)

### アプリワークベンチのコア機能（中国語・英語対照）

ログイン後に表示される 5 つのコア運用ページ。同じページが中国語 / 英語のインターフェース言語でどう表示されるか（右上の言語スイッチャーでリアルタイムに切替、リロード不要）：

| 機能 | 簡体中文 | English |
|------|----------|---------|
| リモート設定 | ![リモート設定-中国語](doc/img/15.png) | ![Remote Config-EN](doc/img/16.png) |
| バージョン更新 | ![バージョン更新-中国語](doc/img/17.png) | ![Version Update-EN](doc/img/18.png) |
| パッケージ管理 | ![パッケージ管理-中国語](doc/img/19.png) | ![Package Management-EN](doc/img/20.png) |
| 決済設定 | ![決済設定-中国語](doc/img/21.png) | ![Payment Config-EN](doc/img/22.png) |
| 広告コールバック検証 | ![広告コールバック検証-中国語](doc/img/23.png) | ![Ad Callback-EN](doc/img/24.png) |

## エコシステム構成

GameFrameX は以下のリポジトリで構成されます。ビルド時のツールチェーンがプロトコルと設定のコードを生成し、実行時はゲームクライアント・ゲームサーバー・運営バックエンドが連携します。

```mermaid
%%{init: {"flowchart": {"curve": "linear", "nodeSpacing": 30, "rankSpacing": 50}}}%%
flowchart TB
    subgraph Toolchain["Build-time toolchain"]
        direction TB
        Luban["luban<br/>Luban fork"] -- "Luban.dll · daily sync" --> Config["Config<br/>Excel tables"]
        Tools["Tools<br/>ProtoExport"] -- "ProtoExport.dll · weekly sync" --> Protobuf["Protobuf<br/>proto3 definitions"]
        FairyGUI["FairyGUIProject<br/>UI source"]
    end
    subgraph Runtime["Runtime"]
        direction TB
        subgraph Clients["Game clients"]
            direction TB
            Unity["Unity<br/>Unity 2019.4.40+ · C#"]
            Godot["Godot<br/>Godot 4.7 · C#"]
            Cocos["CocosCreator<br/>Cocos 3.8 · TS"]
            Laya["LayaBox<br/>LayaAir · TS"]
        end
        subgraph Backend["Backend"]
            direction TB
            Admin["Admin<br/>Admin · Client · Hub API"]
            subgraph GameServers["Game servers"]
                direction TB
                Server["Server<br/>NuGet framework"]
                ServerSource["Server.Source<br/>source framework"]
            end
        end
    end
    subgraph Infrastructure["Infrastructure"]
        direction TB
        subgraph Storage["Storage"]
            direction TB
            Mongo[("MongoDB<br/>game servers")]
            Postgres[("PostgreSQL<br/>Admin")]
        end
        subgraph Libraries["Base libraries · NuGet"]
            direction TB
            Foundation["Foundation<br/>.NET base libraries"]
            SuperSocket["SuperSocket<br/>socket framework"]
        end
    end
    Toolchain -- "generated code · config data · UI packages" --> Runtime
    Clients -- "TCP / WebSocket / HTTP" --> Backend
    Admin -- "HTTP management API" --> GameServers
    Runtime -- "data storage · NuGet packages" --> Infrastructure
```

### コアプロジェクト

| プロジェクト | 役割 | 技術スタック | 成果物と連携 |
|------|------|------|------|
| [Unity](https://github.com/GameFrameX/GameFrameX.Unity) | Unity クライアントプロジェクト：GameFrameX UPM レジストリから 90+ のフレームワークパッケージを必要に応じて導入、HybridCLR によるホットアップデート、ログイン・バッグ・ルームのサンプル付き | Unity 2019.4.40+, HybridCLR, YooAsset, FairyGUI, UniTask | Protobuf / Config / FairyGUIProject の成果物を利用；TCP 29100 + HTTP 28080 で Server、HTTP 20808 で Admin Client API に接続 |
| [Server](https://github.com/GameFrameX/GameFrameX.Server) | Actor モデルのゲームサーバー：永続化状態の Apps 層とホットアップデート可能な Hotfix ロジック層を厳密に分離、単一／マルチプロセス構成に対応 | .NET 10, MongoDB, Docker | TCP 29100 / WebSocket 29110 / HTTP 28080 で待ち受け；Admin 向けにメール・アイテム付与・プレイヤー管理などの HTTP 管理 API を提供 |
| [Admin](https://github.com/GameFrameX/GameFrameX.Admin) | 単一ゲーム運営バックエンド：Admin（20802）/ Client（20808）/ Hub（20806）の 3 ホスト + Vue 管理画面。サーバー区、お知らせ、メール、注文・決済、リモート設定、ライブ運営をカバー | .NET 10, SqlSugar, PostgreSQL, Vue 3.5, PrimeVue | サーバー区ごとの `HttpManageUrl` で Server を呼び出し；Client API がゲームクライアントにグローバル情報・ログイン・決済コールバックを提供 |
| [luban](https://github.com/GameFrameX/luban) | Luban 設定ツールの GameFrameX フォーク：公式 v5.1.0 をマージし、`gameframex` テーブルインポーターと `cs-dotnet-json` / `cs-dotnet-bin` などの生成ターゲットを追加 | .NET 10, Scriban | Luban の tar.gz と Docker イメージを公開；毎日 Config の `Tools/` に同期 |
| [Config](https://github.com/GameFrameX/GameFrameX.Config) | 設定テーブルリポジトリ：Excel データ表 + スキーマ + ローカライズ表。スクリプト一つでクライアント・サーバーのコードとデータを生成 | Excel, Luban | `gen-client-*` → Unity; `gen-server-json` → Server; `gen-server-bin` → Server.Source; `gen-godot-json` / `gen-cocos-json` / `gen-laya-json` → Godot / CocosCreator / LayaBox |
| [Protobuf](https://github.com/GameFrameX/GameFrameX.Protobuf) | 統一ネットワークプロトコル定義：proto3。ファイル名の数値モジュール ID ごとにメッセージとエラーコードを整理（正数＝クライアント↔サーバー、負数＝サーバー内部） | proto3 | `Proto2CsExport_Server` → Server; `Proto2CsExport_Client` → Unity; `Proto2CsExport_Godot` / `Proto2TsExport_CocosCreator` / `Proto2TsExport_LayaBox` → 他のクライアント；push ごとに CI が 6 言語の Release を公開 |
| [Tools](https://github.com/GameFrameX/GameFrameX.Tools) | ProtoExport プロトコルコード生成ツール：C# / TypeScript / C++ / Lua / Go の 5 言語、CLI・Avalonia GUI・Docker の 3 形態 | .NET 10, Avalonia 12 | `ProtoExport.dll` を毎週 Protobuf の `Tools/` に同期；Docker イメージ `gameframex/gameframex-tools` |

### 周辺システム

| プロジェクト | 役割 | 技術スタック | 成果物と連携 |
|------|------|------|------|
| [Foundation](https://github.com/GameFrameX/GameFrameX.Foundation) | モジュール化された .NET ライブラリ群：ローカライズ、JSON、コマンドライン引数、ログ、HTTP、暗号化、ハッシュ、ORM 基底エンティティ、冪等性 | .NET | NuGet パッケージとして Server・Server.Source・Admin・Tools が参照 |
| [SuperSocket](https://github.com/GameFrameX/GameFrameX.SuperSocket) | SuperSocket の GameFrameX フォーク：.NET 10 ターゲット、依存性注入、弱いネットワーク向け KCP / ReliableSession | .NET 10 | NuGet：Server（ClientEngine）、Server.Source（Server / Kcp / Udp / WebSocket） |
| [Server.Source](https://github.com/GameFrameX/GameFrameX.Server.Source) | フレームワーク層をリポジトリ内ソースとして持つサーバー版：サービスディスカバリ、プロセス間 RemoteMessaging（TCP / KCP）、マルチプロセス構成 | .NET 10, MongoDB, Docker | Config `gen-server-bin` の出力先；`docker-compose.multi.yml` でマルチプロセス起動 |
| [Godot](https://github.com/GameFrameX/GameFrameX.Godot) | Godot .NET クライアント：再利用可能なランタイムモジュール、エディタープラグイン、ホットアップデートアセンブリ | Godot 4.7, C# | Config `gen-godot-json`, Protobuf `Proto2CsExport_Godot`；TCP 29100 / WebSocket 29110 で Server に接続 |
| [CocosCreator](https://github.com/GameFrameX/GameFrameX.CocosCreator) | Unity 版をベースに移植した Cocos Creator クライアント | Cocos Creator 3.8, TypeScript | `npm run gen:proto` / `gen:config` が Protobuf / Config のスクリプトを呼び出し；WebSocket で Server に接続 |
| [LayaBox](https://github.com/GameFrameX/GameFrameX.LayaBox) | LayaBox クライアント | LayaAir, TypeScript | Config `gen-laya-json`, Protobuf `Proto2TsExport_LayaBox`；WebSocket で Server に接続 |
| [FairyGUIProject](https://github.com/GameFrameX/GameFrameX.FairyGUIProject) | FairyGUI エディタープロジェクト：全ゲーム UI のデザインソース | FairyGUI Editor | UI パッケージを Unity `Assets/Bundles/UI`、コードを `Assets/Hotfix/UI/FairyGUI/` に出力 |

## ドキュメントとリソース

- プロジェクトドキュメントサイト：<https://gameframex.doc.alianblank.com>
- バックエンド運用マニュアル（詳細）：[`GameFrameX.Admin.Unified.Api/README.ja.md`](GameFrameX.Admin.Unified.Api/README.ja.md)
- フロントエンド運用マニュアル（詳細）：[`GameFrameX.Admin.Unified.Vue/README.ja.md`](GameFrameX.Admin.Unified.Vue/README.ja.md)
- Issue フィードバック：[GitHub Issues](https://github.com/GameFrameX/Admin/issues)
- クライアント / 管理画面 / Web の 3 リポジトリ：[プロジェクトリポジトリ](#プロジェクトリポジトリ) を参照

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

リリース記録は [GitHub Releases](https://github.com/GameFrameX/Admin/releases) をご覧ください。

## ライセンス

詳しくは [LICENSE.md](LICENSE.md) をご参照ください。

<!--
EN: See [LICENSE.md](LICENSE.md) for license information.
zh-CN: 详见 [LICENSE.md](LICENSE.md) 文件。
zh-TW: 詳見 [LICENSE.md](LICENSE.md) 檔案。
ja: 詳しくは [LICENSE.md](LICENSE.md) をご参照ください。
ko: 자세한 내용은 [LICENSE.md](LICENSE.md) 파일을 참조하세요.
-->
