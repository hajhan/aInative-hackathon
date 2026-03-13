# SideReport 개발 로드맵

> 약물 부작용 보고 서비스 — 약봉투 사진 한 장으로 시작하는 부작용 리포팅 플랫폼

---

## 전체 일정 요약

| 스프린트 | 기간 | 범위 |
|----------|------|------|
| Sprint 1 | 2주 | 프로젝트 기반 설정 + 인증 + DB 설계 |
| Sprint 2 | 2주 | OCR 인식 + 약품 정보 파싱 |
| Sprint 3 | 2주 | 부작용 보고 + 내 복용약 관리 + AI 분석 (MVP 완성) |
| Sprint 4 | 2주 | 보고 이력 조회 + 의료진 공유 (PDF/링크) + 알림 |
| Sprint 5 | 2주 | 부작용 예측 모델 + 커뮤니티/통계 |
| Sprint 6 | 1주 | 성능 최적화 + 보안 강화 + 운영 안정화 |

- **MVP 범위**: Sprint 1 ~ 3
- **Post-MVP 범위**: Sprint 4 ~ 6

---

## Sprint 1 — 프로젝트 기반 설정 및 인증

### 목표
모든 기능 개발의 토대가 되는 인프라, 프로젝트 구조, 데이터베이스 스키마, 사용자 인증 시스템을 구축한다.

### 작업 항목

#### 인프라 & DevOps
- [ ] Docker Compose 파일 작성 (Next.js 컨테이너, ASP.NET Core 컨테이너, PostgreSQL 컨테이너)
- [ ] 개발/스테이징/프로덕션 환경 분리를 위한 `.env` 파일 및 환경 변수 전략 수립
- [ ] GitHub Actions CI 파이프라인 구성 (빌드 + 테스트 자동화)
- [ ] Azure Blob Storage (또는 AWS S3) 버킷 생성 및 접근 권한 설정

#### 백엔드 — ASP.NET Core
- [ ] ASP.NET Core 프로젝트 초기화 (Clean Architecture 폴더 구조: Api / Application / Domain / Infrastructure)
- [ ] ASP.NET Core Identity + JWT 인증 설정 (회원가입, 로그인, 토큰 발급/갱신, 로그아웃)
- [ ] PostgreSQL 연결 설정 (Entity Framework Core + Npgsql)
- [ ] 초기 데이터베이스 마이그레이션 작성
  - `Users` 테이블 (id, email, password_hash, name, created_at)
  - `Medications` 테이블 (id, user_id, drug_name, dosage, frequency, start_date, end_date)
  - `AdverseReports` 테이블 (id, user_id, reported_at, severity, notes)
  - `ReportSymptoms` 테이블 (id, report_id, symptom_name, is_official)
  - `ReportMedications` 테이블 (id, report_id, medication_id)
  - `OcrImages` 테이블 (id, user_id, storage_url, processed_at, delete_after_processing)
- [ ] Global Exception Handler 미들웨어 구현
- [ ] Swagger / OpenAPI 문서 설정

#### 프론트엔드 — Next.js (TypeScript)
- [ ] Next.js 프로젝트 초기화 (TypeScript, App Router, Tailwind CSS)
- [ ] 공통 레이아웃 컴포넌트 작성 (헤더, 하단 네비게이션 바)
- [ ] 회원가입 / 로그인 페이지 UI 구현 (고령자 친화 대형 글꼴)
- [ ] JWT 토큰 관리 유틸리티 (axios interceptor + refresh token 로직)
- [ ] 전역 상태 관리 설정 (Zustand 또는 Context API)
- [ ] PWA 설정 (`next-pwa`: manifest.json, service worker)

### 완료 기준
- Docker Compose로 전체 스택 로컬 실행 가능
- 회원가입 → 로그인 → JWT 발급 흐름 E2E 동작
- 데이터베이스 마이그레이션이 오류 없이 적용됨
- CI 파이프라인이 PR마다 빌드 성공 여부를 체크함

### 예상 산출물
- `docker-compose.yml` (개발용)
- ASP.NET Core 백엔드 프로젝트 (인증 API 포함)
- Next.js 프론트엔드 프로젝트 (로그인/회원가입 화면)
- PostgreSQL 초기 스키마 마이그레이션 파일
- GitHub Actions CI 워크플로우 파일

---

## Sprint 2 — 약봉투 OCR 인식 및 약품 정보 파싱

