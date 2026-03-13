"use client";

import Link from "next/link";
import { useRouter } from "next/navigation";
import { useAuthStore } from "@/store/authStore";
import { apiClient } from "@/lib/apiClient";
import { getRefreshToken, clearTokens } from "@/lib/tokenStorage";

export function Header() {
  const router = useRouter();
  const { user, isAuthenticated, logout } = useAuthStore();

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
      router.push("/login");
    }
  };

  return (
    <header className="bg-white border-b border-gray-200 sticky top-0 z-40">
      <div className="max-w-md mx-auto px-4 py-3 flex items-center justify-between">
        <Link href="/home" className="text-xl font-bold text-primary-600">
          사이드리포트
        </Link>

        <div className="flex items-center gap-3">
          {isAuthenticated && user && (
            <>
              <span className="text-sm text-gray-600 hidden sm:block">
                {user.name}
              </span>
              <button
                onClick={handleLogout}
                className="text-sm text-gray-500 hover:text-red-500 transition-colors min-h-0 px-2 py-1"
              >
                로그아웃
              </button>
            </>
          )}
        </div>
      </div>
    </header>
  );
}
