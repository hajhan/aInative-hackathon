# Sprint 1 — 프로젝트 기반 설정 및 인증

> **구현 완료 일자**: 2026-03-13
> **기간**: 2주
> **담당 서비스**: SideReport (약물 부작용 보고 플랫폼)
> **목적**: 모든 기능 개발의 토대가 되는 인프라, 프로젝트 구조, 데이터베이스 스키마, 사용자 인증 시스템 구축

---

## 1. 스프린트 목표

| # | 목표 |
|---|------|
| 1 | Docker Compose로 Next.js + ASP.NET Core + PostgreSQL 전체 스택을 로컬에서 한 번에 실행 가능하게 한다 |
| 2 | ASP.NET Core Clean Architecture 프로젝트 구조(Api / Application / Domain / Infrastructure)를 확립한다 |
| 3 | 회원가입 → 로그인 → JWT 발급 → 토큰 갱신 E2E 인증 흐름을 완성한다 |
| 4 | Sprint 2~3의 기능 개발에 필요한 6개 DB 테이블 초기 마이그레이션을 완료한다 |
| 5 | Next.js PWA 기반 프론트엔드에서 로그인/회원가입 화면을 구현하고 백엔드 인증 API와 연동한다 |
| 6 | GitHub Actions CI 파이프라인으로 PR마다 빌드 및 테스트 자동 검증이 동작한다 |

---

## 2. 작업 목록

### 2-1. 인프라 & DevOps

#### T-INF-01: Docker Compose 구성
**목적**: 개발 환경 전체 스택을 단일 명령(`docker compose up`)으로 실행

구현 단계:
1. 루트 경로에 `docker-compose.yml` 작성
   - `frontend` 서비스: Next.js (Node 20 Alpine, 포트 3000)
   - `backend` 서비스: ASP.NET Core (dotnet 8 SDK, 포트 5000)
   - `db` 서비스: PostgreSQL 16 Alpine (포트 5432)
2. 각 서비스에 `healthcheck` 설정 추가 (backend는 `/health` 엔드포인트 대기)
3. `volumes` 설정으로 DB 데이터 영속화 (`pgdata` named volume)
4. 서비스 간 `depends_on` + `condition: service_healthy` 설정
5. `networks` 단일 내부 네트워크(`sidereport-net`) 구성

#### T-INF-02: 환경 변수 전략
**목적**: 개발/스테이징/프로덕션 환경 분리

구현 단계:
1. 루트에 `.env.example` 파일 작성 (커밋 대상)
2. `.env` 파일은 `.gitignore`에 추가 (시크릿 보호)
3. 환경별 변수 정의:
   - `POSTGRES_USER`, `POSTGRES_PASSWORD`, `POSTGRES_DB`
   - `JWT_SECRET_KEY`, `JWT_ISSUER`, `JWT_AUDIENCE`, `JWT_EXPIRY_MINUTES`
   - `REFRESH_TOKEN_EXPIRY_DAYS`
   - `ASPNETCORE_ENVIRONMENT` (Development / Staging / Production)
   - `NEXT_PUBLIC_API_BASE_URL`
4. `docker-compose.yml`에서 `env_file: .env` 참조

#### T-INF-03: GitHub Actions CI 파이프라인
**목적**: PR마다 빌드 및 테스트 자동화

구현 단계:
1. `.github/workflows/ci.yml` 작성
2. 트리거: `pull_request` (main 브랜치 대상), `push` (main 브랜치)
3. 백엔드 Job:
   - `dotnet restore` → `dotnet build --no-restore` → `dotnet test --no-build`
   - dotnet SDK 8.0 사용
4. 프론트엔드 Job:
   - `npm ci` → `npm run build` → `npm run lint`
   - Node 20 사용
5. 두 Job 병렬 실행 구성

#### T-INF-04: Azure Blob Storage / AWS S3 버킷 준비
**목적**: Sprint 2 OCR 이미지 업로드를 위한 사전 준비

구현 단계:
1. Azure Blob Storage 컨테이너 또는 S3 버킷 생성 (이름: `sidereport-ocr-images`)
2. 접근 정책: Private (공개 접근 차단)
3. Lifecycle 정책: 업로드 후 24시간 내 자동 삭제 설정 (개인정보 보호)
4. 접근 키/Connection String을 `.env.example`에 항목 추가

