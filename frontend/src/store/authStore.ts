import { create } from "zustand";

export interface User {
  id: string;
  email: string;
  name: string;
}

interface AuthState {
  user: User | null;
  accessToken: string | null;
  isAuthenticated: boolean;

  // 액션
  login: (user: User, accessToken: string) => void;
  logout: () => void;
  updateToken: (accessToken: string) => void;
}

/**
 * 인증 전역 상태 (Zustand)
 * - accessToken: 메모리에만 보관 (XSS 방지)
 * - refreshToken: tokenStorage.ts를 통해 sessionStorage에 보관
 */
export const useAuthStore = create<AuthState>((set) => ({
  user: null,
  accessToken: null,
  isAuthenticated: false,

  login: (user, accessToken) =>
    set({ user, accessToken, isAuthenticated: true }),

  logout: () =>
    set({ user: null, accessToken: null, isAuthenticated: false }),

  updateToken: (accessToken) =>
    set({ accessToken }),
}));