### 목표
약봉투 사진을 업로드하면 OCR로 텍스트를 추출하고, 식약처 공공 API와 대조해 약품명·용량·복용 횟수를 정확히 파싱하는 핵심 기능을 완성한다.

### 작업 항목

#### 백엔드 — OCR 처리 파이프라인
- [ ] Azure AI Vision API (또는 Google Cloud Vision API) 연동 서비스 클래스 구현
  - 이미지 업로드 → OCR 요청 → 텍스트 응답 파싱
- [ ] 이미지 업로드 엔드포인트 구현 (`POST /api/ocr/upload`)
  - 이미지를 Azure Blob Storage / S3에 임시 저장
  - OCR 처리 완료 후 이미지 즉시 삭제 옵션 적용 (개인정보 보호 요구사항)
- [ ] OCR 텍스트에서 약품명, 용량, 복용 횟수를 추출하는 파서 구현
  - 정규식 기반 1차 파싱 (예: "OOO 정 500mg 1일 3회")
  - 파싱 실패 시 원본 텍스트 반환하여 사용자 수동 입력 유도
- [ ] 식약처 공공 API 연동 서비스 구현
  - 파싱된 약품명으로 식약처 API 조회 (약품 공식명칭, 성분, 효능 정보 가져오기)
  - 결과를 DB `Medications` 테이블에 저장
- [ ] OCR 처리 결과 응답 DTO 설계 및 엔드포인트 구현 (`GET /api/ocr/{jobId}/result`)
- [ ] OCR 응답 속도 5초 이내 달성을 위한 비동기 처리 (Background Job 또는 async/await 최적화)

#### 백엔드 — 약품 데이터 초기화
- [ ] KAERS(한국의약품안전관리원) 공공 데이터 다운로드 및 DB 적재 스크립트 작성
  - `KnownSideEffects` 테이블: (id, drug_name, symptom_name, frequency_rank, source)
- [ ] 식약처 API 호출 결과 캐싱 레이어 구현 (동일 약품명 중복 호출 방지)

#### 프론트엔드 — OCR 화면
- [ ] 약봉투 이미지 업로드/촬영 화면 구현
  - 갤러리 업로드 버튼
  - 카메라 직접 촬영 버튼 (PWA MediaDevices API)
  - 업로드 전 이미지 미리보기
- [ ] OCR 처리 중 로딩 스피너 및 진행 상태 표시 UI
- [ ] OCR 결과 확인 화면 구현
  - 인식된 약품 목록 카드 표시 (약품명, 용량, 복용 횟수)
  - 각 항목 수동 수정 가능한 인라인 편집 기능
  - "저장" 버튼으로 복용약 목록에 추가
- [ ] OCR 인식 실패 시 수동 약품 입력 폼 제공

### 완료 기준
- 실제 약봉투 사진 5종 테스트 시 약품명 인식 정확도 85% 이상
- OCR API 호출부터 결과 화면 렌더링까지 5초 이내
- 식약처 API 대조 후 공식 약품명으로 보정 동작 확인
- 이미지 처리 후 즉시 삭제 옵션 정상 동작 확인

### 예상 산출물
- OCR 처리 API 엔드포인트 (`/api/ocr/*`)
- 식약처 / KAERS 연동 서비스 클래스
- 약봉투 촬영 및 결과 확인 프론트엔드 화면
- KAERS 데이터 초기 적재 스크립트
- `KnownSideEffects` DB 테이블 및 마이그레이션

---

## Sprint 3 — 부작용 보고 + 내 복용약 관리 + AI 분석 (MVP 완성)

### 목표
부작용 보고 제출, 내 복용약 목록 관리, 약물 공식 부작용 및 상호작용 안내 기능을 완성하여 MVP를 출시 가능한 상태로 만든다.

### 작업 항목

#### 백엔드 — 부작용 보고 API
- [ ] 부작용 보고 생성 엔드포인트 구현 (`POST /api/reports`)
  - 요청 바디: 복용약 목록, 증상 목록(선택 + 직접 입력), 심각도(경미/보통/심각), 메모
- [ ] 부작용 보고 목록 조회 엔드포인트 (`GET /api/reports`) — 날짜/약품/증상 필터 지원
- [ ] 부작용 보고 상세 조회 엔드포인트 (`GET /api/reports/{id}`)
- [ ] 특정 약품에 대한 자주 보고된 증상 목록 반환 엔드포인트 (`GET /api/drugs/{drugName}/symptoms`)
  - KAERS 데이터 기반으로 빈도 순 정렬하여 반환