---

### 2-2. 백엔드 (ASP.NET Core, C#)

#### T-BE-01: Clean Architecture 프로젝트 구조 초기화
**목적**: 유지보수 가능한 레이어 분리 구조 확립

구현 단계:
1. `dotnet new sln -n SideReport` 솔루션 생성
2. 4개 프로젝트 생성 및 솔루션에 추가:
   - `SideReport.Domain` (Class Library): 엔티티, 도메인 이벤트, 공통 인터페이스
   - `SideReport.Application` (Class Library): Use Cases, DTOs, IRepository 인터페이스, IUnitOfWork
   - `SideReport.Infrastructure` (Class Library): EF Core DbContext, Repository 구현체, JWT 서비스
   - `SideReport.Api` (ASP.NET Core Web API): 컨트롤러, 미들웨어, DI 등록
3. 프로젝트 참조 설정:
   - `Api` → `Application` → `Domain`
   - `Infrastructure` → `Application`
   - `Api` → `Infrastructure` (DI 등록 목적)
4. NuGet 패키지 설치:
   - `Microsoft.EntityFrameworkCore.Design` (Api 프로젝트)
   - `Npgsql.EntityFrameworkCore.PostgreSQL` (Infrastructure)
   - `Microsoft.AspNetCore.Authentication.JwtBearer` (Api)
   - `Microsoft.AspNetCore.Identity.EntityFrameworkCore` (Infrastructure)
   - `BCrypt.Net-Next` (Infrastructure)
   - `Swashbuckle.AspNetCore` (Api)

#### T-BE-02: PostgreSQL 연결 및 Entity Framework Core 설정
**목적**: DB 연결 및 ORM 기반 마련

구현 단계:
1. `Infrastructure` 프로젝트에 `ApplicationDbContext` 클래스 작성
   - `IdentityDbContext<ApplicationUser>` 상속
   - `DbSet<>` 프로퍼티: Medications, AdverseReports, ReportSymptoms, ReportMedications, OcrImages
2. `appsettings.json` 및 `appsettings.Development.json`에 ConnectionString 항목 추가
   - 환경 변수 치환: `${POSTGRES_USER}`, `${POSTGRES_PASSWORD}` 등
3. `Api/Program.cs`에서 `AddDbContext<ApplicationDbContext>` 등록
4. `dotnet ef migrations add InitialCreate` 명령으로 초기 마이그레이션 생성
5. `dotnet ef database update`로 마이그레이션 적용 확인

#### T-BE-03: 도메인 엔티티 정의 (6개 테이블)
**목적**: DB 스키마 매핑을 위한 도메인 모델 작성

구현 단계:
1. `Domain/Entities/` 경로에 다음 엔티티 클래스 작성:
   - `ApplicationUser` (IdentityUser 확장): `Name`, `CreatedAt`
   - `Medication`: `Id`, `UserId`, `DrugName`, `Dosage`, `Frequency`, `StartDate`, `EndDate?`
   - `AdverseReport`: `Id`, `UserId`, `ReportedAt`, `Severity` (enum), `Notes?`
   - `ReportSymptom`: `Id`, `ReportId`, `SymptomName`, `IsOfficial`
   - `ReportMedication`: `Id`, `ReportId`, `MedicationId`
   - `OcrImage`: `Id`, `UserId`, `StorageUrl`, `ProcessedAt?`, `DeleteAfterProcessing`
2. 각 엔티티에 Navigation Property 정의 (EF Core 관계 매핑)
3. `ApplicationDbContext`의 `OnModelCreating`에서 Fluent API로 제약 조건 설정

#### T-BE-04: JWT 인증 구현 (회원가입 / 로그인 / 토큰 갱신 / 로그아웃)
**목적**: 보안 인증 시스템 구축

구현 단계:
1. `Infrastructure/Services/JwtTokenService.cs` 작성:
   - `GenerateAccessToken(ApplicationUser user)` → JWT 액세스 토큰 발급 (만료: 15분)
   - `GenerateRefreshToken()` → 랜덤 Refresh Token 발급 (만료: 7일)
   - `ValidateToken(string token)` → 토큰 유효성 검증
