"use client";

import React from "react";

/**
 * 전역 Provider 래퍼 컴포넌트
 * Zustand, React Query 등 클라이언트 전용 Provider를 여기에 등록
 */
export function Providers({ children }: { children: React.ReactNode }) {
  return <>{children}</>;
}
