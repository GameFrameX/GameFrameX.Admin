<div align="center">

<img src="https://download.alianblank.com/gameframex/gameframex_logo_320.png" alt="GameFrameX Logo" width="160" />

# GameFrameX Admin Unified API

[![License](https://img.shields.io/badge/license-blue.svg)](./LICENSE.md)
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

これは **GameFrameX ゲーム管理プラットフォーム**のバックエンドサービスで、.NET 10 + SqlSugar で書かれています。フロントエンド担当がブラウザでクリックするすべてのボタン、ゲームクライアントが送るすべてのリクエスト、最終的に請け負うのはここです。

> 対応フロントエンドは [`GameFrameX.Admin.Unified.Vue`](../GameFrameX.Admin.Unified.Vue)。データベースはローカルではデフォルトで SQLite のゼロ設定、本番では PostgreSQL。このリポジトリは「API を出す、データを保存する、定期ジョブを回す」だけを担います。

## 目次

- [プロジェクト概要](#プロジェクト概要)
- [技術スタック](#技術スタック)
- [3つのホスト、それぞれ独立](#3つのホストそれぞれ独立)
- [リポジトリのディレクトリ構成](#リポジトリのディレクトリ構成)
- [クイックスタート](#クイックスタート)
- [どうやって動いているのか](#どうやって動いているのか)
- [機能モジュール一覧](#機能モジュール一覧)
- [新しい CRUD サービスを追加する](#新しい-crud-サービスを追加する)
- [開発規約](#開発規約)
- [よくある質問とハマりポイント](#よくある質問とハマりポイント)
- [デプロイ](#デプロイ)
- [OpenAPI をフロントエンドへエクスポート](#openapi-をフロントエンドへエクスポート)
- [コマンド早見表](#コマンド早見表)
- [最後に](#最後に)
- [ドキュメントとリソース](#ドキュメントとリソース)
- [変更履歴](#変更履歴)

---

## プロジェクト概要

ひとことで言うと：ゲーム運営プラットフォームに必要なすべてのバックエンド API を提供します。運営側のユーザー追加、メニュー変更、注文確認、メール送信、バージョン配信、ログ閲覧、ゲームクライアントのログイン・課金・設定取得、CI ビルドマシンのバージョン書き戻し——これらすべての作業のデータ出口がここに集まっています。

バックエンドには**3 つの独立したホスト**が住んでいて、それぞれ別プロセスで動き、別々のポートを占有し、互いに足を引っ張りません：

- **Admin API**（`GameFrameX.Web.Api`、ポート `20802`）—— 管理画面用の API
- **Client API**（`GameFrameX.Client.Api`、ポート `20808`）—— ゲームクライアント用の API（ログイン、決済、設定取得）
- **Hub API**（`GameFrameX.Hub.Api`、ポート `20806`）—— 外部システム / ゲーム側 / CI ビルド / サードパーティ WebHook 用の集約 API

Client API と Hub API はどちらもより「スリム」です：Client API は Entity + Payment だけを参照し、旧フレームワークには触れません。Hub API は従来型の Controller ルーティングで、リポジトリを直接使います。動的 API の方式は採用していません（後で詳しく説明します）。

**知っておくべき 3 つのこと**：

1. **Service を書けば自動的に API が生える**（Admin API）。`IDynamicApiController` を実装すると、クラス内の public メソッドが自動的に REST エンドポイントになります。Controller を手書きしたり `[Route]` を手で貼ったりする必要はありません。これが Admin API の魂です。
2. **エンティティを書けば自動的にテーブルができる**。SqlSugar Code-First：起動時にエンティティクラスをスキャン → テーブル生成 → シードデータ投入。CREATE TABLE の SQL を 1 行も書かなくてよく、フィールドを変えたら再起動するだけでテーブル構造が追従します。
3. **3 ホストは単一のメイン DB を共有**。リポジトリは `ConfigId=main` を使用し、ログインと業務データにマルチ DB の軸はありません。ソフトデリートと作成者ベースの権限は維持します。

---

## 技術スタック

| カテゴリ | 技術選定 | かみ砕きメモ |
|------|------|----------|
| ランタイム | .NET 10 | このコード群を動かすためのエンジン（`Directory.Build.props` で `net10.0`） |
| 言語バージョン | C# 10 | `LangVersion=10`。record、file-scoped namespace などが使える |
| フレームワーク | ASP.NET Core ネイティブホスティング | 追加フレームワークなし、純正の一式だけで構成 |
| ORM | SqlSugar (Code-First) | C# エンティティクラスを書くとデータベーステーブルを生成してくれる |
| 認証 | JWT Bearer + HMAC Signature | Admin/Client/Hub は同じ JWT を共有。オープン API は別途署名を使用 |
| リアルタイム通信 | SignalR（任意で Redis Backplane） | サーバーからブラウザへ能動的にメッセージをプッシュする用途 |
| キャッシュ | Memory / Redis | 単一マシンならメモリで十分、複数台なら Redis |
| 定期ジョブ | プロジェクト独自 Scheduler | ジョブは DB に保存、マルチインスタンスで協調し、重複実行なし |
| ログ | データベース + ElasticSearch + Serilog | 操作ログは DB へ、大容量は ES へ |
| データベース | SQLite（ローカルデフォルト）/ PostgreSQL（本番）、MySQL / Oracle など 10 数種にも対応 | DB 切り替えの接続文字列の書き方はDB 切り替えの接続文字列の書き方は[データベース・セクションの早見表](#技術スタック)を参照を参照 |
| コンテナ化 | Docker Compose | コマンド 1 つで 3 つのホストをすべて起動 |

---

## 3つのホスト、それぞれ独立

```
Admin API（20802） Client API（20808） Hub API（20806）
    │ │ │
    │ 管理画面用 │ ゲームクライアント用 │ 外部システム / ゲーム側 / CI 用
    │ ログイン / ユーザー / メニュー / │ ログイン / 決済 / 設定取得 │ サーバー / リソースバージョン /
    │ 注文 / ログ / 定期ジョブ │ │ ビルド報告 / 機能スイッチ同期
    │ │ │
    └──────────── 共有 Entity ──────────────┬─────────────────────┘
                                           │
                                           ├── 共有 Payment（Client のみ使用）
                                           └── 3者それぞれ直接 SqlSugar リポジトリを使用
```

依存関係はこんな形です（左から右が参照方向）：

```
Web.Api → Web.Core → Application.{System, Game, Client, Business} → Core → Entity → Core.Enum
Client.Api（独立、Entity + Payment のみ参照）
Hub.Api（独立、Application.Game + Core + Entity を参照するが、Controller は Application Service を呼ばない）
```

肝心なのはこれ：**ビジネス層はホストを逆参照しない**。`Application.*` と `Core.*` は `Web.Api` の存在を一切知らず、依存は下向きにしか流れません。だから Client API / Hub API は Entity だけをきれいに切り出して使えるのです。

### なぜ Hub API は Admin API と違う作りなのか

Admin API は「動的 API」——Service を書けば自動的に API が生えます。Hub API は「従来型 Controller」——`[Route("api/[controller]/[action]")]` のルーティングを手書きし、Controller に `Repository<T>` を直接注入してエンティティを操作し、**Application Service は呼びません**。これは Hub.Api の意図的な規約です：外部システム向けで、ロジックは単純・軽量・高速であるべきなので、Controller が自ら重複チェックと所属検証を行い、Admin のレイヤー分けを経由しないのです。

詳しい違いは [どうやって動いているのか](#どうやって動いているのか) の [Hub API：第三の扉](#hub-api第三の扉) を参照してください。

---

## リポジトリのディレクトリ構成

```
GameFrameX.Admin.Unified.Api/
├── GameFrameX.Web.Api/ # Admin API ホスト（起動エントリ、compose サービス名 web-api）
├── GameFrameX.Client.Api/ # Client API ホスト（独立した WebApplication）
├── GameFrameX.Hub.Api/ # Hub API ホスト（独立した WebApplication、従来型 Controller ルーティング）
├── GameFrameX.Web.Core/ # Admin の起動ロジック、ミドルウェア、DI 設定（要注目）
├── GameFrameX.Application/ # アプリケーション層（設定ファイル、シードデータ）
├── GameFrameX.Application.System/ # システム管理サービス（ユーザー/ロール/メニュー/ログ…）
├── GameFrameX.Application.Game/ # ゲーム管理サービス（サーバー/注文/メール/引換コード…）
├── GameFrameX.Application.Client/ # クライアント管理サービス（バージョン/チャネル/リソースパッケージ…）
├── GameFrameX.Application.Business/ # 業務サービス（デバイス / デバイスグループ）
├── GameFrameX.Entity/ # エンティティ層（Base / System / Game / Client）
├── GameFrameX.Core/ # コア層（リポジトリ、サービス基底クラス、認証、SignalR）
├── GameFrameX.Core.Enum/ # 列挙型定義
├── GameFrameX.Core.Extension/ # 拡張メソッドと共通 DTO（要注目、ここに車輪を探しに来ることが多い）
├── GameFrameX.Core.SqlSugar/ # SqlSugar 設定、AOP、インターセプター
├── GameFrameX.Core.Utility/ # ユーティリティクラス
├── GameFrameX.Core.BaseInputOutput/ # 基礎入出力モデル
├── Payment/ # 決済連携（Client API のみ参照）
│ ├── GameFrameX.Payment.Abstract/ # 決済抽象インターフェース（先に定義、後に実装）
│ ├── GameFrameX.Payment.AliPay/ # Alipay
│ ├── GameFrameX.Payment.Apple/ # Apple IAP（アプリ内課金検証）
│ ├── GameFrameX.Payment.Apple.AppStoreServer/ # App Store Server API v2
│ ├── GameFrameX.Payment.Google/ # Google Play
│ └── GameFrameX.Payment.Xsolla/ # Xsolla
├── Dockerfile.web-api # Admin API の Dockerfile（sdk:10.0 → aspnet:10.0）
├── Dockerfile.client-api # Client API の Dockerfile
├── Dockerfile.hub-api # Hub API の Dockerfile
└── （統一オーケストレーションはリポジトリルートの docker-compose.yml へ移動済み：PostgreSQL + 3 API + フロントエンドの計 5 サービス）
```

普段のコード変更は 99%、`Application.*` と `Entity` の中で行われます。`Core.*` は地盤なので、触る前に誰が依存しているか把握しておくのが吉です。

---

## クイックスタート

### 動作環境

- .NET 10 SDK
- Visual Studio 2022 / Rider / VS Code、使いやすいものをどうぞ
- データベースの事前インストールは不要：ローカルでソースを直接実行する場合、デフォルトで SQLite を使います（ファイルはリポジトリルートの `db/` ディレクトリに置かれる）。PostgreSQL に接続するときにインストールするか、下の Docker Compose（PG 内蔵）をそのまま使ってください

### データベース：デフォルトはゼロ設定、本番は PostgreSQL

**ローカルでソースを直接実行（デフォルト SQLite）**：何も設定しなくて OK です。デフォルト接続は `GameFrameX.Application/Configuration/Database.json` にあり、3 つの host がリポジトリルート `db/` ディレクトリ下の同じ SQLite ライブラリファイルを共有します（WAL モード）。**リポジトリのルートディレクトリで `dotnet run` を実行する**のが決まりごとです（接続文字列はプロセスの作業ディレクトリ基準で解決されます）。

注意：SQLite はローカル開発・デバッグ専用です。本番環境での SQLite 使用は禁止です（並行書き込みが弱い、独立したサービスプロセスがない、バックアップ・復旧や運用監視の能力が不足）。PostgreSQL を使ってください。

**本番 / コンテナ（PostgreSQL、環境変数でペア上書き）**：データベースを上書きする場合、タイプと接続文字列は必ず**ペアで**設定します。片方だけの上書きは起動時に即エラーになります。3 つの host（Web / Client / Hub）は**同じペアの変数**を使います：

- `DBTYPE=PostgreSQL` + `CONNECTIONSTRING=Host=...`
- 3 ホストは `ConfigId=main` のみを使用します。単一メイン DB がすべての DB であり、他の DB の自動作成・導出は行いません。
- DB 接続の注入は統一（低→高）：`Database.json` フォールバック &lt; 明示的な `DbConnection` セクション &lt; 起動引数（`LauncherOptions` の単一入口：`--DbType PostgreSQL --ConnectionString "..."` と `DBTYPE/CONNECTIONSTRING` 環境変数が同じ入口、コマンドライン優先、必ずペアで）。`DbConnectionOptions` オプションクラスは廃止され、組み立てと検証は `LauncherOptions` + `MainDatabaseConnectionGuard` が直接担当します。
<details>
<summary>全 34 種 DbType の接続文字列チートシート（SqlSugar 公式ドキュメントから抜粋）</summary>

> 出典：SqlSugar 公式ドキュメント donet5.com「データベース特色」（2026-08-21 抜粋）。SqlSugarCore 5.1.4.216 の `DbType` 列挙 34 値すべてを網羅。例のホスト / DB 名 / アカウント / パスワードは公式のデモ値です。DB 切り替え時は該当する値を `Database.json` の `DbType` / `ConnectionString` フィールドに反映してください。

| DbType | 连接串示例 | 备注 |
|---|---|---|
| MySql | `server=localhost;Database=SqlSugar4xTest;Uid=root;Pwd=haosql` | 可选 `Port=3306`、`Charset=utf8mb4` |
| SqlServer | `server=.;uid=sa;pwd=haosql;database=SQLSUGAR4XTEST` | |
| Sqlite | `DataSource=SqlSugar4xTest.sqlite` | 或绝对路径 `DataSource=c:\database\SqlSugar4xTest.sqlite` |
| Oracle | `Data Source=localhost/orcl;User ID=system;Password=haha` | |
| PostgreSQL | `PORT=5432;DATABASE=SqlSugar4xTest;HOST=localhost;PASSWORD=haosql;USER ID=postgres` | 多架构可加 `searchpath=架构名` |
| Dm（达梦） | `Server=localhost;User Id=SYSDBA;PWD=SYSDBA;DATABASE=新DB;SCHEMA=myshcema` | |
| Kdbndp（人大金仓） | `Server=127.0.0.1;Port=54321;UID=SYSTEM;PWD=system;database=SQLSUGAR4XTEST1` | |
| Oscar（神通） | `PORT=2003;DATABASE=osrdb;HOST=localhost;PASSWORD=szoscar55;USER ID=SYSDBA` | |
| MySqlConnector | 连接串同 MySql | .NET Core 高版本直接用 DbType=MySql，仅 .NET Framework 需此值 |
| Access | `Provider=Microsoft.ACE.OleDB.15.0;Data Source=test.accdb` | |
| OpenGauss | `PORT=5432;DATABASE=SqlSugar5Demo;HOST=localhost;PASSWORD=postgres;USER ID=postgres` | |
| QuestDB | `host=localhost;port=8812;username=admin;password=quest;database=qdb;ServerCompatibilityMode=NoTypeLoading;` | |
| HG（瀚高） | `Server=27.151.1;Port=5866;UId=design;Password=123;Database=design;searchpath=design` | |
| ClickHouse | `host=localhost;port=8123;user=default;password=;database=default` | |
| GBase（8s） | `Driver={GBase ODBC DRIVER (64-Bit)};Host=localhost;Service=19088;Server=gbase01;Database=testdb;Protocol=onsoctcp;` | ODBC 驱动 |
| Odbc | `Driver={驱动名};....` | 键值对按目标库 ODBC 驱动文档填写；需 SqlSugar.OdbcCore 5.1.4.160+ 并启动时注册 `InstanceFactory.CustomAssemblies` |
| OceanBaseForOracle | 参考 Oracle 格式 | OceanBase Oracle 模式；官方页面未给显式串（MySQL 模式用 DbType=OceanBase） |
| TDengine | `Host=localhost;Port=6030;Username=root;Password=taosdata;Database=power` | |
| GaussDB | `PORT=5432;DATABASE=SqlSugar5Demo;HOST=localhost;PASSWORD=postgres;USER ID=postgres` | ODBC 方式 |
| OceanBase | `server=localhost;Database=SqlSugar4xTest;Uid=root;Pwd=haosql` | MySQL 模式 |
| Tidb | `server=localhost;Database=SqlSugar4xTest;Uid=root;Pwd=haosql` | |
| Vastbase（海量） | `PORT=5432;DATABASE=SqlSugar4xTest;HOST=localhost;PASSWORD=haosql;USER ID=postgres;No Reset On Close=true` | |
| PolarDB（阿里） | `server=localhost;Database=SqlSugar4xTest;Uid=root;Pwd=haosql` | MySQL 兼容 |
| Doris | `server=localhost;Database=SqlSugar4xTest;Uid=root;Pwd=haosql;Pooling=false` | |
| Xugu（虚谷） | `IP=111.123.17;DB=HOUSE;User=SYSDBA;PWD=abc;Port=5138;AUTO_COMMIT=on;CHAR_SET=UTF8` | |
| GoldenDB | `server=localhost;Database=SqlSugar4xTest;Uid=root;Pwd=haosql;Pooling=false;` | |
| TDSQLForPGODBC | PostgreSQL 兼容串 | 腾讯 TDSQL PG 模式；官方页面未给显式串；需注册 SqlSugar.TDSQLForPGODBC.TDSQLForPGODBCAdapter |
| TDSQLForOracleODBC | Oracle 兼容串 | 腾讯 TDSQL Oracle 模式；官方页面未给显式串；adapter 注册方式同上 |
| TDSQL | `server=localhost;Database=SqlSugar4xTest;Uid=root;Pwd=haosql` | MySQL 内核 |
| HANA | 官方文档页暂无示例（空壳页，截至 2026-08-21） | |
| DB2 | 官方文档页暂无示例（空壳页，提示先用源码、NuGet 未更新，截至 2026-08-21） | |
| GaussDBNative | 串同 GaussDB 格式 | 原生驱动 |
| DuckDB | `DataSource=train_services.db` | |
| MongoDb | `mongodb://root:123456@222.71.212.3:27017/testDB?authSource=admin` | 或 `host=222.71.212.3;Port=27017;Database=testDB;Username=root;Password=123456;authSource=admin` |
| Custom | 连接串由适配器自定 | 自定义 provider：实现适配器并注册 `InstanceFactory.CustomAssemblies` |

</details>

データベースについて、他にも知っておくべき決まりがいくつかあります：

- **Compose 内蔵 PostgreSQL**。リポジトリルートで `docker compose up` を実行すると `postgres:16-alpine` が一緒に起動します（初回起動時に自動構築、`docker/postgres/init.sql` 参照、データはリポジトリルートの `postgres-data/` bind mount に保存）。ホスト側の既存 postgres コンテナを再利用する場合は、リポジトリルートの `.env` に `POSTGRES_HOST=host.docker.internal` などの変数を設定し、api サービスの `postgres` への `depends_on` を外すだけです。
- **3 つの host は同じデフォルト設定を共有**。Client / Hub は起動時に `Database.json` を自動ロードします（Admin と同一ソース、1 ヵ所変えれば 3 ヵ所に反映）。環境変数での上書き方法は compose と同一です。

### 起動方法 1：Docker Compose ワンコマンド起動（推奨、ゼロ設定）

```bash
# リポジトリルートで実行（Api サブディレクトリではない）
docker compose up -d --build
# 内蔵 PostgreSQL + Admin(20802) + Client(20808) + Hub(20806) + フロントエンド(20800)、起動後 healthcheck が緑になるのを待つ
```

### 起動方法 2：ソース直接実行（ゼロ設定、SQLite）

```bash
# 注意：リポジトリのルートディレクトリで実行。4 つの host が ./db/ の SQLite ライブラリファイルを共有

# Admin API（管理画面）
dotnet run --project GameFrameX.Web.Api
# リッスン http://*:20802、Swagger UI: http://localhost:20802/kapi

# Client API（ゲームクライアント）
dotnet run --project GameFrameX.Client.Api
# リッスン http://*:20808

# Hub API（外部システム / CI / WebHook）
dotnet run --project GameFrameX.Hub.Api
# リッスン http://localhost:20806、Swagger UI: http://localhost:20806/swagger

# Game API（ゲームサーバー側）は Client API に統合済み：GameUser* コントローラーは Client.Api 経由で公開（http://*:20808）
```

Admin API の初回起動時、SqlSugar Code-First が自動で DB とテーブルを構築し、ついでにシードデータ（管理者、ロール、メニュー、辞書など）を流し込みます。空のデータベースに突然テーブルが大量に出てきても慌てないでください。正常な動作です。

### 起動方法 3：単一コンテナ docker run

イメージ内のデフォルト設定は SQLite です（環境変数ゼロでそのまま起動可能、データはコンテナ内 `/app/db/`、volume をマウントすれば永続化できます）。本番での使い方は、ペアの環境変数を注入して PostgreSQL に接続します：

```bash
docker run -p 20802:20802 \
  -e DBTYPE=PostgreSQL \
  -e CONNECTIONSTRING="Host=あなたのPG;Port=5432;Database=gameframex_admin;Username=...;Password=..." \
  あなたのイメージ
```

### 詰まったら？

| 症状 | 最も可能性が高い原因 |
|------|------------|
| 起動時に DB 接続失敗のエラー | `Database.json`（または `DBTYPE/CONNECTIONSTRING` 環境変数）の接続文字列が不正、もしくは PG が起動していない |
| 起動時に「DbType が Sqlite/PostgreSQL だが、接続文字列が…の形式ではない」エラー | 環境変数が片方だけ上書きされている：`DBTYPE` と `CONNECTIONSTRING` は必ずペアで設定すること |
| SQLite の DB ファイルが変な場所にできる | 接続文字列はプロセスの作業ディレクトリ基準で解決される——リポジトリルートに戻って `dotnet run` を実行。プロジェクトディレクトリにできた `db/` は削除すれば OK |
| テーブルが作られない | Code-First スイッチはデフォルトでコード上すべて ON。明示的にオフに設定した覚えがあれば `Database.json` か環境変数を確認 |
| Swagger が開かない | 開発環境以外ではデフォルトで Swagger をマウントしない。下の [OpenAPI のエクスポート](#openapi-をフロントエンドへエクスポート) を参照 |
| シードデータが入らない | シードデータのスイッチがオフになっている。またシードは主キー基準の Upsert なので、シードを変えたら旧レコードを削除しないと再投入されない |
| Hub API の起動時に「JWTSettings 未設定」エラー | Hub は Admin の JWT 設定を再利用するため、`Hub.Api/appsettings.json` に Admin と同じ `JWTSettings` ノードが必要 |
| ポートが占有されている | 20800 / 20802 / 20808 / 20806 を別プロセスに占有されている（フロントエンドのコンテナと dev server が 20800 を取り合う）。`lsof -i :20806` で確認を |

---

## どうやって動いているのか

### 動的 API：Service を書くことがそのまま API 定義になる（Admin API）

Admin API のサービスは `IDynamicApiController` を実装するだけで、フレームワークがクラス内の public メソッドを自動的に REST エンドポイントにします（ルートは camelCase）。Controller を手書きする必要はありません：

```csharp
public class MyService : ITransient, IDynamicApiController
{
    // 自動的に GET /api/my/list になる
    public List<MyEntity> List() => ...;
}
```

ルート生成ルール（必ず覚えてください。フロントエンドは 100% この規則で呼んできます）：

1. クラス名から `Service` / `AppService` / `Controller` の接尾辞を取る（1 回だけ）
2. 先頭を小文字にする
3. デフォルトの接頭辞 `api/` を付ける

| クラス名 | 接尾辞除去後 | 最終ルート接頭辞 |
|------|--------|-------------|
| `GameAppService` | `GameApp` | `/api/gameApp` |
| `SystemUserService` | `SystemUser` | `/api/systemUser` |
| `GameOrderService` | `GameOrder` | `/api/gameOrder` |

メソッド名も同様に後ろに繋がります：`Page()` → `/api/gameApp/page`、`ResetSecret()` → `/api/gameApp/resetSecret`。

> **URL に `Service` 接尾辞が出ることは厳禁**。これは提案ではなく鉄則です——フロントエンドはこの規約で呼び出しを生成しているので、バックエンドのクラス名を変えるとフロントエンドが全部壊れます。

細かい制御が必要なときは、`[HttpPost]`、`[AllowAnonymous]`、`[NonAction]` などの属性を普段どおり使えます。

### 依存性注入：マーカーを付けるだけで登録

`services.AddScoped<>()` を手書きせず、マーカーインターフェースで自動登録します：

| インターフェース | ライフタイム | 使いどころ |
|------|----------|----------|
| `ITransient` | 一時 | resolve のたびに新オブジェクト。ほとんどのサービスはこれ |
| `IScoped` | スコープ | 1 リクエスト内で再利用 |
| `ISingleton` | シングルトン | グローバル唯一。状態を持つものを入れるのは注意 |

インフラコンポーネント（SqlSugar、JWT、SignalR、EventBus など）は `GameFrameX.Web.Core/Startup.cs` で手動登録します。設定パラメータが必要なためです。

### リポジトリと単一メイン DB

`SqlSugarRepository<T>` は常にメイン接続（`ConfigId=main`）を使用し、3 ホストが単一メイン DB を共有します。

### 認証体系：JWT 共通＋署名

**JWT Bearer（3 つのホストで同じ鍵を共有）**

- `JwtHandler` は ASP.NET Core の認可ハンドラとして実装（Admin API）
- 3 者とも同じ `JWTSettings` を読む——だから **Admin のログインで発行した token を Client API と Hub API もそのまま受理します**
- 自動更新：Admin API のレスポンスヘッダ `X-Authorization` に新しい Token が載り、フロントエンドはサイレントに切り替える
- 権限チェック：SuperAdmin はすべてのチェックをスキップ。一般ユーザーはルート名でボタン権限を照合

**HMAC Signature（オープン API 用）**

- `SignatureAuthenticationHandler` が HMAC-SHA256 署名認証を実装
- リクエストヘッダに 4 つ載せる：`accessKey` + `timestamp` + `nonce` + `sign`
- 署名検証の式：`HMACSHA256(secret, "METHOD&URL&accessKey&timestamp&nonce")`
- リプレイ防止：nonce キャッシュ＋時刻ずれチェック（デフォルトで 5 分のずれを許容）

### ソフトデリートと作成者ベースのデータ権限

- ソフトデリートのクエリフィルタは維持します。
- `EntityBasicData` は作成者 `CreatedId` による本人データ権限を扱います。組織ベースの範囲は廃止しました。

### 監査フィールド：自動挿入、手書き不要

SqlSugar AOP が `SqlSugarSetup.SetDbAop()` の中でこの仕事を代行します：

| 操作 | 自動挿入されるフィールド |
|------|-------------|
| Insert | `CreatedTime`、`CreatedId`、`CreatedName` |
| Update | `UpdateTime`、`UpdatedId`、`UpdatedName` |

同時に、データ変更を監査ログ（`SystemLogDiff` / `LogDiff`）に書き込みます。エンティティを作るとき、監査フィールドの値は自分で入れなくてよく、フレームワークが埋めてくれます。

### スノーフレーク ID

分散ユニーク ID は Foundation Snowflake の**コンパクト短 ID レイアウト**で生成します（2026-09-15 に決定凍結、`gfx-doc/decisions/2026-09-15-compact-snowflake-id.md` 参照）：

| レイアウトパラメータ | 値 | 説明 |
|---|---|---|
| epoch | 2026-09-01 | デプロイ紀元。データ投入後は変更不可 |
| WorkerId | 3 ビット（0-7） | インスタンスを区別する唯一の次元 |
| DataCenterId | 0 ビット | 単一データセンター。次元は削除済み |
| シーケンス | 10 ビット | WorkerId ごとに毎ミリ秒 1024 個 |

生成されるのは 13〜14 桁の数値（< 2^53、JS で精度が落ちない）で、Admin ページは主キーをそのまま表示でき、文字列変換が不要です。レイアウトは凍結済み：epoch やビット幅を変える = 過去の Id 空間との重複、すなわちデータ整合性事故です。

マルチインスタンスの衝突防止は **Redis に頼りません**（プロセス内リースはプロセス間で相互排他できないため、その方式は廃止済み）。明示的な設定で相互排他します：同じ DB に書き込む各インスタンス（Web.Api の複数レプリカ / Hub / Client）は `SnowFlake:WorkerId`（環境変数 `SnowFlake__WorkerId`、0-7）が重ならないよう設定する必要があり、範囲外なら起動時に即失敗します——単一データセンターで同一 DB に書くインスタンスは上限 8。デフォルト値は Web.Api=1（appsettings）/ Hub=2 / Client=3（コードのフォールバック）で、単一マシンのオールデフォルト構成なら衝突しません。

SqlSugar の主キー書き戻しによる採番もこの方式に統一されています：`StaticConfig.CustomSnowFlakeFunc` は `SqlSugarSnowflakeFallbackInitializer`（ModuleInitializer）により Core.Extension モジュールのロード時にプロセス単位でインストールされ、どの経路（Hub / Client やテストプロセスを含む）でも SqlSugar 内蔵の 18〜19 桁のロングレイアウトには落ちません。

### グローバルレスポンスラッピング

Admin API の `AdminResultProvider` がすべてのレスポンスに統一の殻を被せます：

```json
{
  "Code": 200,
  "Type": "ok",
  "Message": null,
  "Result": { ... },
  "Extras": null,
  "Time": "2026-01-01T00:00:00"
}
```

フロントエンドはこの構造を前提にしており、`Code === 200` なら通し、それ以外はエラー処理へ。Hub API は独自の `HttpJsonResultData<T>`（`Success` / `Fail` ファクトリメソッド）を使い、構造は似ていますが独立で、`AdminResultProvider` は経由しません。

### SignalR リアルタイム通信

- Hub：`OnlineUserHub`、パス `/hubs/onlineUser`
- 接続は認証済みユーザーを使用します。
- できること：オンラインユーザー一覧、強制ログアウト、メッセージ送信（ユニキャスト / ブロードキャスト / 自分以外 / 特定ユーザー）
- マルチインスタンスデプロイでは任意で Redis Backplane + Data Protection Key の永続化が可能で、メッセージをインスタンス横断でプッシュできる

### Hub API：第三の扉

Hub API（`GameFrameX.Hub.Api`、ポート `20806`）は**外部システム**向けの集約 API で、Admin API・Client API とは立ち位置が異なります。次のような呼び出し元に対応します：

| 呼び出し元 | 用途 | 対応 Controller |
|--------|------|-----------------|
| ゲームサーバー / 運用ツール | コンテナ制御、リソースバージョンの書き戻し | `GameAreaController` / `GameAssetPackageVersionController` |
| CI / ビルドマシン | ビルド完了後にリソースバージョン / パッケージ全体バージョンを書き戻す | `PackageBuildReportController` |
| ゲームサーバー | 機能スイッチ同期 | `FeatureSwitchSyncController` |
| サードパーティプラットフォーム | Hub にコンパイル済み WebHook はありません。退役した Tapd サンプルは `../samples/` にあります | - |

他と違う点がいくつかあるので、コードを書く前に必ず把握してください：

**1. 従来型 Controller ルーティングで、動的 API は使わない。**
基底クラス `BaseController` には `[Route("api/[controller]/[action]")]` が付いており、ルートは手書きで、クラス名からの自動生成には頼りません。だからここの URL は `Service` 接尾辞を自動で取り除き**ません**——`Controller` 名 + `Action` 名でそのまま組み立てます。

**2. Controller は `Repository<T>` を直接使い、Application Service は呼ばない。**
これは Hub.Api の鉄則です。Controller のコンストラクタにリポジトリを注入し、自分で検索・書き込み・重複チェック・所属検証をします。PackageBuildReportController のコメントにはこう直球に書かれています：「Hub.Api Controller の慣例に従う（Repository が Entity を直接操作し、Application Service は呼ばない）。そのため上位層の ScopeValidator も Application.Client の CheckXxxUnique も使わず——Controller が所属と重複チェックを自前で書く」

> つまり：Admin API 向けに作った Validator や Application 層の重複チェックメソッドは、**Hub.Api から見ればデッドコード**です。Hub での再利用は期待しないでください。Hub で検証したければ Controller 内に自分で書きます。

**3. 3 つの認証シーンがあり、それぞれ流れが異なる。**

| シーン | 認証方法 | ユーザーの識別 | 例 |
|------|----------|-----------|------|
| ログインユーザー | JWT（各コントローラーが `[Authorize]` を明示） | 認証済みユーザー | コンテナ制御 / リソースバージョンの書き戻し |
| 匿名 WebHook | - | - | このホストにコンパイル済み WebHook はありません |
| マシン間 | `[AllowAnonymous]` + オープンアクセス HMAC 認証ミドルウェア（マシン経路） | 紐付くユーザー | `PackageBuildReportController` / `FeatureSwitchSyncController` |

マシン証憑の仕組みが検証するのは AccessKey + 署名 + scope で、**JWT ではありません**。だから Controller は `[AllowAnonymous]` で JWT をスキップし、専用ミドルウェアが引き受けます。

**4. メイン DB と認証情報に紐付くユーザー。**
JWT は認証済みユーザーを使用します。マシン経路は AccessKey、HMAC 署名、scope、失効状態、紐付くユーザーを検証し、資格情報はユーザーに直接紐付きます。

**5. 楽観ロックミドルウェアとヘルスチェックを内蔵。**
`OptimisticConcurrencyMiddleware` はパイプラインの先頭にいて、並行制御が必要な書き込み操作を下支えします。ヘルスチェックのエンドポイントは 3 つ：`/health`、`/health/live`、`/health/ready`（compose の liveness probe に使用）。

Hub API のミドルウェアパイプラインの順序（`Program.cs`）：

```
OptimisticConcurrencyMiddleware
  → Swagger（スイッチ制御）
  → CORS
  → Authentication → Authorization
  → オープンアクセス HMAC 認証ミドルウェア（api/PackageBuildReport / /api/FeatureSwitchSync）
  → HealthChecks / MapControllers
```

### 設定ファイル：独立 JSON、必要に応じてロード

Admin API の独立した JSON 設定はすべて `GameFrameX.Application/Configuration/` にあり、起動時に自動スキャンします：

| ファイル | 管轄 |
|------|--------|
| `Database.json` | データベース接続、Code-First スイッチ、シードデータスイッチ |

「引っ越し済み」の設定が 2 つあるので注意。もうあちこち探さないでください：

> プラットフォームの JWT 設定はメイン DB `SystemConfig` の `platform_jwt_settings` に移動済みで、**`JWT.json` は実行時ソースとしてはもう使われません**（Hub API は自分の `appsettings.json` の `JWTSettings` ノードを引き続き読みますが、鍵は Admin と一致させる必要があります）。
>
> `App.json` はすでに引退：リッスンポート、動的 API、CORS、スノーフレーク ID などのホスト・フレームワーク設定は `Web.Entry/appsettings.json` へ。Cryptogram 鍵は環境変数 `GFX_CRYPTOGRAM_*` + 内蔵デフォルトに変更（`CryptogramSecretResolver` 参照）。パスワード強度ポリシーはメイン DB `SystemConfig` の `platform_password_policy_config` へ。サーバー（区服）ごとの HTTP 設定はサーバー単位の DB 設定へ移動しました。

---

## 機能モジュール一覧

> ここにあるサービスは主に Admin API（動的 API）で動きます。Hub API の API 群は独立した従来型 Controller なので、[Hub API：第三の扉](#hub-api第三の扉) を参照してください。

### システム管理

プロジェクト独自の動的 API ルーティングによる完全なバックエンド管理モジュールです。「どんなゲームを作っても必ず必要になる」部分です。

| 機能 | サービスクラス | 説明 |
|------|--------|--------|
| 認証ログイン | `SystemAuthService` | アカウントとパスワードでのログイン、JWT 発行 + 自動更新、CAPTCHA |
| ユーザー管理 | `SystemUserService` | ユーザーの CRUD、パスワードリセット、ユーザー-ロール紐付け |
| ロール管理 | `RoleService` | ロール CRUD、データ範囲制御、ロール-メニュー権限紐付け |
| メニュー権限 | `GlobalMenuService` | ツリー形式メニュー、ボタン単位の権限解決、ルートと API のマッピング |
| 辞書管理 | `GlobalDictTypeService` / `GlobalDictDataService` | 辞書タイプと辞書値の CRUD、状態切替 |
| ファイル管理 | `FileService` | ファイルのアップロード・ダウンロード、OSS オブジェクトストレージ |
| 定期ジョブ | `SystemJobService` | ジョブとトリガーの CRUD、一時停止 / 開始 / 即時実行、クラスタ協調 |
| データベース管理 | `SystemDatabaseService` | テーブル / 列 CRUD、DB・テーブル構造のオンライン管理 |
| オンラインユーザー | `OnlineUserService` | リアルタイムオンライン一覧、強制ログアウト、シングルサインオン |
| ログシステム | `LogOperationService` / `LogExceptionService` / `LogVisitService` / `LogDiffService` | 操作ログ、例外ログ、アクセスログ、データ変更監査、Excel エクスポート対応 |
| キャッシュ管理 | `SystemCacheService` | 統一キャッシュ抽象（Memory / Redis）、キャッシュキー管理 |
| システム設定 | `SystemConfigService` | キー・バリュー設定の CRUD |
| 列挙サービス | `SystemEnumService` | リフレクションでアセンブリ内の列挙型と値を取得 |
| 定数サービス | `SystemConstService` | `[ConstAttribute]` 付きの定数値を取得 |
| オープン API | `SystemOpenAccessService` | AccessKey / Secret 管理、HMAC 署名認証 |

### ゲーム管理

ゲーム運営向けのビジネスモジュール。すべてのサービスは `GenericCrudService` 基底クラスを継承します（service を 1 つ書くと 6 つの API が自動で付く、後述）。

| 機能 | サービスクラス | 説明 |
|------|--------|--------|
| ゲームアカウント | `GameUserEntityService` | メール登録、凍結 / 解除 |
| ゲームキャラクター | `GameRoleEntityService` | キャラクター管理、サンドボックスモード切替、ゲームサーバーからのデータ同期 |
| ゲームサーバー（区服） | `GameAreaEntityService` | サーバー管理、Docker コンテナのライフサイクル（作成 / 起動 / 停止 / 再起動） |
| サーバー分類 | `GameAreaClassifyEntityService` | サーバークラスタの分類 |
| 注文管理 | `GameOrderEntityService` | 決済注文、多軸フィルタ（状態 / キャラクター / サーバー / チャネル / プラットフォーム / 日付） |
| 注文配送 | `GameOrderShipService` | 共有の配送ロジック。ゲームサーバー API を HTTP で呼んでアイテムを送付 |
| ゲーム内メール | `GameMailEntityService` | メール送信、アイテム添付の解析、受信者検証 |
| 引換コード | `GameRedeemCodeEntityService` | 引換コード管理、アイテム添付の解析 |
| 引換コードチャネル | `GameRedeemCodeChannelEntityService` | 引換コードの配布チャネル |
| 引換履歴 | `GameRedeemCodeUseRecordEntityService` | 引換コードの利用履歴 |
| Web ショップ | `GameWebShopEntityService` | Web ショップの商品管理 |
| ショップ注文 | `GameWebShopOrderEntityService` | Web ショップの注文 |
| 商品管理 | `GameCommodityEntityService` | 商品 / 製品管理 |
| アイテム | `GameGoodsEntityService` | ゲーム内のアイテム類 |
| アイテムカタログ | `GameGoodsCatalogueEntityService` | アイテムの分類 |
| 定期イベント | `GameTimeJobEntityService` | ゲーム内の定期タスク / イベント |
| Docker タグ | `GameDockerTagEntityService` | ゲームサーバー Docker イメージのバージョン管理 |
| お知らせ | `GameAnnouncementEntityService` / `GameBannerAnnouncementEntityService` | ゲーム内お知らせ、バナーお知らせ |
| ホーム設定 | `GameHomeEntityService` | ゲームのホーム画面設定 |

### アプリセンター

クライアント App 向けの設定管理モジュールです。

| 機能 | サービスクラス | 説明 |
|------|--------|--------|
| アプリバージョン | `GameAppReleaseVersionService` | App のバージョン管理、強制アップデート制御 |
| リソースパッケージ | `GameAppAssetPackageService` | AssetBundle / リソースパッケージ管理 |
| リソースバージョン | `GameAppAssetPackageVersionService` | リソースパッケージのバージョン追跡 |
| リソース履歴 | `GameAppAssetPackageVersionHistoryService` | リソースバージョンの変更履歴 |
| チャネル定義 | `GlobalChannelDefinitionService` / `ChannelDefinitionService` | 配信チャネル（App Store、Google Play など） |
| デバイス管理 | `DeviceService` | デバイスの登録と識別（`Application.Business` にある） |
| デバイスグループ | `DeviceGroupService` | デバイスのグルーピング（カナリア / A/B テスト、`Application.Business` にある） |
| 多言語 | `GlobalLanguageService` | 対応言語の設定 |
| 分割パッケージ管理 | `GameAppPackageService` | アプリの分割パッケージ、チャネル紐付けとベースパッケージ継承の管理 |
| プラットフォーム管理 | `GlobalPlatformService` | 実行プラットフォーム（iOS、Android など） |
| バージョン定義 | `GameAppVersionDefinitionService` | バージョンとリソースのマッピング |
| クライアント設定 | `GameAppClientConfigService` | クライアントのキー・バリュー設定 |
| 設定キー | `GameAppClientConfigKeyService` | クライアント設定のコンテンツキー |

### Hub API の API 群（独立した一式）

これらは従来型 Controller で、Hub API（20806）上で動き、**動的 API も Application Service も使いません**：

| API | Controller | 認証 | 説明 |
|------|-----------|------|------|
| コンテナ制御 | `GameAreaController` | JWT | サーバーコンテナの起動 / 停止 / 削除、サーバーバージョン登録 |
| リソースバージョンの書き戻し | `GameAssetPackageVersionController` | JWT | 楽観ロック付きリソースバージョン更新 |
| ビルド報告 | `PackageBuildReportController` | マシン証憑 | CI / ビルドマシンがリソースバージョン / パッケージ全体バージョンを書き戻す（冪等 upsert） |
| 機能スイッチ同期 | `FeatureSwitchSyncController` | マシン証憑 | ゲームサーバーに機能スイッチを同期 |

### 決済連携

Client API 専用です。抽象層 + マルチチャネル実装で、新しいチャネルを足すときは抽象層に合わせて実装するだけです：

| チャネル | プロジェクト |
|------|------|
| Alipay | `GameFrameX.Payment.AliPay` |
| Apple IAP | `GameFrameX.Payment.Apple`（アプリ内課金レシート検証） |
| App Store Server | `GameFrameX.Payment.Apple.AppStoreServer`（API v2 通知処理） |
| Google Play | `GameFrameX.Payment.Google` |
| Xsolla | `GameFrameX.Payment.Xsolla` |

---

## 新しい CRUD サービスを追加する

> この節は **Admin API** の動的 API サービス向けです。Hub API は従来型 Controller なのでこの流れには乗りません——Hub の API を足すときは Controller を作ってリポジトリを注入するだけで OK。`GameFrameX.Hub.Api/Controllers/` の既存実装を参考にしてください。

一度なぞってやれば、日常のバックエンド業務の 8 割は把握できます。「ゲームアイテム」を追加すると仮定して進めます。

### 1. エンティティを作成

`GameFrameX.Entity/` の対応するサブディレクトリにエンティティクラスを作成します：

```csharp
[SugarTable(null, "ゲームアイテム")]
[SystemTable]
[IncrementTable]
public class GameItemEntity : EntityBasic
{
    [SugarColumn(ColumnDescription = "アイテム名", Length = 64)]
    public string Name { get; set; }

    [SugarColumn(ColumnDescription = "アイテム数")]
    public int Count { get; set; }
}
```

エンティティの基底クラスは必要に応じて選択：

| 基底クラス | 特徴 | 適用シーン |
|------|------|----------|
| `EntityBaseId` | `Id` のみ | 紐付けテーブル、作成者 / 監査カラムなしのエンティティ |
| `EntityBase` | Id + 監査フィールド + ソフトデリート | メイン DB のエンティティ |
| `EntityBasic` | `EntityBase` を継承 | 汎用ビジネスエンティティ（最頻出） |
| `EntityBasicData` | 作成者ベースのデータ権限 | `CreatedId` でフィルタ |
| `EntityBasicSelect` | Name + Description を追加 | ドロップダウンセレクター用エンティティ |

### 2. シードデータを作成（任意）

`GameFrameX.Application/SeedData/` の対応するサブディレクトリにシードクラスを作成します：

```csharp
public class GameItemSeedData : ISqlSugarEntitySeedData<GameItemEntity>
{
    public IEnumerable<GameItemEntity> HasData()
    {
        return new[]
        {
            new GameItemEntity { Id = YitIdHelper.NextId(), Name = "コイン", Count = 0 }
        };
    }
}
```

シードデータは `StorageableByObject` で主キー基準の InsertOrUpdate が行われるので、何度実行しても安全です。**注意**：主キー基準の Upsert ため、シードの内容を変えても主キーが同じなら旧レコードは上書きされません——新しい値を見るには旧行を削除してから再起動してください。

### 3. Service を書く

`GameFrameX.Application.Game/Service/`（または System / Client / Business）配下にサービスを作成します：

```csharp
[ApiDescriptionSettings(GameConst.GroupName, Order = 100)]
public class GameItemService : GenericCrudService<GameItemEntity, BaseAddInput, BaseUpdateInput,
    GameItemEntity, BasePageInput, ITransient
{
    protected override ISugarQueryable<GameItemEntity> BuildPageQuery(BasePageInput input)
    {
        return Repository.AsQueryable()
            .WhereIF(!string.IsNullOrWhiteSpace(input.SearchKey),
                u => u.Name.Contains(input.SearchKey));
    }
}
```

以上です。自動的に 6 つの API が手に入ります：

| エンドポイント | 説明 |
|------|------|
| `Page` | ページング検索 |
| `Add` | 新規追加 |
| `Update` | 更新 |
| `Get` | Id で 1 件取得 |
| `List` | 一覧（ページングなし） |
| `Delete` | 削除（ソフトデリート） |

ドロップダウンセレクター対応も必要なら、基底クラスを `GenericCrudSelectService` に変えるだけで、`Select` と `CheckNameUnique` の 2 エンドポイントがおまけで付いてきます。

### 4. （必要なら）カスタム DTO

単純なケースでは基底クラスのものをそのまま再利用：`BaseAddInput`、`BaseUpdateInput`（`Id` 付き）、`BaseIdInput`、`BasePageInput`（`SearchKey`、`Page`、`PageSize`、ソートフィールド付き）。

フィールドが足りない場合は、サービスの `Dto/` サブディレクトリか `GameFrameX.Core.Extension/Dto/` に追加します：

```csharp
// 出力 DTO——エンティティを継承するだけ
public class GameItemOutput : GameItemEntity, IBasePageOutput<GameItemEntity> { }

// 更新 DTO——基底クラスを継承してフィールドを追加
public class UpdateGameItemInput : BaseUpdateInput
{
    [Required] public string Name { get; set; }
}
```

### 5. フロントエンドに知らせる

バックエンドの OpenAPI が変わったら、フロントエンドで `openapi.json` を取り直して `pnpm gen:api` を実行する必要があります。さもないとフロントエンドの呼び出し箇所が合わなくなります。具体的な手順は [OpenAPI をフロントエンドへエクスポート](#openapi-をフロントエンドへエクスポート) を参照してください。

---

## 開発規約

> この節は**規約**です——プロジェクト内の全員がこうやっているので、従っておくと楽です。

### Admin API vs Hub API：2 つの流儀を混ぜない

| 観点 | Admin API | Hub API |
|------|-----------|---------|
| ルーティング | 動的 API（`IDynamicApiController`） | 従来型 Controller（`[Route("api/[controller]/[action]")]`） |
| データアクセス | Application Service → リポジトリ経由 | Controller が `Repository<T>` を直接注入 |
| レスポンスラッピング | `AdminResultProvider` が自動でラップ | `HttpJsonResultData<T>` を手動で返す |
| Application 層の再利用 | する | しない（Validator / CheckXxxUnique は Hub ではデッドコード） |
| 新規 API 追加 | Service を書く | Controller 作成 + リポジトリ注入 |

Hub に API を足すとき、Admin 側の層に用意したバリデーションの再利用は**考えないで**ください——Hub は自分で書きます。

### リポジトリ拡張メソッド：作る前にまず探す

`GameFrameX.Core.Extension/RepositoryExtension.cs` に常用のものが揃っています（Hub API の `Repository<T>` でも一部は使えます。たとえば楽観ロック付きの更新）：

| メソッド | 説明 |
|------|--------|
| `FakeDelete` / `FakeDeleteAsync` | ソフトデリート（`IsDeleted = true`） |
| `OrderBuilder(input)` | `BasePageInput` に基づく動的ソート |
| `UpdateWithDiffLog` / `InsertWithDiffLog` | 監査ログ付きの登録・更新・削除 |
| `UpdateWithOptimisticLockAsync` | 楽観ロック付きの更新（Hub API のビルド報告で使用中） |
| `AS<T>()` | クロス DB の検索 / 操作 |
| `RunWithoutFilter` | クエリフィルタを一時的にスキップ（削除済みデータを検索したい場合など） |

### 定期ジョブ

`IJob` インターフェースを実装し、管理画面の「定期ジョブ」ページで設定するか、`[JobDetail]` 属性でマークします：

```csharp
[JobDetail("job_my_task", Description = "私の定期ジョブ")]
public class MyTask : IJob
{
    public async Task ExecuteAsync(JobExecutingContext context, CancellationToken ct)
    {
        // あなたのタスクロジック
    }
}
```

ジョブは 3 種類に対応：内蔵タイプ、スクリプトコンパイル、HTTP 呼び出し。`DbJobPersistence` でデータベースに永続化されるため、マルチインスタンスデプロイでも協調し、複数インスタンスが同じジョブを同時に実行することはありません。

> 注意：注文配送まわりのプロセスロックはマルチインスタンスでは無効になります。この部分は評価中なので、当面はインスタンス横断の相互排他は期待しないでください。

### ミドルウェアパイプライン：リクエストが層ごとに処理される仕組み

Admin API のリクエスト処理の順序はこうなっています（外から内へ）：

```
例外処理 / HSTS
  → 統一エラーステータスコード
  → ローカライズ
  → 静的ファイル
  → ルーティング
  → CORS
  → IP / クライアント レートリミット
  → 認証 / 認可
  → Schedule UI
  → Knife4j (Swagger)
  → Project DI
  → SignalR Hub + MVC ルーティング
```

Hub API のパイプラインは異なります。[Hub API：第三の扉](#hub-api第三の扉) を参照してください。リクエストの問題を切り分けるときは、該当ホストのパイプライン順で「どの層が弾いたのか」を考える方が、やみくもな再起動より効きます。

### C# コードスタイル

このプロジェクトは **C# 10 構文**、**.NET 10 ランタイム**（`Directory.Build.props`：`LangVersion=10` + `net10.0`）で書かれます。record、target-typed new、file-scoped namespace、global using が使えます。ただし鉄則が 1 つ：**すべての制御フロー文には波括弧が必須**、body が 1 行でもです。`using var x = ...;` のような単行 using 宣言も使わず、素直に `using (var x = ...) { }` と書きます。

---

## よくある質問とハマりポイント

### FAQ

**Service を書いたのに API が呼べないのはなぜ？**
3 点をチェック：クラスは `IDynamicApiController` を実装しているか、メソッドは public か、`[NonAction]` が付いていないか。それでもだめなら起動ログを見てください。ルート登録時に出力されます。（これは Admin API 限定の話——Hub API は手書き Controller で、動的ルーティングは存在しません。）

**フロントエンドが呼ぶ URL とクラス名が合わない？**
「Service 接尾辞を除去 + 先頭小文字」のルールを忘れているだけです（Admin API）。`SystemUserService` の URL は `/api/systemUser/...` で、`/api/systemUserService/...` ではありません。フロントエンドの URL に `Service` を付けるのは厳禁。Hub API はこの方式ではなく、URL は Controller 名 + Action 名で組み立てます。

**エンティティを変えたのにテーブル構造が更新されない？**
Code-First はデフォルトではテーブル作成とカラム追加しか行いません。カラム削除や型変更のような破壊的変更は、手動対応か再起動時の注意が必要です。本番でテーブルを変える前に必ずバックアップを。

**SuperAdmin の見えるデータが他の人より多いのはバグ？**
SuperAdmin の権限バイパスは維持します。全ホストがメイン DB を共有し、Hub のマシン経路は専用の認証検証を行います。

**シードデータを変えたのに、再起動しても古いまま？**
シードは主キー基準の Upsert です。主キーが同じなら既存レコードは上書きされません。該当行を削除して再起動するか、主キーを変えてください。

**Hub API が 401 を返すが、同じ token で Admin API は通る？**
Hub は Admin の JWT 鍵を再利用しますが、読むのは自分の `appsettings.json` の `JWTSettings` ノードです。両側の鍵（`IssuerSigningKey` / `ValidIssuer` / `ValidAudience`）は一致が必須。不一致だと発行された token を互いに受理しません。

**ビルド報告（PackageBuildReport）が 401？**
マシン経路はオープンアクセス HMAC 認証ミドルウェアで AccessKey、HMAC 署名、scope、紐付くユーザーを検証します。

### ハマりポイント

**メイン DB のデータが見つからない**
リポジトリは `ConfigId=main` を使用します。データがない場合はメイン接続設定とホスト登録を確認してください。

**監査フィールドが空になる**
監査は SqlSugar AOP が自動挿入します。リポジトリを迂回して `SqlSugarClient` を直接操作すると AOP が走らず、フィールドが空になります。素直に `SqlSugarRepository<T>` を使ってください。（Hub API がリポジトリを直接使うのは OK です。）

**JWT がすぐ切れる / 自動更新が効かない**
レスポンスヘッダに `X-Authorization` があるか確認。なければ `JwtHandler` の更新ロジックと token 有効期限の設定をチェック。プラットフォームの JWT 設定はメイン DB `SystemConfig` の `platform_jwt_settings` に移動済みなので、`JWT.json` はもう触らないでください。

**Hub に API を足したら、バリデーションロジックが Admin と合わない**
正常です。Hub は Admin の Application 層バリデーションを再利用しません。Hub Controller 内に手書きするか、両側にそれぞれバリデーションを書くことを受け入れるか——それが Hub の現時点の明確なトレードオフです。

**定期ジョブがマルチインスタンスで重複実行される**
ジョブはデータベース + クラスタ協調に依存します。`DbJobPersistence` が正常に動いているか、クラスタ協調の設定漏れがないかを確認。注文配送まわりのプロセスロックはマルチインスタンスで実際に無効になります。既知の落とし穴です。

---

## デプロイ

### Docker Compose（推奨）

```bash
cd .. && docker compose up --build # 統一オーケストレーションはリポジトリルート（フロントエンド込みで計 5 サービス）
```

コマンド 1 つで 3 つのホストをすべて起動できます。ポートマッピングはローカルの `dotnet run` と**完全に一致**します：

| compose サービス | コンテナ名 | ポート | 対応ホスト | Dockerfile |
|--------------|--------|------|----------|-----------|
| `web-api` | `gameframex-web-api` | `20802:20802` | Admin API | `Dockerfile.web-api` |
| `client-api` | `gameframex-client-api` | `20808:20808` | Client API | `Dockerfile.client-api` |
| `hub-api` | `gameframex-hub-api` | `20806:20806` | Hub API | `Dockerfile.hub-api` |

3 つの Dockerfile はいずれもマルチステージビルド（`sdk:10.0` でビルド → `aspnet:10.0` で実行）、非 root ユーザーで動き、タイムゾーンは `Asia/Shanghai` に設定されます。設定ファイルは volume マウントなので、**イメージ構築後も設定を変更可能**で、再 build は不要です。3 つのコンテナはホスト側の PostgreSQL を共有し（`host.docker.internal:5432` 経由）、DB コンテナは別途起動しません。

> Swagger は Production コンテナではデフォルトでマウントされません。3 つのホストとも `Swagger__Enabled=true` で明示的に有効化します（compose の環境変数セクション参照）。

> 命名の経緯：compose サービスはかつて `web-entry` という名前（旧 "Web.Entry" を踏襲）でしたが、2026-10-20 に `web-api` へ改名され、GameFrameX.Web.Api と揃えられました。

### 初回デプロイ：超管パスワードと最初の業務管理者

空の DB での初回起動時、web-api はテーブル作成とシード処理の後に「最初の業務管理者シードブートストラップ」を実行します。2 つの初期アカウントはそれぞれ別の環境変数で与え、役割が異なります：

| 環境変数 | 役割 | 有効条件 |
|---|---|---|
| `GFX_SUPERADMIN_INITIAL_PASSWORD` | プラットフォーム超管 `superadmin` の初期パスワード。未設定の場合はパスワードが空のままとなり、ログインページ初回アクセス時に初回セットアップウィザードで設定します | 初回インストール時のシード挿入時のみ |
| `GAMEFRAMEX_ADMIN_INITIAL_ACCOUNT` + `GAMEFRAMEX_ADMIN_INITIAL_PASSWORD` | **最初の業務管理者**のアカウントと初期パスワード。起動時に自動作成され、内蔵 `sys_admin` ロールにバインドされます（超管専用サブツリーを除く全ノン超管メニューを付与）。以後このアカウントで業務管理画面にログインできます | 2 つの変数をペアで設定し、かつメイン DB に業務管理者アカウントがまだ存在しないこと。パスワードはプラットフォームの強度ポリシーを満たす必要があり、満たさない場合は起動が失敗します |

冪等性：アカウントが既に存在する場合は再作成せずパスワードも上書きしません。`sys_admin` のメニュー付与は起動のたびに新規シードメニューの追加のみを行い、既存の付与を削除しません（運営がロール管理で手動で絞り込んでも再起動で元に戻されることはありません）。両方の変数未設定かつ業務管理者が存在しない場合、起動ログに「業務管理者が未初期化」の警告を出力します。ログにパスワードが出力されることはありません。

> パスワード強度：新規インストールではプラットフォームのパスワードポリシーが**既定で無効**（`strongPassword=false`）のため、2 つの初期アカウントのパスワードは強度チェックされません。有効化すると 6–16 文字で大文字・小文字・数字・記号をすべて含むことが必須になります。初期パスワードはこの規則（12 文字以上）で設定し、サービス公開前にポリシーを有効化してください。

### リリース前チェックリスト：DB 再投入とコンパクト短 ID

この節は 2026-09-15 の旧ロング ID から短い ID への移行のみを説明し、データ変換スクリプトはありません。 以降は移行ツール自体を提供しません。2.0 は単一メイン DB の新規初期化のみサポートし、旧 DB はリリース前の完全バックアップによる全体ロールバックでのみ復元できます。

コンパクト短 ID レイアウトのリリース方針は**リセット可能、移行スクリプトなし**（2026-09-15 決定）：新旧の ID レイアウトは非互換で、本番 PG のメイン DB は「全消し再投入」でリリースし、新旧 ID の混在は許されません。リリース前に以下を確認：

1. **全消し再投入**：本番 PG メイン DB を空にし、各 host の起動で再構築（CodeFirst によるテーブル作成 + シード再投入）。旧 DB の 18〜19 桁のロング Id データを新レイアウトへ移行しないこと——新旧 Id 空間の重複はデータ整合性事故です。
2. **WorkerId 相互排他の確認**：同じ DB に書き込むすべてのインスタンス（Web.Api レプリカ / Hub / Client）の `SnowFlake__WorkerId` が互いに異なり 0-7 の範囲内であること。デフォルト値 1/2/3 は単一マシン構成での非衝突しか保証しません。レプリカを増やす前に必ず明示的に割り当ててください。
3. **桁数の抜き取りチェック**：再投入後、業務テーブルの主キーを抜き取り、13〜14 桁の数値（< 2^53）であることを確認。18〜19 桁が見えたら Foundation の採番を通っていない経路があるので、停止して調査してください。

---

## OpenAPI をフロントエンドへエクスポート

フロントエンドの TypeScript API クライアントは、`openapi.json` + `pnpm gen:api` で生成されます。**バックエンドは Swagger JSON エンドポイントを露出させるだけ**で、エクスポート作業はフロントエンドのリポジトリで行います。

| 項目 | 値 | 出所 |
|---|---|---|
| Admin Swagger ドキュメント名 | `web.api` | `GameFrameX.Web.Core/ApiRouting/AdminOpenApiGroups.cs` |
| Admin Swagger JSON | `http://<host>:20802/swagger/web.api/swagger.json` | `AdminOpenApiGroups.WebGroup.RouteTemplate` |
| Client API Swagger JSON | `http://<host>:20808/swagger/client.api/swagger.json` | `GameFrameX.Client.Api/Program.cs` |
| Hub API Swagger JSON | `http://<host>:20806/swagger/v1/swagger.json` | `GameFrameX.Hub.Api/Program.cs`（`SwaggerDoc("v1", ...)`） |
| **デフォルトスイッチ**（3 者共通） | 開発環境では自動 ON。他の環境では `Swagger:Enabled=true`（環境変数 `Swagger__Enabled=true`）が必須 | 各 `Program.cs` / `Startup.cs` |
| **本番 / Docker ではデフォルト非公開** | はい——Swagger ミドルウェア自体がマウントされず、`/swagger/...` エンドポイントが存在しません | 同上 |

> Hub API の Swagger ドキュメント名は `v1` です（`hub.api` ではありません）。だから URL は `/swagger/v1/swagger.json` になります。Admin のフロントエンドは通常 Admin API の `web.api` だけを使い、Hub と Client の swagger は主にゲーム側 / CI 連携時の API 確認用です。

本番で Swagger をマウントしないのは意図的なものです——API ドキュメントをインターネットに丸裸にしないでください。

```bash
# 1. Admin API の Swagger にアクセスできるようにする（どちらか一方）
ASPNETCORE_ENVIRONMENT=Development dotnet run --project GameFrameX.Web.Api &
# または
Swagger__Enabled=true dotnet run --project GameFrameX.Web.Api &

# 2. フロントエンドのリポジトリルートへ取得
curl -sSL http://localhost:20802/swagger/web.api/swagger.json \
  -o ../GameFrameX.Admin.Unified.Vue/openapi.json
ls -lh ../GameFrameX.Admin.Unified.Vue/openapi.json # だいたい 3 MB あるはず

# 3. フロントエンドで生成を実行
( cd ../GameFrameX.Admin.Unified.Vue && pnpm gen:api && git diff src/api/ )
```

> バックエンドの OpenAPI に変更があったら（service エンドポイントの追加 / 変更 / 削除）、必ず再エクスポート + フロントエンドでの `pnpm gen:api` を行ってください。さもないとフロントエンドの呼び出し箇所とバックエンドが一致しません。フロントエンドの `pnpm run quality` 内の `check:routes` はバックエンドのメニューシードでフロントエンドのコンポーネントパスを突き合わせ、不一致があるとエラーになります。

---

## コマンド早見表

| コマンド | 用途 |
|------|--------|
| `dotnet run --project GameFrameX.Web.Api` | Admin API を起動（20802） |
| `dotnet run --project GameFrameX.Client.Api` | Client API を起動（20808） |
| `dotnet run --project GameFrameX.Hub.Api` | Hub API を起動（20806） |
| `dotnet build` | ソリューション全体をビルド |
| `Swagger__Enabled=true dotnet run --project GameFrameX.Web.Api` | 非 dev 環境で起動し Swagger を公開 |
| `ASPNETCORE_ENVIRONMENT=Development dotnet run --project GameFrameX.Web.Api` | dev 環境で起動（Swagger 自動 ON） |
| `cd .. && docker compose up --build` | Docker で一括起動（リポジトリルートの統一オーケストレーション：PG + 3 API + フロントエンド） |
| `curl -sSL http://localhost:20802/swagger/web.api/swagger.json -o ../GameFrameX.Admin.Unified.Vue/openapi.json` | Admin Swagger JSON をフロントエンド用にエクスポート |

よく使うアドレス：

| アドレス | 説明 |
|------|------|
| `http://localhost:20802/kapi` | Admin API の Knife4j / Swagger UI |
| `http://localhost:20802/swagger/web.api/swagger.json` | Admin Swagger JSON（docName=`web.api`） |
| `http://localhost:20806/swagger` | Hub API の Swagger UI |
| `http://localhost:20806/swagger/v1/swagger.json` | Hub Swagger JSON（docName=`v1`） |
| `http://localhost:20806/health` | Hub API のヘルスチェック |
| `http://localhost:20808` | Client API |
| `/hubs/onlineUser` | Admin API の SignalR オンラインユーザー Hub |

---

## 最後に

合言葉：

> **3 つのホストはそれぞれ独立：Admin は Service を書くことがそのまま API、Client はログインと決済、Hub は外部向けの従来型 Controller。エンティティを書けばテーブルができ、URL に Service は付かず、3 ホストは単一のメイン DB を共有。Hub は Admin のレイヤーを再利用せず、バリデーションは自分で書く。OpenAPI を変えたらフロントエンドへの再生成依頼を忘れずに。**

詰まっても粘着しない：まず上のハマりポイントを見て、解決しなければ `gfx-doc/`（プロジェクトルートのドキュメントシステム一式）を漁り、それでもだめなら同僚に聞いてください。

具体的なコードを見たければ、ここへ飛んでください：

- `GameFrameX.Web.Core/Startup.cs` —— Admin の起動設定、DI 登録、ミドルウェアパイプライン
- `GameFrameX.Hub.Api/Program.cs` —— Hub の起動設定、ミドルウェアパイプライン、JWT 再利用
- `GameFrameX.Hub.Api/Controllers/PackageBuildReportController.cs` —— Hub の「リポジトリ直接操作、Application Service は呼ばない」のお手本
- `GameFrameX.Core.SqlSugar/` —— SqlSugar 設定、AOP、単一メイン DB へのアクセス
- `GameFrameX.Core/` —— リポジトリ基底クラス、`GenericCrudService`、認証、SignalR
- `GameFrameX.Core.Extension/RepositoryExtension.cs` —— リポジトリ拡張メソッド（常用の車輪）
- `GameFrameX.Application/SeedData/` —— シードデータ
- `GameFrameX.Application/Configuration/` —— 独立した JSON 設定ファイル

## ドキュメントとリソース

- プロジェクトドキュメントサイト：<https://gameframex.doc.alianblank.com>
- プロジェクト概要：[`../README.ja.md`](../README.ja.md)
- フロントエンド実行マニュアル：[`../GameFrameX.Admin.Unified.Vue/README.ja.md`](../GameFrameX.Admin.Unified.Vue/README.ja.md)
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

リリースノートは [GitHub Releases](https://github.com/GameFrameX/Admin/releases) を参照してください。

## ライセンス

詳しくは [LICENSE.md](LICENSE.md) をご参照ください。

<!--
EN: See [LICENSE.md](LICENSE.md) for license information.
zh-CN: 详见 [LICENSE.md](LICENSE.md) 文件。
zh-TW: 詳見 [LICENSE.md](LICENSE.md) 檔案。
ja: 詳しくは [LICENSE.md](LICENSE.md) をご参照ください。
ko: 자세한 내용은 [LICENSE.md](LICENSE.md) 파일을 참조하세요.
-->