2. `Domain/Entities/RefreshToken.cs` 추가 (UserId, Token, Expires, IsRevoked, CreatedAt)
3. `Application/Auth/` 에 Use Case 인터페이스 및 DTO 정의:
   - `RegisterCommand`, `RegisterResult`
   - `LoginCommand`, `LoginResult`
   - `RefreshTokenCommand`, `RefreshTokenResult`
4. `Infrastructure/Services/AuthService.cs` 구현:
   - BCrypt 비밀번호 해싱
   - Refresh Token DB 저장 및 갱신
   - 로그아웃 시 Refresh Token 폐기(IsRevoked = true)
5. `Api/Controllers/AuthController.cs` 작성:
   - `POST /api/auth/register`
   - `POST /api/auth/login`
   - `POST /api/auth/refresh`
   - `POST /api/auth/logout` (인증 필요)
6. `Api/Program.cs`에 JWT Bearer 인증 미들웨어 등록

#### T-BE-05: Global Exception Handler 미들웨어
**목적**: 일관된 에러 응답 포맷 보장

구현 단계:
1. `Api/Middleware/GlobalExceptionHandlerMiddleware.cs` 작성:
   - `IMiddleware` 인터페이스 구현
   - 예외 유형별 HTTP 상태 코드 매핑:
     - `NotFoundException` → 404
     - `ValidationException` → 400
     - `UnauthorizedException` → 401
     - 그 외 → 500
2. 표준 오류 응답 DTO 정의: `{ "statusCode": int, "message": string, "detail": string? }`
3. `Program.cs`에 `app.UseMiddleware<GlobalExceptionHandlerMiddleware>()` 등록
4. `Domain/Exceptions/` 에 커스텀 예외 클래스 작성

#### T-BE-06: Swagger / OpenAPI 문서 설정
**목적**: API 탐색 및 프론트엔드 개발 지원

구현 단계:
1. `Program.cs`에 `AddSwaggerGen` 등록
2. JWT Bearer 인증 헤더를 Swagger UI에서 입력 가능하도록 SecurityDefinition 추가
3. XML 주석 기반 API 설명 활성화 (`GenerateDocumentationFile` 빌드 옵션)
4. Development 환경에서만 Swagger UI 노출 (`app.UseSwagger()`, `app.UseSwaggerUI()`)
5. Swagger 접근 경로: `/swagger`

---

### 2-3. 프론트엔드 (Next.js, TypeScript, Tailwind CSS)

#### T-FE-01: Next.js 프로젝트 초기화
**목적**: 프론트엔드 기술 스택 기반 구성

구현 단계:
1. `npx create-next-app@latest frontend --typescript --tailwind --app --src-dir` 실행
2. 추가 패키지 설치:
   - `axios` (HTTP 클라이언트)
   - `zustand` (전역 상태 관리)
   - `next-pwa` (PWA 지원)
   - `react-hook-form` + `zod` (폼 유효성 검사)
3. `tsconfig.json` 절대 경로 alias 설정 (`@/` → `./src/`)
4. Tailwind CSS 설정 (`tailwind.config.ts`): 기본 폰트 크기 16px 이상 보장

#### T-FE-02: PWA 설정
**목적**: 모바일 설치형 앱 경험 제공

구현 단계:
1. `next.config.js`에 `next-pwa` 플러그인 설정
   - `dest: 'public'`, `disable: process.env.NODE_ENV === 'development'`
2. `public/manifest.json` 작성:
   - `name: "SideReport"`, `short_name: "사이드리포트"`
   - `display: "standalone"`, `theme_color`, `background_color`
   - 앱 아이콘 (`192x192`, `512x512`)
3. `app/layout.tsx`에 `<link rel="manifest" href="/manifest.json" />` 추가
4. Service Worker 캐싱 전략: Network First (API), Cache First (정적 자산)

#### T-FE-03: 공통 레이아웃 컴포넌트
**목적**: 일관된 UI 구조 확립

구현 단계:
1. `src/components/layout/Header.tsx` 작성 (로고, 로그인 상태 표시)
2. `src/components/layout/BottomNav.tsx` 작성 (홈 / 복용약 / 보고 / 이력 탭)
3. `src/app/(main)/layout.tsx` 작성: Header + BottomNav 포함한 공통 레이아웃
4. 인증이 필요한 경로 보호: `src/middleware.ts`에서 JWT 없으면 `/login`으로 redirect

