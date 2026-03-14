"use client";

import { useEffect } from "react";
import { useRouter } from "next/navigation";
import { useAuthStore } from "@/store/authStore";
import { getRefreshToken } from "@/lib/tokenStorage";

/**
 * 클라이언트 사이드 인증 가드
 * 미들웨어 쿠키 우회를 방어하는 실질적 보호 레이어
 */
export function AuthGuard({ children }: { children: React.ReactNode }) {
  const router = useRouter();
  const { isAuthenticated } = useAuthStore();

  useEffect(() => {
    // Zustand 상태와 refreshToken 모두 없으면 미인증으로 판단
    if (!isAuthenticated && !getRefreshToken()) {
      document.cookie = "sr_auth_flag=; path=/; max-age=0";
      router.replace("/login");
    }
  }, [isAuthenticated, router]);

  if (!isAuthenticated && !getRefreshToken()) return null;

  return <>{children}</>;
}
