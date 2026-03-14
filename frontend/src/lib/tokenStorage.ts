/**
 * 토큰 저장/조회/삭제 유틸리티
 *
 * 보안 전략:
 * - Access Token: Zustand 메모리 스토어에만 보관 (탭/새로고침 시 휘발)
 * - Refresh Token: sessionStorage (탭 닫으면 삭제, httpOnly Cookie가 더 안전하나 SSR 미사용 시 대안)
 */

const REFRESH_TOKEN_KEY = "sr_rt";

/**
 * Access Token과 Refresh Token을 함께 저장
 * - accessToken은 Zustand store에서 별도 관리 (여기서는 refresh만 저장)
 */
export function saveTokens(_accessToken: string, refreshToken: string): void {
  if (typeof window === "undefined") return;
  // _accessToken은 Zustand store에서 관리 (login() 호출 시 처리됨)
  sessionStorage.setItem(REFRESH_TOKEN_KEY, refreshToken);
}

/**
 * Refresh Token 조회
 */
export function getRefreshToken(): string | null {
  if (typeof window === "undefined") return null;
  return sessionStorage.getItem(REFRESH_TOKEN_KEY);
}

/**
 * 모든 토큰 삭제
 */
export function clearTokens(): void {
  if (typeof window === "undefined") return;
  sessionStorage.removeItem(REFRESH_TOKEN_KEY);
}
