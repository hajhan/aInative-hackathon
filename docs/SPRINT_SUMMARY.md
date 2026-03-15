# SideReport 스프린트 요약

> **서비스**: SideReport — 약봉투 OCR로 약품을 인식하고 부작용을 보고하는 서비스 (일반 환자/보호자 대상)
> **최종 업데이트**: 2026-03-16

---

## 스프린트 진행 현황

| 스프린트 | 완료일 | 목표 | 상태 |
|---------|--------|------|------|
| Sprint 1 | 2026-03-13 | 프로젝트 기반 & 인증 시스템 | ✅ 완료 |
| Sprint 2 | 2026-03-13 | 약봉투 OCR & 약품 파싱 파이프라인 | ✅ 완료 |
| Sprint 3 | 2026-03-15 | 부작용 보고 & 복용약 관리 & AI 분석 | ✅ 완료 |

---

## Sprint 1 — 프로젝트 기반 & 인증 시스템

**완료일**: 2026-03-13

### 목표
모든 기능 개발의 토대가 되는 인프라, 프로젝트 구조, DB 스키마, 사용자 인증 시스템 구축

### 주요 구현 내용

| 구분 | 구현 내용 |
|------|-----------|
| 인프라 | Docker Compose (frontend:3000, backend:5000, db:5432), GitHub Actions CI |
| 백엔드 | Clean Architecture 4레이어 (Domain / Application / Infrastructure / Api) |
| DB | EF Core + PostgreSQL, 6개 테이블 초기 마이그레이션 |
| 인증 | JWT Access Token (15분, HS256) + Refresh Token (7일, rotation) |
| API | `/api/auth/register`, `/api/auth/login`, `/api/auth/refresh`, `/api/auth/logout` |
| 프론트엔드 | Next.js 14 PWA, Zustand 인증 스토어, 로그인/회원가입 화면, axios 인터셉터 (401 자동 갱신) |

### 주요 기술 결정

- `ApplicationUser` (IdentityUser 상속)는 Infrastructure 레이어에 위치, Domain 엔티티는 `string UserId` FK만 보유
- Refresh Token rotation은 DB 트랜잭션으로 감싸 동시성 이슈 방지
- Next.js 미들웨어는 `sr_auth_flag` 쿠키 기반 UX 힌트 — 실제 보안 게이트는 API JWT

---

## Sprint 2 — 약봉투 OCR & 약품 파싱 파이프라인

**완료일**: 2026-03-13

### 목표
약봉투 사진 → OCR → 약품명/용량/횟수 파싱 → 식약처 검증 파이프라인 구현

### 주요 구현 내용

| 구분 | 구현 내용 |
|------|-----------|
| Domain | `OcrStatus` Enum, `KnownSideEffect` / `DrugCache` 엔티티, `OcrImage` 확장 (RawOcrText, ParsedResultJson) |
| OCR 서비스 | `MockOcrService` (기본) / `GoogleVisionOcrService` (`Ocr:UseMock=false` 시 활성화) |
| 파싱 | `DrugTextParserService` — 정규식 기반 약품명/용량/복용 횟수 추출 |
| 약품 정보 | `MockDrugInfoService` (기본, 20개 프리셋) / `MfdsDrugInfoService` (공공데이터포털 식약처 API + DB 캐시) |
| 파이프라인 | `OcrPipelineService` — 업로드 → OCR → 파싱 → 검증 오케스트레이션 |
| API | `POST /api/ocr/upload`, `GET /api/ocr/{id}`, `POST /api/ocr/{id}/confirm` |
| 시드 데이터 | `kaers-seed.json` 117건 (약품-부작용 쌍) 앱 시작 시 자동 적재 |
| 프론트엔드 | OCR 업로드 화면, 결과 확인/편집 화면, `ocrStore` (Zustand) |

### 주요 기술 결정

- OCR/DrugInfo 서비스 모두 `UseMock` 플래그로 토글 — API 키 없이 개발 가능
- 이미지 저장: 로컬 파일시스템 (`LocalImageStorageService`) — `IImageStorageService` 인터페이스로 추후 Cloud Storage 전환 용이
- OCR 처리: 동기 async/await — Vision API 응답 1~3초, Job Queue 불필요 (해커톤 범위)

---

## Sprint 3 — 부작용 보고 & 복용약 관리 & AI 분석

**완료일**: 2026-03-15

### 목표
MVP 완성 — 핵심 도메인 기능 (부작용 보고서 CRUD + 복용약 관리 + Claude AI 분석)

### 주요 구현 내용

