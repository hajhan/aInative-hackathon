"use client";

import { useState } from "react";
import Link from "next/link";
import { useRouter } from "next/navigation";
import { useForm } from "react-hook-form";
import { zodResolver } from "@hookform/resolvers/zod";
import { z } from "zod";
import { apiClient } from "@/lib/apiClient";
import { BrandLogo } from "@/components/auth/BrandLogo";

// 유효성 검사 스키마
const registerSchema = z
  .object({
    name: z
      .string()
      .min(1, "이름을 입력해 주세요.")
      .max(100, "이름은 100자를 초과할 수 없습니다."),
    email: z
      .string()
      .min(1, "이메일을 입력해 주세요.")
      .email("올바른 이메일 형식이 아닙니다."),
    password: z
      .string()
      .min(8, "비밀번호는 8자 이상이어야 합니다.")
      .regex(/[A-Z]/, "대문자를 하나 이상 포함해야 합니다.")
      .regex(/[0-9]/, "숫자를 하나 이상 포함해야 합니다."),
    passwordConfirm: z.string().min(1, "비밀번호 확인을 입력해 주세요."),
  })
  .refine((data) => data.password === data.passwordConfirm, {
    message: "비밀번호가 일치하지 않습니다.",
    path: ["passwordConfirm"],
  });

type RegisterFormData = z.infer<typeof registerSchema>;

export default function RegisterPage() {
  const router = useRouter();
  const [serverError, setServerError] = useState<string | null>(null);

  const {
    register,
    handleSubmit,
    formState: { errors, isSubmitting },
  } = useForm<RegisterFormData>({
    resolver: zodResolver(registerSchema),
  });

  const onSubmit = async (data: RegisterFormData) => {
    setServerError(null);
    try {
      await apiClient.post("/api/auth/register", {
        name: data.name,
        email: data.email,
        password: data.password,
      });

      router.push("/login?registered=true");
    } catch (error: unknown) {
      const err = error as { response?: { data?: { message?: string } } };
      setServerError(
        err.response?.data?.message ?? "회원가입에 실패했습니다. 다시 시도해 주세요."
      );
    }
  };

  return (
    <div className="min-h-screen bg-gray-50 flex flex-col justify-center px-4 py-12">
      <div className="w-full max-w-sm mx-auto">
        <BrandLogo />

        {/* 회원가입 폼 */}
        <div className="card">
          <h2 className="text-xl font-bold text-gray-900 mb-6">회원가입</h2>

          {serverError && (
            <div className="mb-4 p-3 bg-red-50 border border-red-200 rounded-xl">
              <p className="text-sm text-red-600">{serverError}</p>
            </div>
          )}

          <form onSubmit={handleSubmit(onSubmit)} noValidate className="space-y-5">
            {/* 이름 */}
            <div>
              <label htmlFor="name" className="form-label">
                이름
              </label>
              <input
                id="name"
                type="text"
                autoComplete="name"
                placeholder="이름을 입력해 주세요"
                className={`input-field ${errors.name ? "input-error" : ""}`}
                {...register("name")}
              />
              {errors.name && (
                <p className="error-message">{errors.name.message}</p>
              )}
            </div>

            {/* 이메일 */}
            <div>
              <label htmlFor="email" className="form-label">
                이메일
              </label>
              <input
                id="email"
                type="email"
                autoComplete="email"
                placeholder="이메일을 입력해 주세요"
                className={`input-field ${errors.email ? "input-error" : ""}`}
                {...register("email")}
              />
              {errors.email && (
                <p className="error-message">{errors.email.message}</p>
              )}
            </div>

            {/* 비밀번호 */}
            <div>
              <label htmlFor="password" className="form-label">
                비밀번호
              </label>
              <input
                id="password"
                type="password"
                autoComplete="new-password"
                placeholder="8자 이상, 대문자·숫자 포함"
                className={`input-field ${errors.password ? "input-error" : ""}`}
                {...register("password")}
              />
              {errors.password && (
                <p className="error-message">{errors.password.message}</p>
              )}
            </div>

            {/* 비밀번호 확인 */}
            <div>
              <label htmlFor="passwordConfirm" className="form-label">
                비밀번호 확인
              </label>
              <input
                id="passwordConfirm"
                type="password"
                autoComplete="new-password"
                placeholder="비밀번호를 다시 입력해 주세요"
                className={`input-field ${errors.passwordConfirm ? "input-error" : ""}`}
                {...register("passwordConfirm")}
              />
              {errors.passwordConfirm && (
                <p className="error-message">{errors.passwordConfirm.message}</p>
              )}
            </div>

            {/* 회원가입 버튼 */}
            <button
              type="submit"
              disabled={isSubmitting}
              className="btn-primary mt-2"
            >
              {isSubmitting ? "처리 중..." : "회원가입"}
            </button>
          </form>

          {/* 로그인 링크 */}
          <div className="mt-6 text-center">
            <p className="text-sm text-gray-600">
              이미 계정이 있으신가요?{" "}
              <Link
                href="/login"
                className="text-primary-600 font-semibold hover:underline"
              >
                로그인
              </Link>
            </p>
          </div>
        </div>
      </div>
    </div>
  );
}
