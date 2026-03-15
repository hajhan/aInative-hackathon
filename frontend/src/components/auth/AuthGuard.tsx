"use client";

import { useEffect, useState } from "react";
import { useRouter } from "next/navigation";
import { useAuthStore } from "@/store/authStore";
import { getRefreshToken } from "@/lib/tokenStorage";

/**
 * 클라이언트 사이드 인증 가드
 * - hydration 완료 전 null 반환 방지 (로그인 사용자 화면 깜빡임 방지)
 * - 미들웨어 쿠키 우회를 방어하는 실질적 보호 레이어
 */
export function AuthGuard({ children }: { children: React.ReactNode }) {
  const router = useRouter();
  const { isAuthenticated } = useAuthStore();
  const [mounted, setMounted] = useState(false);

  useEffect(() => {
    setMounted(true);
  }, []);

  useEffect(() => {
    if (!mounted) return;
    // Zustand 상태와 refreshToken 모두 없으면 미인증으로 판단
    if (!isAuthenticated && !getRefreshToken()) {
      document.cookie = "sr_auth_flag=; path=/; max-age=0";
      router.replace("/login");
    }
  }, [mounted, isAuthenticated, router]);

  // hydration 전: 스켈레톤 표시 (깜빡임 및 의도치 않은 리다이렉트 방지)
  if (!mounted) {
    return <div className="min-h-screen bg-gray-50" />;
  }

  if (!isAuthenticated && !getRefreshToken()) return null;

  return <>{children}</>;
}