#### T-FE-04: 회원가입 / 로그인 화면
**목적**: 사용자 인증 진입점 UI 구현

구현 단계:
1. `src/app/(auth)/login/page.tsx` 작성:
   - 이메일 + 비밀번호 입력 폼 (react-hook-form + zod 유효성)
   - 로그인 버튼 (최소 터치 영역 44x44px)
   - 회원가입 화면 링크
   - 폰트 크기 최소 16px (고령자 접근성)
2. `src/app/(auth)/register/page.tsx` 작성:
   - 이름 + 이메일 + 비밀번호 + 비밀번호 확인 입력 폼
   - 유효성 검사 에러 메시지 표시
3. 두 화면 모두 인증 API 호출 후 응답 처리 (성공 시 메인 화면으로 이동)

#### T-FE-05: JWT 토큰 관리 유틸리티
**목적**: 인증 토큰 자동 처리 및 갱신

구현 단계:
1. `src/lib/apiClient.ts`: axios 인스턴스 생성
   - `baseURL: process.env.NEXT_PUBLIC_API_BASE_URL`
   - Request Interceptor: `Authorization: Bearer <accessToken>` 헤더 자동 추가
   - Response Interceptor: 401 응답 시 Refresh Token으로 재발급 후 원 요청 재시도
   - Refresh 실패 시 로그아웃 처리 및 `/login` 리다이렉트
2. `src/lib/tokenStorage.ts`: 토큰 저장/조회/삭제 유틸리티
   - Access Token: `sessionStorage` (탭 닫으면 삭제)
   - Refresh Token: `httpOnly Cookie` (서버사이드 설정 권장)

#### T-FE-06: 전역 상태 관리 (Zustand)
**목적**: 인증 상태 및 사용자 정보 전역 공유

구현 단계:
1. `src/store/authStore.ts` 작성:
   - 상태: `user: User | null`, `accessToken: string | null`, `isAuthenticated: boolean`
   - 액션: `login(user, token)`, `logout()`, `updateToken(token)`
2. `src/store/index.ts`: 스토어 export 통합
3. `src/app/providers.tsx`: Zustand Provider 래퍼 컴포넌트 작성

---

## 3. 구현 순서

Sprint 1의 작업은 다음 순서로 진행한다. 의존 관계를 고려하여 병렬 진행 가능한 구간을 표시한다.

```
[Phase 1 — 환경 구성] (1~2일차)
  ├── T-INF-01: Docker Compose 구성
  ├── T-INF-02: 환경 변수 전략 수립
  └── T-BE-01: Clean Architecture 프로젝트 구조 초기화
      (위 세 작업은 병렬 진행 가능)

[Phase 2 — DB 기반] (3~4일차)
  ├── T-BE-03: 도메인 엔티티 정의
  └── T-BE-02: PostgreSQL 연결 및 EF Core 설정
      ↓ (T-BE-03 완료 후)
  └── EF Core 초기 마이그레이션 생성 및 적용

[Phase 3 — 백엔드 핵심 기능] (5~7일차)
  ├── T-BE-05: Global Exception Handler 미들웨어
  ├── T-BE-06: Swagger 설정
  └── T-BE-04: JWT 인증 구현
      (T-BE-05, T-BE-06은 T-BE-04와 병렬 진행 가능)

[Phase 4 — 프론트엔드 기반] (5~7일차, 백엔드와 병렬)
  ├── T-FE-01: Next.js 프로젝트 초기화
  ├── T-FE-02: PWA 설정
  └── T-FE-03: 공통 레이아웃 컴포넌트

[Phase 5 — 프론트엔드 인증 UI] (8~9일차)
  ├── T-FE-06: Zustand 전역 상태 설정
  ├── T-FE-05: JWT 토큰 관리 유틸리티
  └── T-FE-04: 회원가입 / 로그인 화면
      (T-BE-04 완료 후 백엔드 연동)

[Phase 6 — CI 및 통합 검증] (10일차)
  ├── T-INF-03: GitHub Actions CI 파이프라인
  ├── T-INF-04: Blob Storage / S3 버킷 준비
  └── E2E 인증 흐름 통합 테스트
```

---

## 4. 완료 기준 체크리스트