#### 백엔드 — 내 복용약 관리 API
- [ ] 복용약 추가 엔드포인트 (`POST /api/medications`)
- [ ] 복용약 수정 엔드포인트 (`PUT /api/medications/{id}`)
- [ ] 복용약 삭제 엔드포인트 (`DELETE /api/medications/{id}`)
- [ ] 복용약 목록 조회 엔드포인트 (`GET /api/medications`)

#### 백엔드 — AI 분석 서비스
- [ ] 선택된 증상이 해당 약품의 공식 부작용인지 판별하는 서비스 구현
  - KAERS 데이터 + 식약처 데이터 기반 조회
  - 응답에 `is_official: true/false`, `official_source` 필드 포함
- [ ] 동시 복용 약물 간 상호작용 주의 안내 서비스 구현
  - 식약처 API의 병용 금기/주의 데이터 활용
  - 상호작용이 있는 약물 쌍 목록 및 주의 메시지 반환
- [ ] 분석 결과 엔드포인트 (`POST /api/analysis/check`)

#### 프론트엔드 — 부작용 보고 화면
- [ ] 부작용 보고 스텝 UI 구현 (다단계 폼)
  - Step 1: 복용약 목록 확인 및 선택
  - Step 2: 증상 선택 (체크박스 목록, KAERS 기반 빈도 순 표시) + 직접 입력 텍스트 필드
  - Step 3: 심각도 선택 (경미 / 보통 / 심각 버튼)
  - Step 4: 추가 메모 입력 및 제출 확인
- [ ] 증상 선택 시 실시간으로 공식 부작용 여부 뱃지 표시 ("공식 부작용" / "미보고 증상")
- [ ] 약물 상호작용 경고 배너 표시 (복용약 2종 이상 선택 시)
- [ ] 보고 제출 완료 화면 (면책 고지 포함)

#### 프론트엔드 — 내 복용약 관리 화면
- [ ] 복용약 목록 화면 구현 (카드 형태, 약품명/용량/복용 기간 표시)
- [ ] 복용약 추가 모달/페이지 구현 (수동 입력 폼)
- [ ] 복용약 수정/삭제 기능 구현
- [ ] 홈 화면 구현 (복용약 요약 + 빠른 보고 버튼)

#### 공통
- [ ] 면책 고지 텍스트 컴포넌트 작성 ("본 서비스는 의료 진단 서비스가 아닙니다...")
- [ ] 접근성 개선: 최소 글꼴 크기 16px 적용, 버튼 최소 터치 영역 44x44px 보장

### 완료 기준
- 약봉투 촬영 → OCR 인식 → 증상 선택 → 보고 제출까지 전체 플로우 E2E 동작
- 부작용 보고 완료율 테스트: 시나리오 10건 중 7건 이상 완료 (70% 기준)
- 공식 부작용 여부 뱃지가 KAERS 데이터와 일치
- 복용약 추가/수정/삭제 CRUD 정상 동작
- 면책 고지가 보고 완료 화면에 표시됨

### 예상 산출물
- 부작용 보고 API 전체 (`/api/reports/*`, `/api/analysis/*`)
- 내 복용약 관리 API 전체 (`/api/medications/*`)
- 부작용 보고 스텝 폼 화면
- 내 복용약 관리 화면
- 홈 화면
- **MVP 배포 가능한 Docker 이미지** (docker-compose.prod.yml)

---

## Sprint 4 — 보고 이력 조회 + 의료진 공유 + 알림 (Post-MVP)

### 목표
사용자가 자신의 보고 이력을 체계적으로 조회하고, 의료진과 공유할 수 있는 PDF/링크 기능과 푸시 알림을 제공한다.

### 작업 항목

#### 백엔드
- [ ] 보고 이력 조회 API 고도화 (날짜 범위, 약품명, 증상명 복합 필터링 + 페이지네이션)
- [ ] PDF 보고서 생성 서비스 구현 (QuestPDF 또는 iTextSharp 라이브러리 활용)
  - 포함 내용: 복용약 목록, 증상 목록, 심각도, 보고 날짜, 면책 고지
