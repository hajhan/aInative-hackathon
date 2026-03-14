# Sprint 3 — 부작용 보고 & 복용약 관리

**기간**: 2026-03-15
**브랜치**: `sprint3`
**목표**: 부작용 보고서 CRUD API + 내 복용약 관리 API + Claude AI 분석 + 프론트엔드 UI

---

## 완료 체크리스트

### Phase 1 — Application 레이어 (Commands / Results)
- ✅ `IReportService` 인터페이스
- ✅ `IMedicationService` 인터페이스
- ✅ `IAiAnalysisService` 인터페이스
- ✅ `CreateReportCommand` / `ReportResult` DTO
- ✅ `CreateMedicationCommand` / `MedicationResult` DTO
- ✅ `AiAnalysisResult` DTO (공식 부작용 판별, 약물 상호작용)

### Phase 2 — Infrastructure 서비스
- ✅ `ReportService` (부작용 보고 CRUD, KAERS 대조 자동 IsOfficial 표시)
- ✅ `MedicationService` (복용약 CRUD)
- ✅ `ClaudeAiAnalysisService` (Claude API — 약물 상호작용 + 부작용 분석)
- ✅ DependencyInjection.cs 등록

### Phase 3 — API 컨트롤러
- ✅ `ReportsController`
  - `POST /api/reports` — 보고서 생성
  - `GET /api/reports` — 내 보고서 목록
  - `GET /api/reports/{id}` — 보고서 상세
  - `DELETE /api/reports/{id}` — 보고서 삭제
- ✅ `MedicationsController`
  - `POST /api/medications` — 복용약 추가
  - `GET /api/medications` — 내 복용약 목록
  - `PUT /api/medications/{id}` — 복용약 수정
  - `DELETE /api/medications/{id}` — 복용약 삭제
- ✅ `AiController`
  - `POST /api/ai/analyze` — 보고서 AI 분석

### Phase 4 — 프론트엔드
- ✅ `lib/types/report.ts` / `medication.ts` 타입
- ✅ `store/reportStore.ts` Zustand
- ✅ `store/medicationStore.ts` Zustand
- ✅ `app/(main)/report/page.tsx` — 부작용 보고 4단계 폼
  - Step 1: 복용약 선택
  - Step 2: 증상 입력
  - Step 3: 심각도 선택
  - Step 4: 메모 + AI 분석 + 제출
- ✅ `app/(main)/medications/page.tsx` — 내 복용약 관리
- ✅ `app/(main)/reports/page.tsx` — 내 보고서 목록
- ✅ `app/(main)/home/page.tsx` — 보고서 목록 + 빠른 보고 버튼 추가

### Phase 5 — 테스트
- ⬜ `ReportServiceTests.cs`
- ✅ `MedicationServiceTests.cs` (5개 케이스)

---

## 주요 아키텍처 결정

| 항목 | 결정 | 이유 |
|------|------|------|
| AI 분석 | Claude claude-haiku-4-5 (Anthropic SDK) | 빠르고 저비용, 해커톤 적합 |
| 약물 상호작용 | Claude API 프롬프트 기반 | KAERS 데이터와 조합, 별도 DB 불필요 |
| IsOfficial | KnownSideEffects DB 대조 (보고 시) | 실시간 판별 |
| AI 환경변수 | `ANTHROPIC_API_KEY` | 표준 SDK 환경 변수 |

---

## 환경 변수 (신규)

```bash
ANTHROPIC_API_KEY=...    # Claude AI 분석용
```

---

## 수동 검증 절차

```bash
# 백엔드 테스트
dotnet test SideReport.sln

# 복용약 추가
curl -X POST http://localhost:5000/api/medications \
  -H "Authorization: Bearer <token>" \
  -H "Content-Type: application/json" \
  -d '{"drugName":"타이레놀","dosage":"500mg","frequency":"1일 3회","startDate":"2026-03-01"}'

# 부작용 보고
curl -X POST http://localhost:5000/api/reports \
  -H "Authorization: Bearer <token>" \
  -H "Content-Type: application/json" \
  -d '{"medicationIds":["<id>"],"symptoms":["두통","어지러움"],"severity":"Mild"}'
```
