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

인디 게임 개발자를 위한 올인원 솔루션 · 인디 개발자의 꿈을 실현

<br />

[문서](https://gameframex.doc.alianblank.com) · [빠른 시작](#빠른-시작) · QQ 그룹: 467608841 / 233840761
<br />

[English](README.md) | [简体中文](README.zh-CN.md) | [繁體中文](README.zh-TW.md) | [日本語](README.ja.md) | **한국어**

</div>

---

GameFrameX 게임 관리 플랫폼의 프런트엔드 프로젝트로, Vue 3 + TypeScript + Vite + PrimeVue 기반으로 구축되었습니다.

> 본 프로젝트는 [vue-next-admin](https://gitee.com/lyt-top/vue-next-admin)에서 fork 하여 GameFrameX의 멀티 테넌트 / 앱 스코프 모델에 맞게 개조한 것입니다. 현재 버전 ![version](https://img.shields.io/github/package-json/v/GameFrameX/Admin?filename=GameFrameX.Admin.Unified.Vue/package.json)(package.json 기반, 자동 동기화).

## 목차

- [프로젝트 개요](#프로젝트-개요)
- [기술 스택](#기술-스택)
- [저장소 디렉터리 구조](#저장소-디렉터리-구조)
- [빠른 시작](#빠른-시작)
- [동작 방식](#동작-방식)
- [API 및 백엔드 규칙](#api-및-백엔드-규칙)
- [새 페이지 추가하기](#새-페이지-추가하기)
- [개발 규칙](#개발-규칙)
- [자주 묻는 질문과 함정](#자주-묻는-질문과-함정)
- [배포](#배포)
- [명령어 요약](#명령어-요약)
- [백엔드에서 OpenAPI 내보내기](#백엔드에서-openapi-내보내기)
- [마치며](#마치며)
- [문서 및 자료](#문서-및-자료)
- [변경 로그](#변경-로그)

---

## 프로젝트 개요

**GameFrameX 게임 관리 플랫폼**을 위한 프런트엔드입니다. 운영, GM, 고객 지원, 인프라 담당자가 매일 브라우저에서 메뉴를 수정하고, 사용자를 추가하고, 주문을 확인하고, 버전을 배포하고, 로그를 확인합니다——**이 프로젝트가 바로 그 일을 합니다**.

백엔드는 .NET으로 작성되었으며 저장소는 [`GameFrameX.Admin.Unified.Api`](../GameFrameX.Admin.Unified.Api)에 있고, 데이터베이스는 기본적으로 PostgreSQL입니다. 프런트엔드는 Vue 3 + Vite + PrimeVue + Pinia + Vue Router + vue-i18n + Axios 같은 널리 쓰이는 기술 스택을 사용하며, 별난 기교는 없습니다.

**꼭 알아야 할 세 가지**:

1. **메뉴, 권한, 문구는 모두 백엔드가 제어**합니다. 프런트엔드는 메뉴를 직접 작성하지 않고, 메뉴 추가는 백엔드 메뉴 관리에서 해야 합니다. 프런트엔드는 백엔드가 반환한 메뉴 트리를 렌더링할 뿐입니다.
2. **백엔드 동적 라우팅**. 로그인 후 프런트엔드는 `/api/globalMenu/loginMenuTree`를 호출해 메뉴를 가져오고, `component` 필드로 `src/views/`에서 해당 `.vue` 파일을 찾은 뒤 `router.addRoute()`로 등록합니다. 따라서 페이지 파일만 추가하면 되고, 라우팅 파일은 건드릴 필요가 없습니다.
3. **JWT 자동 갱신**. 로그인으로 받은 token은 `localStorage`에 저장되며, 만료 전에 프런트엔드가 요청 헤더에 `X-Authorization`을 넣어 백엔드가 조용히 재발급하므로, 한 번 로그인하면 오래 사용할 수 있습니다.

---

## 기술 스택

| 구분 | 선택 | 버전 |
|---|---|---|
| 프레임워크 | Vue 3（Composition API + `<script setup lang="ts">`） | 3.5.x |
| 빌드 | Vite | 6.x |
| UI 컴포넌트 라이브러리 | PrimeVue（Aura 테마）+ PrimeFlex + PrimeIcons | 4.5.x |
| 상태 관리 | Pinia + pinia-plugin-persistedstate | 3.x |
| 라우팅 | Vue Router（hash 모드, 백엔드 동적 제어） | 4.x |
| 언어 | TypeScript（strict） | 5.3.x |
| 국제화 | vue-i18n（5개 언어） | 11.x |
| 차트 | ECharts + echarts-gl + echarts-wordcloud | 6.x |
| 실시간 통신 | @microsoft/signalr | 10.x |
| 리치 텍스트 | wangEditor + editor-for-vue | 5.x |
| 코드 에디터 | Monaco Editor | 0.44.x |
| HTTP | Axios + 자체 제작 인터셉터 | 1.x |
| 단위 테스트 | Vitest（jsdom + @vue/test-utils） | 3.x |
| 엔드투엔드 테스트 | Playwright（시스템 Edge） | 1.64.x |
| 아이콘 | FontAwesome 7（vue-fontawesome + 자동 온디맨드） | 7.x |
| 폼 | @primevue/forms + 자체 제작 formResolver | 4.5.x |
| 에디터 보조 | code-inspector-plugin（Shift 누른 채 클릭으로 소스 위치 이동） | — |
| 패키지 관리 | pnpm（Corepack 자동 활성화） | ≥ 11.5.2 |
| Node | — | ≥ 16（권장 20） |

> Vue Router는 **hash 모드**（`createWebHashHistory`）이므로 배포 시 서버 rewrite가 필요 없지만, URL에 `#`이 붙습니다.

---

## 저장소 디렉터리 구조

### 최상위

```
GameFrameX.Admin.Unified.Vue/
├── src/                        # 비즈니스 소스 코드 (업무 시간의 99%는 여기)
├── public/                     # 정적 리소스
│   └── config/config.js        # Vite 시작 시 생성되는 런타임 설정
├── scripts/                    # 공학 스크립트 (gen-api, check-frontend 등)
├── openapi.json                # 백엔드 Swagger 내보내기, pnpm gen:api의 입력
├── build/                      # Docker / Nginx / 배포 스크립트
├── Dockerfile                  # 다단계 빌드 이미지
├── .cnb.yml                    # CNB 클라우드 네이티브 빌드 / 배포 파이프라인
├── .env / .env.development / .env.production   # 환경 변수
├── eslint.config.mjs           # ESLint 9 flat config
├── .prettierrc.json
├── tsconfig.json / tsconfig.typecheck.json
├── vite.config.ts              # Vite 설정
├── index.html                  # SPA 진입점 (public/config/config.js 주입)
├── package.json                # scripts / dependencies
├── pnpm-lock.yaml
├── pnpm-workspace.yaml
├── CHANGELOG.md
├── LICENSE     # 오픈소스 라이선스 파일
└── README.md                   # 이 파일
```

### src/ 상세

```
src/
├── api/                          # 백엔드 호출 위치 (※)
│   ├── _shared/                  # 자동 생성된 공유 레이어 (HttpClient + data-contracts)
│   ├── <module-camel>/           # 백엔드 tag당 하나의 디렉터리, index.ts (Api class) + types.ts 포함
│   │   ├── index.ts              # 자동 생성 + 끝에 최상위 함수 export
│   │   └── types.ts              # 이 모듈에서만 사용하는 data-contracts
│   └── <비즈니스 수작성>.ts       # 극소수의 수작성 확장, 생성물과 충돌하지 않는 이름 사용
│
│   현재 존재하는 모듈 (일부):
│   systemAuth / systemUser / systemRole / systemMenu / systemConfig /
│   globalMenu / globalDictType / globalLanguage / globalPlatform /
│   gameAppReleaseVersion / gameAppAssetPackage / gameHomeEntity /
│   gameOrderEntity / gameMailEntity / gameRedeemCodeEntity /
│   platformCaptchaConfig / platformJwtConfig / platformPasswordPolicyConfig …
│
├── api-services/                 # 과거 유산 디렉터리, 비웠음, 무시
│
├── composables/                    # 컴포지션 함수 (※)
│   ├── useCrudTable.ts             테이블 페이지네이션 / 정렬 / loading 래핑
│   ├── useEditDialog.ts            추가 / 수정 다이얼로그 (감사 필드 제거, isEnabled 기본값)
│   ├── useTableColumnPersistence.ts  컬럼 너비 / 순서 / 표시 여부 로컬 영속화
│   ├── useCurrentApp.ts            현재 앱 (AppId) 상태 + 로드 / 전환
│   ├── useConfigAppScope.ts        앱 스코프 변경
│   ├── useRequiredFields.ts        필수 필드 집합
│   ├── usePrimeTablePage.ts        테이블 페이지네이션 hook
│   ├── useRouteAppIdQuerySync.ts   AppId query와 테이블 동기화
│   ├── formResolver.ts             폼 검증 규칙 어댑터
│   └── types/                      컴포지션 함수의 TS 타입
│
├── components/                   # 공용 컴포넌트
│   ├── crud-data-table/            PrimeVue DataTable의 CRUD 래핑
│   ├── crud-form-fields/           폼 필드 통일 렌더링
│   ├── base-edit-dialog/           추가 / 수정 다이얼로그 기반 클래스
│   ├── search-form/                검색 바, authPrefix 자동 권한 버튼 추가 지원
│   ├── time-column/                테이블 컬럼: long 초 자동 포맷
│   ├── form-label / form-help-text 폼 필드 라벨 / 도움말 텍스트
│   ├── app-confirm-dialog          전역 확인 다이얼로그
│   ├── config-app-scope-picker     앱 스코프 선택기
│   ├── narrow-screen-hint          좁은 화면 힌트
│   ├── notice-bar                  공지 바
│   ├── cropper / drag-verify / editor / icon-selector / reward-items-editor / svg-icon / table
│
├── directive/                    # 커스텀 디렉티브
│   ├── authDirective.ts            v-auth — 권한 없을 때 숨김
│   ├── customDirective.ts          v-waves / v-drag / v-reclick (연속 클릭 방지)
│   ├── blockLoading.ts             v-block-loading — 블록 전체 loading 오버레이
│   └── index.ts                    등록 진입점
│
├── i18n/                         # 국제화
│   ├── lang/                       프레임워크 수준 (zh-cn / en / zh-tw / ja / ko)
│   └── pages/                      비즈니스 수준, 모듈별 하위 디렉터리
│       ├── system/ / client/ / game/ / home/ / login/ / menu/
│       ├── personal/ / layout/ / errors/ / formI18n/ / chart/
│       └── platform* / global*Config/
│
├── layout/                       # 레이아웃
│   ├── splitter/                   메인 레이아웃 컨테이너
│   ├── navMenu/                    좌측 / 수평 메뉴
│   ├── navBars/                    상단 바 (사용자 / 알림 / 전체 화면 / 테마)
│   ├── routerView/                 keep-alive 컨테이너
│   ├── footer / logo / upgrade     기타 레이아웃 요소
│   └── index.vue                   레이아웃 진입점
│
├── router/                       # 라우팅 (※)
│   ├── index.ts                    라우트 인스턴스 + 가드
│   ├── frontEnd.ts                 프런트엔드 제어 라우팅 (폴백, 현재는 미사용)
│   ├── backEnd.ts                  백엔드 동적 라우팅 핵심
│   ├── appWorkspaceRoutes.ts       앱 워크스페이스 라우팅
│   ├── globalConfigRoutes.ts       전역 설정 라우팅
│   └── route.ts                    정적 / 폴백 라우팅 (/login / 404 / 401)
│
├── stores/                       # Pinia 상태
│   ├── themeConfig.ts              테마 / 레이아웃 / 다크 / 워터마크 / globalI18n / isRequestRoutes
│   ├── userInfo.ts                 사용자 정보 + authBtnList + constList + dictList
│   ├── routesList.ts               메뉴 트리
│   ├── tagsViewRoutes.ts           Tabs 바
│   ├── keepAliveNames.ts           keep-alive 캐시된 name 목록
│   └── index.ts                    Pinia 인스턴스 + 영속화 어댑터
│
├── theme/                        # SCSS 테마
│   ├── z-index.scss                통일된 z-index 단계
│   ├── breakpoints.scss            브레이크포인트 (--bp-lg = 1024)
│   ├── primevue-overrides.scss     PrimeVue token 오버라이드
│   └── common/transition.scss      공용 전환 효과
│
├── types/                        # TS 타입 선언
│
├── utils/                        # 유틸리티 함수 (순수 함수 유지)
│   ├── request.ts                  수작성 API용 axios 인스턴스
│   ├── axios-utils.ts              생성 API용 HttpClient + getAPI 싱글턴 캐시
│   ├── http/shared.ts              ※ 공유 인터셉터 (JWT / refresh / 오류 코드 i18n)
│   ├── storage.ts                  Local / Session (키에 __NEXT_NAME__: 접두사 추가)
│   ├── authFunction.ts             auth() / hasAnyAuth() 함수형 권한
│   ├── message.ts                  showSuccess / showError / showConfirm (PrimeVue Toast)
│   ├── watermark.ts                워터마크
│   ├── isEnabled.ts                IsEnabled 정규화 및 신규 추가 기본값
│   ├── rowVersion.ts               행 버전 읽기 (rowVersion / RowVersion 호환)
│   ├── targetTenant.ts             테넌트 간 헤더 (X-Target-Tenant-Id)
│   ├── primevue-locale.ts          PrimeVue 컴포넌트 언어 전환
│   ├── formatTime.ts               날짜 / 시간 포맷
│   ├── download.ts / exportExcel.ts  내보내기
│   ├── sanitize.ts                 DOMPurify 래핑
│   ├── fontawesome.ts              FA 자동 온디맨드
│   ├── icons.ts                    FA 이름 매핑
│   ├── build.ts                    CDN 설정
│   ├── appWorkspace.ts             ※ 앱 스코프 유틸리티 (resolveRouteScope 등)
│   ├── arrayOperation.ts / commonFunction.ts / base64Conver.ts / crudConfig.ts / getStyleSheets.ts / loading.ts / mitt.ts / other.ts / template-utils.ts
│   └── http/                       인터셉터 분리
│
├── views/                        # 비즈니스 페이지
│   ├── system/                     시스템 관리 (사용자 / 역할 / 메뉴 / 조직 / 테넌트 / 사전 / 로그 / 설정 / 알림 / 플러그인 / 인쇄 / 지역 / 온라인 사용자 ……)
│   ├── game/                       게임 관리 (서버 / 상품 / 주문 / 메일 / 교환 코드 / 상점 / 공지 / 아이템 / 재화 ……)
│   ├── client/                     클라이언트 관리 (플랫폼 / 버전 / 채널 / 기기 / 언어 / 리소스 패키지 / 오픈 API ……)
│   ├── appCenter/                  앱 센터 (appEntry / appSelector / appWorkspace / gameAppEntity 등)
│   ├── contentCenter / chart / chartCenter / chartCenter / opsCenter / trustCenter / automationCenter / goodsCenter / redeemCenter / webShopCenter / playerCenter
│   ├── home/                       홈 대시보드
│   ├── login/                      로그인
│   ├── personal/                   개인 센터
│   ├── error/                      404 / 401
│   └── global/                     전역 라우팅 컨테이너
│
├── App.vue                       # 루트 컴포넌트
├── main.ts                       # ※ 앱 진입점
├── auto-imports.d.ts             # unplugin-auto-import 생성
└── components.d.ts               # unplugin-vue-components 생성
```

### scripts/ 상세

```
scripts/
├── gen-api.mjs                   # pnpm gen:api — swagger-typescript-api로 프로그래매틱 생성
├── post-gen.mjs                  # 생성물 분할 + src/api/<module>/로 재조립
├── migrate-api-services.mjs      # 과거 마이그레이션 스크립트
├── migrate-to-api-dir.mjs        # api-services/를 api/로 병합
├── check-i18n-keys.mjs           # pnpm run lint:i18n — 5개 언어 키 완전성
├── check-frontend-standards.mjs  # pnpm run check:frontend — 크로스 플랫폼 규칙 게이트
│   ├─ modes: all / routes / permissions / api / api-query-get
│   │         / text / sys-abbreviation / text-baseline / self-test
│   ├─ routes         백엔드 메뉴 component ↔ src/views/<file>
│   ├─ permissions    프런트엔드 v-auth 선언 ↔ 백엔드 시드 Btn
│   ├─ api            수작성 vs 생성 API 중복 review
│   ├─ api-query-get  page/list/query류 인터페이스의 GET params 전송 금지
│   ├─ text           변경된 파일의 하드코딩 중국어 스캔
│   └─ sys-abbreviation C179 회귀: Sys/sys 비즈니스 약어 금지
├── smoke-test.mjs                # 스모크 테스트
├── fix-collisions.mjs            # 충돌 수정
├── frontend-standards-allowlist.json   # 규칙 예외 목록
└── i18n-text-baseline.json       # 하드코딩 중국어 예외 기준선 (C151 과거 부채)
```

---

## 빠른 시작

### 환경

- Node.js ≥ 16（권장 20）
- pnpm ≥ 11.5.2（`corepack enable`로 자동 활성화）

### 실행하기

```bash
git clone https://github.com/GameFrameX/Admin.git
cd Admin/GameFrameX.Admin.Unified.Vue
corepack enable
pnpm install
pnpm run dev    # 기본 http://localhost:20800
```

브라우저를 열고 → 로그인 페이지가 보이면 → 백엔드 담당자에게 계정을 요청하세요.

### 막혔다면?

| 증상 | 가장 유력한 원인 |
|---|---|
| `pnpm: command not found` | 미설치: `npm i -g pnpm@11.5.2` |
| 포트 20800 점유됨 | `.env`의 `VITE_PORT=9000` 변경 후 dev 재시작 |
| 브라우저 흰 화면 | 터미널 마지막 몇 줄 확인, Vite가 컴파일 오류를 출력함 |
| 로그인 실패 | 백엔드 없음 / 백엔드 미실행 / `.env.development`의 `VITE_API_URL`이 잘못됨 |

### 환경 변수

| 변수 | 설명 | 개발 기본값 |
|---|---|---|
| `VITE_API_URL` | 백엔드 API 기본 주소 | `http://127.0.0.1:20802` |
| `VITE_PORT` | dev 포트 | `20800` |
| `VITE_OPEN` | 브라우저 자동 열기 여부 | `false` |
| `VITE_OPEN_CDN` | CDN 외부화 활성화 여부 | `false` |
| `VITE_PUBLIC_PATH` | 프로덕션 공용 경로 | 비어 있음 |

Vite 시작 시 이 값들을 `public/config/config.js`의 `window.__env__`에 기록합니다. Docker 배포 시 `entrypoint.sh`가 컨테이너 환경 변수에 따라 이 파일을 다시 생성하므로, **동일한 이미지를 환경 변수로 다른 환경에 맞게 전환할 수 있습니다**.

---

## 동작 방식

### 시작 흐름 (`src/main.ts`)

`index.html`이 `public/config/config.js`를 로드（`window.__env__`에 기록）→ `src/main.ts` 로드 → 순서대로 마운트:

1. Pinia（영속화 플러그인 포함）
2. Router（hash 모드）
3. i18n（`src/i18n/**`에서 자동 glob）
5. PrimeVue + Aura 테마 + ToastService + ConfirmationService
6. `@vue-js-cron/light`, `vue-grid-layout`, `vue-signature-pad`, `vue3-tree-org`
7. 커스텀 디렉티브 (v-auth / v-waves / v-reclick / v-block-loading)

`App.vue`는 이어서 네 가지 작업을 합니다: 라우팅 변경 시 title 감지, 다크 테마 초기화, 워터마크 주입, PrimeVue Locale 바인딩, SignalR 메시지 서비스 초기화.

### 로그인과 메뉴 등록

브라우저에서 로그인 페이지 확인 → 계정 입력 → 프런트엔드가 `/api/systemAuth/login` 전송 → 백엔드 검증 + JWT 발급 → 프런트엔드가 token을 `localStorage`에 저장 → `/dashboard/home`으로 이동.

라우트 가드 `router.beforeEach`（`src/router/index.ts`）가 "아직 메뉴가 없다"고 판단하면 두 인터페이스를 병렬로 가져옵니다:

- `/api/systemAuth/userInfo` → 사용자 정보 + `buttons` 권한 배열
- `/api/globalMenu/loginMenuTree` → 메뉴 트리

메뉴는 대략 이렇게 생겼습니다 (DevTools Network에서 확인):

```json
[
  { "path": "/dashboard",    "name": "dashboard",  "component": "/home/index" },
  { "path": "/system/user",  "name": "systemUser", "component": "/system/user/index" }
]
```

`backEnd.ts`는 `import.meta.glob('../views/**/*.{vue,tsx}')`로 모든 뷰 파일을 미리 로드해 표로 만들고, `dynamicImport()`로 `component` 문자열을 컴포넌트 함수에 매핑한 뒤 `router.addRoute()`로 등록합니다.

**따라서 메뉴가 변경되면 백엔드 재시작 + 프런트엔드 재로그인해야 반영됩니다**. 프런트엔드 자체는 메뉴 목록을 작성하지 않습니다.

버튼 권한도 비슷합니다: `<Button v-auth="'systemUser:add'">` 커스텀 디렉티브가 `buttons` 배열에서 조회하고, 없으면 `display: none`으로 숨깁니다.

### HTTP 인터셉터 (`src/utils/http/shared.ts`)

두 개의 axios 인스턴스가 동일한 인터셉터 팩토리를 공유합니다:

- `src/utils/request.ts`의 `service` —— 수작성 API용, GET params 직렬화 지원
- `src/utils/axios-utils.ts`의 `sharedHttpClient` —— swagger 생성 API용, `Accept-Language` 자동 추가

**요청 인터셉트** (4단계):

1. `Authorization: Bearer <access-token>` 자동 추가; JWT 만료가 임박하면 동시에 `X-Authorization: Bearer <x-refresh-token>`을 추가해 백엔드가 조용히 재발급
2. `X-Target-Tenant-Id` 자동 추가 (테넌트 간 전환 시 `utils/targetTenant.ts`가 제어)
3. `request.ts`는 GET 요청의 `data`를 query string으로 직렬화 (`tansParams`)
4. `axios-utils.ts`는 `Accept-Language: <globalI18n>` 추가

**응답 인터셉트** (4단계):

1. HTTP 401 → token 삭제 및 reload (`clearAccessTokens`)
2. 응답 헤더의 `access-token` / `x-access-token` → LocalStorage에 자동 기록 (token 조용한 재발급)
3. 비즈니스 코드 `code === 200`이면 통과; 기타 오류 코드:
   - `errors.codes.<ERROR_CODE>`가 i18n에 있으면 → 현지화 문구 표시
   - `VALIDATION.FAILED` → 백엔드 원본 message 투과 (필드 수준 오류 보존)
   - 기타 → 백엔드 `message` 표시
4. 카멜케이스화 (첫 글자 대문자 → 소문자) + 오류 알림은 PrimeVue Toast 사용

### 앱 스코프 (멀티 테넌트)

백엔드 메뉴 각 항목에는 `app_scope`（`platform` / `tenant` / `app`）와 `menu_context`가 붙습니다. 프런트엔드는 `meta.appScope` + `meta.menuContext`로 표시합니다:

| Scope | 의미 | 라우팅 예시 |
|---|---|---|
| `platform` | 플랫폼 수준 (최고 관리자만 보임, 테넌트 간) | `/platform/*` |
| `tenant` | 테넌트 수준 (모든 앱 공통, appId query 없음) | `/dashboard/*`, `/system/*` |
| `app` | 앱 워크스페이스 내 (`appId` query 필수) | `/gameCenter/*` |

이동 로직은 `src/utils/appWorkspace.ts`의 `decideTenantAppEntry()` / `resolveRouteScope()`에 있으며, `router/index.ts`의 `resolveTenantAppRedirect()`가 가드에서 트리거합니다.

### Pinia 영속화

| Store | 저장 | 주요 필드 |
|---|---|---|
| `themeConfig` | localStorage | 테마 / 레이아웃 / 다크 / 워터마크 / globalI18n / `isRequestRoutes` |
| `userInfo` | sessionStorage (`pick: ['constList', 'dictList']`만) | 시스템 상수 + 사전 (시작이 느려서 캐시) |
| `routesList` | 영속화 안 함 | 현재 메뉴 트리 |
| `tagsViewRoutes` | 영속화 안 함 | Tabs 바 |
| `keepAliveNames` | 영속화 안 함 | keep-alive include |

모든 키에는 자동으로 `__NEXT_NAME__:` 접두사가 붙습니다 (`Local.setKey` 사용), 앱 간 충돌을 방지합니다.

---

## API 및 백엔드 규칙

### 2계층 API

| 계층 | 위치 | 출처 | 호출 |
|---|---|---|---|
| **생성 API** | `src/api/<module>/{index.ts,types.ts}` | swagger-typescript-api 프로그래매틱 생성 | `getAPI(SomeApi).someMethod()` |
| **수작성 API** | `src/api/<비즈니스>.ts` | 비즈니스에 필요한 확장 / 래핑 | `request({ url, method, data })` |

생성 API 호출 패러다임:

```ts
import { getAPI } from '/@/utils/axios-utils'
import { SystemUser } from '/@/api/systemUser'

const api = getAPI(SystemUser)                       // 싱글턴 캐시, 여러 번 호출해도 비용 없음
const res = await api.systemUserPagePost({ page: 1, pageSize: 20 })
const user = res.data.result.items[0]                 // 자동 정규화된 { code, result, ... }
```

수작성 API 호출 패러다임:

```ts
import request from '/@/utils/request'

export const pageX = (params: PageParams) =>
  request<Result<PageResult<X>>>({
    url: '/api/game/x/page',
    method: 'post',
    data: params,
  })
```

### URL 명명 (동적 API 라우팅 규칙)

백엔드는 자체 제작 `IDynamicApiController`（`Web.Core/ApiRouting/AdminApiRouteRules` 구현）로 service 클래스명에서 URL을 자동 생성합니다: **`Service` / `AppService` / `Controller` 접미사 제거 → 첫 글자 소문자 → `api/` 접두사 추가**.

| 백엔드 클래스명 | 프런트엔드 URL |
|---|---|
| `SystemAuthService` | `/api/systemAuth/...` |
| `GameAppService` | `/api/gameApp/...` |
| `SystemUserService` | `/api/systemUser/...` |

**프런트엔드 URL에 `Service` 접미사를 절대 남기지 마세요**. 신규 입문자가 100% 밟는 함정입니다.

### 오류 코드 → i18n

백엔드의 안정적인 오류 코드는 `src/i18n/pages/errors/<lang>.ts`의 `errors.codes.<CODE_WITH_UNDERSCORE>`에 자동 매핑됩니다:

- `AUTH.FORBIDDEN` → `errors.codes.AUTH_FORBIDDEN`
- `D0008` → `errors.codes.D0008`
- `VALIDATION.FAILED` → 백엔드 message 투과 (필드 수준 오류 보존)

새 오류 코드는 `errors/zh-cn.ts`에 해당 key를 추가하면 됩니다.

### 백엔드 계약 변경 흐름

1. 백엔드가 OpenAPI / Swagger 수정
2. 새 `openapi.json` 내보내기 → 이 저장소 루트에 교체
3. `pnpm gen:api` → `git diff src/api/` 확인
4. 필드 이름 변경 / 제거 → 프런트엔드 호출부 수정 (TS 컴파일이 오류를 냄)
5. `pnpm run check:frontend api` 실행 (수작성과 생성 중복 확인)
6. `pnpm run quality` 반드시 통과

---

## 새 페이지 추가하기

따라 하면 일상 업무의 80%를 익히게 됩니다. 「공지 사항」 목록 페이지를 추가해 봅니다.

### 1. 파일 생성

```
src/views/system/demo/
├── index.vue          # 목록 페이지
└── component/
    └── editDemo.vue   # 추가/수정 다이얼로그
```

### 2. 목록 페이지 작성

`src/views/system/demo/index.vue`의 핵심 구조 (전체 스타일은 생략):

```vue
<template>
  <div class="p-3">
    <!-- 상단: 검색 + 추가 -->
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

    <!-- 테이블 -->
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

### 3. 다이얼로그 작성

`src/views/system/demo/component/editDemo.vue`:

```vue
<template>
  <BaseEditDialog
    v-model:visible="state.isShowDialog"
    :default-form="() => ({ title: '', content: '' })"
    :add-api="(data) => getAPI(DemoApi).demoAddPost(data)"
    :update-api="(data) => getAPI(DemoApi).demoUpdatePut(data)"
    :rules="{ title: { required: true, message: '请输入标题' } }"
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

`CrudDataTable`과 `BaseEditDialog`는 프로젝트에 내장된 바퀴 (`src/components/`)로, 페이지네이션 / loading / 작업 컬럼 / 추가 수정 판단 / 제출 loading을 래핑했습니다. **직접 바퀴를 재발명하지 마세요**.

### 4. i18n 추가 (5개 언어 모두 필수)

`src/i18n/pages/system/demo/zh-cn.ts`:

```ts
export default {
  title: '通知公告',
  form: { title: '标题', content: '内容' },
}
```

나머지 4개 언어 파일도 **반드시 생성**해야 합니다 (기계 복사만 해도):

```
src/i18n/pages/system/demo/
├── zh-cn.ts
├── en.ts
├── zh-tw.ts
├── ja.ts
└── ko.ts
```

하나라도 빠지면 `pnpm run lint:i18n`이 fail 됩니다.

### 5. 백엔드에 메뉴 등록 요청

코드는 끝났지만 아직 페이지가 보이지 않습니다——메뉴는 백엔드가 관리합니다. 백엔드 담당자에게 알려주세요:

```
path:        /system/demo
name:        systemDemo
component:   /system/demo/index       ← src/views/ 내 경로와 일치해야 함
title:       通知公告
titleCode:   system.demo.title        ← zh-cn.ts에 존재해야 함
parent:      /system
```

백엔드 commit + 배포 후 프런트엔드에 재로그인하면 메뉴가 보입니다.

### 6. 제출

```bash
pnpm run quality      # 모든 게이트 한 번에 실행
git add .
git commit -m "feat(system.demo): 新增通知公告列表 + 弹窗"
git push
```

`pnpm run quality`가 실패하면 오류에 따라 수정하세요: i18n key 누락은 번역 보충, 권한이 메뉴 시드에 없으면 백엔드에 추가 요청, 하드코딩 중국어는 i18n로 전환.

---

## 개발 규칙

> 이 절은 **제약 사항**입니다——CI가 검사하며, 위반하면 build fail입니다.

### 명명 규칙

| 분류 | 스타일 | 예시 |
|---|---|---|
| 뷰 디렉터리 / 파일 | kebab-case | `src/views/system/user/index.vue` |
| API 모듈 디렉터리 | camelCase (백엔드 swagger tag) | `src/api/systemUser/` |
| Pinia store 파일 | camelCase | `themeConfig.ts` |
| Composables 파일 | `useXxx` | `useEditDialog.ts` |
| 유틸리티 함수 파일 | camelCase | `authFunction.ts` |
| 권한 문자열 | `<resource>:<action>` | `systemUser:page`, `gameHomeEntity:query` |
| 라우트 name | camelCase | `systemUser` |
| 라우트 path | kebab-case | `/system/user` |
| i18n key | 점 계층 | `system.user.title`, `common.button.search` |
| 오류 코드 key | 대문자 언더스코어 | `AUTH_FORBIDDEN` |

### 디렉터리 규칙

- 뷰 `<module>`은 백엔드 메뉴 `component`의 첫 세그먼트와 일치시켜, 사람 ↔ 라우팅 ↔ 소스의 3방향 위치 파악을 쉽게 합니다
- 다이얼로그는 `views/<module>/component/`에 통일
- 단일 페이지 전용 컴포넌트는 `views/<module>/component/`에 두고, `src/components/`(재사용 풀)에 넣지 **마세요**
- API 모듈명은 생성물과 같지 않게 (`check:api`가 표시해 사람이 review)

### 권한 규칙

- 템플릿에서는 `v-auth="'xxx'"` / `v-auth="['x','y']"` / `v-auth-all`
- `v-if` / props에서는 `auth('xxx')` 또는 `hasAnyAuth('x','y')` (`src/utils/authFunction.ts`)
- 모든 권한 문자열은 백엔드 메뉴 시드의 `MenuTypeEnum.Btn` 항목에 존재해야 하며, 그렇지 않으면 `pnpm run check:permissions` fail
- 새 권한 추가 흐름: 백엔드에 시드 항목 추가 → 프런트엔드에서 `v-auth` 참조 → `check:permissions` 실행

### i18n 규칙

- **모든 i18n 키는 5개 언어에 모두 존재해야 함** (`zh-cn / en / zh-tw / ja / ko`), `pnpm run lint:i18n`이 자동 검사
- 템플릿과 `<script setup>`에서 **하드코딩 중국어 엄격 금지**, `pnpm run check:frontend text`가 변경된 파일을 스캔
- 과거 부채는 `scripts/i18n-text-baseline.json` 지문으로 예외 처리; 마이그레이션 기간에는 `pnpm run check:frontend text-baseline`으로 증분 기준선 사용 가능
- 새 오류 코드는 `errors/zh-cn.ts`에 `errors.codes.<CODE_WITH_UNDERSCORE>` 추가

### HTTP 규칙

- **page / list / query / search / filter류 인터페이스는 GET으로 params / data 전송 금지**——`pnpm run check:frontend api-query-get`이 검사
- 백엔드 생성 API는 반드시 `xxxPagePost` / `xxxListPost` / `xxxQueryPost` 사용
- 수작성 API는 `request({ method: 'post', data: params })` 사용
- 비즈니스 코드에서 `Authorization` / `Accept-Language` / `X-Tenant-Id`를 수동으로 추가하지 **마세요**——인터셉터가 자동 처리
- 응답에서 수동으로 `showError`하지 **마세요**——인터셉터가 비즈니스 코드를 자동 처리

### 제출 흐름

```bash
pnpm run quality      # 필수 통과: lint + typecheck + test:unit + build + check:frontend
git add .
git commit -m "<scope>(<module>): <subject>"   # 중국어 / 영어 모두 가능, ≤ 60자
git push
```

> `pnpm run quality`로 묶인 총 게이트로, 2026-10-20부터 Vitest 단위/컴포넌트 테스트가 lint와 build 사이에 포함되었습니다;
> 단위 테스트에서 회귀가 잡히면 build 이전에 차단되어 통합 비용이 절감됩니다. 새로운 순수 로직 유틸리티는 `src/<dir>/<name>.test.ts`에 같은 위치에 단위 테스트를 추가하세요;
> 새 Vue 컴포넌트는 `src/components/form-label/index.test.ts`를 참고하세요 (`mount` + provider 주입 + slot 텍스트 검증).
> 개발 중에는 `pnpm run test:unit:watch`로 watch UI를 사용할 수 있습니다.

- scope: `feat / fix / refactor / docs / style / test / chore / perf`
- module: `system.user` / `api` / `router` / `docs` / `i18n`…
- body는 동기 + 핵심 변경 사항 설명
- 브랜치: `feat/<module>-<desc>` / `fix/<module>-<desc>`

### 위반 예시 (이렇게 쓰지 마세요)

```ts
// ❌ axios.get('/api/xxx', { params }) — query류 인터페이스의 GET 금지
const res = await axios.get('/api/user/page', { params })

// ✅
const res = await request({ url: '/api/user/page', method: 'post', data })

// ❌ localStorage.setItem('themeConfig', ...) — 접두사가 자동으로 붙지 않음
window.localStorage.setItem('themeConfig', JSON.stringify({...}))

// ✅
import { Local } from '/@/utils/storage'
Local.set('themeConfig', {...})

// ❌ URL에 Service 접미사 포함
const url = '/api/systemUserService/page'

// ✅
const url = '/api/systemUser/page'

// ❌ 템플릿에 하드코딩 중국어
<span>搜索</span>

// ✅
<span>{{ t('common.button.search') }}</span>

// ❌ isShowDialog / openDialog / submit 삼종세트 수작성
defineExpose({ isShowDialog, openDialog, submit })

// ✅ useEditDialog composable 사용
const { isShowDialog, openDialog, submit } = useEditDialog({ ... })
```

---

## 자주 묻는 질문과 함정

### FAQ

**메뉴 / 권한 / 문구는 왜 백엔드에서 오나요?**
프런트엔드는 "누가 무엇을 볼 수 있는지" 알 필요가 없기 때문입니다. 백엔드가 역할에 따라 계산해 반환하면 프런트엔드는 렌더링만 합니다. 백엔드가 변경되어 발행되면 프런트엔드는 수정할 필요가 없습니다.

**JWT가 뭔가요? 자동 갱신은 어떻게 되나요?**
영화 티켓입니다. 서버가 암호화된 티켓을 주고, 요청 헤더에 `Authorization: Bearer xxx`를 담습니다. 만료 전에 프런트엔드가 자동으로 `X-Authorization`을 추가해 백엔드가 조용히 재발급합니다.

**i18n은 왜 5개 파일인가요?**
다국어 제품이기 때문에 CI가 `zh-cn / en / zh-tw / ja / ko` 모두 존재를 강제합니다. 하나라도 빠지면 fail입니다. 새로 작성한 key는 먼저 zh-cn을 기계 복사하고 번역은 천천히 채우면 됩니다.

**`getAPI(...)`와 `request(...)` 중 무엇을 쓰나요?**
- `getAPI(SomeApi).someMethod()` —— 자동 생성 API (`src/api/<module>/`)
- `request({ url, method })` —— 수작성 API (`src/api/login.ts` 같은 것)
두 가지 모두 동일한 HTTP 인터셉터를 공유하므로, 직접 axios를 만들지 마세요.

**`v-auth` / `v-block-loading` / `v-reclick`가 뭔가요?**
- `v-auth="'xxx'"` —— 권한 없으면 `display: none`
- `v-block-loading="loading"` —— 블록 전체에 loading 오버레이
- `v-reclick` —— 버튼 연속 클릭 방지
정의는 `src/directive/`에 있으며, 템플릿에서 attribute처럼 바로 씁니다.

**`src/api-services/`는 뭘 하는 곳인가요?**
과거 유산 디렉터리로 폐기되었습니다. 새로 생성되는 API는 모두 `src/api/<module>/`에 있습니다.

### 함정

**로그인 후 흰 화면 / 메뉴가 안 나옴**
DevTools → Network에서 `/api/systemAuth/login`이 200인지 확인. 아니면 → `VITE_API_URL`과 백엔드 확인; 맞으면 → `/userInfo`와 `/loginMenuTree` 확인, 하나라도 200이 아니면 `/login`으로 이동됩니다. 둘 다 200인데도 흰 화면이면 → Console에서 JS 오류 확인, 대부분 component가 존재하지 않는 .vue를 가리킵니다.

**메뉴는 들어가지는데 페이지 404**
`pnpm run check:routes` 실행, 대부분 백엔드 메뉴의 `component` 경로가 `src/views/`와 맞지 않는 경우입니다.

**버튼이 전부 안 보임**
Network에서 `/userInfo` 응답의 `buttons` 필드에 기대한 문자열이 있는지 확인; 있는데 안 보이면 → `v-auth` 문자열이 **완전히 일치**해야 함 (대소문자 구분); 없으면 → 백엔드 담당자에게 문의, 당신의 역할이 이 권한을 받지 못한 것입니다.

**i18n에서 missing key 오류**
`pnpm run lint:i18n` 실행 후 오류에 따라 5개 언어 파일의 누락된 key를 보충하세요.

**`check:frontend text` 전체 폭발**
새로 하드코딩된 중국어가 있는지 확인 → i18n로 전환; 아니면 과거 부채가 마이그레이션 기간에 걸린 것 → `pnpm run check:frontend text-baseline`으로 증분 기준선 사용 (신중하게).

**Docker 배포 후 API 주소가 여전히 예전 것**
1. `docker exec -it <container> cat /usr/share/nginx/html/config/config.js`로 `window.__env__.VITE_API_URL` 확인
2. 예전 값이면 → 시작 시 `-e VITE_API_URL=...`을 전달하지 않은 것
3. 새 값인데 브라우저가 예전 것이면 → Ctrl+F5 / Cmd+Shift+R 강제 새로고침
4. 그래도 안 되면 → 컨테이너가 예전 이미지로 시작된 것, `docker pull`로 최신 가져오기

**빌드 OOM**
Docker Desktop → Preferences → Resources → Memory 8G+로 조정, Swap 4G+로 조정. 빌드 명령에는 이미 `NODE_OPTIONS=--max-old-space-size=4096`이 추가되어 있습니다.

**401 후 자동으로 로그인으로 이동 안 함**
`localStorage.access-token` 만료 여부; JWT payload `exp`; 백엔드 `/api/systemAuth/refresh-token`이 새 token을 반환하는지 확인.

---

## 배포

### Docker (권장)

> 단일 저장소 통합 (2026-10-19): Vue Dockerfile의 build context가 저장소 루트로 상향되었고,
> `build/default.conf` / `build/entrypoint.sh`도 저장소 루트 `build/`로 이동했습니다. 저장소 루트에서 빌드를 실행하세요.

```bash
docker build -t gameframex-admin-web:dev -f GameFrameX.Admin.Unified.Vue/Dockerfile .
docker run -d -p 80:80 gameframex-admin-web:dev                                          # 기본 API 주소
docker run -d -p 80:80 -e VITE_API_URL=https://api.example.com gameframex-admin-web:dev   # 사용자 지정 API
```

컨테이너에는 작은 마법이 있습니다: `build/entrypoint.sh`가 시작 시 환경 변수를 읽어 `config.js`를 다시 쓰므로, **동일한 이미지를 환경 변수로 다른 환경에 맞게 전환할 수 있습니다**.

### docker-build-push.sh 전체 매개변수

```bash
./build/docker-build-push.sh --tag v2.4.33
./build/docker-build-push.sh \
  --registry registry.cn-hangzhou.aliyuncs.com \
  --namespace myns \
  --tag v2.4.33
./build/docker-build-push.sh \
  --platforms linux/amd64,linux/arm64 \
  --tag v2.4.33
./build/docker-build-push.sh --no-push                # 푸시 없이 빌드만
./build/docker-build-push.sh \
  --registry registry.cn-hangzhou.aliyuncs.com \
  --namespace myns \
  --username your_user \
  --password your_pass \
  --tag v2.4.33                                       # 프라이빗 저장소 인증
```

| 매개변수 | 설명 | 기본값 |
|---|---|---|
| `--registry URL` | 컨테이너 저장소 주소 | 비어 있음 (Docker Hub) |
| `--namespace NAME` | 네임스페이스 / 사용자 | `alianblank` |
| `--name NAME` | 이미지 이름 | `gameframex-admin-web` |
| `--tag TAG` | 이미지 태그 | git tag 또는 `브랜치-날짜-SHA` |
| `--platforms LIST` | 대상 플랫폼 | `linux/amd64` |
| `--no-push` | 푸시 없이 빌드만 | — |
| `--username USER` | 저장소 사용자 이름 | — |
| `--password PASS` | 저장소 비밀번호 | — |

이미지 태그 자동 생성 규칙: 현재 commit에 git tag가 있으면 → tag 사용; 없으면 → `브랜치명-날짜-SHA`; 동시에 `latest`도 태깅.

### CNB 파이프라인

`main` / `develop` push 시 자동 빌드 + 테스트 환경 배포; tag를 붙이면 `latest` 동시 태깅 + 자동 배포. 자세한 규칙은 `.cnb.yml` 참고.

### 순수 정적

`pnpm run build` 산출물은 `GameFrameX.Admin.Unified.Vue/build/dist/`에 있습니다 (`vite.config.ts`의 `outDir`은 여전히 cwd 기준 상대 경로 `build/dist`이며, 하위 디렉터리 기준으로 적용됨). SPA fallback (`try_files $uri /index.html`)을 지원하는 HTTP 서버라면 무엇이든 호스팅할 수 있습니다. Nginx 템플릿은 저장소 루트 `build/default.conf`에 있습니다 (2026-10-19 단일 저장소 통합으로 상향된 Vue `build/`에서 이동됨).

### Docker Compose 예시

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

## 명령어 요약

| 명령 | 용도 |
|---|---|
| `pnpm run dev` | 개발 서버 시작 |
| `pnpm run build` | 프로덕션 빌드 (산출물은 `GameFrameX.Admin.Unified.Vue/build/dist/`, vite `outDir`은 cwd 기준 상대 경로) |
| `pnpm run lint` / `lint-fix` | ESLint 검사 / 자동 수정 |
| `pnpm run format` / `format-fix` | Prettier 검사 / 자동 수정 |
| `pnpm run typecheck` | tsc 타입 검사 |
| `pnpm run test:unit` / `test:unit:watch` / `test:unit:coverage` | Vitest 단위 / 컴포넌트 단위 테스트 (jsdom + @vue/test-utils, v8 커버리지 포함) |
| `pnpm run test:e2e` / `test:e2e:headed` / `test:e2e:list` / `test:e2e:report` | Playwright 엔드투엔드 (시스템 Edge, 스위트 순차 실행) |
| `pnpm gen:api` | `openapi.json`에서 프런트엔드 API 클라이언트 재생성 |
| `pnpm run lint:i18n` | 5개 언어 i18n 키 완전성 검사 |
| `pnpm run check:routes` | 백엔드 메뉴 component와 프런트엔드 뷰 정합성 확인 |
| `pnpm run check:permissions` | 프런트엔드 권한 선언과 백엔드 시드 정합성 확인 |
| `pnpm run check:api` | 수작성과 생성 API 중복 review |
| `pnpm run check:text` | 하드코딩 중국어 스캔 (변경된 파일에서 fail) |
| `pnpm run check:frontend` | 위의 모든 것 + Sys 약어 회귀 |
| `pnpm run quality` | **모든 게이트 한 번에 실행**, 제출 전 필수 |

`pnpm gen:api`의 전제 조건: 백엔드에서 `openapi.json` 내보내기——다음 절 참고.

---

## 백엔드에서 OpenAPI 내보내기

`pnpm gen:api`는 저장소 루트의 `openapi.json`만 인식하지만, **프런트엔드가 스스로 생성하지는 않습니다**. 백엔드 Swagger 엔드포인트에서 가져와야 합니다.

백엔드는 .NET + Swashbuckle이며, Swagger JSON은 다음에 노출됩니다:

```
http://localhost:20802/swagger/web.api/swagger.json
```

> Swagger는 **기본적으로 dev 환경에서만 켜집니다**. 프로덕션 / Docker에서는 `Swagger__Enabled=true`（또는 `Swagger:Enabled=true`）를 설정해야 이 엔드포인트가 노출됩니다——백엔드 `Startup.cs`의 스위치 동작으로, 프로덕션에서 기본적으로 문서가 유출되지 않도록 하기 위함입니다.

```bash
# 1. 백엔드 시작 (둘 중 하나로 Swagger 접근 가능)
ASPNETCORE_ENVIRONMENT=Development dotnet run --project ../GameFrameX.Admin.Unified.Api/GameFrameX.Web.Api &
# 또는
Swagger__Enabled=true dotnet run --project ../GameFrameX.Admin.Unified.Api/GameFrameX.Web.Api &

# 2. swagger.json 가져오기
curl -sSL http://localhost:20802/swagger/web.api/swagger.json -o openapi.json
ls -lh openapi.json   # 약 ~3 MB여야 함

# 3. 프런트엔드 생성 실행
pnpm gen:api
git diff src/api/     # 파괴적 변경이 있는지 확인
```

Client / Hub API도 같은 패턴입니다: `/swagger/client.api/swagger.json` (포트 20808), `/swagger/hub.api/swagger.json`. 프런트엔드 Admin 프로젝트는 Admin API만 사용하므로 일반적으로 이 두 개는 가져올 필요가 없습니다.

### 자동화 스크립트 (선택)

팀에서 자주 한다면 스크립트를 작성하세요 (직접 작성하세요, 제가 대신 쓰지는 않습니다). 최소 골격:

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

## 마치며

구호:

> **메뉴는 백엔드에서, 권한은 백엔드에서, 국제화는 반드시 5개 언어, 제출 전 `pnpm run quality` 실행.**

막히면 혼자 끝까지 붙잡지 말고: 먼저 함정 절을 보고, 해결되지 않으면 git log를 확인하고, 그래도 안 되면 동료에게 물어보세요.

구체적인 코드를 보려면:

- `src/router/index.ts` — 라우트 가드
- `src/router/backEnd.ts` — 백엔드 메뉴 → 프런트엔드 라우팅
- `src/utils/http/shared.ts` — HTTP 인터셉터
- `src/stores/userInfo.ts` — 사용자 정보 / 버튼 권한
- `src/composables/useEditDialog.ts` — 다이얼로그 핵심
- `src/utils/appWorkspace.ts` — 멀티 테넌트 앱 스코프
- `scripts/check-frontend-standards.mjs` — 크로스 플랫폼 규칙 게이트

## 문서 및 자료

- 프로젝트 문서 사이트: <https://gameframex.doc.alianblank.com>
- 프로젝트 개요: [`../README.ko.md`](../README.ko.md)
- 백엔드 실행 매뉴얼: [`../GameFrameX.Admin.Unified.Api/README.ko.md`](../GameFrameX.Admin.Unified.Api/README.ko.md)
- Issue 피드백: <https://github.com/GameFrameX/Admin/issues>

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

버전 변경 기록은 [`CHANGELOG.md`](./CHANGELOG.md)에서 확인하세요.

## 라이선스

자세한 내용은 [LICENSE](./LICENSE) 파일을 참조하세요.

<!--
EN: See [LICENSE](./LICENSE) for license information.
zh-CN: 详见 [LICENSE](./LICENSE) 文件。
zh-TW: 詳見 [LICENSE](./LICENSE) 檔案。
ja: 詳しくは [LICENSE](./LICENSE) をご参照ください。
ko: 자세한 내용은 [LICENSE](./LICENSE) 파일을 참조하세요.
-->
