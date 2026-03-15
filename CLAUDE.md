# CLAUDE.md

This file provides guidance to Claude Code (claude.ai/code) when working with code in this repository.

## 프로젝트 개요

**SideReport** — 약봉투 OCR로 약품을 인식하고 부작용을 보고하는 서비스. 일반 환자/보호자 대상.

- **Backend**: `backend/` — ASP.NET Core 8 (C#), Clean Architecture, EF Core + PostgreSQL, JWT 인증
- **Frontend**: `frontend/` — Next.js 14 (TypeScript), App Router, Tailwind CSS, PWA
- **Infra**: Docker Compose로 전체 스택 실행

---

## 빌드 및 실행 명령어

### 전체 스택 (Docker)

```bash
cp .env.example .env        # 최초 1회: 환경 변수 설정
docker compose up --build   # 전체 스택 실행 (db → backend → frontend 순)
docker compose down         # 종료
```

서비스 포트: frontend `3000`, backend `5000`, db `5432`

### 백엔드 (로컬)

```bash
cd backend
dotnet restore SideReport.sln
dotnet build SideReport.sln
ASPNETCORE_ENVIRONMENT=Development dotnet run --project src/SideReport.Api
```

**Windows**에서는 동일 명령 줄에 환경 변수를 함께 지정해야 한다:
```cmd
set ASPNETCORE_ENVIRONMENT=Development&& dotnet run --project src/SideReport.Api
```

```bash
# 테스트 전체
dotnet test SideReport.sln

# 테스트 단일 실행
dotnet test --filter "FullyQualifiedName~JwtTokenServiceTests"
```

**주의**: 서버가 실행 중이면 Debug 빌드 DLL이 잠겨 `dotnet build`가 실패한다. 서버를 종료하거나 `dotnet test`(별도 프로세스)로 확인한다.

Swagger UI: `http://localhost:5000/swagger` (Development 환경만)
Health check: `http://localhost:5000/health`

### 프론트엔드 (로컬)

```bash
cd frontend
npm install
npm run dev -- -p 3100   # 로컬 개발 포트: 3100
npm run build
npm run lint             # Vercel 배포 전 필수
```

`NEXT_PUBLIC_API_BASE_URL` 미설정 시 `http://localhost:5000`으로 폴백.

---

## 아키텍처

### 백엔드 — Clean Architecture

```
SideReport.Domain        ← 순수 C# 엔티티, Enum, 예외 (외부 의존성 없음)
SideReport.Application   ← 인터페이스, Command/Result DTO (Domain만 참조)
SideReport.Infrastructure← EF Core DbContext, 서비스 구현체, Identity (Application 참조)
SideReport.Api           ← Controller, Middleware, Program.cs (Application+Infrastructure 참조)
```

**핵심 규칙**: `ApplicationUser`(IdentityUser 상속)는 Infrastructure 레이어에 위치. Domain 엔티티는 `string UserId` FK만 갖고 `ApplicationUser` 내비게이션 프로퍼티를 갖지 않는다.

**의존성 주입**: `DependencyInjection.cs`의 `AddInfrastructure()` 확장 메서드가 EF Core, Identity, 서비스를 한 번에 등록.

**DB 자동 마이그레이션**: `Program.cs` 시작 시 `db.Database.Migrate()` 자동 실행. 기존 테이블 충돌(42P07) 시 `IF NOT EXISTS` 보정 쿼리로 처리.

**JWT 흐름**: Access Token (15분, HS256) + Refresh Token (7일, DB 저장). 갱신 시 기존 Refresh Token 폐기(rotation). Refresh Token rotation은 DB 트랜잭션으로 감싼다.

**조건부 서비스 등록** (`DependencyInjection.cs`):
- `Ocr:UseMock=true` → `MockOcrService`, `false` → `GoogleVisionOcrService`
- `DrugInfo:UseMock=true` → `MockDrugInfoService`, `false` → `MfdsDrugInfoService`
- `ANTHROPIC_API_KEY` 설정 시 → `ClaudeAiAnalysisService`, 미설정 시 → `NoOpAiAnalysisService` (503 반환)

### 프론트엔드 — Next.js App Router

```
src/app/(auth)/          ← 비인증 라우트 (login, register)
src/app/(main)/          ← 인증 필요 라우트 (home, ocr, report, reports, medications)
src/lib/apiClient.ts     ← axios 인스턴스, JWT 인터셉터, 401 자동 갱신 (queue 기반)
src/store/               ← Zustand 스토어 (auth, ocr, medication, report)
src/middleware.ts        ← 라우트 보호 (sr_auth_flag 쿠키 기반, UX용 힌트 — 보안 게이트는 API JWT)
src/components/auth/AuthGuard.tsx ← 클라이언트 사이드 인증 가드 (hydration 완료 후 판단)
```

**내비게이션**: 페이지 간 이동은 반드시 Next.js `<Link>` 사용. `<a href>` 사용 시 풀 페이지 리로드로 Zustand 스토어 초기화 → auth 상태 소실 → 자동 로그아웃 버그 발생.

**`<Link>` 안에 `<button>` 중첩 금지**: HTML 시맨틱 오류. 대신 Link에 직접 버튼 스타일을 적용하거나 `useRouter`를 사용한다.

**날짜 처리**: `new Date().toLocaleDateString('en-CA')`로 로컬 YYYY-MM-DD 반환 (타임존 오차 방지). `toISOString().slice(0,10)` 사용 금지.

**JWT payload 디코딩**: `atob()` 단독 사용 시 한글 깨짐. `login/page.tsx`의 `decodeJwtPayload()` 헬퍼 함수(TextDecoder 기반)를 재사용한다.

**OCR → 복용약 등록 흐름**: `ocrStore`의 `returnPath`에 출발 페이지를 저장하고 OCR 완료 후 복귀. `medications`/`report` 페이지에서 OCR로 이동 시 반드시 `setReturnPath`를 먼저 호출한다.

**AuthGuard hydration**: `AuthGuard`는 `mounted` 상태로 hydration 완료 여부를 확인한다. 새로고침 시 Zustand는 초기값(`isAuthenticated: false`)으로 시작하므로, mounted 이전에는 스켈레톤을 렌더링한다.

### DB 스키마

Identity 테이블 외에 다음 커스텀 테이블 존재:
- `RefreshTokens` — JWT 갱신 토큰
- `Medications` — 사용자 복용약 (DrugName, Dosage, Frequency, StartDate, EndDate)
- `AdverseReports` — 부작용 보고서 (Severity: Mild/Moderate/Severe)
- `ReportSymptoms` — 보고서별 증상 목록 (IsOfficial: KAERS DB 대조 결과)
- `ReportMedications` — 보고서-복용약 연결 (N:M)
- `OcrImages` — 약봉투 이미지 (OCR 처리 후 삭제 옵션)
- `KnownSideEffects` — KAERS 공식 약물-부작용 쌍 (시작 시 `data/kaers-seed.json`에서 적재)
- `DrugCaches` — 식약처 API 응답 캐시

---

## 언어 및 커뮤니케이션 규칙

- 기본 응답 언어: **한국어**
- 코드 주석, 커밋 메시지, 문서: 한국어
- 변수명/함수명: 영어 (코드 표준 준수)
- UI 텍스트: 한국어, 최소 글꼴 16px, 버튼 터치 영역 44×44px (고령자 접근성)

---

## 스프린트 워크플로우

새 스프린트 시작 시:
1. `git checkout -b sprint{N}` 으로 브랜치 생성 (worktree 사용 금지)
2. `docs/sprint/sprint{N}.md` 계획 수립
3. 구현 완료 후 sprint1에 머지 → push → GitHub Actions CD 자동 배포

체크리스트 형식: `- ✅ 완료`, `- ⬜ 미완료` (GFM `[x]`/`[ ]` 사용 금지)

### 스프린트 검증

```bash
dotnet test SideReport.sln          # 백엔드 테스트 (서버 종료 후 실행)
curl http://localhost:5000/health   # 헬스체크
npm run lint                        # 프론트엔드 lint (Vercel 빌드 전 필수)
```

---

## 배포

- **프론트엔드**: Vercel — sprint1 push 시 자동 배포. 설정: `frontend/vercel.json`
- **백엔드**: Fly.io (`sidereport-backend`) — GitHub Actions CD (`.github/workflows/cd.yml`). 설정: `backend/fly.toml`
- **CORS**: `Program.cs`에 허용 오리진 하드코딩 + `CORS_ORIGINS` 환경 변수로 추가 도메인 주입 가능

### Fly.io 환경 변수 설정

```bash
flyctl secrets set KEY=VALUE --app sidereport-backend
```

필수 secrets: `ConnectionStrings__DefaultConnection`, `Jwt__SecretKey`, `CORS_ORIGINS`
선택 secrets: `ANTHROPIC_API_KEY`, `GOOGLE_VISION_API_KEY`, `MFDS_API_KEY`

---

## 환경 변수 (.env)

`.env.example`을 복사하여 사용. 로컬 개발 시 `backend/src/SideReport.Api/appsettings.Development.json`에 `ConnectionStrings.DefaultConnection` 설정 (git 미추적 권장).

| 환경 변수 | 용도 | 기본값 |
|-----------|------|--------|
| `OCR_USE_MOCK` | Google Vision 대신 Mock OCR 사용 | `true` |
| `DRUG_INFO_USE_MOCK` | 식약처 API 대신 Mock 사용 | `true` |
| `ANTHROPIC_API_KEY` | Claude AI 분석 (미설정 시 503 반환) | 없음 |
| `GOOGLE_VISION_API_KEY` | `OCR_USE_MOCK=false` 시 필요 | 없음 |
| `MFDS_API_KEY` | 공공데이터포털 식약처 API 키 | 없음 |