| 구분 | 구현 내용 |
|------|-----------|
| 복용약 관리 | `MedicationService` CRUD, `MedicationsController` (POST/GET/PUT/DELETE) |
| 부작용 보고 | `ReportService` — KAERS DB 대조 `IsOfficial` 자동 표시 |
| AI 분석 | `ClaudeAiAnalysisService` (Claude Haiku — 약물 상호작용 + 부작용 분석) |
| Fallback | `ANTHROPIC_API_KEY` 미설정 시 `NoOpAiAnalysisService` (503 반환) |
| API | Reports CRUD 4개, Medications CRUD 4개, `POST /api/ai/analyze` |
| 프론트엔드 | 부작용 보고 4단계 폼 (복용약 선택 → 증상 → 심각도 → AI 분석/제출) |
| 프론트엔드 | 복용약 관리 페이지, 보고서 목록 페이지, 홈 대시보드 업데이트 |

### 주요 기술 결정

- Claude Haiku 선택: 빠른 응답 속도, 저비용 (해커톤 환경 최적)
- `IsOfficial` 판별: 보고 생성 시 `KnownSideEffects` 테이블 실시간 대조
- `ANTHROPIC_API_KEY` 조건부 DI — AI 없이도 나머지 기능 정상 동작

---

## 서비스 아키텍처 다이어그램

```
┌──────────────────────────────────────────────────────────────────────────────┐
│                              SideReport 서비스 구조                           │
└──────────────────────────────────────────────────────────────────────────────┘

  [사용자 (모바일/웹)]
       │  HTTPS
       ▼
┌─────────────────────────────────────────────────────────┐
│                   Frontend (Vercel)                      │
│                  Next.js 14 + TypeScript                 │
│  ┌─────────────────────────────────────────────────┐    │
│  │              Next.js App Router                  │    │
│  │  ┌─────────┐  ┌──────────────┐  ┌───────────┐  │    │
│  │  │  (auth) │  │    (main)    │  │middleware │  │    │
│  │  │  login  │  │ home / ocr   │  │(라우트 보호)│  │    │
│  │  │register │  │ report/s     │  │sr_auth_flag│  │    │
│  │  └─────────┘  │ medications  │  └───────────┘  │    │
│  │               └──────────────┘                  │    │
│  │                                                  │    │
│  │  ┌─────────────────────────────────────────┐    │    │
│  │  │          Zustand 스토어                  │    │    │
│  │  │  authStore │ ocrStore │ reportStore      │    │    │
│  │  │            │          │ medicationStore  │    │    │
│  │  └─────────────────────────────────────────┘    │    │
│  │                                                  │    │
│  │  ┌────────────────────────────────────────┐     │    │
│  │  │  apiClient.ts (axios)                  │     │    │
│  │  │  • JWT Bearer 자동 첨부                 │     │    │
│  │  │  • 401 → Refresh → 재시도 (큐 방식)    │     │    │
│  │  └────────────────────────────────────────┘     │    │
│  └─────────────────────────────────────────────────┘    │
└───────────────────────┬─────────────────────────────────┘
                        │  HTTP REST API
                        ▼
┌───────────────────────────────────────────────────────────────────┐
│                    Backend (Fly.io)                                │
│                 ASP.NET Core 8 — Clean Architecture                │
│                                                                    │
│  ┌──────────────────────────────────────────────────────────┐     │
│  │  API Layer (SideReport.Api)                               │     │
│  │  ┌────────────┐ ┌───────────────┐ ┌──────────────────┐  │     │
│  │  │AuthController│ │OcrController │ │ReportsController │  │     │
│  │  │/api/auth/* │ │/api/ocr/*    │ │/api/reports/*    │  │     │
│  │  └────────────┘ └───────────────┘ └──────────────────┘  │     │
│  │  ┌─────────────────────┐  ┌─────────────────────────┐   │     │
│  │  │MedicationsController│  │     AiController         │   │     │
│  │  │/api/medications/*   │  │  /api/ai/analyze         │   │     │
│  │  └─────────────────────┘  └─────────────────────────┘   │     │
│  │  GlobalExceptionHandlerMiddleware │ JWT Bearer Auth       │     │
│  └──────────────────────────────────────────────────────────┘     │
│                            │                                       │
│  ┌──────────────────────────────────────────────────────────┐     │
│  │  Application Layer (SideReport.Application)               │     │
│  │  IAuthService │ IOcrPipelineService │ IReportService      │     │
│  │  IMedicationService │ IAiAnalysisService │ IDrugInfoService│     │
│  └──────────────────────────────────────────────────────────┘     │
│                            │                                       │
│  ┌──────────────────────────────────────────────────────────┐     │
│  │  Infrastructure Layer (SideReport.Infrastructure)         │     │
│  │                                                           │     │
│  │  ┌──────────────────────┐  ┌────────────────────────┐   │     │
│  │  │  OCR 서비스           │  │  약품 정보 서비스        │   │     │
│  │  │  MockOcrService  ─┐  │  │  MockDrugInfoService ─┐│   │     │
│  │  │  GoogleVisionOcr ─┘  │  │  MfdsDrugInfoService ─┘│   │     │
│  │  │  (UseMock 토글)      │  │  (UseMock 토글)         │   │     │
│  │  └──────────────────────┘  └────────────────────────┘   │     │
│  │                                                           │     │
│  │  ┌──────────────────────┐  ┌────────────────────────┐   │     │
│  │  │  AI 분석 서비스       │  │  인증 서비스            │   │     │
│  │  │  ClaudeAiAnalysis ─┐ │  │  AuthService           │   │     │
│  │  │  NoOpAiAnalysis  ──┘ │  │  JwtTokenService       │   │     │
│  │  │  (API 키 유무 토글)   │  │  (Access+Refresh Token)│   │     │
│  │  └──────────────────────┘  └────────────────────────┘   │     │
│  │                                                           │     │
│  │  DrugTextParserService │ OcrPipelineService               │     │
│  │  ReportService (KAERS IsOfficial 자동 대조)               │     │
│  │  MedicationService │ LocalImageStorageService             │     │
│  └──────────────────────────────────────────────────────────┘     │
└───────────────────────┬───────────────────────────────────────────┘
                        │
          ┌─────────────┼──────────────┐
          ▼             ▼              ▼
┌──────────────┐ ┌────────────┐ ┌──────────────────┐
│ PostgreSQL   │ │ Google     │ │  Claude API       │
│ (Fly.io)     │ │ Vision API │ │  (Anthropic)      │
│              │ │  (OCR)     │ │  약물 상호작용     │
│ • AspNetUsers│ └────────────┘ │  부작용 분석       │
│ • RefreshToken│               └──────────────────┘
│ • Medications│ ┌────────────┐
│ • AdverseReport│ │  식약처   │
│ • ReportSymptom│ │ 공공 API  │
│ • ReportMedication│ │ (MFDS)  │
│ • OcrImages  │ └────────────┘
│ • KnownSideEffects│
│ • DrugCaches │
└──────────────┘
```

