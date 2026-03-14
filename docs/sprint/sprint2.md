# Sprint 2 — 약봉투 OCR 인식 및 약품 정보 파싱

**기간**: 2026-03-13
**브랜치**: `sprint2`
**목표**: 약봉투 사진 → OCR → 약품 파싱 → 식약처 검증 파이프라인 구현

---

## 완료 체크리스트

### Phase 1 — Domain & 마이그레이션
- ✅ `OcrStatus` Enum 추가 (Pending, Processing, Completed, Failed)
- ✅ `KnownSideEffect` 엔티티 추가 (KAERS 약품-부작용 쌍)
- ✅ `DrugCache` 엔티티 추가 (식약처 API 캐시)
- ✅ `OcrImage` 엔티티 확장 (RawOcrText, ParsedResultJson, Status 필드)
- ✅ EF Core 마이그레이션 `AddOcrAndDrugEntities` 추가
- ✅ `ApplicationDbContext` DbSet 추가 (KnownSideEffects, DrugCaches)

### Phase 2 — Application 인터페이스 & DTO
- ✅ `IOcrService` 인터페이스
- ✅ `IImageStorageService` 인터페이스
- ✅ `IDrugInfoService` 인터페이스
- ✅ `IDrugParserService` 인터페이스
- ✅ `IOcrPipelineService` 인터페이스
- ✅ `UploadOcrImageCommand` DTO
- ✅ `OcrUploadResult`, `ParsedDrugItem`, `DrugInfo` DTO

### Phase 3 — Infrastructure 서비스
- ✅ `MockOcrService` (샘플 약봉투 텍스트 3종 반환)
- ✅ `GoogleVisionOcrService` (Google Cloud Vision REST API)
- ✅ `LocalImageStorageService` (wwwroot/uploads/ocr/ 로컬 저장)
- ✅ `DrugTextParserService` (정규식 기반 약품명/용량/횟수 파싱)
- ✅ `MockDrugInfoService` (타이레놀, 아스피린 등 20개 프리셋)
- ✅ `MfdsDrugInfoService` (공공데이터포털 식약처 API + DB 캐시)
- ✅ `OcrPipelineService` (전체 파이프라인 오케스트레이션)

### Phase 4 — DI & 설정
- ✅ `DependencyInjection.cs` 업데이트 (Mock/Real 조건부 등록)
- ✅ `appsettings.json` OCR/DrugInfo 설정 섹션 추가
- ✅ `.env.example` OCR 환경변수 추가
- ✅ `docker-compose.yml` 환경변수 전달 및 업로드 볼륨 추가
- ✅ `Program.cs` 요청 크기 제한, 정적 파일, KAERS 시드 적재

### Phase 5 — API 컨트롤러
- ✅ `OcrController` 구현
  - `POST /api/ocr/upload` — 이미지 업로드 → OCR → 파싱
  - `GET /api/ocr/{id}` — 결과 조회
  - `POST /api/ocr/{id}/confirm` — 약품 Medications에 저장

### Phase 6 — KAERS 시드 데이터
- ✅ `backend/data/kaers-seed.json` (117건, 주요 약품 부작용)
- ✅ 시작 시 자동 적재 로직

### Phase 7 — 프론트엔드
- ✅ `lib/types/ocr.ts` TypeScript 타입
- ✅ `store/ocrStore.ts` Zustand OCR 상태 관리
- ✅ `store/index.ts` ocrStore export 추가
- ✅ `components/ocr/ImageUploader.tsx` (카메라 + 갤러리)
- ✅ `components/ocr/ProcessingSpinner.tsx`
- ✅ `components/ocr/OcrResultCard.tsx`
- ✅ `components/ocr/DrugEditModal.tsx`
- ✅ `components/ocr/ManualDrugForm.tsx`
- ✅ `app/(main)/ocr/page.tsx` — 이미지 업로드 화면
- ✅ `app/(main)/ocr/result/page.tsx` — 결과 확인/편집
- ✅ `app/(main)/home/page.tsx` — OCR 버튼 활성화 (`/ocr` 링크)
- ✅ `middleware.ts` `/ocr` 경로 보호 추가

### Phase 8 — 테스트
- ✅ `DrugTextParserServiceTests.cs` (10+ 케이스)
- ✅ `OcrPipelineServiceTests.cs` (Mock 기반 서비스 테스트)

---

## 주요 아키텍처 결정

| 항목 | 결정 | 이유 |
|------|------|------|
| OCR | Mock 기본, `Ocr:UseMock=false` 시 Google Vision | 키 없이도 개발 가능 |
| 이미지 저장 | 로컬 파일시스템 | 해커톤용, IImageStorageService로 추후 전환 가능 |
| 식약처 API | Mock 기본, `DrugInfo:UseMock=false` 시 공공데이터포털 | 오프라인 개발 지원 |
| KAERS 시드 | JSON 파일 → DB 적재 | 117개 약품-부작용 쌍 |
| OCR 처리 | 동기 async/await | Vision API 1-3초, Job Queue 불필요 |

---

## 환경 변수 (신규)

```bash
OCR_USE_MOCK=true                  # false: Google Cloud Vision 사용
GOOGLE_VISION_API_KEY=...          # OCR_USE_MOCK=false 시 필요
DRUG_INFO_USE_MOCK=true            # false: 공공데이터포털 식약처 API 사용
MFDS_API_KEY=...                   # DRUG_INFO_USE_MOCK=false 시 필요
```

---

## 수동 검증 절차

```bash
# 전체 스택 빌드
docker compose up --build

# 백엔드 테스트
dotnet test SideReport.sln

# OCR E2E 테스트 (Mock 모드)
# 1. /login → 로그인
# 2. 홈 화면 → "사진 촬영 / 갤러리 선택" 버튼 클릭
# 3. 이미지 선택 → "인식하기" 버튼
# 4. 결과 확인 → 수정/삭제/추가
# 5. "저장하기" → 홈으로 이동
```
