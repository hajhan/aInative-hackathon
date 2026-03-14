import axios, {
  AxiosInstance,
  AxiosError,
  InternalAxiosRequestConfig,
} from "axios";
import { getRefreshToken, clearTokens, saveTokens } from "./tokenStorage";

// Zustand store 참조 (circular import 방지를 위해 dynamic import 대신 직접 import)
let getAccessToken: (() => string | null) | null = null;
let updateToken: ((token: string) => void) | null = null;
let logoutUser: (() => void) | null = null;

/**
 * API 클라이언트에 Zustand 스토어를 주입 (App 초기화 시 호출)
 */
export function initApiClient(
  getTokenFn: () => string | null,
  updateTokenFn: (token: string) => void,
  logoutFn: () => void
) {
  getAccessToken = getTokenFn;
  updateToken = updateTokenFn;
  logoutUser = logoutFn;
}

/**
 * axios 인스턴스
 */
export const apiClient: AxiosInstance = axios.create({
  baseURL: process.env.NEXT_PUBLIC_API_BASE_URL || "http://localhost:5000",
  timeout: 10000,
  headers: {
    "Content-Type": "application/json",
  },
});

// ─── Request Interceptor ──────────────────────────────────────────────────────
// Authorization 헤더 자동 주입
apiClient.interceptors.request.use(
  (config: InternalAxiosRequestConfig) => {
    const token = getAccessToken?.();
    if (token) {
      config.headers.Authorization = `Bearer ${token}`;
    }
    return config;
  },
  (error) => Promise.reject(error)
);

// ─── Response Interceptor ─────────────────────────────────────────────────────
// 401 응답 시 Refresh Token으로 재발급 후 원 요청 재시도
let isRefreshing = false;
let failedQueue: Array<{
  resolve: (value: string) => void;
  reject: (error: unknown) => void;
}> = [];

function processQueue(error: unknown, token: string | null = null) {
  failedQueue.forEach(({ resolve, reject }) => {
    if (error) {
      reject(error);
    } else {
      resolve(token!);
    }
  });
  failedQueue = [];
}

apiClient.interceptors.response.use(
  (response) => response,
  async (error: AxiosError) => {
    const originalRequest = error.config as InternalAxiosRequestConfig & {
      _retry?: boolean;
    };

    // 401이 아니거나 이미 재시도한 경우 그냥 에러 반환
    if (error.response?.status !== 401 || originalRequest._retry) {
      return Promise.reject(error);
    }

    // 로그인/리프레시 요청 자체에서 401이면 로그아웃
    if (
      originalRequest.url?.includes("/api/auth/login") ||
      originalRequest.url?.includes("/api/auth/refresh")
    ) {
      return Promise.reject(error);
    }

    if (isRefreshing) {
      // 이미 토큰 갱신 중이면 큐에 추가
      return new Promise<string>((resolve, reject) => {
        failedQueue.push({ resolve, reject });
      }).then((token) => {
        originalRequest.headers.Authorization = `Bearer ${token}`;
        return apiClient(originalRequest);
      });
    }

    originalRequest._retry = true;
    isRefreshing = true;

    const refreshToken = getRefreshToken();
    if (!refreshToken) {
      isRefreshing = false;
      handleLogout();
      return Promise.reject(error);
    }

    try {
      const response = await apiClient.post<{
        accessToken: string;
        expiresIn: number;
      }>("/api/auth/refresh", { refreshToken });

      const newAccessToken = response.data.accessToken;

      // 토큰 갱신
      updateToken?.(newAccessToken);
      saveTokens(newAccessToken, refreshToken);

      apiClient.defaults.headers.common.Authorization = `Bearer ${newAccessToken}`;
      processQueue(null, newAccessToken);

      originalRequest.headers.Authorization = `Bearer ${newAccessToken}`;
      return apiClient(originalRequest);
    } catch (refreshError) {
      processQueue(refreshError);
      handleLogout();
      // 호출자에게 원본 에러(401)를 반환 (refreshError 대신)
      return Promise.reject(error);
    } finally {
      isRefreshing = false;
    }
  }
);

function handleLogout() {
  clearTokens();
  logoutUser?.();
  // Authorization 기본 헤더 초기화
  delete apiClient.defaults.headers.common.Authorization;
  if (typeof window !== "undefined") {
    document.cookie = "sr_auth_flag=; path=/; max-age=0";
    window.location.href = "/login";
  }
}
