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

인디 게임 개발자를 위한 올인원 솔루션 · 인디 개발자의 꿈을 실현

<br />

[문서](https://gameframex.doc.alianblank.com) · [빠른 시작](#빠른-시작) · QQ 그룹: 467608841 / 233840761
<br />

[English](README.md) | [简体中文](README.zh-CN.md) | [繁體中文](README.zh-TW.md) | [日本語](README.ja.md) | **한국어**

</div>

---

**GameFrameX 게임 관리 플랫폼**의 백엔드 서비스로, .NET 10 + SqlSugar로 작성되었습니다. 프런트엔드 담당자가 브라우저에서 클릭하는 모든 버튼, 게임 클라이언트가 보내는 모든 요청은 최종적으로 여기서 처리됩니다.

> 프런트엔드는 [`GameFrameX.Admin.Unified.Vue`](../GameFrameX.Admin.Unified.Vue)를 함께 사용합니다. 데이터베이스는 로컬에서 기본적으로 SQLite 무설정, 프로덕션에서는 PostgreSQL을 사용합니다. 이 리포지토리는 "API 제공, 데이터 저장, 정기 작업 실행"만 담당합니다.

## 목차

- [프로젝트 개요](#프로젝트-개요)
- [기술 스택](#기술-스택)
- [세 개의 호스트, 각자의 역할](#세-개의-호스트-각자의-역할)
- [리포지토리 디렉터리 구조](#리포지토리-디렉터리-구조)
- [빠른 시작](#빠른-시작)
- [어떻게 실행되는가](#어떻게-실행되는가)
- [기능 모듈 한눈에 보기](#기능-모듈-한눈에-보기)
- [새 CRUD 서비스 추가하기](#새-crud-서비스-추가하기)
- [개발 규칙](#개발-규칙)
- [자주 묻는 질문과 함정](#자주-묻는-질문과-함정)
- [배포](#배포)
- [프런트엔드용 OpenAPI 내보내기](#프런트엔드용-openapi-내보내기)
- [명령어 치트시트](#명령어-치트시트)
- [마치며](#마치며)
- [문서 및 자료](#문서-및-자료)
- [변경 로그](#변경-로그)

---

## 프로젝트 개요

한 줄 요약: 게임 운영 플랫폼에 필요한 모든 백엔드 API를 제공합니다. 운영자가 사용자 추가, 메뉴 수정, 주문 조회, 메일 발송, 버전 푸시, 로그 확인을 하고; 게임 클라이언트가 로그인, 결제, 설정 조회를 하고; CI 빌드 머신이 버전을 기록하는——이 모든 작업의 데이터 출구가 여기입니다.

백엔드에는 **세 개의 독립적인 호스트**가 있으며, 각자 자체 프로세스와 포트로 실행되어 서로에게 영향을 주지 않습니다:

- **Admin API**(`GameFrameX.Web.Api`, 포트 `20802`)—— 관리 백오피스용 API
- **Client API**(`GameFrameX.Client.Api`, 포트 `20808`)—— 게임 클라이언트용 API(로그인, 결제, 설정 조회)
- **Hub API**(`GameFrameX.Hub.Api`, 포트 `20806`)—— 외부 시스템 / 게임 클라이언트 / CI 빌드 / 서드파티 WebHook용 통합 API

Client API와 Hub API는 더 "가볍습니다": Client API는 Entity + Payment만 참조하고 기존 프레임워크를 건드리지 않습니다; Hub API는 전통적인 Controller 라우팅을 사용하고 리포지토리를 직접 사용하며, 동적 API 방식을 따르지 않습니다(자세한 내용은 아래에서).

**꼭 알아야 할 세 가지**:

1. **Service를 작성하면 API가 자동 생성됩니다**(Admin API). `IDynamicApiController`를 구현하면 클래스의 public 메서드가 자동으로 REST 엔드포인트가 되어, Controller를 직접 작성하거나 `[Route]`를 붙일 필요가 없습니다. Admin API의 핵심입니다.
2. **엔티티를 작성하면 테이블이 자동 생성됩니다**. SqlSugar Code-First: 시작 시 엔티티 클래스를 스캔 → 테이블 생성 → 시드 데이터 입력. 한 줄의 DDL도 작성할 필요가 없으며, 필드를 수정하고 재시작하면 테이블 구조가 따라옵니다.
3. **멀티 테넌시가 기본 내장되어 있습니다**. 엔티티에 마커만 붙이면 쿼리에 `TenantId` 필터가 자동으로 붙고 소프트 삭제도 자동으로 처리되며, SuperAdmin만 모든 필터를 통과할 수 있습니다. 비즈니스 로직을 작성할 때 거의 존재를 느낄 수 없습니다.

---

## 기술 스택

| 분류 | 선택 | 초보자를 위한 설명 |
|------|------|----------|
| 런타임 | .NET 10 | 이 코드를 실행하는 엔진(`Directory.Build.props`에서 `net10.0`) |
| 언어 버전 | C# 10 | `LangVersion=10`, record, file-scoped namespace 등 사용 가능 |
| 프레임워크 | ASP.NET Core 네이티브 호스트 | 별도 프레임워크 없이 네이티브 방식 사용 |
| ORM | SqlSugar (Code-First) | C# 엔티티 클래스를 작성하면 데이터베이스 테이블을 생성 |
| 인증 | JWT Bearer + HMAC Signature | Admin/Client/Hub가 동일한 JWT 공유; 개방형 API는 별도 서명 사용 |
| 실시간 통신 | SignalR (선택적 Redis Backplane) | 서버가 브라우저에 능동적으로 메시지를 푸시 |
| 캐시 | Memory / Redis | 단일 머신은 메모리로 충분, 다중 머신은 Redis 사용 |
| 정기 작업 | 프로젝트 자체 Scheduler | 작업을 데이터베이스에 저장하고 다중 인스턴스 간 조율, 중복 실행 없음 |
| 로그 | 데이터베이스 + ElasticSearch + Serilog | 작업 로그는 DB에, 대량 데이터는 ES 사용 |
| 데이터베이스 | SQLite(로컬 기본) / PostgreSQL(프로덕션), MySQL / Oracle 등 10여 종도 지원 | 다른 DB 연결 문자열 작성법은 [치트시트 문서](GameFrameX.Application/Configuration/Database-ConnectionStrings.md) 참조 |
| 컨테이너화 | Docker Compose | 한 번의 명령으로 세 개의 호스트를 모두 실행 |

---

## 세 개의 호스트, 각자의 역할

```
Admin API（20802）              Client API（20808）            Hub API（20806）
    │                              │                              │
    │ 관리 백오피스용                │ 게임 클라이언트용              │ 외부 시스템 / 게임 클라이언트 / CI용
    │ 로그인 / 사용자 / 메뉴 /       │ 로그인 / 결제 / 설정 조회      │ 구역 서버 / 리소스 버전 / 아이템 /
    │ 주문 / 로그 / 정기 작업        │                              │ 교환 코드 / 빌드 보고 / Tapd WebHook
    │                              │                              │
    └──────────── Entity 공유 ──────────────┬─────────────────────┘
                                           │
                                           ├── Payment 공유(Client만 사용)
                                           └── 세 호스트 모두 각자 SqlSugar 리포지토리를 직접 사용
```

의존 관계는 다음과 같습니다(왼쪽에서 오른쪽이 참조 방향):

```
Web.Api → Web.Core → Application.{System, Game, Client, Tenant} → Core → Entity → Core.Enum
Client.Api（독립, Entity + Payment만 참조）
Hub.Api（독립, Application.Game + Core + Entity 참조, 단 Controller는 Application Service를 호출하지 않음）
```

한 가지만 기억하세요: **비즈니스 계층은 호스트를 역참조하지 않습니다**. `Application.*`과 `Core.*`는 `Web.Api`의 존재를 절대 알지 못하며, 의존성은 아래 방향으로만 흐릅니다. 그래야 Client API / Hub API가 Entity를 깔끔하게 분리해서 사용할 수 있습니다.

### Hub API는 왜 Admin API와 모양이 다른가

Admin API는 "동적 API"——Service를 작성하면 API가 자동 생성; Hub API는 "전통적 Controller"——`[Route("api/[controller]/[action]")]`로 라우팅을 직접 작성하고, Controller에 `Repository<T>`를 직접 주입하여 엔티티를 조작하며, **Application Service를 호출하지 않습니다**. 이것은 Hub.Api의 의도적인 규칙입니다: 외부 시스템을 대상으로 하므로 로직이 단순하고 가볍고 빨라야 하기에, Controller가 직접 중복 검사와 소유권 검증을 수행하고 Admin의 계층 구조를 따르지 않습니다.

자세한 차이는 [어떻게 실행되는가](#어떻게-실행되는가)의 [Hub API: 세 번째 문](#hub-api-세-번째-문)을 참조하세요.

---

## 리포지토리 디렉터리 구조

```
GameFrameX.Admin.Unified.Api/
├── GameFrameX.Web.Api/            # Admin API 호스트(시작 진입점, compose 서비스명 web-api)
├── GameFrameX.Client.Api/         # Client API 호스트(독립 WebApplication)
├── GameFrameX.Hub.Api/            # Hub API 호스트(독립 WebApplication, 전통적 Controller 라우팅)
├── GameFrameX.Web.Core/           # Admin 시작 로직, 미들웨어, DI 구성(핵심)
├── GameFrameX.Application/        # 애플리케이션 계층(구성 파일, 시드 데이터)
├── GameFrameX.Application.System/ # 시스템 관리 서비스(사용자/역할/메뉴/테넌트/로그...)
├── GameFrameX.Application.Game/   # 게임 관리 서비스(구역 서버/주문/메일/교환 코드...)
├── GameFrameX.Application.Client/ # 클라이언트 관리 서비스(버전/채널/리소스 패키지...)
├── GameFrameX.Application.Tenant/ # 테넌트 수준 서비스(기기 / 기기 그룹)
├── GameFrameX.Entity/             # 엔티티 계층(Base / System / Game / Client)
├── GameFrameX.Core/               # 핵심 계층(리포지토리, 서비스 기본 클래스, 인증, SignalR)
├── GameFrameX.Core.Enum/          # 열거형 정의
├── GameFrameX.Core.Extension/     # 확장 메서드와 공용 DTO(핵심, 재사용 가능한 유틸리티를 찾으러 오는 곳)
├── GameFrameX.Core.SqlSugar/      # SqlSugar 구성, AOP, 인터셉터
├── GameFrameX.Core.Utility/       # 유틸리티 클래스
├── GameFrameX.Core.BaseInputOutput/ # 기본 입출력 모델
├── Payment/                       # 결제 연동(Client API만 참조)
│   ├── GameFrameX.Pay.Abstract/          # 결제 추상 인터페이스(정의 먼저, 구현 나중)
│   ├── GameFrameX.Pay.AliPay/            # Alipay
│   ├── GameFrameX.Pay.Apple/             # Apple IAP(인앱 결제 검증)
│   ├── GameFrameX.Pay.Apple.AppStoreServer/  # App Store Server API v2
│   ├── GameFrameX.Pay.Google/            # Google Play
│   └── GameFrameX.Pay.Xsolla/            # Xsolla
├── Dockerfile.web-api             # Admin API의 Dockerfile(sdk:10.0 → aspnet:10.0)
├── Dockerfile.client-api          # Client API의 Dockerfile
├── Dockerfile.hub-api             # Hub API의 Dockerfile
└── (통합 오케스트레이션은 리포지토리 루트의 docker-compose.yml로 이동됨: PostgreSQL + 3개 API + 프런트엔드 총 5개 서비스)
```

평소 코드 수정 작업의 99%는 `Application.*`과 `Entity`에서 이루어집니다. `Core.*`는 기반 구조이므로, 수정하기 전에 누가 의존하는지 먼저 파악하는 것이 좋습니다.

---

## 빠른 시작

### 환경 요구 사항

- .NET 10 SDK
- Visual Studio 2022 / Rider / VS Code 중 편한 것 선택
- 데이터베이스 사전 설치 불필요: 로컬 소스 직접 실행 시 기본적으로 SQLite 사용(파일은 리포지토리 루트 `db/` 디렉터리에 생성); PostgreSQL 연결이 필요할 때 설치하거나 아래의 Docker Compose를 사용(내장 PG)

### 데이터베이스: 기본 무설정, 프로덕션은 PostgreSQL

**로컬 소스 직접 실행(기본 SQLite)**: 아무것도 설정할 필요 없습니다. 기본 연결은 `GameFrameX.Application/Configuration/Database.json`에 있으며, 세 개의 host가 리포지토리 루트 `db/` 디렉터리의 동일한 SQLite 데이터베이스 파일(WAL 모드)을 공유합니다. 규칙은 **리포지토리 루트 디렉터리에서 `dotnet run` 실행**입니다(연결 문자열은 프로세스 작업 디렉터리 기준으로 해석).

주의: SQLite는 로컬 개발/디버깅 전용입니다. 운영/프로덕션 환경에서는 SQLite 사용이 금지됩니다(동시 쓰기 취약, 독립 서비스 프로세스 없음, 백업 복구 및 운영 모니터링 기능 부족). PostgreSQL을 사용하세요.

**프로덕션 / 컨테이너(PostgreSQL, 환경 변수 쌍으로 오버라이드)**: 데이터베이스를 오버라이드할 때는 타입과 연결 문자열을 반드시 **쌍으로** 설정해야 하며, 절반만 설정하면 시작 시 바로 오류가 발생합니다. 세 개의 host(Web / Client / Hub)는 **동일한 변수 쌍**을 사용합니다:

- `GFX_MAIN_DB_TYPE=PostgreSQL` + `GFX_MAIN_DB_CONNECTION_STRING=Host=...`
- 기본 테넌트 DB는 설정 불필요: 메인 DB 연결 문자열에서 자동 유도(DB명만 변경, 아래 참조)
- 기존 배포의 `DbType` / `ConnectionString` 시작 파라미터도 여전히 사용 가능(호환 계층), 시작 시 마이그레이션 경고가 출력되므로 빠르게 전환하세요
- 다른 데이터베이스(MySQL / Oracle / DM…… 전체 34종 DbType)로 전환하는 연결 문자열 작성법과 예시: [`GameFrameX.Application/Configuration/Database-ConnectionStrings.md`](GameFrameX.Application/Configuration/Database-ConnectionStrings.md)

데이터베이스에 대해 알아야 할 몇 가지 규칙:

- **테넌트 DB는 자동 생성됩니다**. 일반 테넌트 생성 시 시스템이 테넌트 ID에 따라 `gameframex_admin_tenant_<TenantId>` 데이터베이스와 해당 로그인 역할을 자동 생성하며, **SuperAdmin이 수동으로 테넌트 DB 연결 문자열을 입력할 필요가 없습니다**.
- **기본 테넌트 DB도 자동 유도됩니다(C269)**. 메인 DB 연결 문자열 → 기본 테넌트 DB 연결 문자열: PostgreSQL / MySQL은 DB명만 `gameframex_admin_tenant_1300000000001`로 변경(일반 테넌트 DB와 동일한 규칙, 계정/비밀번호는 메인 DB 상속, MySQL DB 생성 시 utf8mb4 강제); SQLite는 메인 DB와 동일한 디렉터리에 `gameframex_admin_tenant_1300000000001.db`로 생성. PostgreSQL / MySQL / SQLite만 지원하며 다른 타입은 시작 시 오류 발생. 명시적 구성이 우선: `Database.json`에 `ConfigId=1300000000001` 항목을 다시 추가하거나 `GFX_DEFAULT_TENANT_DB_TYPE` + `GFX_DEFAULT_TENANT_DB_CONNECTION_STRING`을 쌍으로 설정.
- **기존 배포 업그레이드(BREAKING)**: 기본 테넌트 DB명이 `gameframex_admin_tenant_default`에서 `gameframex_admin_tenant_1300000000001`로 변경됨. 둘 중 하나 선택: a. 온라인 이름 변경 `ALTER DATABASE gameframex_admin_tenant_default RENAME TO gameframex_admin_tenant_1300000000001`(레지스트리는 자동으로 유도값을 따라감); b. `GFX_DEFAULT_TENANT_DB_*`를 쌍으로 설정해 기존 DB명을 그대로 유지.
- **Compose에 PostgreSQL 내장**. 리포지토리 루트에서 `docker compose up` 실행 시 `postgres:16-alpine`이 자동으로 시작됨(첫 시작 시 자동 DB 생성, `docker/postgres/init.sql` 참조, 데이터는 리포지토리 루트 `postgres-data/` bind mount에 저장). 호스트의 기존 postgres 컨테이너를 재사용하려면 리포지토리 루트 `.env`에 `POSTGRES_HOST=host.docker.internal` 등 변수를 설정하고 api 서비스의 `postgres`에 대한 `depends_on`을 제거하면 됩니다.
- **세 개의 host가 동일한 기본 구성을 공유**. Client / Hub 시작 시 `Database.json`을 자동으로 로드(Admin과 동일한 소스, 한 곳 수정 시 세 곳에 적용); 환경 변수 오버라이드 방식은 compose와 일치.

### 시작 방법 1: Docker Compose 원클릭 실행(권장, 무설정)

```bash
# 리포지토리 루트에서 실행(Api 하위 디렉터리 아님)
docker compose up -d --build
# 내장 PostgreSQL + Admin(20802) + Client(20808) + Hub(20806) + 프런트엔드(20800), 시작 후 healthcheck가 초록색이 될 때까지 대기
```

### 시작 방법 2: 소스 직접 실행(무설정, SQLite)

```bash
# 주의: 리포지토리 루트 디렉터리에서 실행, 네 개의 host가 ./db/의 SQLite DB 파일 공유

# Admin API(관리 백오피스)
dotnet run --project GameFrameX.Web.Api
# http://*:20802 리스닝, Swagger UI: http://localhost:20802/kapi

# Client API(게임 클라이언트)
dotnet run --project GameFrameX.Client.Api
# http://*:20808 리스닝

# Hub API(외부 시스템 / CI / WebHook)
dotnet run --project GameFrameX.Hub.Api
# http://localhost:20806 리스닝, Swagger UI: http://localhost:20806/swagger

# Game API(게임 서버 측)는 Client API에 병합됨(C268): GameUser* 컨트롤러가 Client.Api에 노출(http://*:20808)
```

Admin API 첫 시작 시 SqlSugar Code-First가 자동으로 DB와 테이블을 생성하고, 시드 데이터(관리자, 역할, 메뉴, 사전 등)를 입력합니다. 빈 데이터베이스에 갑자기 테이블이 잔뜩 생겨도 놀라지 마세요. 정상적인 동작입니다.

### 시작 방법 3: 단일 컨테이너 docker run

이미지의 기본 구성은 SQLite입니다(환경 변수 없이 바로 실행 가능, 데이터는 컨테이너 내 `/app/db/`, volume 마운트로 영속화 가능). 프로덕션에서는 쌍으로 된 환경 변수를 주입해 PostgreSQL에 연결합니다:

```bash
docker run -p 20802:20802 \
  -e GFX_MAIN_DB_TYPE=PostgreSQL \
  -e GFX_MAIN_DB_CONNECTION_STRING="Host=PG호스트;Port=5432;Database=gameframex_admin;Username=...;Password=..." \
  -e GFX_DEFAULT_TENANT_DB_TYPE=PostgreSQL \
  -e GFX_DEFAULT_TENANT_DB_CONNECTION_STRING="Host=PG호스트;Port=5432;Database=gameframex_admin_tenant_1300000000001;Username=...;Password=..." \
  사용할 이미지
```

### 막혔다면?

| 증상 | 가장 유력한 원인 |
|------|------------|
| 시작 시 데이터베이스 연결 실패 | `Database.json`(또는 `GFX_MAIN_DB_*` 환경 변수) 연결 문자열이 잘못되었거나 PG가 실행되지 않음 |
| 시작 시 "DbType이 Sqlite/PostgreSQL인데 연결 문자열이 …스타일이 아님" 오류 | 환경 변수가 절반만 오버라이드됨: `GFX_MAIN_DB_TYPE`과 `GFX_MAIN_DB_CONNECTION_STRING`은 반드시 쌍으로 설정 |
| 시작 시 "기본 테넌트 DB 자동 유도 미지원" 오류 | 메인 DB가 PostgreSQL / MySQL / SQLite 외의 DbType 사용, `GFX_DEFAULT_TENANT_DB_TYPE` + `GFX_DEFAULT_TENANT_DB_CONNECTION_STRING`을 쌍으로 명시적 설정 필요 |
| SQLite DB 파일 위치가 잘못됨 | 연결 문자열은 프로세스 작업 디렉터리 기준으로 해석——리포지토리 루트 디렉터리에서 `dotnet run` 실행; 프로젝트 디렉터리에서 생긴 `db/`는 삭제하면 됨 |
| 테이블이 생성되지 않음 | Code-First 스위치는 기본적으로 코드에서 모두 켜짐(C278); 명시적으로 끄는 항목을 구성한 적이 있다면 `Database.json` 또는 환경 변수 확인 |
| Swagger가 열리지 않음 | 개발 환경이 아닌 경우 기본적으로 Swagger가 마운트되지 않음, 아래의 [OpenAPI 내보내기](#프런트엔드용-openapi-내보내기) 참조 |
| 시드 데이터가 입력되지 않음 | 시드 데이터 스위치가 꺼져 있음; 시드는 기본 키 기준 Upsert이므로, 시드를 수정하면 이전 레코드를 삭제해야 다시 입력됨 |
| Hub API 시작 시 "JWTSettings 미구성" 오류 | Hub는 Admin의 JWT 구성을 재사용, `Hub.Api/appsettings.json`에 Admin과 동일한 `JWTSettings` 노드가 있어야 함 |
| 포트 점유 | 20800 / 20802 / 20808 / 20806을 다른 프로세스가 점유(프런트엔드 컨테이너와 dev server가 20800을 두고 경합), `lsof -i :20806`으로 확인 |

---

## 어떻게 실행되는가

### 동적 API: Service를 작성하면 곧 API(Admin API)

Admin API의 서비스가 `IDynamicApiController`를 구현하면, 프레임워크가 클래스의 public 메서드를 자동으로 REST 엔드포인트로 변환합니다(라우팅은 camelCase). Controller를 직접 작성할 필요가 없습니다:

```csharp
public class MyService : ITransient, IDynamicApiController
{
    // 자동으로 GET /api/my/list 생성
    public List<MyEntity> List() => ...;
}
```

라우팅 생성 규칙(반드시 기억하세요. 프런트엔드는 100% 이 규칙에 따라 호출합니다):

1. 클래스명에서 `Service` / `AppService` / `Controller` 접미사 제거(한 번만)
2. 첫 글자를 소문자로
3. 기본 접두사 `api/` 추가

| 클래스명 | 접미사 제거 후 | 최종 라우팅 접두사 |
|------|--------|-------------|
| `GameAppService` | `GameApp` | `/api/gameApp` |
| `SystemUserService` | `SystemUser` | `/api/systemUser` |
| `GameOrderService` | `GameOrder` | `/api/gameOrder` |

메서드명도 마찬가지로 뒤에 붙습니다: `Page()` → `/api/gameApp/page`, `ResetSecret()` → `/api/gameApp/resetSecret`.

> **URL에 `Service` 접미사가 나타나는 것은 엄격히 금지**. 권고가 아니라 하드 규칙입니다——이 규칙으로 생성된 호출을 프런트엔드가 사용하므로, 백엔드 클래스명이 바뀌면 프런트엔드 전체가 망가집니다.

세밀한 제어가 필요할 때는 `[HttpPost]`, `[AllowAnonymous]`, `[NonAction]` 등의 특성을 평소처럼 사용하면 됩니다.

### 의존성 주입: 마커만 붙이면 등록

`services.AddScoped<>()`를 직접 작성할 필요 없이, 마커 인터페이스로 자동 등록됩니다:

| 인터페이스 | 생명주기 | 언제 사용 |
|------|----------|----------|
| `ITransient` | Transient | resolve할 때마다 새 객체, 대부분의 서비스에 사용 |
| `IScoped` | Scoped | 한 번의 요청 내에서 재사용 |
| `ISingleton` | Singleton | 전역 유일, 상태를 가진 것을 넣을 때는 주의 |

인프라 컴포넌트(SqlSugar, JWT, SignalR, EventBus 등)는 구성 파라미터가 필요하므로 `GameFrameX.Web.Core/Startup.cs`에서 수동으로 등록됩니다.

### 리포지토리와 다중 데이터베이스 라우팅

`SqlSugarRepository<T>`는 SqlSugar의 `SimpleClient<T>`를 상속하며, 생성자에서 엔티티의 특성에 따라 데이터를 저장할 데이터베이스를 결정합니다:

| 엔티티 특성 | 라우팅 대상 |
|----------|----------|
| `[SystemTable]` | 메인 데이터베이스 |
| `[LogTable]` | 로그 데이터베이스 |
| `[Tenant]` | 테넌트 독립 데이터베이스 |
| 마커 없음 | JWT Claim의 TenantId 기준으로 라우팅 |

엔티티를 작성하고 마커를 붙이기만 하면, 나머지 라우팅은 리포지토리가 처리합니다.

### 인증 체계: JWT 공유 + 서명

**JWT Bearer(세 개의 호스트가 동일한 키 공유)**

- `JwtHandler`는 ASP.NET Core 인증 핸들러 기반으로 구현(Admin API)
- 세 호스트가 동일한 `JWTSettings`를 읽음——따라서 **Admin 로그인으로 발급한 token을 Client API와 Hub API도 그대로 인정**
- 자동 갱신: Admin API의 응답 헤더 `X-Authorization`에 새 Token을 담아 보내면 프런트엔드가 조용히 교체
- 권한 검사: SuperAdmin은 모든 검사를 통과; 일반 사용자는 라우팅명으로 버튼 권한 매칭

**HMAC Signature(개방형 API용)**

- `SignatureAuthenticationHandler`가 HMAC-SHA256 서명 인증 구현
- 요청 헤더에 4개 필수: `accessKey` + `timestamp` + `nonce` + `sign`
- 서명 검증 공식: `HMACSHA256(secret, "METHOD&URL&accessKey&timestamp&nonce")`
- 재생 공격 방지: nonce 캐시 + 시간 편차 검사(기본 5분 편차 허용)

### 멀티 테넌시와 소프트 삭제: 쿼리 자동 필터링

- `ITenantIdFilter`: 쿼리에 `TenantId` 필터가 자동으로 붙어 테넌트 간 데이터가 자연스럽게 격리
- `ISafeDeletedFilter`: 쿼리에 `IsDeleted == false`가 자동으로 붙고, 삭제는 마킹만
- SuperAdmin은 모든 필터를 통과하므로 전체 데이터를 볼 수 있음(**단 Hub API는 SuperAdmin을 거부, 아래 참조**)

이 두 가지는 "비즈니스 작성 시 거의 느끼지 못하지만 항상 안전망이 되어주는" 메커니즘입니다.

### 감사 필드: 자동 채움, 직접 작성 불필요

SqlSugar AOP가 `SqlSugarSetup.SetDbAop()`에서 이 작업을 처리합니다:

| 작업 | 자동으로 채워지는 필드 |
|------|-------------|
| Insert | `CreatedTime`, `CreatedId`, `CreatedName`, `TenantId` |
| Update | `UpdateTime`, `UpdatedId`, `UpdatedName` |

동시에 데이터 변경 사항을 감사 로그(`SystemLogDiff` / `TenantLogDiff`)에 기록합니다. 따라서 엔티티를 만들 때 감사 필드를 직접 할당할 필요 없이 프레임워크가 채워줍니다.

### 스노우플레이크 ID

분산 유일 ID는 Foundation Snowflake의 **컴팩트 숏 ID 레이아웃**으로 생성됩니다(2026-09-15 결정 확정, `gfx-doc/decisions/2026-09-15-compact-snowflake-id.md` 참조):

| 레이아웃 파라미터 | 값 | 설명 |
|---|---|---|
| epoch | 2026-09-01 | 배포 기준 시점, 데이터 적재 후 변경 불가 |
| WorkerId | 3비트(0-7) | 유일한 인스턴스 구분 차원 |
| DataCenterId | 0비트 | 단일 데이터 센터, 차원 제거됨 |
| 시퀀스 | 10비트 | WorkerId당 밀리초당 1024개 |

13-14자리 숫자가 생성되며(< 2^53, JS에서 정밀도 손실 없음), Admin 페이지에서 기본 키를 문자열 변환 없이 그대로 표시할 수 있습니다. 레이아웃은 확정되었습니다: epoch나 비트 폭 변경 = 기존 Id 공간과 중복, 곧 데이터 무결성 사고입니다.

다중 인스턴스 충돌 방지는 **Redis가 아닌** 명시적 구성 상호 배제로 처리됩니다(프로세스 내 임대는 프로세스 간 상호 배제가 불가능해 해당 방식은 폐기됨). 동일한 DB에 쓰는 모든 인스턴스(Web.Api 다중 복제본 / Hub / Client)는 `SnowFlake:WorkerId`(환경 변수 `SnowFlake__WorkerId`, 0-7)를 서로 다르게 구성해야 하며, 범위를 벗어나면 시작 시 바로 실패합니다——단일 데이터 센터에서 동일 DB 인스턴스는 최대 8개. 기본값은 Web.Api=1(appsettings) / Hub=2 / Client=3(코드 폴백), 단일 머신 기본 배포에서는 서로 충돌하지 않습니다.

SqlSugar의 기본 키 자동 할당도 이 방식으로 통일됩니다: `StaticConfig.CustomSnowFlakeFunc`는 `SqlSugarSnowflakeFallbackInitializer`(ModuleInitializer)가 Core.Extension 모듈 로드 시 프로세스 수준으로 설치하며, 어떤 경로(Hub / Client 및 테스트 프로세스 포함)에서도 SqlSugar 내장 18-19자리 긴 레이아웃으로 폴백되지 않습니다.

### 전역 응답 래핑

Admin API의 `AdminResultProvider`가 모든 응답을 통일된 형식으로 감싸줍니다:

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

프런트엔드는 이 구조를 기대하며, `Code === 200`이면 통과, 아니면 오류 처리로 이동합니다. Hub API는 자체 `HttpJsonResultData<T>`(`Success` / `Fail` 팩토리 메서드)를 사용하며, 구조는 비슷하지만 독립적이고 `AdminResultProvider`를 거치지 않습니다.

### SignalR 실시간 통신

- Hub: `OnlineUserHub`, 경로 `/hubs/onlineUser`
- 테넌트별 그룹화(`GROUP_ONLINE_{tenantId}`), A 테넌트의 메시지가 B 테넌트로 새지 않음
- 가능한 기능: 온라인 사용자 목록, 강제 로그아웃, 메시지 발송(유니캐스트 / 브로드캐스트 / 자신 제외 / 특정 사용자 지정)
- 다중 인스턴스 배포 시 선택적 Redis Backplane + Data Protection Key 영속화로 메시지를 인스턴스 간에 푸시

### Hub API: 세 번째 문

Hub API(`GameFrameX.Hub.Api`, 포트 `20806`)는 **외부 시스템**을 위한 통합 API로, Admin API, Client API와는 포지션이 다릅니다. 다음 호출자를 위해 서비스합니다:

| 호출자 | 하는 일 | 해당 Controller |
|--------|------|-----------------|
| 게임 클라이언트 / 운영 도구 | 구역 서버 조회, 리소스 버전 조회, 아이템 조회, 교환 코드 검증 | `GameAreaController` / `GameAssetPackageVersionController` / `GameGoodsController` / `GameRedeemCodeController` |
| CI / 빌드 머신 | 빌드 완료 후 리소스 버전 / 패키지 버전 기록 | `PackageBuildReportController` |
| 서드파티 플랫폼 | Tapd WebHook 콜백 수신 | `TapdWebHookController` |

코드를 작성하기 전에 반드시 알아야 할 남들과 다른 특징들:

**1. 전통적 Controller 라우팅, 동적 API 미사용.**
기본 클래스 `BaseController`에 `[Route("api/[controller]/[action]")]`가 표시되어, 라우팅을 직접 작성하며 클래스명 자동 생성에 의존하지 않습니다. 따라서 여기의 URL은 `Service` 접미사가 자동으로 제거되지 **않습니다**——`Controller`명 + `Action`명으로 그대로 조합됩니다.

**2. Controller가 `Repository<T>`를 직접 사용, Application Service 호출 안 함.**
이것은 Hub.Api의 철칙입니다. Controller 생성자에 리포지토리를 주입하고, 직접 조회하고, 직접 작성하고, 직접 중복 검사하고, 직접 소유권을 검증합니다. PackageBuildReportController의 주석에 명확히 나와 있습니다: "Hub.Api Controller 규칙 준수(Repository가 Entity를 직접 조작, Application Service 호출 안 함)로 인해 계층화된 ScopeValidator를 올리지 않았고 Application.Client의 CheckXxxUnique를 재사용하지 않음——Controller가 직접 소유권 검증과 중복 검사를 작성함."

> 의미: Admin API에 제출한 Validator, Application 계층의 중복 검사 메서드는 **Hub.Api에는 죽은 코드**이며, Hub에서 재사용될 것으로 기대하지 마세요. Hub에서 검증이 필요하면 Controller에 직접 작성하세요.

**3. 세 가지 인증 시나리오, 각각 다른 방식.**

| 시나리오 | 인증 방식 | 테넌트 출처 | 예시 |
|------|----------|-----------|------|
| 로그인 사용자 | JWT(`BaseController` 기본 `[Authorize]`) | JWT에서 파싱 | 구역 서버 / 리소스 / 아이템 / 교환 코드 조회 |
| 익명 WebHook | `[AllowAnonymous]` | 요청 헤더에서 | `TapdWebHookController` |
| 머신 간 통신 | `[AllowAnonymous]` + `TenantGameAppOpenAccessAuthMiddleware` | 자격 증명에 바인딩된 App | `PackageBuildReportController`(빌드 보고) |

머신 자격 증명 방식은 AccessKey + 서명 + scope를 검증하며 **JWT가 아닙니다**. 따라서 Controller에 `[AllowAnonymous]`를 표시해 JWT를 건너뛰고, 전용 미들웨어가 처리합니다.

**4. 테넌트는 미들웨어로 통일 파싱하며, 슈퍼 관리자를 거부.**
`HubTenantMiddleware`(`/health` 제외 모든 경로)가 테넌트 Id를 `HttpContext.Items`에 통일 기록: 인증 요청은 JWT에서, 익명 WebHook은 요청 헤더에서 가져옵니다. 그다음 `DelegateSqlSugarTenantIdResolver`가 이 Id를 SqlSugar에 전달해 테넌트 라우팅을 수행합니다. **SuperAdmin은 바로 거부됩니다**——Hub는 테넌트 도메인의 서비스이므로, 플랫폼 수준 슈퍼 관리자가 Hub를 통해 특정 테넌트의 데이터에 침투해서는 안 됩니다.

**5. 자체 낙관적 잠금 미들웨어와 헬스 체크 내장.**
`OptimisticConcurrencyMiddleware`가 파이프라인 맨 앞에 있어 동시성 제어가 필요한 쓰기 작업의 안전망이 됩니다. 헬스 체크 엔드포인트는 3개: `/health`, `/health/live`, `/health/ready`(compose가 생존 프로브로 사용).

Hub API의 미들웨어 파이프라인 순서(`Program.cs`):

```
OptimisticConcurrencyMiddleware
  → Swagger(스위치 제어)
  → CORS
  → Authentication → Authorization
  → HubTenantMiddleware(/health 제외; 테넌트 Id 기록; 슈퍼 관리자 거부)
  → TenantGameAppOpenAccessAuthMiddleware(仅 /api/PackageBuildReport)
  → HealthChecks / MapControllers
```

### 구성 파일: 독립 JSON, 필요 시 로드

Admin API의 독립 JSON 구성은 모두 `GameFrameX.Application/Configuration/`에 있으며, 시작 시 자동으로 스캔합니다:

| 파일 | 관리 항목 |
|------|--------|
| `Database.json` | 데이터베이스 연결, Code-First 스위치, 시드 데이터 스위치 |
| `Swagger.json` | Swagger / Knife4j UI 구성 |
| `SMS.json` | SMS 서비스 구성 |
| `Upload.json` | 파일 업로드, OSS 구성 |
| `Wechat.json` | WeChat 공식 계정 / 미니 프로그램 구성 |
| `Enum.json` | 열거형 스캔 어셈블리 |

이미 "이사 간" 구성 두 개가 있으니 더 찾아다니지 마세요:

> 플랫폼 JWT 구성은 메인 DB `SystemConfig`의 `platform_jwt_settings`로 이동되었으며, **더 이상 `JWT.json`을 런타임 소스로 사용하지 않습니다**(Hub API는 여전히 자체 `appsettings.json`의 `JWTSettings` 노드를 읽지만, 키는 Admin과 일치해야 함).
>
> `App.json`은 폐기됨: 리스닝 포트, 동적 API, CORS, 스노우플레이크 ID 등의 호스트·프레임워크 구성은 `Web.Entry/appsettings.json`으로 이동; Cryptogram 키는 환경 변수 `GFX_CRYPTOGRAM_*` + 내장 기본값으로 변경(`CryptogramSecretResolver` 참조); 비밀번호 강도 정책은 메인 DB `SystemConfig`의 `platform_password_policy_config`로 이동; 구역 서버 Server HTTP 구성은 구역 서버 수준 DB 구성으로 이동.

---

## 기능 모듈 한눈에 보기

> 여기의 서비스는 주로 Admin API에서 실행됩니다(동적 API). Hub API의 인터페이스는 별도의 전통적 Controller 세트이며, [Hub API: 세 번째 문](#hub-api-세-번째-문)을 참조하세요.

### 시스템 관리

프로젝트 자체 동적 API 라우팅 기반의 완전한 백오피스 관리 모듈입니다. "어떤 게임을 만들든 반드시 필요한" 부분입니다.

| 기능 | 서비스 클래스 | 설명 |
|------|--------|--------|
| 인증 로그인 | `SystemAuthService` | 계정 비밀번호, 휴대폰 인증 코드 로그인, JWT 발급 + 자동 갱신, 캡차 |
| 사용자 관리 | `SystemUserService` | 사용자 CRUD, 비밀번호 초기화, 사용자-역할 바인딩(주의: 이 서비스가 실제로 관리하는 것은 `TenantUser`이며, 명명이 다소 오해의 소지가 있음) |
| 역할 관리 | `TenantRoleService` | 역할 CRUD, 데이터 범위 제어, 역할-메뉴 권한 바인딩 |
| 메뉴 권한 | `GlobalMenuService` | 트리 메뉴, 버튼 수준 권한 파싱, 라우팅과 API 매핑 |
| 조직 구조 | `TenantOrgService` | 트리 조직 CRUD, 역할-조직 데이터 범위 |
| 직위 관리 | `TenantPosService` | 직위 CRUD, 소유권 검증 |
| 테넌트 관리 | `SystemTenantService` | 멀티 테넌트 CRUD, 독립 데이터베이스 연결 구성 |
| 사전 관리 | `GlobalDictTypeService` / `GlobalDictDataService` | 사전 타입과 사전 값 CRUD, 상태 전환 |
| 파일 관리 | `TenantFileService` | 파일 업로드/다운로드, OSS 객체 스토리지 |
| 정기 작업 | `SystemJobService` | 작업과 트리거 CRUD, 일시 정지 / 시작 / 즉시 실행, 클러스터 조율 |
| 데이터베이스 관리 | `SystemDatabaseService` | 테이블 / 컬럼 CRUD, DB 테이블 구조 온라인 관리 |
| 공지 사항 | `TenantNoticeService` | 공지 발행, SignalR 브로드캐스트, 읽음 / 안 읽음 추적 |
| 온라인 사용자 | `TenantOnlineUserService` | 실시간 온라인 목록, 강제 로그아웃, 싱글 사인온 |
| 로그 시스템 | `LogOperationService` / `LogExceptionService` / `LogVisitService` / `LogDiffService` | 작업 로그, 예외 로그, 접속 로그, 데이터 변경 감사, Excel 내보내기 지원 |
| 캐시 관리 | `SystemCacheService` | 통일 캐시 추상화(Memory / Redis), 캐시 키 관리 |
| 시스템 구성 | `SystemConfigService` | 키-값 쌍 구성 CRUD |
| 열거형 서비스 | `SystemEnumService` | 리플렉션으로 어셈블리의 열거형 타입과 값 조회 |
| 상수 서비스 | `SystemConstService` | `[ConstAttribute]`로 표시된 상수 값 조회 |
| SMS 서비스 | `SystemSmsService` | Alibaba Cloud SMS 인증 코드, 60초 TTL |
| 지역 관리 | `SystemRegionService` | 행정구역 5단계 트리, 통계국 동기화 지원 |
| 인쇄 템플릿 | `TenantPrintService` | 인쇄 템플릿 CRUD |
| 개방형 인터페이스 | `SystemOpenAccessService` | AccessKey / Secret 관리, HMAC 서명 인증 |
| 플러그인 관리 | `SystemPluginService` | 런타임에 C# 코드 컴파일, 동적으로 API 엔드포인트로 등록 |
| WeChat | `SystemWechatService` / `SystemWxOpenService` | 공식 계정 OAuth2, 미니 프로그램 로그인, 템플릿 메시지, JS-SDK |
| WeChat Pay | `SystemWechatPayService` | JSAPI 주문(직렬 / 파트너), 결제 콜백 |
| 서버 모니터링 | `SystemServerService` | 호스트 정보, CPU / 메모리, 디스크, 로드된 어셈블리 |

### 게임 관리

게임 운영을 위한 비즈니스 모듈로, 모든 서비스는 `GenericCrudService` 기본 클래스를 상속합니다(서비스 하나 작성으로 여섯 개의 API 자동 생성, 뒤에서 설명).

| 기능 | 서비스 클래스 | 설명 |
|------|--------|--------|
| 게임 계정 | `GameUserEntityService` | 이메일 가입, 정지 / 해제 |
| 게임 캐릭터 | `GameRoleEntityService` | 캐릭터 관리, 샌드박스 모드 전환, 게임 서버에서 데이터 동기화 |
| 게임 구역 서버 | `GameAreaEntityService` | 구역 서버 관리, Docker 컨테이너 라이프사이클(생성 / 시작 / 중지 / 재시작) |
| 구역 서버 분류 | `GameAreaClassifyEntityService` | 구역 서버 클러스터 분류 |
| 주문 관리 | `GameOrderEntityService` | 결제 주문, 다차원 필터(상태 / 캐릭터 / 구역 서버 / 채널 / 플랫폼 / 날짜) |
| 주문 발송 | `GameOrderShipService` | 공유 발송 로직, HTTP로 게임 서버 API 호출해 아이템 지급 |
| 게임 메일 | `GameMailEntityService` | 메일 발송, 아이템 첨부 파싱, 수신자 검증 |
| 교환 코드 | `GameRedeemCodeEntityService` | 교환 코드 관리, 아이템 첨부 파싱 |
| 교환 코드 채널 | `GameRedeemCodeChannelEntityService` | 교환 코드 배포 채널 |
| 교환 기록 | `GameRedeemCodeUseRecordEntityService` | 교환 코드 사용 기록 |
| Web 상점 | `GameWebShopEntityService` | Web 상점 상품 관리 |
| 상점 주문 | `GameWebShopOrderEntityService` | Web 상점 주문 |
| 상품 관리 | `GameCommodityEntityService` | 상품 / 제품 관리 |
| 아이템 | `GameGoodsEntityService` | 게임 내 아이템 / 재화 |
| 아이템 카탈로그 | `GameGoodsCatalogueEntityService` | 아이템 분류 |
| 정기 이벤트 | `GameTimeJobEntityService` | 게임 내 정기 작업 / 이벤트 |
| Docker 태그 | `GameDockerTagEntityService` | 게임 서버 Docker 이미지 버전 관리 |
| 공지 | `GameAnnouncementEntityService` / `GameBannerAnnouncementEntityService` | 게임 공지, 캐러셀 공지 |
| 홈 구성 | `GameHomeEntityService` | 게임 홈 구성 |

### 앱 센터

클라이언트 App 대상의 구성 관리 모듈입니다.

| 기능 | 서비스 클래스 | 설명 |
|------|--------|--------|
| 앱 버전 | `GameAppReleaseVersionService` | App 버전 관리, 강제 업데이트 제어 |
| 리소스 패키지 | `GameAppAssetPackageService` | AssetBundle / 리소스 패키지 관리 |
| 리소스 버전 | `GameAppAssetPackageVersionService` | 리소스 패키지 버전 추적 |
| 리소스 이력 | `GameAppAssetPackageVersionHistoryService` | 리소스 버전 변경 기록 |
| 채널 정의 | `GlobalChannelDefinitionService` / `TenantChannelDefinitionService` | 배포 채널(App Store, Google Play 등) |
| 기기 관리 | `TenantDeviceService` | 기기 등록과 식별(`Application.Tenant`에 위치) |
| 기기 그룹 | `TenantDeviceGroupService` | 기기 그룹(그레이스케일 / A/B 테스트, `Application.Tenant`에 위치) |
| 다국어 | `GlobalLanguageService` | 지원 언어 구성 |
| 분할 패키지 관리 | `GameAppPackageService` | 앱 분할 패키지, 채널 바인딩과 마스터 패키지 상속 관리 |
| 플랫폼 관리 | `GlobalPlatformService` | 실행 플랫폼(iOS, Android 등) |
| 버전 정의 | `GameAppVersionDefinitionService` | 버전-리소스 매핑 |
| 클라이언트 구성 | `GameAppClientConfigService` | 클라이언트 키-값 쌍 구성 |
| 구성 키 | `GameAppClientConfigKeyService` | 클라이언트 구성 콘텐츠 키 |

### Hub API의 인터페이스(별도 세트)

전통적 Controller로, Hub API(20806)에서 실행되며, **동적 API를 사용하지 않고 Application Service를 호출하지 않습니다**:

| 인터페이스 | Controller | 인증 | 설명 |
|------|-----------|------|------|
| 구역 서버 조회 | `GameAreaController` | JWT | 게임 클라이언트에 구역 서버 목록 제공 |
| 리소스 패키지 버전 | `GameAssetPackageVersionController` | JWT | 리소스 버전 조회, 낙관적 잠금 업데이트 |
| 아이템 조회 | `GameGoodsController` | JWT | 게임 내 아이템 조회 |
| 교환 코드 | `GameRedeemCodeController` | JWT | 교환 코드 검증 관련 |
| 빌드 보고 | `PackageBuildReportController` | 머신 자격 증명 | CI / 빌드 머신의 리소스 버전 / 패키지 버전 기록(멱등 upsert) |
| Tapd WebHook | `TapdWebHookController` | 익명(요청 헤더에서 테넌트 추출) | Tapd 콜백 수신 |

### 결제 연동

Client API 전용입니다. 추상 계층 + 다중 채널 구현이며, 새 채널 추가는 추상 계층을 따라 구현하면 됩니다:

| 채널 | 프로젝트 |
|------|------|
| Alipay | `GameFrameX.Pay.AliPay` |
| Apple IAP | `GameFrameX.Pay.Apple`(인앱 결제 영수증 검증) |
| App Store Server | `GameFrameX.Pay.Apple.AppStoreServer`(API v2 알림 처리) |
| Google Play | `GameFrameX.Pay.Google` |
| Xsolla | `GameFrameX.Pay.Xsolla` |

---

## 새 CRUD 서비스 추가하기

> 이 절은 **Admin API**의 동적 API 서비스를 다룹니다. Hub API는 전통적 Controller로 이 방식을 따르지 않습니다——Hub 인터페이스 추가는 Controller 생성 + 리포지토리 주입이면 충분하며, `GameFrameX.Hub.Api/Controllers/`의 기존 코드를 참조하세요.

한 번 따라 해보면 일상 백엔드 작업의 80%를 익힐 수 있습니다. "게임 아이템"을 추가한다고 가정합니다.

### 1. 엔티티 생성

`GameFrameX.Entity/`의 해당 하위 디렉터리에 엔티티 클래스를 생성합니다:

```csharp
[SugarTable(null, "게임 아이템")]
[SystemTable]
[IncrementTable]
public class GameItemEntity : EntityBasic
{
    [SugarColumn(ColumnDescription = "아이템 이름", Length = 64)]
    public string Name { get; set; }

    [SugarColumn(ColumnDescription = "아이템 수량")]
    public int Count { get; set; }
}
```

엔티티 기본 클래스는 필요에 따라 선택합니다:

| 기본 클래스 | 특징 | 적용 시나리오 |
|------|------|----------|
| `EntityBaseId` | `Id`만 있음 | 연결 테이블, 테넌트 없는 엔티티 |
| `EntityTenantBase` | Id + 테넌트 + 감사 + 소프트 삭제 | 시스템 테이블(외부 참조) |
| `EntityBasic` | `EntityTenantBase` 상속 | 범용 비즈니스 엔티티(가장 많이 사용) |
| `EntityBasicData` | 조직 데이터 권한 추가 | 조직별 필터링이 필요한 데이터 |
| `EntityBasicSelect` | Name + Description 추가 | 드롭다운 셀렉터 엔티티 |

### 2. 시드 데이터 생성(선택)

`GameFrameX.Application/SeedData/`의 해당 하위 디렉터리에 시드 클래스를 생성합니다:

```csharp
public class GameItemSeedData : ISqlSugarEntitySeedData<GameItemEntity>
{
    public IEnumerable<GameItemEntity> HasData()
    {
        return new[]
        {
            new GameItemEntity { Id = YitIdHelper.NextId(), Name = "골드", Count = 0 }
        };
    }
}
```

시드 데이터는 `StorageableByObject`로 기본 키 기준 InsertOrUpdate를 수행하므로 반복 실행해도 안전합니다. **주의**: 기본 키 기준 Upsert이기 때문에, 시드 내용을 수정해도 기본 키가 그대로면 이전 레코드는 덮어쓰이지 않습니다——새 값을 보려면 이전 행을 삭제하고 재시작하세요.

### 3. Service 작성

`GameFrameX.Application.Game/Service/`(또는 System / Client / Tenant) 아래에 서비스를 생성합니다:

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

끝입니다. 이제 여섯 개의 인터페이스가 자동으로 생깁니다:

| 엔드포인트 | 설명 |
|------|------|
| `Page` | 페이징 조회 |
| `Add` | 추가 |
| `Update` | 수정 |
| `Get` | Id로 단건 조회 |
| `List` | 목록(페이징 없음) |
| `Delete` | 삭제(소프트 삭제) |

드롭다운 셀렉터 지원이 필요하면 기본 클래스를 `GenericCrudSelectService`로 바꾸면, `Select`와 `CheckNameUnique` 두 엔드포인트가 추가로 생깁니다.

### 4. (필요 시) 커스텀 DTO

간단한 시나리오는 기본 클래스의 것을 그대로 재사용하세요: `BaseAddInput`, `BaseUpdateInput`(`Id` 포함), `BaseIdInput`, `BasePageInput`(`SearchKey`, `Page`, `PageSize`, 정렬 필드 포함).

필드가 부족할 때는 서비스의 `Dto/` 하위 디렉터리 또는 `GameFrameX.Core.Extension/Dto/`에 추가합니다:

```csharp
// 출력 DTO——엔티티를 상속하기만 하면 됨
public class GameItemOutput : GameItemEntity, IBasePageOutput<GameItemEntity> { }

// 수정 DTO——기본 클래스 상속 후 필드 추가
public class UpdateGameItemInput : BaseUpdateInput
{
    [Required] public string Name { get; set; }
}
```

### 5. 프런트엔드에 알리기

백엔드 OpenAPI가 변경되면, 프런트엔드에서 `openapi.json`을 다시 내보내고 `pnpm gen:api`를 실행해야 합니다. 그렇지 않으면 프런트엔드 호출 지점이 맞지 않습니다. 구체적인 절차는 [프런트엔드용 OpenAPI 내보내기](#프런트엔드용-openapi-내보내기)를 참조하세요.

---

## 개발 규칙

> 이 절은 **규칙**입니다——프로젝트에서 모두 이렇게 하므로, 따르면 편합니다.

### Admin API vs Hub API: 두 체계를 섞지 마세요

| 차원 | Admin API | Hub API |
|------|-----------|---------|
| 라우팅 | 동적 API(`IDynamicApiController`) | 전통적 Controller(`[Route("api/[controller]/[action]")]`) |
| 데이터 접근 | Application Service 경유 → 리포지토리 | Controller에 `Repository<T>` 직접 주입 |
| 응답 래핑 | `AdminResultProvider`가 자동으로 감쌈 | `HttpJsonResultData<T>` 수동 반환 |
| Application 계층 재사용 | 예 | 아니오(Validator / CheckXxxUnique는 Hub에 죽은 코드) |
| 새 인터페이스 추가 | Service 작성 | Controller 생성 + 리포지토리 주입 |

Hub에 인터페이스를 추가할 때 Admin 쪽의 계층화된 검증을 재사용하려고 하지 **마세요**——Hub에 직접 작성합니다.

### 리포지토리 확장 메서드: 바퀴부터 찾고 만들기

`GameFrameX.Core.Extension/RepositoryExtension.cs`에 자주 쓰는 것들이 이미 준비되어 있습니다(Hub API의 `Repository<T>`도 일부 사용 가능, 예: 낙관적 잠금 업데이트):

| 메서드 | 설명 |
|------|--------|
| `FakeDelete` / `FakeDeleteAsync` | 소프트 삭제(`IsDeleted = true`) |
| `OrderBuilder(input)` | `BasePageInput` 기준 동적 정렬 |
| `UpdateWithDiffLog` / `InsertWithDiffLog` | 감사 로그가 딸린 추가/수정 |
| `UpdateWithOptimisticLockAsync` | 낙관적 잠금 업데이트(Hub API 빌드 보고에서 사용) |
| `AS<T>()` | 크로스 데이터베이스 조회 / 조작 |
| `RunWithoutFilter` | 쿼리 필터 임시 우회(예: 삭제된 데이터 조회) |

### 정기 작업

`IJob` 인터페이스를 구현하고, 관리 백오피스의 "정기 작업" 페이지에서 구성하거나 `[JobDetail]` 특성으로 표시합니다:

```csharp
[JobDetail("job_my_task", Description = "내 정기 작업")]
public class MyTask : IJob
{
    public async Task ExecuteAsync(JobExecutingContext context, CancellationToken ct)
    {
        // 작업 로직
    }
}
```

작업은 세 가지 타입을 지원합니다: 내장 타입, 스크립트 컴파일, HTTP 호출. `DbJobPersistence`로 데이터베이스에 영속화되므로 다중 인스턴스 배포 시에도 조율이 되어, 여러 인스턴스가 같은 작업을 동시에 실행하지 않습니다.

> 주의: 주문 발송 부분의 프로세스 잠금은 다중 인스턴스에서 실패합니다. 이 부분은 아직 평가 중이므로 인스턴스 간 상호 배제를 기대하지 마세요.

### 미들웨어 파이프라인: 요청은 어떻게 계층별로 처리되는가

Admin API의 요청 처리 순서는 다음과 같습니다(바깥부터 안쪽):

```
예외 처리 / HSTS
  → 통일 오류 상태 코드
  → 로컬라이제이션
  → 정적 파일
  → 라우팅
  → CORS
  → IP / 클라이언트 속도 제한
  → 인증 / 인가
  → Schedule UI
  → Knife4j (Swagger)
  → Project DI
  → SignalR Hub + MVC 라우팅
```

Hub API의 파이프라인은 다르며, [Hub API: 세 번째 문](#hub-api-세-번째-문)을 참조하세요. 요청 문제를 조사할 때 해당 호스트의 파이프라인 순서로 "어느 계층이 막았는지" 생각하는 것이, 무작정 재시작하는 것보다 효과적입니다.

### C# 코드 스타일

이 프로젝트는 **C# 10 구문**, **.NET 10 런타임**을 따릅니다(`Directory.Build.props`: `LangVersion=10` + `net10.0`). record, target-typed new, file-scoped namespace, global using을 사용할 수 있습니다. 단, 하나의 철칙: **모든 제어 흐름 문에는 중괄호가 필수**입니다. 본문이 한 줄이어도 예외가 아닙니다. `using var x = ...;` 같은 한 줄 using 선언도 사용하지 말고, `using (var x = ...) { }`로 성실하게 작성하세요.

---

## 자주 묻는 질문과 함정

### FAQ

**Service를 작성했는데 인터페이스가 호출되지 않나요?**
세 가지를 확인하세요: 클래스가 `IDynamicApiController`를 구현했는지, 메서드가 public인지, `[NonAction]`이 표시되지 않았는지. 그래도 안 되면 시작 로그를 보세요. 라우팅 등록 시 출력됩니다.(이것은 Admin API에만 해당——Hub API는 직접 작성한 Controller라 동적 라우팅이 없습니다.)

**프런트엔드가 호출하는 URL이 클래스명과 맞지 않나요?**
"Service 접미사 제거 + 첫 글자 소문자" 규칙을 잊은 것입니다(Admin API). `SystemUserService`의 URL은 `/api/systemUser/...`이지 `/api/systemUserService/...`가 아닙니다. 프런트엔드 URL에 `Service`가 있어서는 안 됩니다. Hub API는 이 규칙을 따르지 않으며, URL은 Controller명 + Action명으로 조합됩니다.

**엔티티를 수정했는데 테이블 구조가 업데이트되지 않나요?**
Code-First는 기본적으로 테이블 생성, 컬럼 추가만 수행합니다. 컬럼 삭제, 컬럼 타입 변경 같은 파괴적 변경은 수동 처리하거나 재시작 시 주의가 필요할 수 있습니다. 프로덕션 환경에서 테이블 변경 전에 먼저 백업하세요.

**SuperAdmin이 보는 데이터가 남들보다 많은데 버그인가요?**
아닙니다. SuperAdmin은 모든 테넌트 필터와 소프트 삭제 필터를 통과하므로 전체를 볼 수 있습니다. 의도된 설계입니다. **단 Hub API는 SuperAdmin의 접근을 거부합니다**——Hub는 테넌트 도메인만 서비스합니다.

**시드 데이터를 수정했는데 재시작해도 여전히 이전 값인가요?**
시드는 기본 키 기준 Upsert입니다. 기본 키가 그대로면 기존 레코드가 덮어써지지 않습니다. 해당 행을 삭제하고 재시작하거나, 기본 키를 바꾸세요.

**Hub API에서 401이 뜨는데, 같은 token으로 Admin API를 호출하면 정상인가요?**
Hub는 Admin의 JWT 키를 재사용하지만, 자체 `appsettings.json`의 `JWTSettings` 노드를 읽습니다. 양쪽 키(`IssuerSigningKey` / `ValidIssuer` / `ValidAudience`)가 반드시 일치해야 합니다. 불일치하면 발급된 token을 서로 인정하지 않습니다.

**빌드 보고(PackageBuildReport)에서 401?**
JWT 인증이 아니라 머신 자격 증명(AccessKey + 서명 + scope)입니다. `TenantGameAppOpenAccessAuthMiddleware`를 거치며, 자격 증명은 Admin 백오피스의 개방형 인터페이스 관리에서 먼저 생성하고 해당 App에 바인딩해야 합니다.

### 함정

**다중 데이터베이스 라우팅이 동작하지 않음**
엔티티의 특성이 올바르게 표시되었는지 확인하세요: `[SystemTable]` / `[LogTable]` / `[Tenant]`. 마커 없는 엔티티는 기본적으로 JWT Claim의 TenantId로 라우팅되며, 로그인 컨텍스트가 없으면 메인 DB로 라우팅됩니다.

**감사 필드가 비어 있음**
감사는 SqlSugar AOP로 자동 채워집니다. 리포지토리를 우회해 `SqlSugarClient`를 직접 조작하면 AOP가 실행되지 않아 필드가 비게 됩니다. 성실하게 `SqlSugarRepository<T>`를 사용하세요.(Hub API는 리포지토리를 직접 사용하므로 문제없습니다.)

**JWT가 계속 만료됨 / 자동 갱신이 동작하지 않음**
응답 헤더에 `X-Authorization`이 있는지 확인하세요. 없다면 `JwtHandler`의 갱신 로직과 token 유효 기간 구성을 점검하세요. 플랫폼 JWT 구성은 메인 DB `SystemConfig`의 `platform_jwt_settings`로 이동되었으니, 더 이상 `JWT.json`을 수정하지 마세요.

**Hub에 인터페이스를 추가했더니 검증 로직이 Admin과 맞지 않음**
정상입니다. Hub는 Admin의 Application 계층 검증을 재사용하지 않습니다. Hub Controller에 직접 작성하거나, 양쪽 검증 로직을 각각 유지하는 것을 받아들이세요——이것이 Hub의 현재 명확한 트레이드오프입니다.

**정기 작업이 다중 인스턴스에서 중복 실행됨**
작업은 데이터베이스 + 클러스터 조율에 의존합니다. `DbJobPersistence`가 정상 동작하는지, 클러스터 조율 구성이 빠지지 않았는지 확인하세요. 주문 발송 부분의 프로세스 잠금은 다중 인스턴스에서 실제로 실패하며, 알려진 함정입니다.

---

## 배포

### Docker Compose(권장)

```bash
cd .. && docker compose up --build   # 통합 오케스트레이션은 리포지토리 루트에 위치(프런트엔드 포함 총 5개 서비스)
```

한 번의 명령으로 세 개의 호스트를 모두 실행합니다. 포트 매핑은 로컬 `dotnet run`과 **완전히 동일**합니다:

| compose 서비스 | 컨테이너명 | 포트 | 해당 호스트 | Dockerfile |
|--------------|--------|------|----------|-----------|
| `web-api` | `gameframex-web-api` | `20802:20802` | Admin API | `Dockerfile.web-api` |
| `client-api` | `gameframex-client-api` | `20808:20808` | Client API | `Dockerfile.client-api` |
| `hub-api` | `gameframex-hub-api` | `20806:20806` | Hub API | `Dockerfile.hub-api` |

세 개의 Dockerfile 모두 다단계 빌드(`sdk:10.0` 빌드 → `aspnet:10.0` 실행)이며, root가 아닌 사용자로 실행되고 시간대는 `Asia/Shanghai`로 설정됩니다. 구성 파일은 volume으로 마운트되므로 **이미지 빌드 후에도 구성을 변경할 수 있어** 다시 build할 필요가 없습니다. 세 개의 컨테이너는 호스트 머신의 PostgreSQL을 공유합니다(`host.docker.internal:5432` 경유). 별도의 데이터베이스 컨테이너를 띄우지 않습니다.

> Swagger는 Production 컨테이너에서 기본적으로 마운트되지 않으며, 세 호스트 모두 `Swagger__Enabled=true`로 명시적으로 활성화합니다(compose의 환경 변수 섹션 참조).

> 역사적 명명: compose 서비스명은 과거 `web-entry`(구버전 "Web.Entry"에서 유래)였으나, 2026-10-20부터 `web-api`로 변경되어 GameFrameX.Web.Api와 정렬되었습니다.

### 출시 전 체크리스트: 데이터베이스 재적재와 컴팩트 숏 ID

컴팩트 숏 ID 레이아웃의 출시 방침은 **리셋 가능, 마이그레이션 스크립트 없음**입니다(2026-09-15 결정): 신규/기존 ID 레이아웃은 호환되지 않으므로, 프로덕션 PG 메인 DB / 테넌트 DB는 "비우고 재적재" 방식으로 출시하며, 신규/기존 ID 혼재 쓰기를 허용하지 않습니다. 출시 전에 확인하세요:

1. **비우고 재적재**: 프로덕션 PG 메인 DB + 전체 테넌트 DB를 비우고, 각 host 시작 시 재구축(CodeFirst 테이블 생성 + 시드 데이터 재적재). 기존 DB의 18-19자리 긴 Id 데이터를 새 레이아웃으로 마이그레이션하지 마세요——신규/기존 Id 공간 중복은 데이터 무결성 사고입니다.
2. **WorkerId 상호 배제 확인**: 동일한 DB에 쓰는 모든 인스턴스(Web.Api 복제본 / Hub / Client)의 `SnowFlake__WorkerId`가 서로 다르고 0-7 범위 내인지 확인. 기본값 1/2/3은 단일 머신 배포에서만 충돌이 없음을 보장; 복제본 확장 전에 반드시 명시적으로 할당하세요.
3. **규모 샘플 검사**: 재적재 후 비즈니스 테이블의 기본 키를 샘플링하면 13-14자리 숫자여야 합니다(< 2^53). 18-19자리가 보이면 Foundation 발급 경로를 벗어난 것이므로 중단하고 조사하세요.

---

## 프런트엔드용 OpenAPI 내보내기

프런트엔드의 TypeScript API 클라이언트는 `openapi.json` + `pnpm gen:api`로 생성됩니다. **백엔드는 Swagger JSON 엔드포인트를 노출하는 것만 담당**하며, 내보내기 작업은 프런트엔드 리포지토리에서 수행합니다.

| 항목 | 값 | 출처 |
|---|---|---|
| Admin Swagger 문서명 | `web.api` | `GameFrameX.Web.Core/ApiRouting/AdminOpenApiGroups.cs` |
| Admin Swagger JSON | `http://<host>:20802/swagger/web.api/swagger.json` | `AdminOpenApiGroups.WebGroup.RouteTemplate` |
| Client API Swagger JSON | `http://<host>:20808/swagger/client.api/swagger.json` | `GameFrameX.Client.Api/Program.cs` |
| Hub API Swagger JSON | `http://<host>:20806/swagger/v1/swagger.json` | `GameFrameX.Hub.Api/Program.cs`(`SwaggerDoc("v1", ...)`) |
| **기본 스위치**(세 호스트 동일) | 개발 환경에서 자동 켜짐; 다른 환경에서는 `Swagger:Enabled=true` 필수(환경 변수 `Swagger__Enabled=true`) | 각 `Program.cs` / `Startup.cs` |
| **프로덕션 / Docker 기본 미노출** | 예——Swagger 미들웨어가 애초에 마운트되지 않아 `/swagger/...` 엔드포인트가 없음 | 동일 |

> Hub API의 Swagger 문서명은 `v1`입니다(`hub.api`가 아님). 따라서 URL이 `/swagger/v1/swagger.json`입니다. Admin 프런트엔드는 보통 Admin API의 `web.api`만 사용하며, Hub와 Client의 swagger는 주로 게임 클라이언트 / CI 통합 시 인터페이스 확인용입니다.

프로덕션에서 Swagger가 기본적으로 마운트되지 않는 것은 의도된 것입니다——API 문서를 공개망에 그대로 노출하지 마세요.

```bash
# 1. Admin API의 Swagger에 접근 가능하게 하기(둘 중 하나)
ASPNETCORE_ENVIRONMENT=Development dotnet run --project GameFrameX.Web.Api &
# 또는
Swagger__Enabled=true dotnet run --project GameFrameX.Web.Api &

# 2. 프런트엔드 리포지토리 루트로 내려받기
curl -sSL http://localhost:20802/swagger/web.api/swagger.json \
  -o ../GameFrameX.Admin.Unified.Vue/openapi.json
ls -lh ../GameFrameX.Admin.Unified.Vue/openapi.json   # 약 3 MB여야 정상

# 3. 프런트엔드에서 생성 실행
( cd ../GameFrameX.Admin.Unified.Vue && pnpm gen:api && git diff src/api/ )
```

> 백엔드 OpenAPI가 일단 변경되면(서비스 엔드포인트 추가 / 수정 / 삭제), 반드시 다시 내보내고 프런트엔드에서 `pnpm gen:api`를 실행해야 합니다. 그렇지 않으면 프런트엔드 호출 지점과 백엔드가 맞지 않습니다. 프런트엔드 `pnpm run quality`의 `check:routes`는 백엔드 메뉴 시드로 프런트엔드 컴포넌트 경로를 정렬 검사하며, 맞지 않으면 오류가 발생합니다.

---

## 명령어 치트시트

| 명령어 | 용도 |
|------|--------|
| `dotnet run --project GameFrameX.Web.Api` | Admin API 시작(20802) |
| `dotnet run --project GameFrameX.Client.Api` | Client API 시작(20808) |
| `dotnet run --project GameFrameX.Hub.Api` | Hub API 시작(20806) |
| `dotnet build` | 전체 솔루션 빌드 |
| `Swagger__Enabled=true dotnet run --project GameFrameX.Web.Api` | dev 외 환경에서 시작하며 Swagger 노출 |
| `ASPNETCORE_ENVIRONMENT=Development dotnet run --project GameFrameX.Web.Api` | dev 환경에서 시작(Swagger 자동 켜짐) |
| `cd .. && docker compose up --build` | Docker 원클릭 실행(리포지토리 루트 통합 오케스트레이션: PG + 3개 API + 프런트엔드) |
| `curl -sSL http://localhost:20802/swagger/web.api/swagger.json -o ../GameFrameX.Admin.Unified.Vue/openapi.json` | Admin Swagger JSON을 프런트엔드로 내보내기 |

자주 쓰는 주소:

| 주소 | 설명 |
|------|------|
| `http://localhost:20802/kapi` | Admin API의 Knife4j / Swagger UI |
| `http://localhost:20802/swagger/web.api/swagger.json` | Admin Swagger JSON(docName=`web.api`) |
| `http://localhost:20806/swagger` | Hub API의 Swagger UI |
| `http://localhost:20806/swagger/v1/swagger.json` | Hub Swagger JSON(docName=`v1`) |
| `http://localhost:20806/health` | Hub API 헬스 체크 |
| `http://localhost:20808` | Client API |
| `/hubs/onlineUser` | Admin API의 SignalR 온라인 사용자 Hub |

---

## 마치며

핵심 구호:

> **세 개의 호스트는 각자의 역할: Admin은 Service 작성이 곧 API 작성, Client는 로그인·결제 담당, Hub는 대외용 전통적 Controller. 엔티티 작성이 곧 테이블 생성, URL에 Service 없음, 멀티 테넌시는 자동 안전망. Hub는 Admin의 계층 구조를 재사용하지 않고 검증을 직접 작성. OpenAPI를 변경했으면 프런트엔드에게 재생성을 알리는 것을 잊지 말 것.**

막히면 혼자 끝까지 붙잡지 말고: 먼저 위의 함정을 확인하고, 해결되지 않으면 `gfx-doc/`(프로젝트 루트의 문서 시스템)를 뒤져보고, 그래도 안 되면 동료에게 물으세요.

구체적인 코드를 보고 싶다면 이곳으로:

- `GameFrameX.Web.Core/Startup.cs` —— Admin 시작 구성, DI 등록, 미들웨어 파이프라인
- `GameFrameX.Hub.Api/Program.cs` —— Hub 시작 구성, 미들웨어 파이프라인, JWT 재사용
- `GameFrameX.Hub.Api/Controllers/PackageBuildReportController.cs` —— Hub의 "리포지토리 직접 조작, Application Service 호출 안 함" 예제
- `GameFrameX.Core.SqlSugar/` —— SqlSugar 구성, AOP, 다중 데이터베이스 라우팅
- `GameFrameX.Core/` —— 리포지토리 기본 클래스, `GenericCrudService`, 인증, SignalR
- `GameFrameX.Core.Extension/RepositoryExtension.cs` —— 리포지토리 확장 메서드(자주 쓰는 유틸리티)
- `GameFrameX.Application/SeedData/` —— 시드 데이터
- `GameFrameX.Application/Configuration/` —— 독립 JSON 구성 파일

## 문서 및 자료

- 프로젝트 문서 사이트: <https://gameframex.doc.alianblank.com>
- 프로젝트 개요: [`../README.ko.md`](../README.ko.md)
- 프런트엔드 실행 매뉴얼: [`../GameFrameX.Admin.Unified.Vue/README.ko.md`](../GameFrameX.Admin.Unified.Vue/README.ko.md)
- Issue 제출: <https://github.com/GameFrameX/Admin/issues>

## 커뮤니티 및 지원

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

## 변경 로그

릴리스 기록은 [GitHub Releases](https://github.com/GameFrameX/Admin/releases)에서 확인하세요.

## 라이선스

자세한 내용은 [LICENSE.md](LICENSE.md) 파일을 참조하세요.

<!--
EN: See [LICENSE.md](LICENSE.md) for license information.
zh-CN: 详见 [LICENSE.md](LICENSE.md) 文件。
zh-TW: 詳見 [LICENSE.md](LICENSE.md) 檔案。
ja: 詳しくは [LICENSE.md](LICENSE.md) をご参照ください。
ko: 자세한 내용은 [LICENSE.md](LICENSE.md) 파일을 참조하세요.
-->
