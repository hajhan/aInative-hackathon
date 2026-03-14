"use client";

import { useState } from "react";
import Link from "next/link";
import { useRouter } from "next/navigation";
import { useForm } from "react-hook-form";
import { zodResolver } from "@hookform/resolvers/zod";
import { z } from "zod";
import { useAuthStore } from "@/store/authStore";
import { apiClient } from "@/lib/apiClient";
import { saveTokens } from "@/lib/tokenStorage";
import { BrandLogo } from "@/components/auth/BrandLogo";

// 유효성 검사 스키마
const loginSchema = z.object({
  email: z
    .string()
    .min(1, "이메일을 입력해 주세요.")
    .email("올바른 이메일 형식이 아닙니다."),
  password: z.string().min(1, "비밀번호를 입력해 주세요."),
});

type LoginFormData = z.infer<typeof loginSchema>;

/** JWT payload를 UTF-8 안전하게 디코딩 */
function decodeJwtPayload(token: string): Record<string, unknown> {
  try {
    const base64 = token.split(".")[1].replace(/-/g, "+").replace(/_/g, "/");
    return JSON.parse(
      new TextDecoder().decode(Uint8Array.from(atob(base64), (c) => c.charCodeAt(0)))
    );
  } catch {
    return {};
  }
}

export default function LoginPage() {
  const router = useRouter();
  const { login } = useAuthStore();
  const [serverError, setServerError] = useState<string | null>(null);

  const {
    register,
    handleSubmit,
    formState: { errors, isSubmitting },
  } = useForm<LoginFormData>({
    resolver: zodResolver(loginSchema),
  });

  const onSubmit = async (data: LoginFormData) => {
    setServerError(null);
    try {
      const response = await apiClient.post<{
        accessToken: string;
        refreshToken: string;
        expiresIn: number;
      }>("/api/auth/login", data);

      const { accessToken, refreshToken } = response.data;

      saveTokens(accessToken, refreshToken);

      // 서버가 서명한 JWT payload에서 사용자 정보 추출
      const payload = decodeJwtPayload(accessToken);
      login(
        {
          id: String(payload.sub ?? ""),
          email: String(payload.email ?? data.email),
          name: String(payload.name ?? ""),
        },
        accessToken
      );

      // 미들웨어 UX 힌트 쿠키 설정 (보안 게이트가 아닌 빠른 리다이렉트용)
      document.cookie = "sr_auth_flag=1; path=/";

      router.push("/home");
    } catch (error: unknown) {
      const err = error as { response?: { data?: { message?: string } } };
      setServerError(
        err.response?.data?.message ?? "로그인에 실패했습니다. 다시 시도해 주세요."
      );
    }
  };

  return (
    <div className="min-h-screen bg-gray-50 flex flex-col justify-center px-4 py-12">
      <div className="w-full max-w-sm mx-auto">
        <BrandLogo />

        {/* 로그인 폼 */}
        <div className="card">
          <h2 className="text-xl font-bold text-gray-900 mb-6">로그인</h2>

          {serverError && (
            <div className="mb-4 p-3 bg-red-50 border border-red-200 rounded-xl">
              <p className="text-sm text-red-600">{serverError}</p>
            </div>
          )}

          <form onSubmit={handleSubmit(onSubmit)} noValidate className="space-y-5">
            <div>
              <label htmlFor="email" className="form-label">이메일</label>
              <input
                id="email"
                type="email"
                autoComplete="email"
                placeholder="이메일을 입력해 주세요"
                className={`input-field ${errors.email ? "input-error" : ""}`}
                {...register("email")}
              />
              {errors.email && <p className="error-message">{errors.email.message}</p>}
            </div>

            <div>
              <label htmlFor="password" className="form-label">비밀번호</label>
              <input
                id="password"
                type="password"
                autoComplete="current-password"
                placeholder="비밀번호를 입력해 주세요"
                className={`input-field ${errors.password ? "input-error" : ""}`}
                {...register("password")}
              />
              {errors.password && <p className="error-message">{errors.password.message}</p>}
            </div>

            <button type="submit" disabled={isSubmitting} className="btn-primary mt-2">
              {isSubmitting ? "로그인 중..." : "로그인"}
            </button>
          </form>

          <div className="mt-6 text-center">
            <p className="text-sm text-gray-600">
              아직 계정이 없으신가요?{" "}
              <Link href="/register" className="text-primary-600 font-semibold hover:underline">
                회원가입
              </Link>
            </p>
          </div>
        </div>
      </div>
    </div>
  );
}