### 인프라 & DevOps
- ✅ `docker compose up` 명령 한 번으로 프론트엔드(3000), 백엔드(5000), DB(5432) 실행 완료
- ✅ `.env.example` 파일이 모든 필수 환경 변수 항목을 포함
- ✅ GitHub Actions CI가 PR 생성 시 백엔드 빌드 및 프론트엔드 빌드를 자동 실행
- ✅ CI 파이프라인에서 테스트 실패 시 PR Merge 차단
- ✅ Azure Blob Storage 또는 S3 버킷 생성 및 접근 권한 확인 완료

### 백엔드
- ✅ `Api / Application / Domain / Infrastructure` 4개 프로젝트가 솔루션에 포함
- ✅ `dotnet build` 경고 없이 성공
- ✅ EF Core 마이그레이션이 오류 없이 PostgreSQL에 적용됨
- ✅ 6개 테이블(Users, Medications, AdverseReports, ReportSymptoms, ReportMedications, OcrImages)이 DB에 생성됨
- ✅ `POST /api/auth/register` 정상 동작 (201 Created 반환)
- ✅ `POST /api/auth/login` 정상 동작 (Access Token + Refresh Token 반환)
- ✅ `POST /api/auth/refresh` 정상 동작 (만료된 Access Token 갱신)
- ✅ `POST /api/auth/logout` 정상 동작 (Refresh Token 폐기)
- ✅ 잘못된 요청 시 표준 에러 응답 포맷(`statusCode`, `message`) 반환
- ✅ `/swagger` 경로에서 Swagger UI 접근 가능
- ✅ Swagger UI에서 JWT Bearer 토큰 입력 후 인증 테스트 가능

### 프론트엔드
- ✅ `npm run build` 경고 없이 성공
- ✅ 로그인 화면 렌더링 정상 (이메일, 비밀번호 입력 + 제출 버튼)
- ✅ 회원가입 화면 렌더링 정상 (이름, 이메일, 비밀번호, 확인 입력)
- ✅ 폼 유효성 검사 에러 메시지 표시 확인
- ✅ 로그인 성공 시 메인 화면으로 리다이렉트
- ✅ 로그인 실패 시 에러 메시지 표시
- ✅ axios Interceptor로 401 응답 시 자동 토큰 갱신 동작 확인
- ✅ `manifest.json` 정상 로드 및 PWA 설치 가능 확인 (Lighthouse PWA 체크)
- ✅ 모바일 기준 최소 글꼴 크기 16px 적용 확인

### 통합 (E2E)
- ✅ 회원가입 → 로그인 → JWT 발급 → 보호된 API 호출 전체 플로우 동작
- ✅ Docker 컨테이너 환경에서 프론트엔드 ↔ 백엔드 ↔ DB 통신 정상

---

## 5. 폴더 구조

Sprint 1 완료 후 예상되는 프로젝트 폴더/파일 구조:

```
sidereport/                              # 모노레포 루트
├── docker-compose.yml                   # 개발용 Docker Compose
├── .env.example                         # 환경 변수 템플릿 (커밋 대상)
├── .env                                 # 실제 환경 변수 (gitignore)
├── .github/
│   └── workflows/
│       └── ci.yml                       # GitHub Actions CI
│
├── backend/                             # ASP.NET Core 솔루션 루트
│   ├── SideReport.sln
│   ├── SideReport.Domain/
│   │   ├── SideReport.Domain.csproj
│   │   ├── Entities/
│   │   │   ├── ApplicationUser.cs
│   │   │   ├── Medication.cs
│   │   │   ├── AdverseReport.cs
│   │   │   ├── ReportSymptom.cs
│   │   │   ├── ReportMedication.cs
│   │   │   ├── OcrImage.cs
│   │   │   └── RefreshToken.cs
│   │   ├── Enums/
│   │   │   └── Severity.cs
│   │   └── Exceptions/
│   │       ├── NotFoundException.cs
│   │       ├── ValidationException.cs
│   │       └── UnauthorizedException.cs
│   │
│   ├── SideReport.Application/
│   │   ├── SideReport.Application.csproj
│   │   ├── Auth/
│   │   │   ├── Commands/
│   │   │   │   ├── RegisterCommand.cs
│   │   │   │   ├── LoginCommand.cs
│   │   │   │   └── RefreshTokenCommand.cs
│   │   │   └── Results/
│   │   │       ├── RegisterResult.cs
│   │   │       └── LoginResult.cs
│   │   └── Interfaces/
│   │       ├── IAuthService.cs
│   │       └── IJwtTokenService.cs
│   │
│   ├── SideReport.Infrastructure/
│   │   ├── SideReport.Infrastructure.csproj
│   │   ├── Persistence/
│   │   │   ├── ApplicationDbContext.cs
│   │   │   └── Migrations/
│   │   │       └── [타임스탬프]_InitialCreate.cs
│   │   └── Services/
│   │       ├── AuthService.cs
│   │       └── JwtTokenService.cs
│   │
│   └── SideReport.Api/
│       ├── SideReport.Api.csproj
│       ├── Program.cs
│       ├── appsettings.json
│       ├── appsettings.Development.json
│       ├── Controllers/
│       │   └── AuthController.cs
│       ├── Middleware/
│       │   └── GlobalExceptionHandlerMiddleware.cs
│       └── DTOs/
│           └── ErrorResponse.cs
│
└── frontend/                            # Next.js 프론트엔드 루트
    ├── package.json
    ├── tsconfig.json
    ├── next.config.js                   # next-pwa 설정 포함
    ├── tailwind.config.ts
    ├── public/
    │   ├── manifest.json                # PWA 매니페스트
    │   └── icons/                       # PWA 아이콘 (192x192, 512x512)
    └── src/
        ├── app/
        │   ├── layout.tsx               # 루트 레이아웃
        │   ├── providers.tsx            # Zustand 등 Provider 래퍼
        │   ├── (auth)/
        │   │   ├── login/
        │   │   │   └── page.tsx
        │   │   └── register/
        │   │       └── page.tsx
        │   └── (main)/
        │       └── layout.tsx           # Header + BottomNav 공통 레이아웃
        ├── components/
        │   └── layout/
        │       ├── Header.tsx
        │       └── BottomNav.tsx
        ├── lib/
        │   ├── apiClient.ts             # axios 인스턴스 + Interceptor
        │   └── tokenStorage.ts          # 토큰 저장/조회 유틸리티
        ├── store/
        │   ├── authStore.ts             # Zustand 인증 스토어
        │   └── index.ts
        └── middleware.ts                # 인증 라우트 보호
```

---

## 6. 주요 API 엔드포인트

Sprint 1에서 구현할 인증 관련 API 목록:

| 메서드 | 경로 | 설명 | 인증 필요 | 상태 코드 |
|--------|------|------|-----------|-----------|
| `POST` | `/api/auth/register` | 회원가입 | 불필요 | 201 Created |
| `POST` | `/api/auth/login` | 로그인 (Access + Refresh Token 발급) | 불필요 | 200 OK |
| `POST` | `/api/auth/refresh` | Access Token 갱신 | 불필요 (Refresh Token 필요) | 200 OK |
| `POST` | `/api/auth/logout` | 로그아웃 (Refresh Token 폐기) | Bearer Token 필요 | 204 No Content |
| `GET`  | `/health` | 헬스체크 (Docker healthcheck용) | 불필요 | 200 OK |
| `GET`  | `/swagger` | Swagger UI | 불필요 (개발환경만) | 200 OK |

### 요청/응답 예시

**POST /api/auth/register**
```json
// Request
{
  "name": "홍길동",
  "email": "hong@example.com",
  "password": "P@ssword123!"
}

// Response 201
{
  "userId": "uuid",
  "email": "hong@example.com",
  "name": "홍길동"
}
```

**POST /api/auth/login**
```json
// Request
{
  "email": "hong@example.com",
  "password": "P@ssword123!"
}

// Response 200
{
  "accessToken": "eyJhbGciOiJIUzI1NiIsInR5cCI6IkpXVCJ9...",
  "refreshToken": "dGhpcyBpcyBhIHJlZnJlc2ggdG9rZW4...",
  "expiresIn": 900
}
```

**POST /api/auth/refresh**
```json
// Request
{
  "refreshToken": "dGhpcyBpcyBhIHJlZnJlc2ggdG9rZW4..."
}

// Response 200
{
  "accessToken": "eyJhbGciOiJIUzI1NiIsInR5cCI6IkpXVCJ9...",
  "expiresIn": 900
}
```