- [ ] 공유 링크 생성 엔드포인트 (`POST /api/reports/{id}/share`)
  - 토큰 기반 단기 접근 URL 생성 (유효 기간 설정 가능)
- [ ] 웹 푸시 알림 서비스 구현 (Web Push API / Firebase Cloud Messaging)
  - 특정 약품의 부작용 급증 감지 로직 (최근 7일 보고 건수 임계치 초과 시)
  - 복용 시간 알림 스케줄러 구현 (ASP.NET Core Background Service)
- [ ] 알림 구독/해제 엔드포인트 (`POST /api/notifications/subscribe`, `DELETE /api/notifications/subscribe`)

#### 프론트엔드
- [ ] 보고 이력 목록 화면 구현 (날짜/약품/증상 필터 UI, 무한 스크롤 또는 페이지네이션)
- [ ] 보고서 상세 화면 구현
- [ ] PDF 다운로드 버튼 및 공유 링크 복사 버튼 구현
- [ ] 공유 링크로 접근 시 읽기 전용 보고서 화면 구현 (비인증 접근 허용)
- [ ] 푸시 알림 권한 요청 및 구독 설정 UI 구현
- [ ] 복용 시간 알림 설정 화면 구현

### 완료 기준
- 보고 이력 필터링 및 페이지네이션 동작 확인
- PDF 생성 및 다운로드 정상 동작 (모바일 브라우저 포함)
- 공유 링크로 비인증 사용자가 보고서 열람 가능
- 테스트 디바이스에서 푸시 알림 수신 확인

### 예상 산출물
- 보고 이력 조회 화면
- PDF 보고서 생성 서비스 및 다운로드 API
- 공유 링크 기능
- 푸시 알림 서비스 (FCM 또는 Web Push)
- 알림 설정 화면

---

## Sprint 5 — 부작용 예측 모델 + 커뮤니티/통계 (Post-MVP)

### 목표
축적된 사용자 데이터와 KAERS 공공 데이터를 결합하여 미보고 부작용 패턴을 탐지하고, 익명 통계를 커뮤니티에 공개한다.

### 작업 항목

#### 백엔드 — 예측 모델
- [ ] 사용자 부작용 데이터 집계 파이프라인 구현 (약품 + 증상 조합 빈도 집계)
- [ ] 통계적 이상 탐지 로직 구현
  - 특정 약품-증상 조합의 보고 빈도가 KAERS 기준 대비 유의미하게 높을 경우 플래그 처리
  - 결과를 `EmergingSignals` 테이블에 저장
- [ ] 약물 조합별 부작용 예측 API 엔드포인트 (`POST /api/prediction/combination`)
- [ ] 배치 집계 스케줄러 구현 (야간 주기 실행, ASP.NET Core Background Service)

#### 백엔드 — 커뮤니티/통계 API
- [ ] 약품별 익명 부작용 통계 조회 API (`GET /api/stats/drugs/{drugName}`)
  - 증상별 보고 비율, 심각도 분포 반환
  - 개인 식별 불가한 집계 수치만 제공
- [ ] 신호 탐지된 약품 목록 조회 API (`GET /api/stats/signals`)

#### 프론트엔드
- [ ] 약품 상세 페이지 구현 (통계: 증상 빈도 차트, 심각도 분포 차트)
- [ ] "이 약을 복용한 OOO명이 보고한 증상" 통계 카드 컴포넌트
- [ ] 신호 탐지 알림 배너 ("주의: 최근 이 약에 대한 새로운 부작용 보고가 증가하고 있습니다")
- [ ] 약물 조합 예측 결과 화면 구현

### 완료 기준
- 집계 배치 스케줄러가 야간에 오류 없이 실행됨
- 약품 통계 화면에서 개인 식별 데이터 노출 없음 (Privacy 검증)
- 약물 조합 예측 API 응답 시간 2초 이내
- 차트 라이브러리(Recharts 등) 기반 통계 시각화 동작 확인

### 예상 산출물
- 이상 탐지 배치 서비스
- 커뮤니티 통계 API
- 약품 통계 시각화 화면
- 약물 조합 예측 화면

---

## Sprint 6 — 성능 최적화 + 보안 강화 + 운영 안정화 (Post-MVP)

### 목표
프로덕션 운영에 필요한 성능, 보안, 모니터링 인프라를 완성하여 서비스를 안정적으로 운영 가능한 상태로 만든다.

