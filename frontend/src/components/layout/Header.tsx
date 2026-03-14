"use client";

import { useState } from "react";
import Link from "next/link";
import { useRouter } from "next/navigation";
import { useAuthStore } from "@/store/authStore";
import { apiClient } from "@/lib/apiClient";
import { getRefreshToken, clearTokens } from "@/lib/tokenStorage";

export function Header() {
  const router = useRouter();
  const { user, isAuthenticated, logout } = useAuthStore();
  const [showConfirm, setShowConfirm] = useState(false);

  const handleLogout = async () => {
    try {
      const refreshToken = getRefreshToken();
      if (refreshToken) {
        await apiClient.post("/api/auth/logout", { refreshToken });
      }
    } catch {
      // 실패해도 클라이언트 측 로그아웃은 진행
    } finally {
      clearTokens();
      logout();
      // 미들웨어 인증 플래그 쿠키 삭제
      document.cookie = "sr_auth_flag=; path=/; max-age=0";
      router.push("/login");
    }
  };

  return (
    <>
      <header className="bg-white border-b border-gray-200 sticky top-0 z-40">
        <div className="max-w-md mx-auto px-4 py-3 flex items-center justify-between">
          <Link href="/home" className="text-xl flex items-center gap-1.5">
            <span>💊</span>
            <span className="brand-logo">사이드리포트</span>
          </Link>

          <div className="flex items-center gap-3">
            {isAuthenticated && user && (
              <>
                <span className="text-sm text-gray-600 hidden sm:block">
                  {user.name}
                </span>
                <button
                  onClick={() => setShowConfirm(true)}
                  className="text-sm text-gray-500 hover:text-red-500 transition-colors min-h-0 px-2 py-1"
                >
                  로그아웃
                </button>
              </>
            )}
          </div>
        </div>
      </header>

      {/* 로그아웃 확인 모달 */}
      {showConfirm && (
        <div className="fixed inset-0 z-50 bg-black/50 flex items-center justify-center p-4">
          <div className="bg-white rounded-2xl shadow-xl w-full max-w-xs p-6 text-center">
            <p className="text-lg font-bold text-gray-900 mb-2">로그아웃</p>
            <p className="text-sm text-gray-500 mb-6">정말 로그아웃 하시겠어요?</p>
            <div className="flex gap-3">
              <button
                onClick={() => setShowConfirm(false)}
                className="btn-secondary flex-1"
              >
                취소
              </button>
              <button
                onClick={() => { setShowConfirm(false); handleLogout(); }}
                className="flex-1 bg-red-500 hover:bg-red-600 text-white font-semibold py-3 px-4 rounded-xl text-base min-h-touch transition-colors"
              >
                로그아웃
              </button>
            </div>
          </div>
        </div>
      )}
    </>
  );
}