---

## 핵심 데이터 흐름

### OCR → 복용약 등록 흐름 (Sprint 2)

```
사용자 (약봉투 사진)
  │
  ▼
POST /api/ocr/upload
  │
  ├─► [MockOcrService / GoogleVisionOcrService]
  │     → 이미지에서 텍스트 추출
  │
  ├─► DrugTextParserService
  │     → 정규식으로 약품명/용량/복용 횟수 파싱
  │
  ├─► [MockDrugInfoService / MfdsDrugInfoService]
  │     → 식약처 DB 검증 + DrugCache 저장
  │
  └─► OcrImage DB 저장 (RawOcrText, ParsedResultJson)

  ▼ 사용자 결과 확인/편집

POST /api/ocr/{id}/confirm
  └─► Medications DB 저장
```

### 부작용 보고 흐름 (Sprint 3)

```
사용자 (4단계 폼)
  │
  ├─ Step 1: 복용약 선택 (GET /api/medications)
  ├─ Step 2: 증상 입력
  ├─ Step 3: 심각도 선택 (Mild / Moderate / Severe)
  └─ Step 4: AI 분석 요청

  ▼

POST /api/ai/analyze
  └─► ClaudeAiAnalysisService (Haiku)
        → 약물 상호작용 경고
        → 공식 부작용 분류

  ▼

POST /api/reports
  ├─► KnownSideEffects 대조 → IsOfficial 자동 판별
  └─► AdverseReport + ReportSymptoms + ReportMedications 저장
```

---

## DB 스키마 요약

```
AspNetUsers ──┬── RefreshTokens     (1:N — JWT 갱신)
              ├── Medications        (1:N — 복용약)
              ├── AdverseReports ────┬── ReportSymptoms    (1:N — 증상)
              │                     └── ReportMedications  (N:M — 보고서↔복용약)
              └── OcrImages          (1:N — OCR 이미지)

KnownSideEffects  (KAERS 시드 데이터 — 117건)
DrugCaches        (식약처 API 응답 캐시)
```

---

## 환경 변수 요약

| 환경 변수 | 용도 | 기본값 |
|-----------|------|--------|
| `OCR_USE_MOCK` | Google Vision 대신 Mock OCR | `true` |
| `DRUG_INFO_USE_MOCK` | 식약처 API 대신 Mock | `true` |
| `ANTHROPIC_API_KEY` | Claude AI 분석 (미설정 시 503) | 없음 |
| `GOOGLE_VISION_API_KEY` | `OCR_USE_MOCK=false` 시 필요 | 없음 |
| `MFDS_API_KEY` | 공공데이터포털 식약처 API 키 | 없음 |

---

## 배포 현황

| 서비스 | 플랫폼 | 트리거 |
|--------|--------|--------|
| 프론트엔드 | Vercel | sprint1 push 시 자동 배포 |
| 백엔드 | Fly.io (`sidereport-backend`) | GitHub Actions CD (`.github/workflows/cd.yml`) |
| DB | Fly.io PostgreSQL | 백엔드 시작 시 자동 마이그레이션 |
