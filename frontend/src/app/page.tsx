import { redirect } from "next/navigation";

/**
 * 루트 페이지 — 인증 상태에 따라 리다이렉트
 * 미들웨어에서도 처리하지만 서버 컴포넌트에서 명시적으로 처리
 */
export default function RootPage() {
  // 미들웨어(middleware.ts)가 JWT 확인 후 적절히 redirect 처리
  // 미들웨어가 없는 경우 로그인 페이지로 fallback
  redirect("/login");
}
