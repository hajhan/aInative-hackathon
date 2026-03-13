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
dotnet run --project src/SideReport.Api

# 테스트 전체
dotnet test SideReport.sln

# 테스트 단일 실행
dotnet test --filter "FullyQualifiedName~JwtTokenServiceTests"
```

Swagger UI: `http://localhost:5000/swagger` (Development 환경만)
Health check: `http://localhost:5000/health`

### 프론트엔드 (로컬)

```bash
cd frontend
npm install
npm run dev    # http://localhost:3000
npm run build
npm run lint
```

---

## 아키텍처

### 백엔드 — Clean Architecture

```
SideReport.Domain        ← 순수 C# 엔티티, Enum, 예외 (외부 의존성 없음)
SideReport.Application   ← 인터페이스, Command/Result DTO (Domain만 참조)
SideReport.Infrastructure← EF Core DbContext, 서비스 구현체, Identity (Application 참조)
SideReport.Api           ← Controller, Middleware, Program.cs (Application+Infrastructure 참조)
```

**핵심 규칙**: `ApplicationUser`(IdentityUser 상속)는 Infrastructure 레이어에 위치 (`Infrastructure/Identity/ApplicationUser.cs`). Domain 엔티티는 `string UserId` FK만 갖고 `ApplicationUser` 내비게이션 프로퍼티를 갖지 않는다.

**의존성 주입**: `DependencyInjection.cs`의 `AddInfrastructure()` 확장 메서드가 EF Core, Identity, 서비스를 한 번에 등록. `Program.cs`에서 `builder.Services.AddInfrastructure(builder.Configuration)` 호출.

**DB 자동 마이그레이션**: `Program.cs` 시작 시 `db.Database.Migrate()` 자동 실행.

**JWT 흐름**: Access Token (15분, HS256) + Refresh Token (7일, DB 저장). 갱신 시 기존 Refresh Token 폐기(rotation).

### 프론트엔드 — Next.js App Router

```
src/app/(auth)/          ← 비인증 라우트 (login, register)
src/app/(main)/          ← 인증 필요 라우트 (home 등)
src/lib/apiClient.ts     ← axios 인스턴스, JWT 인터셉터, 401 자동 갱신 (queue 기반)
src/store/authStore.ts   ← Zustand 인증 상태
src/middleware.ts        ← 라우트 보호 (미인증 시 /login 리디렉션)
```

`NEXT_PUBLIC_API_BASE_URL` 환경 변수로 백엔드 URL 설정.

### DB 스키마

Identity 테이블 외에 다음 커스텀 테이블 존재:
- `RefreshTokens` — JWT 갱신 토큰 (UserId FK → AspNetUsers)
- `Medications` — 사용자 복용약
- `AdverseReports` — 부작용 보고서 (Severity: Mild/Moderate/Severe)
- `ReportSymptoms` — 보고서별 증상 목록
- `ReportMedications` — 보고서-복용약 연결 (N:M)
- `OcrImages` — 약봉투 이미지 (OCR 처리 후 삭제 옵션)

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
2. **sprint-planner** 에이전트로 `docs/sprint/sprint{N}.md` 계획 수립
3. karpathy-guidelines skill 준수하며 구현
4. 구현 완료 후 **sprint-close** 에이전트로 마무리 (ROADMAP 업데이트, PR 생성, 코드 리뷰, 자동 검증)

체크리스트 형식: `- ✅ 완료`, `- ⬜ 미완료` (GFM `[x]`/`[ ]` 사용 금지)

### 스프린트 검증 — 자동 실행 가능 항목

```bash
docker compose exec backend dotnet test    # 백엔드 테스트
curl http://localhost:5000/health          # 헬스체크
```

수동 필요: `docker compose up --build` (Docker 재빌드), 브라우저 UI 확인

---

## 환경 변수 (.env)

`.env.example`을 복사하여 사용. 필수 변경 항목:
- `JWT_SECRET_KEY` — 32자 이상 랜덤 문자열
- `POSTGRES_PASSWORD` — 강력한 패스워드

Sprint 2 이후 OCR 기능을 위해 Azure Blob Storage 또는 AWS S3 설정 필요.