**에러 응답 공통 포맷**
```json
{
  "statusCode": 400,
  "message": "이미 사용 중인 이메일입니다.",
  "detail": null
}
```

---

## 7. DB 스키마

Sprint 1에서 생성할 6개 테이블 정의:

### Users (ASP.NET Core Identity 기반)
ASP.NET Core Identity의 `AspNetUsers` 테이블을 확장하여 사용한다.

| 컬럼명 | 타입 | 제약 조건 | 설명 |
|--------|------|-----------|------|
| `Id` | `varchar(450)` | PK | Identity 기본 ID (GUID) |
| `UserName` | `varchar(256)` | UNIQUE | Identity 기본 필드 (이메일과 동일 값) |
| `Email` | `varchar(256)` | UNIQUE, NOT NULL | 로그인 이메일 |
| `PasswordHash` | `text` | NOT NULL | BCrypt 해시 |
| `Name` | `varchar(100)` | NOT NULL | 사용자 이름 (확장 필드) |
| `CreatedAt` | `timestamptz` | NOT NULL, DEFAULT NOW() | 가입 시각 (확장 필드) |

### RefreshTokens
| 컬럼명 | 타입 | 제약 조건 | 설명 |
|--------|------|-----------|------|
| `Id` | `bigint` | PK, IDENTITY | 자동 증가 ID |
| `UserId` | `varchar(450)` | FK → AspNetUsers.Id | 소유 사용자 |
| `Token` | `varchar(512)` | NOT NULL, UNIQUE | Refresh Token 값 |
| `Expires` | `timestamptz` | NOT NULL | 만료 시각 |
| `IsRevoked` | `boolean` | NOT NULL, DEFAULT false | 폐기 여부 |
| `CreatedAt` | `timestamptz` | NOT NULL, DEFAULT NOW() | 발급 시각 |

### Medications
| 컬럼명 | 타입 | 제약 조건 | 설명 |
|--------|------|-----------|------|
| `Id` | `uuid` | PK, DEFAULT gen_random_uuid() | 고유 ID |
| `UserId` | `varchar(450)` | FK → AspNetUsers.Id, NOT NULL | 소유 사용자 |
| `DrugName` | `varchar(200)` | NOT NULL | 약품명 |
| `Dosage` | `varchar(100)` | NULL | 용량 (예: "500mg") |
| `Frequency` | `varchar(100)` | NULL | 복용 횟수 (예: "1일 3회") |
| `StartDate` | `date` | NOT NULL | 복용 시작일 |
| `EndDate` | `date` | NULL | 복용 종료일 (복용 중이면 null) |
| `CreatedAt` | `timestamptz` | NOT NULL, DEFAULT NOW() | 등록 시각 |

### AdverseReports
| 컬럼명 | 타입 | 제약 조건 | 설명 |
|--------|------|-----------|------|
| `Id` | `uuid` | PK, DEFAULT gen_random_uuid() | 고유 ID |
| `UserId` | `varchar(450)` | FK → AspNetUsers.Id, NOT NULL | 보고 사용자 |
| `ReportedAt` | `timestamptz` | NOT NULL, DEFAULT NOW() | 보고 시각 |
| `Severity` | `varchar(20)` | NOT NULL | 심각도: 'Mild', 'Moderate', 'Severe' |
| `Notes` | `text` | NULL | 추가 메모 |

### ReportSymptoms
| 컬럼명 | 타입 | 제약 조건 | 설명 |
|--------|------|-----------|------|
| `Id` | `uuid` | PK, DEFAULT gen_random_uuid() | 고유 ID |
| `ReportId` | `uuid` | FK → AdverseReports.Id, NOT NULL | 연결 보고서 |
| `SymptomName` | `varchar(200)` | NOT NULL | 증상명 |
| `IsOfficial` | `boolean` | NOT NULL, DEFAULT false | 공식 부작용 여부 (KAERS 대조) |

### ReportMedications
| 컬럼명 | 타입 | 제약 조건 | 설명 |
|--------|------|-----------|------|
| `Id` | `uuid` | PK, DEFAULT gen_random_uuid() | 고유 ID |
| `ReportId` | `uuid` | FK → AdverseReports.Id, NOT NULL | 연결 보고서 |
| `MedicationId` | `uuid` | FK → Medications.Id, NOT NULL | 연결 복용약 |