### 작업 항목

#### 성능 최적화
- [ ] PostgreSQL 쿼리 성능 분석 및 인덱스 추가 (자주 조회되는 컬럼: user_id, drug_name, reported_at)
- [ ] Next.js 이미지 최적화 (`next/image`) 및 번들 사이즈 분석 (Lighthouse 점수 목표: 90+)
- [ ] OCR API 응답 캐싱 전략 검토 (동일 이미지 해시 중복 처리 방지)
- [ ] API 응답 페이지네이션 및 데이터 직렬화 최적화

#### 보안 강화
- [ ] 의료 데이터 암호화 저장 검토 (PostgreSQL 컬럼 레벨 암호화 또는 애플리케이션 레벨 암호화)
- [ ] Rate Limiting 미들웨어 적용 (OCR API 엔드포인트 특히 강화)
- [ ] OWASP Top 10 기준 취약점 점검 및 조치
- [ ] 입력값 유효성 검사 강화 (FluentValidation 전체 API 적용)
- [ ] HTTPS 강제 적용 및 보안 헤더 설정 (HSTS, CSP, X-Frame-Options)

#### 모니터링 & 운영
- [ ] 애플리케이션 로깅 구성 (Serilog → Azure Monitor 또는 ELK Stack)
- [ ] 헬스체크 엔드포인트 구현 (`/health`, `/health/ready`)
- [ ] Docker Compose 프로덕션 설정 최종화 (리소스 제한, 재시작 정책)
- [ ] 데이터베이스 백업 자동화 스크립트 작성
- [ ] 식약처 API 라이선스 및 데이터 활용 범위 최종 법무 검토

### 완료 기준
- Lighthouse Performance 점수 90 이상 (모바일 기준)
- 부하 테스트(동시 사용자 50명) 시 OCR API 응답 95퍼센타일 5초 이내
- OWASP 점검 항목 Critical/High 취약점 0건
- 헬스체크 엔드포인트 정상 응답 확인

### 예상 산출물
- 최적화된 프로덕션 Docker 이미지
- 모니터링 대시보드 설정
- 보안 점검 보고서
- 데이터베이스 백업 스크립트
- 운영 가이드 문서

---

## 기술 의존성 정리

| 기술 | 용도 | 스프린트 |
|------|------|----------|
| Next.js (TypeScript) + Tailwind CSS | 프론트엔드 | Sprint 1~ |
| ASP.NET Core (C#) | 백엔드 API | Sprint 1~ |
| PostgreSQL + Entity Framework Core | 데이터베이스 | Sprint 1~ |
| Docker + Docker Compose | 컨테이너화 및 배포 | Sprint 1~ |
| Azure AI Vision / Google Cloud Vision | OCR 처리 | Sprint 2~ |
| Azure Blob Storage / AWS S3 | 약봉투 이미지 임시 저장 | Sprint 2~ |
| 식약처 공공 API | 약품 정보 대조 및 보정 | Sprint 2~ |
| KAERS 공공 데이터 | 자주 보고된 증상 목록 제공 | Sprint 2~ |
| ASP.NET Core Identity + JWT | 사용자 인증 | Sprint 1~ |
| Firebase Cloud Messaging / Web Push | 푸시 알림 | Sprint 4~ |
| QuestPDF / iTextSharp | PDF 보고서 생성 | Sprint 4~ |
| Recharts (또는 Chart.js) | 통계 시각화 | Sprint 5~ |
| Serilog + Azure Monitor | 로깅 및 모니터링 | Sprint 6~ |

---

## 주요 리스크 및 대응 방안

| 리스크 | 영향도 | 대응 방안 |
|--------|--------|-----------|
| 식약처 API 할당량 초과 | 높음 | 결과 캐싱 레이어 구현, API 요청 최소화 |
| OCR 정확도 85% 미달 | 높음 | 수동 입력 폼 항상 제공, OCR 실패 시 graceful fallback |
| 약봉투 이미지 개인정보 노출 | 매우 높음 | OCR 처리 즉시 삭제 옵션 기본값 ON, 이미지 URL 외부 노출 금지 |
| KAERS 데이터 라이선스 제약 | 중간 | 데이터 활용 범위 사전 법무 검토 (Sprint 1 시작 전) |
| 의료 정보 오남용 | 높음 | 면책 고지 필수 노출, 진단/처방 기능 일체 미제공 |