### OcrImages
| 컬럼명 | 타입 | 제약 조건 | 설명 |
|--------|------|-----------|------|
| `Id` | `uuid` | PK, DEFAULT gen_random_uuid() | 고유 ID |
| `UserId` | `varchar(450)` | FK → AspNetUsers.Id, NOT NULL | 업로드 사용자 |
| `StorageUrl` | `text` | NOT NULL | Blob Storage / S3 URL |
| `ProcessedAt` | `timestamptz` | NULL | OCR 처리 완료 시각 (미완료 시 null) |
| `DeleteAfterProcessing` | `boolean` | NOT NULL, DEFAULT true | 처리 후 즉시 삭제 여부 |
| `CreatedAt` | `timestamptz` | NOT NULL, DEFAULT NOW() | 업로드 시각 |

### 인덱스 (Sprint 1에서 함께 생성)

```sql
-- Medications: 사용자별 복용약 조회 (Sprint 2~3에서 빈번하게 사용)
CREATE INDEX idx_medications_user_id ON "Medications" ("UserId");

-- AdverseReports: 사용자별 보고 조회 + 날짜 정렬 (Sprint 3에서 사용)
CREATE INDEX idx_adverse_reports_user_id ON "AdverseReports" ("UserId");
CREATE INDEX idx_adverse_reports_reported_at ON "AdverseReports" ("ReportedAt" DESC);

-- RefreshTokens: 토큰 값으로 빠른 조회
CREATE INDEX idx_refresh_tokens_token ON "RefreshTokens" ("Token");
CREATE INDEX idx_refresh_tokens_user_id ON "RefreshTokens" ("UserId");
```

---

## 8. 리스크

| # | 리스크 | 영향도 | 발생 가능성 | 대응 방안 |
|---|--------|--------|-------------|-----------|
| R-01 | **ASP.NET Core Identity 스키마와 커스텀 엔티티 충돌** — Identity 기본 테이블과 확장 필드 마이그레이션 충돌 발생 가능 | 중간 | 중간 | `IdentityDbContext<ApplicationUser>` 상속 패턴 사용, 초기 마이그레이션 생성 시 Identity 테이블 포함 여부 명시적 확인 |
| R-02 | **JWT 시크릿 키 노출** — `.env` 파일이 실수로 커밋될 경우 보안 사고 | 높음 | 낮음 | `.gitignore`에 `.env` 추가, pre-commit 훅으로 시크릿 스캔 (`git-secrets` 또는 `truffleHog`) |
| R-03 | **Docker Compose 네트워크 연결 실패** — 백엔드가 DB 헬스체크 전에 시작 시도하여 연결 오류 | 중간 | 중간 | `depends_on: condition: service_healthy` 설정, 백엔드에 DB 연결 재시도 로직 추가 (`Polly` 라이브러리 활용) |
| R-04 | **Refresh Token 보안** — Refresh Token을 LocalStorage에 저장 시 XSS 취약점 | 높음 | 중간 | Refresh Token은 `httpOnly Secure Cookie`에 저장, Access Token만 메모리(Zustand)에 보관 |
| R-05 | **Next.js App Router + next-pwa 호환성 이슈** — next-pwa가 App Router를 완전 지원하지 않을 수 있음 | 낮음 | 중간 | `next-pwa` 최신 버전 확인, 문제 시 `@serwist/next` 또는 커스텀 Service Worker로 대체 |
| R-06 | **EF Core 마이그레이션 충돌** — 팀원 간 동시 마이그레이션 작성 시 충돌 | 낮음 | 높음 | Sprint 1 기간 중 마이그레이션은 1인 담당, 마이그레이션 파일 변경 시 PR 리뷰 필수화 |
| R-07 | **KAERS 데이터 라이선스 제약** — 공공 데이터 활용 범위 미확인 시 법적 리스크 | 높음 | 낮음 | Sprint 1 기간 중 KAERS 데이터 라이선스 및 식약처 API 이용 약관 법무 검토 시작 (Sprint 2 시작 전 완료 목표) |

---

## 참고 문서

- ROADMAP: `docs/ROADMAP.md`
- PRD: `docs/PRD.md`
- 다음 스프린트: `docs/sprint/sprint2.md` (Sprint 2 — OCR 인식 및 약품 정보 파싱)
